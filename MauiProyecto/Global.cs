using Microsoft.Maui.Devices;

namespace APP_MAUI_Apl_Dis_2025_II
{
    public static class Global
    {
        // Configuración de Usuario
        public static int Id_Usuario;
        public static string Rol_Usuario;
        public static bool Activo = false;

        // Configuración de WCF Service
        /// <summary>
        /// IP o nombre del servidor donde está alojado el servicio WCF
        /// Cambiar esta IP según donde esté tu servidor
        /// </summary>
        public static string WCF_SERVER_IP = DeviceInfo.Platform == DevicePlatform.Android ? "192.168.100.156" : "localhost";

        /// <summary>
        /// Puerto del servicio WCF (IIS Express típicamente usa puertos 64594, 64595, etc.)
        /// </summary>
        public static int WCF_SERVER_PORT = 64594;

        /// <summary>
        /// Nombre del proyecto/ruta virtual en IIS
        /// </summary>
        public static string WCF_PROJECT_NAME = "WCF_Services_Apl_Dis_2025_II";

        /// <summary>
        /// URL completa del servicio WCF construida dinámicamente
        /// </summary>
        public static string WCF_SERVICE_URL
        {
            get
            {
                string url = $"http://{WCF_SERVER_IP}:{WCF_SERVER_PORT}/Service1.svc";
                // System.Diagnostics.Debug.WriteLine($"[Global] WCF_SERVICE_URL accedida: {url}");
                return url;
            }
        }

        /// <summary>
        /// Método para cambiar la configuración del servidor WCF
        /// </summary>
        public static void SetWCFServer(string ip, int port)
        {
            WCF_SERVER_IP = ip;
            WCF_SERVER_PORT = port;
            System.Diagnostics.Debug.WriteLine($"[CONFIG] Servidor WCF cambiado a: {WCF_SERVICE_URL}");
        }
    }
}
