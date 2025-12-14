using APP_MAUI_Apl_Dis_2025_II.Core;
using Microsoft.Maui.Controls;
using WCF_Apl_Dis;

namespace APP_MAUI_Apl_Dis_2025_II.Views.Components
{
    public partial class Encabezado : Grid
    {
        public Encabezado()
        {
            InitializeComponent();

            // Escuchar cuando cambian las alertas
            AlertManager.Instancia.OnAlertasActualizadas += CambiarCampana;

            // Estado inicial
            CambiarCampana(AlertManager.Instancia.Alertas.ToList());
        }

        private async void CambiarCampana(List<Cls_Alerta> alertas)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (alertas.Count > 0)
                {
                    // Cambiar imagen
                    btnCampana.Source = "campana_roja.png";

                    
                    await AnimarCampana();
                }
                else
                {
                    btnCampana.Source = "campana.png";
                }
            });
        }

        private async Task AnimarCampana()
        {
            // Animación estilo "notificación"
            for (int i = 0; i < 2; i++)
            {
                await btnCampana.ScaleTo(1.3, 120, Easing.CubicIn);
                await btnCampana.ScaleTo(1.0, 120, Easing.CubicOut);
            }
        }

        private void MenuButton_Clicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = true;
        }

        private async void Notificaciones_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//alertas_page");
        }
    }
}
