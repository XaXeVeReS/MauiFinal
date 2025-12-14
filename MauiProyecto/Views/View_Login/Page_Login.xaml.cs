using WCF_Apl_Dis;
using System.ServiceModel;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Maui.Networking;

namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Login;

public partial class Page_Login : ContentPage
{

	public Page_Login()
	{
		InitializeComponent();
        lblError.IsVisible = false;
    }

    private async void Login_User(object sender, EventArgs e)
    {
        WCF_Apl_Dis.Service1Client Client = null;

        try
        {
            System.Diagnostics.Debug.WriteLine("========== INICIO DE PROCESO DE LOGIN ==========");
            
            if (Connectivity.NetworkAccess != NetworkAccess.Internet && Connectivity.NetworkAccess != NetworkAccess.ConstrainedInternet && Connectivity.NetworkAccess != NetworkAccess.Local)
            {
                System.Diagnostics.Debug.WriteLine($"[LOGIN] Estado de red: {Connectivity.NetworkAccess}");
                // No bloqueamos, solo advertimos en log, porque Local es válido para desarrollo
            }

            if (string.IsNullOrWhiteSpace(correo.Text) || string.IsNullOrWhiteSpace(contrasena.Text))
            {
                System.Diagnostics.Debug.WriteLine("[LOGIN] Campos vacíos detectados");
                lblError.Text = "Por favor ingresa correo y contraseña";
                lblError.IsVisible = true;
                await DisplayAlert("Error", "Por favor ingresa correo y contraseña", "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[LOGIN] Email ingresado: {correo.Text}");
            
            // Crear cliente WCF con la URL del servidor configurada en Global
            System.Diagnostics.Debug.WriteLine($"[LOGIN] URL del servicio: {Global.WCF_SERVICE_URL}");
            
            Client = APP_MAUI_Apl_Dis_2025_II.Services.WCFConfig.CreateWCFClient();

            if (Client.Endpoint.Binding is BasicHttpBinding binding)
            {
                System.Diagnostics.Debug.WriteLine($"[LOGIN] Binding Security Mode: {binding.Security.Mode}");
                System.Diagnostics.Debug.WriteLine($"[LOGIN] Binding Transport ClientCredentialType: {binding.Security.Transport.ClientCredentialType}");
            }

            System.Diagnostics.Debug.WriteLine("[LOGIN] Llamando Login_UserAsync...");
            int id_user = await Client.Login_UserAsync(correo.Text, contrasena.Text);
            System.Diagnostics.Debug.WriteLine($"[LOGIN] Resultado de Login_UserAsync: {id_user}");

            if (id_user > 0)
            {
                System.Diagnostics.Debug.WriteLine($"[LOGIN] ID de usuario válido: {id_user}. Buscando información del usuario...");
                Cls_Usuarios user = null;
                
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[LOGIN] Llamando Search_UserAsync con ID: {id_user}");
                    user = await Client.Search_UserAsync(id_user);
                    
                    if (user != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[LOGIN] Usuario encontrado. Nombre: {user.Nombre}, Rol: {user.Rol}, Activo: {user.Activo}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[LOGIN] Search_UserAsync devolvió null");
                    }
                }
                catch (Exception searchEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[LOGIN] Error en Search_UserAsync: {searchEx.GetType().Name} - {searchEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"[LOGIN] StackTrace: {searchEx.StackTrace}");
                    user = null;
                }

                if (user == null || user.Id_Usuario <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("[LOGIN] Fallback a Get_UsersAsync...");
                    try
                    {
                        var lista = await Client.Get_UsersAsync();
                        if (lista != null && lista.Count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"[LOGIN] Se obtuvieron {lista.Count} usuarios. Buscando por ID...");
                            user = lista.FirstOrDefault(u => u.Id_Usuario == id_user);
                            
                            if (user != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"[LOGIN] Usuario encontrado en lista. Nombre: {user.Nombre}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[LOGIN] Usuario con ID {id_user} no encontrado en lista");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("[LOGIN] Get_UsersAsync devolvió lista vacía o nula");
                        }
                    }
                    catch (Exception listEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"[LOGIN] Error en Get_UsersAsync: {listEx.GetType().Name} - {listEx.Message}");
                    }
                }

                if (user == null || user.Id_Usuario <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("[LOGIN] No se pudo obtener información del usuario. Error fatal.");
                    lblError.Text = "Error de conexión";
                    lblError.IsVisible = true;
                    await DisplayAlert("Error", "No se pudo recuperar la información del usuario. Verifica que el servicio WCF esté funcionando correctamente.", "OK");
                    return;
                }

                // Validación de Rol y Estado
                if (!user.Activo)
                {
                    System.Diagnostics.Debug.WriteLine($"[LOGIN] Usuario {user.Id_Usuario} está inactivo. Verificando estado en BD...");
                    try
                    {
                        bool estadoActual = await Client.Check_User_StatusAsync(user.Id_Usuario);
                        System.Diagnostics.Debug.WriteLine($"[LOGIN] Estado actual en BD: {estadoActual}");
                        
                        if (!estadoActual)
                        {
                            lblError.Text = "Usuario inactivo";
                            lblError.IsVisible = true;
                            await DisplayAlert("Acceso Denegado", "Tu usuario se encuentra inactivo. Contacta al administrador.", "OK");
                            return;
                        }
                        else
                        {
                            user.Activo = true;
                            System.Diagnostics.Debug.WriteLine("[LOGIN] Usuario activado en base de datos");
                        }
                    }
                    catch (Exception statusEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"[LOGIN] Error en Check_User_StatusAsync: {statusEx.Message}");
                        lblError.Text = "Usuario inactivo";
                        lblError.IsVisible = true;
                        await DisplayAlert("Acceso Denegado", "Tu usuario se encuentra inactivo. Contacta al administrador.", "OK");
                        return;
                    }
                }
                
