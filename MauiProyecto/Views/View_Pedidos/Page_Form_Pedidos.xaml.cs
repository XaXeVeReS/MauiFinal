using System.Text.Json;
using System.Threading.Tasks;
using WCF_Apl_Dis;
using APP_MAUI_Apl_Dis_2025_II.Services;

namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Pedidos;

public partial class Page_Form_Pedidos : ContentPage
{
    Service1Client Client;
    List<Cls_DetalleVenta> lista_Detalle;
    public Page_Form_Pedidos()
    {
        InitializeComponent();
        Client = APP_MAUI_Apl_Dis_2025_II.Services.WCFConfig.CreateWCFClient();
        Cargar_Platos();
        lista_Detalle = new List<Cls_DetalleVenta>();
    }

    private async void Cargar_Platos()
    {
        List<Cls_Platos> listaPlatos = (await Client.Get_PlatosAsync()).ToList();

        picker_Platos.ItemsSource = listaPlatos;
        picker_Platos.ItemDisplayBinding = new Binding("Nombre");
    }
    private async void SelectedIndexChanged_picker_Platos(object sender, EventArgs e)
    {
        if (picker_Platos.SelectedItem is Cls_Platos plato)
        {
            Cls_Platos plt = await Client.Search_PlatoAsync(plato.Id_Plato);
            Precio.Text = plt.Precio.ToString();

            List<Cls_Promociones> promociones = (await Client.Get_PromocionesAsync(false)).ToList();

            var listaPromociones = promociones.Where(p => p.Id_Plato == plt.Id_Plato).ToList();

            listaPromociones.Insert(0, new Cls_Promociones
            {
                Descuento = 0
            });

            picker_Descuentos.ItemsSource = listaPromociones;
            picker_Descuentos.ItemDisplayBinding = new Binding("Descuento");

            picker_Descuentos.SelectedIndex = 0;

            Disponible.Text = (await Client.Check_StockAsync(plato.Id_Plato)).ToString();
        }
    }
    private async void Click_btn_agregar(object sender, EventArgs e)
    {
        Cls_DetalleVenta venta = new Cls_DetalleVenta();
        Cls_Platos? plato = picker_Platos.SelectedItem as Cls_Platos;
        Cls_Promociones? promocion = picker_Descuentos.SelectedItem as Cls_Promociones;

        if (plato == null)
        {
            mostrar_mensage("Se debe seleccionar un plato");
        }
        else
        {
            int cantidad = 0;
            if (!int.TryParse(Cantidad.Text, out cantidad) || cantidad <= 0)
            {
                mostrar_mensage("La cantidad requerida debe ser de al menos 1");
            }
            else
            {
                venta.Id_Plato = plato.Id_Plato;
                venta.txt_Plato = plato.Nombre;
                venta.Precio_Unitario = plato.Precio;
                venta.Cantidad = int.Parse(Cantidad.Text);
                if (promocion != null && promocion.Id_Promocion > 0)
                {
                    venta.Id_Promocion = promocion.Id_Promocion;
                    venta.Descuento = promocion.Descuento;
                }
                else { venta.Descuento = 0; }

                Cargar_Detalle(venta);
            }
        }
    }
    private void mostrar_mensage(string mensage)
    {
        lblError.Text = mensage;
        lblError.Opacity = 1;
    }
    private async void Cargar_Detalle(Cls_DetalleVenta venta)
    {
        var detalle = lista_Detalle.FirstOrDefault(x => x.Id_Plato == venta.Id_Plato);
        List<Cls_Promociones> promo = (await Client.Get_PromocionesAsync(false)).ToList();
        var listaPromo = promo.Where(p => p.Id_Plato == venta.Id_Plato).ToList();
        if (detalle != null)
        {
            detalle.Cantidad = venta.Cantidad;

            foreach (Cls_Promociones p in listaPromo.ToArray())
            {
                if (detalle.Cantidad >= p.Cantidad_Aplicable)
                {
                    detalle.Id_Promocion = p.Id_Promocion;
                    detalle.Descuento = p.Descuento;
                }
                else
                {
                    detalle.Id_Promocion = null;
                    detalle.Descuento = 0;
                }
            }
        }
        else
        {
            foreach (Cls_Promociones p in listaPromo.ToArray())
            {
                if (venta.Cantidad >= p.Cantidad_Aplicable)
                {
                    venta.Id_Promocion = p.Id_Promocion;
                    venta.Descuento = p.Descuento;
                }
                else
                {
                    venta.Id_Promocion = null;
                    venta.Descuento = 0;
                }
            }
            lista_Detalle.Add(venta);
        }

        TablaDetalle.ItemsSource = null;
        TablaDetalle.ItemsSource = lista_Detalle;
        Calcular_Precio_Total();

        lblError.Opacity = 0;
        lblError.Text = "";
    }
    private async void Click_btn_Quitar(object sender, EventArgs e)
    {
        var boton = (ImageButton)sender;
        var detalle = (Cls_DetalleVenta)boton.BindingContext;

        lista_Detalle.RemoveAll(x => x.Id_Plato == detalle.Id_Plato);

        TablaDetalle.ItemsSource = null;
        TablaDetalle.ItemsSource = lista_Detalle;
        Calcular_Precio_Total();
    }
    private async void Click_btn_Realizar_Pago(object sender, EventArgs e)
    {
        // Validar que hay detalles
        if (lista_Detalle == null || lista_Detalle.Count == 0)
        {
            mostrar_mensage("El Detalle de venta debe contener al menos un registro");
            return;
        }

        // Validar que hay monto
        if (!float.TryParse(Monto.Text, out float montoTotal) || montoTotal <= 0)
        {
            mostrar_mensage("El monto total debe ser mayor a cero");
            return;
        }

        try
        {
            // Verificar disponibilidad
            if (!PasarelaPagoService.VerificarDisponibilidad())
            {
                mostrar_mensage("No se encuentra la aplicación de pasarela de pago");
                return;
            }

            // Procesar pago
            var (exitoso, mensaje) = await PasarelaPagoService.ProcesarPago((decimal)montoTotal);

            if (exitoso)
            {
                await Insertar_Pedido();
            }
            else
            {
                bool reintentar = await DisplayAlert(
                    "Pago no procesado",
                    $"{mensaje}\n\n¿Desea intentar nuevamente?",
                    "Reintentar",
                    "Cancelar"
                );

                if (!reintentar)
                {
                    mostrar_mensage("Pago cancelado");
                }
            }
        }
        catch (Exception ex)
        {
            mostrar_mensage($"Error al procesar pago: {ex.Message}");
        }
    }
    private async Task Insertar_Pedido()
    {
        if (lista_Detalle != null && lista_Detalle.Count > 0)
        {
            try
            {
                Cls_Ventas venta = new Cls_Ventas();
                venta.Id_Trabajador = Global.Id_Usuario;
                venta.DetalleVenta = lista_Detalle.ToList();
                venta.Metodo_Pago = "Pasarela";

                await Client.Insert_VentaAsync(venta);

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
                mostrar_mensage($"Error al registrar pedido: {ex.Message}");
                await DisplayAlert(
                    "Error Crítico",
                    $"El pago fue procesado pero hubo un error al registrar el pedido:\n{ex.Message}\n\nContacte al administrador.",
                    "OK"
                );
            }
        }
    }
    private async Task Calcular_Precio_Total()
    {
        float MontoTotal = 0;
        int tiempo_p = 0;
        foreach (Cls_DetalleVenta d in lista_Detalle)
        {
            float descuento = d.Descuento ?? 0f;
            MontoTotal += d.Precio_Unitario * d.Cantidad * (1 - descuento);

            Cls_Platos p = await Client.Search_PlatoAsync(d.Id_Plato);
            tiempo_p += p.Tiempo_Preparacion;
        }

        Monto.Text = MontoTotal.ToString();
        Tiempo.Text = tiempo_p.ToString();
    }
    private async void Click_btn_Cancelar_Pedido(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Page_Pedidos");
    }

    private async void SelectedIndexChanged_picker_Promociones(object sender, EventArgs e)
    {
        if (picker_Descuentos.SelectedItem is Cls_Promociones promo)
        {
            if (promo.Cantidad_Aplicable > 0)
            {
                int cantidadActual = 0;

                if (!string.IsNullOrWhiteSpace(Cantidad.Text))
                {
                    int.TryParse(Cantidad.Text.Trim(), out cantidadActual);
                }

                if (cantidadActual < promo.Cantidad_Aplicable)
                {
                    Cantidad.Text = promo.Cantidad_Aplicable.ToString();
                }
            }
        }
    }
}