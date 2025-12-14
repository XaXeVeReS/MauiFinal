using System.Collections.ObjectModel;
using WCF_Apl_Dis;

namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Compras;

public partial class Page_Detalle_Compra : ContentPage, IQueryAttributable
{
    Service1Client Client;
    private int _compraId { get; set; }
    private ObservableCollection<DetalleViewModel> _detalles;

    public Page_Detalle_Compra()
    {
        InitializeComponent();
        Client = APP_MAUI_Apl_Dis_2025_II.Services.WCFConfig.CreateWCFClient();

        _detalles = new ObservableCollection<DetalleViewModel>();
        collectionDetalles.ItemsSource = _detalles;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("CompraId"))
        {
            _compraId = int.Parse(query["CompraId"].ToString());
            CargarDetalle();
        }
    }

    private async void CargarDetalle()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[DETALLE_COMPRA] Cargando compra ID: {_compraId}");

            // Cargar información general
            var compra = await Client.Search_CompraAsync(_compraId);
            if (compra != null)
            {
                lblIdCompra.Text = $"Compra #{compra.Id_Compra}";
                lblFecha.Text = compra.Fecha_Compra.ToString("dd/MM/yyyy");
                lblProveedor.Text = compra.Proveedor ?? "Sin proveedor";
                lblTotal.Text = $"S/. {compra.Total:F2}";
                lblEstado.Text = compra.Estado ?? "Pendiente";
                lblObservaciones.Text = string.IsNullOrWhiteSpace(compra.Observaciones)
                    ? "Sin observaciones"
                    : compra.Observaciones;

                // Color del estado
                borderEstado.BackgroundColor = compra.Estado switch
                {
                    "Pendiente" => Colors.Orange,
                    "Recibida" => Colors.Green,
                    "Cancelada" => Colors.Red,
                    _ => Colors.Gray
                };
            }

            // Cargar detalles
            var detalles = await Client.Get_DetallesCompraByCompraIdAsync(_compraId);
            _detalles.Clear();

            if (detalles != null && detalles.Count > 0)
            {
                foreach (var det in detalles)
                {
                    var insumo = await Client.Search_InsumoAsync(det.Id_Insumo);
                    if (insumo != null)
                    {
                        _detalles.Add(new DetalleViewModel
                        {
                            Nombre_Insumo = insumo.Nombre,
                            Unidad_Medida = insumo.Unidad_Medida,
                            Cantidad = det.Cantidad,
                            Precio_Unitario = (decimal)det.Precio_Unitario
                        });
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[DETALLE_COMPRA] ✓ {detalles.Count} detalles cargados");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[DETALLE_COMPRA] No hay detalles");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DETALLE_COMPRA] ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Error al cargar detalle de compra: {ex.Message}", "OK");
        }
    }

    private async void Volver_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    // ViewModel para detalles
    public class DetalleViewModel
    {
        public string Nombre_Insumo { get; set; }
        public string Unidad_Medida { get; set; }
        public float Cantidad { get; set; }
        public decimal Precio_Unitario { get; set; }

        public string Cantidad_Display => $"{Cantidad} {Unidad_Medida}";
        public string PrecioUnitario_Display => $"S/. {Precio_Unitario:F2} c/u";
        public decimal Subtotal => (decimal)Cantidad * Precio_Unitario;
        public string Subtotal_Display => $"S/. {Subtotal:F2}";
    }
}
