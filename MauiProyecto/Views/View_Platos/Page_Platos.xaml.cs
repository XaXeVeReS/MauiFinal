using System.Collections.ObjectModel;
using WCF_Apl_Dis;
namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Platos;

public partial class Page_Platos : ContentPage
{
    Service1Client servicio;

    public Page_Platos()
	{
		InitializeComponent();
        servicio = APP_MAUI_Apl_Dis_2025_II.Services.WCFConfig.CreateWCFClient();
        Shell.SetNavBarIsVisible(this, false);
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        CargarPlatos();   // <--- AQUÍ SE REFRESCA LA PÁGINA
    }
    private async void btnAgregarPlato(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Page_CrearPlato");
    }
    private async void CargarPlatos()
    {
        try
        {
            var lista = await servicio.Get_PlatosAsync();
            cvPlatos.ItemsSource = lista;  
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void btnEliminar_Clicked(object sender, EventArgs e)
    {
        var boton = (ImageButton)sender;
        var plato = (Cls_Platos)boton.BindingContext;

        bool confirmar = false;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            confirmar = await DisplayAlert("Eliminar",
                        $"¿Eliminar {plato.Nombre}?", "Sí", "No");
        });
        if (!confirmar) return;

        await servicio.Delete_PlatoAsync(plato.Id_Plato);

        CargarPlatos(); // refrescar lista
    }

    private async void btnEditar_Clicked(object sender, EventArgs e)
    {
        var boton = (ImageButton)sender;
        var plato = (Cls_Platos)boton.BindingContext;

        await Shell.Current.GoToAsync($"Page_CrearPlato?Id_Plato={plato.Id_Plato}");
    }
}

