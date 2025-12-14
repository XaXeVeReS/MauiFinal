using APP_MAUI_Apl_Dis_2025_II.Core;
using WCF_Apl_Dis; // Asegúrate de tener el namespace de Cls_Alerta

namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Alertas;

public partial class Page_Alertas : ContentPage
{
    public Page_Alertas()
    {
        InitializeComponent();

        // Carga inicial
        cvAlertas.ItemsSource = AlertManager.Instancia.Alertas;

        // Suscripción a cambios
        AlertManager.Instancia.OnAlertasActualizadas += (lista) =>
        {
            // Usamos MainThread para evitar errores de hilos
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Al usar ObservableCollection directamenta, a veces no es necesario resetear el ItemsSource,
                // pero si prefieres forzarlo:
                cvAlertas.ItemsSource = null;
                cvAlertas.ItemsSource = AlertManager.Instancia.Alertas;
            });
        };
    }

    private void BtnCerrar_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is Cls_Alerta alertaCerrar)
        {
            // CAMBIO CLAVE: Le decimos al Manager que cierre la alerta.
            // Esto la borra de la vista Y la agrega a la lista negra para que no vuelva.
            AlertManager.Instancia.CerrarAlerta(alertaCerrar);
        }
    }
}