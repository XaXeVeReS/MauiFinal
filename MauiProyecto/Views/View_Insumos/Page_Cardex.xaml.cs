using System.Collections.ObjectModel;
using WCF_Apl_Dis;

namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Insumos;

public partial class Page_Cardex : ContentPage, IQueryAttributable
{
    Service1Client Client;
    private int _insumoId;
    private ObservableCollection<CardexViewModel> _todosMovimientos;
    private ObservableCollection<CardexViewModel> _movimientosFiltrados;

    public Page_Cardex()
    {
        InitializeComponent();
        Client = APP_MAUI_Apl_Dis_2025_II.Services.WCFConfig.CreateWCFClient();
        
        _todosMovimientos = new ObservableCollection<CardexViewModel>();
        _movimientosFiltrados = new ObservableCollection<CardexViewModel>();
        collectionMovimientos.ItemsSource = _movimientosFiltrados;
        
        pickerTipoMovimiento.SelectedIndex = 0; // Todos
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("InsumoId"))
        {
            _insumoId = (int)query["InsumoId"];
            CargarDatos();
        }
    }

    private async void CargarDatos()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[CARDEX] Cargando datos del insumo ID: {_insumoId}");

            // Cargar información del insumo
            var insumo = await Client.Search_InsumoAsync(_insumoId);
            if (insumo != null)
            {
                lblNombreInsumo.Text = $"Insumo: {insumo.Nombre}";
                lblStockActual.Text = $"Stock Actual: {insumo.Stock_Disponible} {insumo.Unidad_Medida}";
                lblStockMinimo.Text = $"Stock Mínimo: {insumo.Stock_Minimo} {insumo.Unidad_Medida}";
            }

            // Cargar movimientos del cardex
            var movimientos = await Client.Get_CardexByInsumoAsync(_insumoId);

            _todosMovimientos.Clear();
            if (movimientos != null && movimientos.Count > 0)
            {
                // Ordenar por fecha descendente (más recientes primero)
                var movimientosOrdenados = movimientos.OrderByDescending(m => m.Fecha_Movimiento).ToArray();

                foreach (var mov in movimientosOrdenados)
                {
                    _todosMovimientos.Add(new CardexViewModel(mov));
                }

                System.Diagnostics.Debug.WriteLine($"[CARDEX] ✓ {movimientos.Count} movimientos cargados");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[CARDEX] No hay movimientos");
            }

            AplicarFiltro();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CARDEX] ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Error al cargar cardex: {ex.Message}", "OK");
        }
    }

    private void AplicarFiltro()
    {
        _movimientosFiltrados.Clear();

        var tipoSeleccionado = pickerTipoMovimiento.SelectedIndex switch
        {
            1 => "Entrada",
            2 => "Salida",
            _ => null
        };

        var filtrados = tipoSeleccionado == null
            ? _todosMovimientos
            : _todosMovimientos.Where(m => m.Tipo_Movimiento == tipoSeleccionado);

        foreach (var mov in filtrados)
        {
            _movimientosFiltrados.Add(mov);
        }

        System.Diagnostics.Debug.WriteLine($"[CARDEX] Filtro aplicado: {_movimientosFiltrados.Count} movimientos");
    }

    private void OnFiltroChanged(object sender, EventArgs e)
    {
        AplicarFiltro();
    }

    private void Actualizar_Clicked(object sender, EventArgs e)
    {
        CargarDatos();
    }

    private async void Volver_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    // ViewModel para bindeo
    public class CardexViewModel
    {
        public string Tipo_Movimiento { get; set; }
        public float Cantidad { get; set; }
        public string Cantidad_Formatted { get; set; }
        public string Motivo { get; set; }
        public DateTime Fecha_Movimiento { get; set; }
        public string Fecha_Movimiento_Formatted { get; set; }
        public int Id_Usuario { get; set; }
        public string Usuario_Display { get; set; }
        public Color Color_Tipo { get; set; }

        public CardexViewModel(Cls_CardexInsumos cardex)
        {
            Tipo_Movimiento = cardex.Tipo_Movimiento;
            Cantidad = cardex.Cantidad;
            Motivo = cardex.Motivo ?? "Sin descripción";
            Fecha_Movimiento = cardex.Fecha_Movimiento;
            Id_Usuario = cardex.Id_Usuario;

            // Formatear fecha
            Fecha_Movimiento_Formatted = Fecha_Movimiento.ToString("dd/MM/yyyy HH:mm");

            // Formatear cantidad con signo
            Cantidad_Formatted = Tipo_Movimiento == "Entrada"
                ? $"+{Cantidad}"
                : $"-{Cantidad}";

            // Usuario
            Usuario_Display = $"Usuario ID: {Id_Usuario}";

            // Color según tipo
            Color_Tipo = Tipo_Movimiento == "Entrada"
                ? Colors.Green
                : Colors.Red;
        }
    }
}
