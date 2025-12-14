using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCF_Apl_Dis;

namespace APP_MAUI_Apl_Dis_2025_II.Core
{
    public class AlertDetector
    {
        private readonly Service1Client servicio;

        public AlertDetector()
        {
            servicio = new Service1Client();
        }

        /// <summary>
        /// Llama al WCF, obtiene alertas y actualiza el AlertManager
        /// </summary>
        public async Task EjecutarDeteccionAsync()
        {
            try
            {
                var alertas = await servicio.Get_AlertasAsync();

                // Entregar alertas al Manager
                AlertManager.Instancia.EstablecerAlertas(alertas.ToList());
            }
            catch (Exception ex)
            {
                // Opcional: podrías mostrar un DisplayAlert
                Console.WriteLine("Error en AlertDetector: " + ex.Message);
            }
        }
    }
}
