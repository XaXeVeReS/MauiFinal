using WCF_Apl_Dis;

namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Insumos;

public partial class Page_Form_Insumo : ContentPage, IQueryAttributable
{
    Service1Client Client;
    private int? _insumoId = null;
    private Cls_Insumos _insumoActual = null;

    public Page_Form_Insumo()
    {
        InitializeComponent();
        Client = APP_MAUI_Apl_Dis_2025_II.Services.WCFConfig.CreateWCFClient();
        pickerUnidad.SelectedIndex = 0; // Kg por defecto
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("InsumoId"))
        {
            _insumoId = (int)query["InsumoId"];
            CargarInsumo(_insumoId.Value);
        }
    }

    private async void CargarInsumo(int id)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[FORM_INSUMO] Cargando insumo ID: {id}");

            _insumoActual = await Client.Search_InsumoAsync(id);

            if (_insumoActual != null)
            {
                lblTitulo.Text = "EDITAR INSUMO";
                btnGuardar.Text = "Actualizar";

                txtNombre.Text = _insumoActual.Nombre;
                txtStockDisponible.Text = _insumoActual.Stock_Disponible.ToString();
                txtStockMinimo.Text = _insumoActual.Stock_Minimo.ToString();

                // Seleccionar unidad de medida
                int index = pickerUnidad.Items.IndexOf(_insumoActual.Unidad_Medida);
                if (index >= 0)
                    pickerUnidad.SelectedIndex = index;

                System.Diagnostics.Debug.WriteLine($"[FORM_INSUMO] ✓ Insumo cargado: {_insumoActual.Nombre}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FORM_INSUMO] ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Error al cargar insumo: {ex.Message}", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }

    private async void Guardar_Clicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[FORM_INSUMO] Guardando insumo...");

        // Validaciones
        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            MostrarError("El nombre del insumo es requerido");
            return;
        }

        if (pickerUnidad.SelectedIndex < 0)
        {
            MostrarError("Seleccione una unidad de medida");
            return;
        }

        if (!float.TryParse(txtStockDisponible.Text, out float stockDisponible) || stockDisponible < 0)
        {
            MostrarError("El stock disponible debe ser un número válido mayor o igual a 0");
            return;
        }

        if (!float.TryParse(txtStockMinimo.Text, out float stockMinimo) || stockMinimo < 0)
        {
            MostrarError("El stock mínimo debe ser un número válido mayor o igual a 0");
            return;
        }

        try
        {
            btnGuardar.IsEnabled = false;

            var insumo = new Cls_Insumos
            {
                Nombre = txtNombre.Text.Trim(),
                Unidad_Medida = pickerUnidad.Items[pickerUnidad.SelectedIndex],
                Stock_Disponible = stockDisponible,
                Stock_Minimo = stockMinimo
            };

            if (_insumoId.HasValue)
            {
                // Actualizar
                insumo.Id_Insumo = _insumoId.Value;
                System.Diagnostics.Debug.WriteLine($"[FORM_INSUMO] Actualizando insumo ID: {insumo.Id_Insumo}");

                await Client.Update_InsumoAsync(insumo);

                System.Diagnostics.Debug.WriteLine("[FORM_INSUMO] ✓ Insumo actualizado");
                await DisplayAlert("Éxito", "Insumo actualizado correctamente", "OK");
            }
            else
            {
                // Insertar
                System.Diagnostics.Debug.WriteLine("[FORM_INSUMO] Insertando nuevo insumo");

                await Client.Insert_InsumoAsync(insumo, Global.Id_Usuario);

                System.Diagnostics.Debug.WriteLine("[FORM_INSUMO] ✓ Insumo creado");
                await DisplayAlert("Éxito", "Insumo creado correctamente", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FORM_INSUMO] ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Error al guardar insumo: {ex.Message}", "OK");
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
}
