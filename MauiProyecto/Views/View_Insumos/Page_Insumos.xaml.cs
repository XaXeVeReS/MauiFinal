using WCF_Apl_Dis;
using System.Collections.ObjectModel;

namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Insumos;

public partial class Page_Insumos : ContentPage
{
    Service1Client Client;
    ObservableCollection<InsumoViewModel> ListaInsumos;
    ObservableCollection<InsumoViewModel> ListaFiltrada;

    public Page_Insumos()
	{
		InitializeComponent();
        Client = APP_MAUI_Apl_Dis_2025_II.Services.WCFConfig.CreateWCFClient();
        ListaInsumos = new ObservableCollection<InsumoViewModel>();
        ListaFiltrada = new ObservableCollection<InsumoViewModel>();
        pickerFiltro.SelectedIndex = 0; // Todos
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarInsumos();
    }

    private async Task CargarInsumos()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[INSUMOS] Cargando lista de insumos...");
            loadingIndicator.IsVisible = true;
            loadingIndicator.IsRunning = true;

            var insumos = await Client.Get_InsumosAsync();
            
            ListaInsumos.Clear();
            
            if (insumos != null && insumos.Count > 0)
            {
                foreach (var insumo in insumos)
                {
                    ListaInsumos.Add(new InsumoViewModel(insumo));
                }
                System.Diagnostics.Debug.WriteLine($"[INSUMOS] ✓ {insumos.Count} insumos cargados");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[INSUMOS] No se encontraron insumos");
            }

            AplicarFiltro();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[INSUMOS] ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Error al cargar insumos: {ex.Message}", "OK");
        }
        finally
        {
            loadingIndicator.IsVisible = false;
            loadingIndicator.IsRunning = false;
        }
    }

    private void OnSearch(object sender, TextChangedEventArgs e)
    {
        AplicarFiltro();
    }

    private void OnFiltroChanged(object sender, EventArgs e)
    {
        AplicarFiltro();
    }

    private void AplicarFiltro()
    {
        var textoBusqueda = searchBar.Text?.ToLower() ?? "";
        var filtroSeleccionado = pickerFiltro.SelectedIndex;

        var filtrados = ListaInsumos.Where(i =>
        {
            // Filtro de búsqueda por nombre
            bool coincideBusqueda = string.IsNullOrWhiteSpace(textoBusqueda) || 
                                   i.Nombre.ToLower().Contains(textoBusqueda);

            // Filtro por estado de stock
            bool coincideFiltro = filtroSeleccionado switch
            {
                0 => true, // Todos
                1 => i.Insumo.Stock_Disponible <= i.Insumo.Stock_Minimo, // Stock Bajo
                2 => i.Insumo.Stock_Disponible > i.Insumo.Stock_Minimo, // Stock Normal
                _ => true
            };

            return coincideBusqueda && coincideFiltro;
        }).ToList();

        ListaFiltrada.Clear();
        foreach (var insumo in filtrados)
        {
            ListaFiltrada.Add(insumo);
        }

        listaInsumos.ItemsSource = ListaFiltrada;
    }

    private async void Nuevo_Insumo_Clicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[INSUMOS] Navegando a nuevo insumo");
        await Shell.Current.GoToAsync(nameof(Page_Form_Insumo));
    }

    private async void Editar_Insumo_Clicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var insumo = button?.CommandParameter as InsumoViewModel;

        if (insumo != null)
        {
            System.Diagnostics.Debug.WriteLine($"[INSUMOS] Editando insumo: {insumo.Nombre}");
            
            var navigationParameter = new Dictionary<string, object>
            {
                { "InsumoId", insumo.Id_Insumo }
            };

            await Shell.Current.GoToAsync(nameof(Page_Form_Insumo), navigationParameter);
        }
    }

    private async void Eliminar_Insumo_Clicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var insumo = button?.CommandParameter as InsumoViewModel;

        if (insumo != null)
        {
            bool confirmar = await DisplayAlert(
                "Confirmar Eliminación",
                $"¿Está seguro que desea eliminar el insumo '{insumo.Nombre}'?",
                "Eliminar",
                "Cancelar"
            );

            if (confirmar)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[INSUMOS] Eliminando insumo ID: {insumo.Id_Insumo}");
                    
                    await Client.Delete_InsumoAsync(insumo.Id_Insumo);
                    
                    System.Diagnostics.Debug.WriteLine("[INSUMOS] ✓ Insumo eliminado");
                    await DisplayAlert("Éxito", "Insumo eliminado correctamente", "OK");
                    
                    await CargarInsumos();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[INSUMOS] ERROR al eliminar: {ex.Message}");
                    await DisplayAlert("Error", $"Error al eliminar insumo: {ex.Message}", "OK");
                }
            }
        }
    }

    private async void Ver_Cardex_Clicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var insumo = button?.CommandParameter as InsumoViewModel;

        if (insumo != null)
        {
            System.Diagnostics.Debug.WriteLine($"[INSUMOS] Ver cardex de: {insumo.Nombre}");
            
            var navigationParameter = new Dictionary<string, object>
            {
                { "InsumoId", insumo.Id_Insumo },
                { "InsumoNombre", insumo.Nombre }
            };

            await Shell.Current.GoToAsync(nameof(Page_Cardex), navigationParameter);
        }
    }
}

// ViewModel para manejar el color del indicador de stock
public class InsumoViewModel
{
    public Cls_Insumos Insumo { get; set; }

    public InsumoViewModel(Cls_Insumos insumo)
    {
        Insumo = insumo;
    }

    public int Id_Insumo => Insumo.Id_Insumo;
    public string Nombre => Insumo.Nombre;
    public float Stock_Disponible => Insumo.Stock_Disponible;
    public float Stock_Minimo => Insumo.Stock_Minimo;
    public string Unidad_Medida => Insumo.Unidad_Medida;

    public Color Stock_Color
    {
        get
        {
            if (Insumo.Stock_Disponible <= Insumo.Stock_Minimo)
                return Colors.Red; // Stock bajo
            else if (Insumo.Stock_Disponible <= Insumo.Stock_Minimo * 1.5)
                return Colors.Orange; // Stock medio
            else
                return Colors.Green; // Stock normal
        }
    }
}