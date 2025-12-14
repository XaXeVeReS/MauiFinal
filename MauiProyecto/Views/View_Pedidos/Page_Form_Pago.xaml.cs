using System.Text.Json;
using WCF_Apl_Dis;
using APP_MAUI_Apl_Dis_2025_II.Services;

namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Pedidos;

public partial class Page_Form_Pago : ContentPage, IQueryAttributable
{
    Service1Client Client;
    public List<Cls_DetalleVenta> Lista_Detalle;
    private bool _pagoAprobado = false;

    public Page_Form_Pago()
	{
		InitializeComponent();
        Client = APP_MAUI_Apl_Dis_2025_II.Services.WCFConfig.CreateWCFClient();
        Lista_Detalle = new List<Cls_DetalleVenta>();
	}

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("Lista_Detalle"))
        {
            if (query.TryGetValue("Lista_Detalle", out var value))
            {
                string json = Uri.UnescapeDataString(value.ToString());
                Lista_Detalle = JsonSerializer.Deserialize<List<Cls_DetalleVenta>>(json);
                Calcular_Precio_Total();
            }
        }
    }
    private async void btn_Realizar_Pago(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[PAGO] ========== INICIO PROCESO DE PAGO ==========");
        
        // Validar que hay monto
        if (!float.TryParse(Monto.Text, out float montoTotal) || montoTotal <= 0)
        {
            lblError.Text = "El monto total debe ser mayor a cero";
            lblError.TextColor = Colors.Red;
            lblError.IsVisible = true;
            System.Diagnostics.Debug.WriteLine("[PAGO] ERROR: Monto inválido");
            return;
        }

        // Validar que hay detalles
        if (Lista_Detalle == null || Lista_Detalle.Count == 0)
        {
            lblError.Text = "El Detalle de venta debe contener al menos un registro";
            lblError.TextColor = Colors.Red;
            lblError.IsVisible = true;
            System.Diagnostics.Debug.WriteLine("[PAGO] ERROR: Sin detalles de venta");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[PAGO] Monto total a pagar: {montoTotal:C}");
        System.Diagnostics.Debug.WriteLine($"[PAGO] Cantidad de items: {Lista_Detalle.Count}");

        try
        {
            // Deshabilitar botón mientras se procesa
            btn_realizarPago.IsEnabled = false;
            lblError.Text = "Abriendo pasarela de pago...";
            lblError.TextColor = Colors.Blue;
            lblError.IsVisible = true;

            System.Diagnostics.Debug.WriteLine("[PAGO] Verificando disponibilidad de pasarela...");
            
            // Verificar que la pasarela esté disponible
            if (!PasarelaPagoService.VerificarDisponibilidad())
            {
                lblError.Text = "No se encuentra la aplicación de pasarela de pago";
                lblError.TextColor = Colors.Red;
                lblError.IsVisible = true;
                btn_realizarPago.IsEnabled = true;
                System.Diagnostics.Debug.WriteLine("[PAGO] ERROR: Pasarela no disponible");
                return;
            }

            System.Diagnostics.Debug.WriteLine("[PAGO] Llamando a la pasarela de pago...");
            
            // Procesar pago a través de la pasarela
            var (exitoso, mensaje) = await PasarelaPagoService.ProcesarPago((decimal)montoTotal);
            
            System.Diagnostics.Debug.WriteLine($"[PAGO] Resultado de pasarela - Exitoso: {exitoso}, Mensaje: {mensaje}");

            if (exitoso)
            {
                System.Diagnostics.Debug.WriteLine("[PAGO] ✓ Pago APROBADO - Procediendo a insertar pedido");
                _pagoAprobado = true;
                
                // Pago aprobado, proceder a insertar el pedido
                lblError.Text = "Pago aprobado. Registrando pedido...";
                lblError.TextColor = Colors.Green;
                lblError.IsVisible = true;

                await Insertar_Pedido();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[PAGO] ✗ Pago RECHAZADO - {mensaje}");
                
                // Pago rechazado o cancelado
                bool reintentar = await DisplayAlert(
                    "Pago no procesado",
                    $"{mensaje}\n\n¿Desea intentar nuevamente?",
                    "Reintentar",
                    "Cancelar"
                );

                if (reintentar)
                {
                    System.Diagnostics.Debug.WriteLine("[PAGO] Usuario decide reintentar");
                    lblError.Text = "Intente nuevamente el pago";
                    lblError.TextColor = Colors.Orange;
                    lblError.IsVisible = true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[PAGO] Usuario cancela operación");
                    lblError.Text = "Pago cancelado";
                    lblError.TextColor = Colors.Red;
                    lblError.IsVisible = true;
                    
                    // Volver atrás
                    await Shell.Current.GoToAsync("..");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PAGO] ERROR INESPERADO: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[PAGO] StackTrace: {ex.StackTrace}");
            
            lblError.Text = $"Error al procesar pago: {ex.Message}";
            lblError.TextColor = Colors.Red;
            lblError.IsVisible = true;

            await DisplayAlert("Error", $"Ocurrió un error al procesar el pago:\n{ex.Message}", "OK");
        }
        finally
        {
            btn_realizarPago.IsEnabled = true;
            System.Diagnostics.Debug.WriteLine("[PAGO] ========== FIN PROCESO DE PAGO ==========\n");
        }
    }
    private void Calcular_Precio_Total()
    {
        float MontoTotal = 0;
        foreach (Cls_DetalleVenta d in Lista_Detalle)
        {
            float descuento = d.Descuento ?? 0f;
            MontoTotal += d.Cantidad * (d.Precio_Unitario * (1 - descuento));
        }

        Monto.Text = MontoTotal.ToString();
    }

    private async Task Insertar_Pedido()
    {
        if (Lista_Detalle != null && Lista_Detalle.Count > 0)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[PEDIDO] Insertando venta en base de datos...");
                
                Cls_Ventas venta = new Cls_Ventas();
                venta.Id_Trabajador = Global.Id_Usuario;
                venta.DetalleVenta = Lista_Detalle.ToList();

                // Establecer método de pago si está seleccionado
                if (picker_Tipo.SelectedIndex >= 0)
                {
                    venta.Metodo_Pago = picker_Tipo.SelectedItem?.ToString();
                }

                int id_venta = await Client.Insert_Venta_Return_IdAsync(venta);

                System.Diagnostics.Debug.WriteLine("[PEDIDO] ✓ Venta insertada exitosamente");

                lblError.Text = "✅ Pedido registrado exitosamente";
                lblError.TextColor = Colors.Green;
                lblError.IsVisible = true;

                // Mostrar mensaje de éxito
                await DisplayAlert(
                    "Pedido Confirmado",
                    "El pedido ha sido registrado exitosamente.\nEl pago fue procesado correctamente.",
                    "OK"
                );

                // Volver a la página de pedidos
                await Shell.Current.GoToAsync("//pedidos_page");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PEDIDO] ERROR al insertar venta: {ex.Message}");
                
                lblError.Text = $"Error al registrar pedido: {ex.Message}";
                lblError.TextColor = Colors.Red;
                lblError.IsVisible = true;

                // Si falla la inserción del pedido después de un pago exitoso, es crítico
                await DisplayAlert(
                    "Error Crítico",
                    $"El pago fue procesado pero hubo un error al registrar el pedido:\n{ex.Message}\n\nContacte al administrador.",
                    "OK"
                );
            }
        } 
        else 
        {
            System.Diagnostics.Debug.WriteLine("[PEDIDO] ERROR: Sin detalles de venta");
            lblError.Text = "El Detalle de venta debe contener al menos un registro";
            lblError.TextColor = Colors.Red;
            lblError.IsVisible = true; 
        }
    }
}