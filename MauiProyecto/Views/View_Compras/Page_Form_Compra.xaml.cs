using System.Collections.ObjectModel;
using WCF_Apl_Dis;

namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Compras;

public partial class Page_Form_Compra : ContentPage, IQueryAttributable
{
    Service1Client Client;
    private int? _compraId = null;
    private ObservableCollection<DetalleCompraViewModel> _detalles;

    public Page_Form_Compra()
    {
        InitializeComponent();
        Client = APP_MAUI_Apl_Dis_2025_II.Services.WCFConfig.CreateWCFClient();

        _detalles = new ObservableCollection<DetalleCompraViewModel>();
        collectionDetalles.ItemsSource = _detalles;

        dateFechaCompra.Date = DateTime.Now;
        pickerEstado.SelectedIndex = 0; // Pendiente
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("CompraId"))
        {
            _compraId = (int)query["CompraId"];
            CargarCompra(_compraId.Value);
        }
    }

    private async void CargarCompra(int id)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[FORM_COMPRA] Cargando compra ID: {id}");

            var compra = await Client.Search_CompraAsync(id);
            if (compra != null)
            {
                lblTitulo.Text = "EDITAR ORDEN DE COMPRA";
                btnGuardar.Text = "Actualizar";

                txtProveedor.Text = compra.Proveedor;
                dateFechaCompra.Date = compra.Fecha_Compra;
                txtObservaciones.Text = compra.Observaciones;

                int estadoIndex = pickerEstado.Items.IndexOf(compra.Estado ?? "Pendiente");
                if (estadoIndex >= 0)
                    pickerEstado.SelectedIndex = estadoIndex;

                // Cargar detalles
                var detalles = await Client.Get_DetallesCompraByCompraIdAsync(id);
                if (detalles != null && detalles.Count > 0)
                {
                    foreach (var det in detalles)
                    {
                        var insumo = await Client.Search_InsumoAsync(det.Id_Insumo);
                        if (insumo != null)
                        {
                            _detalles.Add(new DetalleCompraViewModel
                            {
                                Id_Insumo = det.Id_Insumo,
                                Nombre_Insumo = insumo.Nombre,
                                Unidad_Medida = insumo.Unidad_Medida,
                                Cantidad = det.Cantidad,
                                Precio_Unitario = (decimal)det.Precio_Unitario
                            });
                        }
                    }
                }

                CalcularTotal();
                System.Diagnostics.Debug.WriteLine($"[FORM_COMPRA] ✓ Compra cargada con {_detalles.Count} detalles");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FORM_COMPRA] ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Error al cargar compra: {ex.Message}", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }

    private async void Agregar_Insumo_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Obtener lista de insumos
            var insumos = await Client.Get_InsumosAsync();
            if (insumos == null || insumos.Count == 0)
            {
                await DisplayAlert("Atención", "No hay insumos disponibles", "OK");
                return;
            }

            // Mostrar selector de insumo
            var nombresInsumos = insumos.Select(i => i.Nombre).ToArray();
            string insumoSeleccionado = await DisplayActionSheet("Seleccionar Insumo", "Cancelar", null, nombresInsumos);

            if (insumoSeleccionado != "Cancelar" && !string.IsNullOrWhiteSpace(insumoSeleccionado))
            {
                var insumo = insumos.First(i => i.Nombre == insumoSeleccionado);

                // Verificar si ya está en la lista
                if (_detalles.Any(d => d.Id_Insumo == insumo.Id_Insumo))
                {
                    await DisplayAlert("Atención", "Este insumo ya está en la lista", "OK");
                    return;
                }

                // Pedir cantidad
                string cantidadStr = await DisplayPromptAsync("Cantidad",
                    $"Ingrese la cantidad ({insumo.Unidad_Medida})",
                    placeholder: "0.0",
                    keyboard: Keyboard.Numeric);

                if (string.IsNullOrWhiteSpace(cantidadStr))
                    return;

                if (!float.TryParse(cantidadStr, out float cantidad) || cantidad <= 0)
                {
                    await DisplayAlert("Error", "La cantidad debe ser mayor a 0", "OK");
                    return;
                }

                // Pedir precio unitario
                string precioStr = await DisplayPromptAsync("Precio Unitario",
                    $"Ingrese el precio unitario (S/.)",
                    placeholder: "0.00",
                    keyboard: Keyboard.Numeric);

                if (string.IsNullOrWhiteSpace(precioStr))
                    return;

                if (!decimal.TryParse(precioStr, out decimal precioUnitario) || precioUnitario <= 0)
                {
                    await DisplayAlert("Error", "El precio debe ser mayor a 0", "OK");
                    return;
                }

                // Agregar detalle
                _detalles.Add(new DetalleCompraViewModel
                {
                    Id_Insumo = insumo.Id_Insumo,
                    Nombre_Insumo = insumo.Nombre,
                    Unidad_Medida = insumo.Unidad_Medida,
                    Cantidad = cantidad,
                    Precio_Unitario = precioUnitario
                });

                CalcularTotal();
                System.Diagnostics.Debug.WriteLine($"[FORM_COMPRA] Detalle agregado: {insumo.Nombre} x {cantidad}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FORM_COMPRA] ERROR al agregar insumo: {ex.Message}");
            await DisplayAlert("Error", $"Error al agregar insumo: {ex.Message}", "OK");
        }
    }

    private void Quitar_Detalle_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is DetalleCompraViewModel detalle)
        {
            _detalles.Remove(detalle);
            CalcularTotal();
            System.Diagnostics.Debug.WriteLine($"[FORM_COMPRA] Detalle eliminado: {detalle.Nombre_Insumo}");
        }
    }

    private void CalcularTotal()
    {
        decimal total = _detalles.Sum(d => d.Subtotal);
        lblTotal.Text = $"S/. {total:F2}";
    }

    private async void Guardar_Clicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[FORM_COMPRA] Guardando compra...");

        // Validaciones
        if (string.IsNullOrWhiteSpace(txtProveedor.Text))
        {
            MostrarError("El proveedor es requerido");
            return;
        }

        if (pickerEstado.SelectedIndex < 0)
        {
            MostrarError("Seleccione un estado");
            return;
        }

        if (_detalles.Count == 0)
        {
            MostrarError("Debe agregar al menos un insumo a la compra");
            return;
        }

        try
        {
            btnGuardar.IsEnabled = false;

            decimal total = _detalles.Sum(d => d.Subtotal);

            var compra = new Cls_Compras
            {
                Fecha_Compra = dateFechaCompra.Date,
                Proveedor = txtProveedor.Text.Trim(),
                Total = (float)total,
                Estado = pickerEstado.Items[pickerEstado.SelectedIndex],
                Observaciones = txtObservaciones.Text?.Trim()
            };

            if (_compraId.HasValue)
            {
                // Actualizar
                compra.Id_Compra = _compraId.Value;
                System.Diagnostics.Debug.WriteLine($"[FORM_COMPRA] Actualizando compra ID: {compra.Id_Compra}");

                await Client.Update_CompraAsync(compra);

                // Eliminar detalles antiguos y agregar nuevos
                await Client.Delete_DetallesCompraAsync(_compraId.Value);

                foreach (var det in _detalles)
                {
                    var detalle = new Cls_DetalleCompras
                    {
                        Id_Compra = _compraId.Value,
                        Id_Insumo = det.Id_Insumo,
                        Cantidad = det.Cantidad,
                        Precio_Unitario = (float)det.Precio_Unitario
                    };
                    await Client.Insert_DetalleCompraAsync(detalle);
                }

                System.Diagnostics.Debug.WriteLine("[FORM_COMPRA] ✓ Compra actualizada");
                await DisplayAlert("Éxito", "Compra actualizada correctamente", "OK");
            }
            else
            {
                // Insertar
               
                System.Diagnostics.Debug.WriteLine("[FORM_COMPRA] Insertando nueva compra");
                List<Cls_DetalleCompras> detalleCompras = new List<Cls_DetalleCompras>();
                // Insertar detalles
                foreach (var det in _detalles)
                {
                    var detalle = new Cls_DetalleCompras
                    {
                        Id_Insumo = det.Id_Insumo,
                        Cantidad = det.Cantidad,
                        Precio_Unitario = (float)det.Precio_Unitario
                    };
                    detalleCompras.Add(detalle);
                }
                compra.Id_Usuario = Global.Id_Usuario;
                compra.DetalleCompras = detalleCompras;
                await Client.Insert_CompraAsync(compra);

                await DisplayAlert("Éxito", "Compra creada correctamente", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FORM_COMPRA] ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Error al guardar compra: {ex.Message}", "OK");
        }
        finally
        {
            btnGuardar.IsEnabled = true;
        }
    }

    private async void Cancelar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private void MostrarError(string mensaje)
    {
        lblError.Text = mensaje;
        lblError.IsVisible = true;
    }

    // ViewModel para detalles
    public class DetalleCompraViewModel
    {
        public int Id_Insumo { get; set; }
        public string Nombre_Insumo { get; set; }
        public string Unidad_Medida { get; set; }
        public float Cantidad { get; set; }
        public decimal Precio_Unitario { get; set; }

        public string Cantidad_Display => $"Cantidad: {Cantidad} {Unidad_Medida}";
        public decimal Subtotal => (decimal)Cantidad * Precio_Unitario;
        public string Subtotal_Display => $"Subtotal: S/. {Subtotal:F2}";
    }
}
