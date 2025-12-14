using System.ServiceModel;
using WCF_Apl_Dis;

namespace APP_MAUI_Apl_Dis_2025_II.Services
{
    /// <summary>
    /// Clase encargada de gestionar la configuración y creación de clientes WCF
    /// Centraliza la configuración de IP, puerto y endpoints
    /// </summary>
    public static class WCFConfig
    {
        /// <summary>
        /// Crea una instancia del cliente WCF con la configuración correcta
        /// </summary>
        public static Service1Client CreateWCFClient()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[WCFConfig] Creando cliente WCF...");
                System.Diagnostics.Debug.WriteLine($"[WCFConfig] URL destino: {Global.WCF_SERVICE_URL}");

                if (Global.WCF_SERVER_IP != "localhost" && Global.WCF_SERVER_IP != "127.0.0.1" && Global.WCF_SERVER_PORT > 1024)
                {
                     System.Diagnostics.Debug.WriteLine($"[WCFConfig] ADVERTENCIA: Estás usando una IP remota ({Global.WCF_SERVER_IP}) con un puerto alto ({Global.WCF_SERVER_PORT}).");
                     System.Diagnostics.Debug.WriteLine($"[WCFConfig] Si el servidor es IIS Express, este rechazará conexiones externas con '400 Bad Request' a menos que esté configurado explícitamente.");
                }

                // Crear la configuración de binding igual al generado automáticamente
                // BasicHttpBinding usa SOAP 1.1 por defecto, compatible con .NET Framework WCF
                var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
                binding.MaxBufferSize = int.MaxValue;
                binding.MaxReceivedMessageSize = int.MaxValue;
                binding.AllowCookies = true; // Igual que el binding generado
                
                // Configurar ReaderQuotas usando Max (igual que el generado)
                binding.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
                
                // Timeouts opcionales
                binding.OpenTimeout = TimeSpan.FromMinutes(1);
                binding.CloseTimeout = TimeSpan.FromMinutes(1);
                binding.SendTimeout = TimeSpan.FromMinutes(1);
                binding.ReceiveTimeout = TimeSpan.FromMinutes(1);
                
                System.Diagnostics.Debug.WriteLine($"[WCFConfig] Binding configurado:");
                System.Diagnostics.Debug.WriteLine($"[WCFConfig] - SecurityMode: {binding.Security.Mode}");
                System.Diagnostics.Debug.WriteLine($"[WCFConfig] - MaxBufferSize: {binding.MaxBufferSize}");
                System.Diagnostics.Debug.WriteLine($"[WCFConfig] - MaxReceivedMessageSize: {binding.MaxReceivedMessageSize}");
                System.Diagnostics.Debug.WriteLine($"[WCFConfig] - TransferMode: {binding.TransferMode}");
                System.Diagnostics.Debug.WriteLine($"[WCFConfig] - TextEncoding: {binding.TextEncoding}");
                System.Diagnostics.Debug.WriteLine($"[WCFConfig] - ReaderQuotas.MaxStringContentLength: {binding.ReaderQuotas.MaxStringContentLength}");
                System.Diagnostics.Debug.WriteLine($"[WCFConfig] - ReaderQuotas.MaxArrayLength: {binding.ReaderQuotas.MaxArrayLength}");

                // Crear el endpoint con la URL configurada en Global
                var endpoint = new EndpointAddress(Global.WCF_SERVICE_URL);

                // Crear y retornar el cliente
                var client = new Service1Client(binding, endpoint);

                System.Diagnostics.Debug.WriteLine($"[WCFConfig] Cliente WCF creado exitosamente");
                System.Diagnostics.Debug.WriteLine($"[WCFConfig] Endpoint: {Global.WCF_SERVICE_URL}");

                return client;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WCFConfig] Error al crear cliente WCF: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[WCFConfig] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Verifica la conectividad con el servidor WCF
        /// </summary>
        public static async Task<bool> VerifyConnection()
        {
            Service1Client client = null;
            try
            {
                client = CreateWCFClient();
                System.Diagnostics.Debug.WriteLine("[WCFConfig] Verificando conectividad con servidor WCF...");

                // Intentar una operación simple para verificar la conexión
                // Nota: Esto depende de tu servicio WCF, ajusta según sea necesario
                await client.OpenAsync();
                client.Close();

                System.Diagnostics.Debug.WriteLine("[WCFConfig] ✓ Conectividad verificada exitosamente");
                return true;
            }
            catch (CommunicationException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WCFConfig] ✗ Error de comunicación: {ex.Message}");
                return false;
            }
            catch (TimeoutException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WCFConfig] ✗ Timeout: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WCFConfig] ✗ Error inesperado: {ex.Message}");
                return false;
            }
            finally
            {
                if (client != null)
                {
                    try
                    {
                        if (client.State == CommunicationState.Faulted)
                            client.Abort();
                        else
                            client.Close();
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Cambia la configuración del servidor WCF
        /// </summary>
        public static void ChangeServer(string ip, int port, string projectName = "WCF_Services_Apl_Dis_2025_II")
        {
            Global.WCF_SERVER_IP = ip;
            Global.WCF_SERVER_PORT = port;
            Global.WCF_PROJECT_NAME = projectName;
            System.Diagnostics.Debug.WriteLine($"[WCFConfig] Configuración actualizada: {Global.WCF_SERVICE_URL}");
        }

        /// <summary>
        /// Obtiene la URL actual del servicio WCF
        /// </summary>
        public static string GetServiceURL()
        {
            return Global.WCF_SERVICE_URL;
        }
    }
}