                if (user.Rol == "Cliente")
                {
                    System.Diagnostics.Debug.WriteLine($"[LOGIN] Acceso denegado. Usuario con rol Cliente no permitido");
                    lblError.Text = "Acceso denegado";
                    lblError.IsVisible = true;
                    await DisplayAlert("Acceso Denegado", "La aplicación móvil es solo para personal autorizado. Los clientes no tienen acceso.", "OK");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[LOGIN] ✓ LOGIN EXITOSO. Usuario: {user.Nombre}, Rol: {user.Rol}");
                
                Global.Id_Usuario = user.Id_Usuario;
                Global.Rol_Usuario = user.Rol;
                Global.Activo = user.Activo;
                
                lblError.IsVisible = false;
                await Shell.Current.GoToAsync("//pedidos_page");
            }
            else if (id_user == -2)
            {
                System.Diagnostics.Debug.WriteLine("[LOGIN] Usuario bloqueado");
                lblError.Text = "Usuario bloqueado";
                lblError.IsVisible = true;
                await DisplayAlert("Acceso Denegado", "Tu usuario ha sido bloqueado por múltiples intentos fallidos.", "OK");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[LOGIN] Credenciales inválidas");
                lblError.Text = "Correo o contraseña incorrectos";
                lblError.IsVisible = true;
                await DisplayAlert("Error de Autenticación", "Correo o contraseña incorrectos.", "OK");
            }
        }
        catch (System.ServiceModel.FaultException faultEx)
        {
            System.Diagnostics.Debug.WriteLine($"[LOGIN] FaultException: {faultEx.Message}");
            
            // Mensajes específicos del servidor
            if (faultEx.Message.Contains("Correo o Contrasena incorrecta") || 
                faultEx.Message.Contains("Correo o contraseña incorrectos"))
            {
                lblError.Text = "Correo o contraseña incorrectos";
                lblError.IsVisible = true;
                await DisplayAlert("Error de Autenticación", "Correo o contraseña incorrectos.", "OK");
            }
            else if (faultEx.Message.Contains("bloqueado"))
            {
                lblError.Text = "Usuario bloqueado";
                lblError.IsVisible = true;
                await DisplayAlert("Acceso Denegado", "Tu usuario ha sido bloqueado por múltiples intentos fallidos.", "OK");
            }
            else
            {
                lblError.Text = faultEx.Message;
                lblError.IsVisible = true;
                await DisplayAlert("Error", faultEx.Message, "OK");
            }
        }
        catch (System.ServiceModel.ProtocolException protEx)
        {
            LogExceptionTree(protEx, "[LOGIN] ProtocolException Tree:");
            
            // Intentar leer el cuerpo de la respuesta si es una WebException (común en errores 400/500)
            if (protEx.InnerException is System.Net.WebException webEx && webEx.Response != null)
            {
                try
                {
                    using (var reader = new System.IO.StreamReader(webEx.Response.GetResponseStream()))
                    {
                        string responseBody = reader.ReadToEnd();
                        System.Diagnostics.Debug.WriteLine($"[LOGIN] Cuerpo del error del servidor: {responseBody}");
                    }
                }
                catch (Exception readEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[LOGIN] No se pudo leer el cuerpo del error: {readEx.Message}");
                }
            }
            
            lblError.Text = "Error de protocolo con el servidor";
            lblError.IsVisible = true;
            
            string errorDetalle = protEx.InnerException != null 
                ? protEx.InnerException.Message 
                : protEx.Message;
            
            await DisplayAlert("Error de Protocolo", 
                $"Hay un problema de comunicación con el servidor WCF.\n\nDetalle: {errorDetalle}\n\nVerifica que el servicio WCF esté ejecutándose correctamente.", 
                "OK");
        }
        catch (System.ServiceModel.CommunicationException commEx)
        {
            LogExceptionTree(commEx, "[LOGIN] CommunicationException Tree:");
            
            lblError.Text = "Error de comunicación con el servidor";
            lblError.IsVisible = true;
            await DisplayAlert("Error de Comunicación", 
                $"No se puede conectar al servidor.\n\nVerifica:\n- Que el servidor WCF esté ejecutándose\n- La dirección IP y puerto sean correctos\n- No haya firewall bloqueando la conexión", 
                "OK");
        }
        catch (TimeoutException timeEx)
        {
            LogExceptionTree(timeEx, "[LOGIN] TimeoutException Tree:");
            lblError.Text = "Tiempo de espera agotado";
            lblError.IsVisible = true;
            await DisplayAlert("Timeout", "La operación tomó demasiado tiempo. Verifica tu conexión.", "OK");
        }
        catch (Exception ex)
        {
            LogExceptionTree(ex, "[LOGIN] Exception Tree:");
            
            lblError.Text = ex.Message;
            lblError.IsVisible = true;
            await DisplayAlert("Error", $"Ocurrió un error inesperado: {ex.Message}", "OK");
        }
        finally
        {
            if (Client != null)
            {
                try
                {
                    if (Client.State == System.ServiceModel.CommunicationState.Faulted)
                    {
                        Client.Abort();
                        System.Diagnostics.Debug.WriteLine("[LOGIN] Cliente abortado (estado Faulted)");
                    }
                    else
                    {
                        await Task.Run(() => Client.Close());
                        System.Diagnostics.Debug.WriteLine("[LOGIN] Cliente cerrado correctamente");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[LOGIN] Error al cerrar cliente: {ex.Message}");
                    Client?.Abort();
                }
            }
            System.Diagnostics.Debug.WriteLine("========== FIN DE PROCESO DE LOGIN ==========\n");
        }
    }

    private void LogExceptionTree(Exception ex, string prefix = "")
    {
        System.Diagnostics.Debug.WriteLine($"{prefix} Tipo: {ex.GetType().Name}");
        System.Diagnostics.Debug.WriteLine($"{prefix} Mensaje: {ex.Message}");
        System.Diagnostics.Debug.WriteLine($"{prefix} StackTrace: {ex.StackTrace}");
        
        if (ex.InnerException != null)
        {
            LogExceptionTree(ex.InnerException, prefix + "  Inner: ");
        }
    }
}