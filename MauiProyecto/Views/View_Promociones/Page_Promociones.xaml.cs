using WCF_Apl_Dis;

using System.Collections.ObjectModel;

namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Promociones;

public partial class Page_Promociones : ContentPage
{
    private Service1Client servicio;

    public Page_Promociones()
    {
        InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);
        servicio = APP_MAUI_Apl_Dis_2025_II.Services.WCFConfig.CreateWCFClient();
        CargarPromociones();
    }

    // ⬆️ ir a crear promoción
    private async void btnAgregarPromocion(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Page_CrearPromociones");
    }

    // LISTAR PROMOCIONES
    private async void CargarPromociones()
    {
        try
        {
            var lista = await servicio.Get_PromocionesAsync(true);
            cvPromociones.ItemsSource = lista;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // ELIMINAR PROMOCIÓN
    private async void btnEliminar_Clicked(object sender, EventArgs e)
    {
        var boton = (ImageButton)sender;
        var promo = (Cls_Promociones)boton.BindingContext;

        bool confirmar = false;

        // ⚠️ DisplayAlert SIEMPRE debe ejecutarse en MainThread
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            confirmar = await DisplayAlert("Eliminar",
                         $"¿Eliminar la promoción '{promo.Nombre}'?",
                         "Sí", "No");
        });

        if (!confirmar) return;

        await servicio.Delete_PromocionAsync(promo.Id_Promocion);

        CargarPromociones(); // refrescar
    }

    // EDITAR PROMOCIÓN
    private async void btnEditar_Clicked(object sender, EventArgs e)
    {
        var boton = (ImageButton)sender;
        var promo = (Cls_Promociones)boton.BindingContext;

        // Navegación con ID
        await Shell.Current.GoToAsync($"Page_CrearPromociones?Id_Promocion={promo.Id_Promocion}");
    }
}
