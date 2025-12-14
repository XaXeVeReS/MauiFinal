using System.Collections.ObjectModel;
using WCF_Apl_Dis;

namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Compras;

public partial class Page_Compras : ContentPage
{
    Service1Client Client;
    private ObservableCollection<CompraViewModel> _todasCompras;
    private ObservableCollection<CompraViewModel> _comprasFiltradas;

    public Page_Compras()
    {
        InitializeComponent();
        Client = APP_MAUI_Apl_Dis_2025_II.Services.WCFConfig.CreateWCFClient();

        _todasCompras = new ObservableCollection<CompraViewModel>();
        _comprasFiltradas = new ObservableCollection<CompraViewModel>();
        collectionCompras.ItemsSource = _comprasFiltradas;

        pickerEstado.SelectedIndex = 0; // Todos
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CargarCompras();
    }

    private async void CargarCompras()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[COMPRAS] Cargando compras...");

            var compras = await Client.Get_ComprasAsync();

            _todasCompras.Clear();
            if (compras != null && compras.Count > 0)
            {
                // Ordenar por fecha descendente
                var comprasOrdenadas = compras.OrderByDescending(c => c.Fecha_Compra).ToArray();

                foreach (var compra in comprasOrdenadas)
                {
                    _todasCompras.Add(new CompraViewModel(compra));
                }

                System.Diagnostics.Debug.WriteLine($"[COMPRAS] ✓ {compras.Count} compras cargadas");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[COMPRAS] No hay compras");
            }

            AplicarFiltro();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[COMPRAS] ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Error al cargar compras: {ex.Message}", "OK");
        }
    }

    private void AplicarFiltro()
    {
        _comprasFiltradas.Clear();

        var textoBusqueda = searchBar.Text?.ToLower() ?? "";
        var estadoSeleccionado = pickerEstado.SelectedIndex switch
        {
            1 => "Pendiente",
            2 => "Recibida",
            3 => "Cancelada",
            _ => null
        };

        var filtradas = _todasCompras.AsEnumerable();

        // Filtrar por búsqueda (proveedor)
        if (!string.IsNullOrWhiteSpace(textoBusqueda))
        {
            filtradas = filtradas.Where(c => c.Proveedor.ToLower().Contains(textoBusqueda));
        }

        // Filtrar por estado
        if (estadoSeleccionado != null)
        {
            filtradas = filtradas.Where(c => c.Estado == estadoSeleccionado);
        }

        foreach (var compra in filtradas)
        {
            _comprasFiltradas.Add(compra);
        }

        System.Diagnostics.Debug.WriteLine($"[COMPRAS] Filtro aplicado: {_comprasFiltradas.Count} compras");
    }

    private void OnSearch(object sender, TextChangedEventArgs e)
    {
        AplicarFiltro();
    }

    private void OnFiltroChanged(object sender, EventArgs e)
    {
        AplicarFiltro();
    }

    private void Actualizar_Clicked(object sender, EventArgs e)
    {
        CargarCompras();
    }

    private async void Nueva_Compra_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Page_Form_Compra");
    }

    private async void Ver_Detalle_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int compraId)
        {
            await Shell.Current.GoToAsync($"Page_Detalle_Compra?CompraId={compraId}");
        }
    }

    private async void Editar_Compra_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int compraId)
        {
            await Shell.Current.GoToAsync($"Page_Form_Compra?CompraId={compraId}");
        }
    }

    private async void Eliminar_Compra_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int compraId)
        {
            bool confirmar = await DisplayAlert("Confirmar",
                "¿Está seguro de eliminar esta orden de compra? Esta acción no se puede deshacer.",
                "Eliminar", "Cancelar");

            if (confirmar)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[COMPRAS] Eliminando compra ID: {compraId}");

                    await Client.Delete_CompraAsync(compraId);

                    System.Diagnostics.Debug.WriteLine("[COMPRAS] ✓ Compra eliminada");
                    await DisplayAlert("Éxito", "Orden de compra eliminada correctamente", "OK");

                    CargarCompras();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[COMPRAS] ERROR al eliminar: {ex.Message}");
                    await DisplayAlert("Error", $"Error al eliminar compra: {ex.Message}", "OK");
                }
            }
        }
    }

    // ViewModel para bindeo
    public class CompraViewModel
    {
        public int Id_Compra { get; set; }
        public string Id_Display { get; set; }
        public DateTime Fecha_Compra { get; set; }
        public string Fecha_Formatted { get; set; }
        public string Proveedor { get; set; }
        public decimal Total { get; set; }
        public string Total_Display { get; set; }
        public string Estado { get; set; }
        public Color Color_Estado { get; set; }
        public string Observaciones { get; set; }
        public bool Puede_Editar { get; set; }
        public bool Puede_Eliminar { get; set; }

        public CompraViewModel(Cls_Compras compra)
        {
            Id_Compra = compra.Id_Compra;
            Id_Display = $"Compra #{compra.Id_Compra}";
            Fecha_Compra = compra.Fecha_Compra;
            Fecha_Formatted = Fecha_Compra.ToString("dd/MM/yyyy");
            Proveedor = compra.Proveedor ?? "Sin proveedor";
            Total = (decimal)compra.Total;
            Total_Display = $"Total: S/. {Total:F2}";
            Estado = compra.Estado ?? "Pendiente";
            Observaciones = string.IsNullOrWhiteSpace(compra.Observaciones)
                ? "Sin observaciones"
                : compra.Observaciones;

            // Color según estado
            Color_Estado = Estado switch
            {
                "Pendiente" => Colors.Orange,
                "Recibida" => Colors.Green,
                "Cancelada" => Colors.Red,
                _ => Colors.Gray
            };

            // Solo se puede editar/eliminar si está Pendiente
            Puede_Editar = Estado == "Pendiente";
            Puede_Eliminar = Estado == "Pendiente";
        }
    }
}
