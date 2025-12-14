using System.Diagnostics;

namespace APP_MAUI_Apl_Dis_2025_II.Services
{
    /// <summary>
    /// Servicio para integración con la Pasarela de Pago WPF
    /// </summary>
    public static class PasarelaPagoService
    {
        /// <summary>
        /// Ruta de la aplicación de la pasarela de pago
        /// </summary>
        private static string RutaPasarela = @"C:\Users\Luis\Documents\Distribuidas\Pasarela_pago\PasarelaWPF\bin\Debug\net8.0-windows\PasarelaWPF.exe";

        /// <summary>
        /// Abre la pasarela de pago y espera el resultado
        /// </summary>
        /// <param name="monto">Monto a cobrar</param>
        /// <returns>Tupla con (exitoso, mensaje)</returns>
        public static async Task<(bool exitoso, string mensaje)> ProcesarPago(decimal monto)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[PASARELA] Iniciando proceso de pago por: {monto:C}");

                // Verificar que la pasarela existe
                if (!File.Exists(RutaPasarela))
                {
                    System.Diagnostics.Debug.WriteLine($"[PASARELA] ERROR: No se encuentra la pasarela en: {RutaPasarela}");
                    return (false, "No se encuentra la aplicación de pasarela de pago");
                }

                // Configurar proceso
                var startInfo = new ProcessStartInfo
                {
                    FileName = RutaPasarela,
                    Arguments = monto.ToString("F2"), // Enviar monto con 2 decimales
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                System.Diagnostics.Debug.WriteLine($"[PASARELA] Ejecutando: {startInfo.FileName} {startInfo.Arguments}");

                // Iniciar proceso
                using (var proceso = Process.Start(startInfo))
                {
                    if (proceso == null)
                    {
                        System.Diagnostics.Debug.WriteLine("[PASARELA] ERROR: No se pudo iniciar el proceso");
                        return (false, "No se pudo iniciar la pasarela de pago");
                    }

                    System.Diagnostics.Debug.WriteLine($"[PASARELA] Proceso iniciado con PID: {proceso.Id}");

                    // Esperar a que el proceso termine
                    await proceso.WaitForExitAsync();

                    int exitCode = proceso.ExitCode;
                    System.Diagnostics.Debug.WriteLine($"[PASARELA] Proceso finalizado con código: {exitCode}");

                    // Interpretar código de salida
                    // 0 = Pago exitoso
                    // 1 = Pago rechazado o cancelado
                    if (exitCode == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("[PASARELA] ✓ Pago APROBADO");
                        return (true, "Pago procesado exitosamente");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[PASARELA] ✗ Pago RECHAZADO o CANCELADO");
                        return (false, "Pago rechazado o cancelado por el usuario");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PASARELA] ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[PASARELA] StackTrace: {ex.StackTrace}");
                return (false, $"Error al procesar pago: {ex.Message}");
            }
        }

        /// <summary>
        /// Configura una ruta personalizada para la pasarela
        /// </summary>
        /// <param name="rutaPersonalizada">Ruta completa al ejecutable de la pasarela</param>
        public static void ConfigurarRutaPasarela(string rutaPersonalizada)
        {
            if (!string.IsNullOrWhiteSpace(rutaPersonalizada))
            {
                RutaPasarela = rutaPersonalizada;
                System.Diagnostics.Debug.WriteLine($"[PASARELA] Ruta configurada: {RutaPasarela}");
            }
        }

        /// <summary>
        /// Verifica si la pasarela está disponible
        /// </summary>
        /// <returns>True si la pasarela existe</returns>
        public static bool VerificarDisponibilidad()
        {
            bool existe = File.Exists(RutaPasarela);
            System.Diagnostics.Debug.WriteLine($"[PASARELA] Disponibilidad: {existe} - Ruta: {RutaPasarela}");
            return existe;
        }
    }
}
