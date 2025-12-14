using WCF_Apl_Dis;
namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Ventas;

public partial class Page_DetalleVenta : ContentPage, IQueryAttributable
{
    Service1Client Client;
    public int Id_Pedido { get; set; }

    private Cls_Ventas ClsVenta { get; set; }

    List<Cls_DetalleVenta> lista_Detalle;

    public Page_DetalleVenta()
	{
		InitializeComponent();
        Client = APP_MAUI_Apl_Dis_2025_II.Services.WCFConfig.CreateWCFClient();
        lista_Detalle = new List<Cls_DetalleVenta>();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("Id_Pedido"))
        {
            Id_Pedido = int.Parse(s: query["Id_Pedido"].ToString());
            Cargar_Pedido();
        }
    }

    private async void Cargar_Pedido()
    {
        Cls_Ventas venta = await Client.Search_VentaAsync(Id_Pedido);
        ClsVenta = venta;

        Cliente.Text = venta.txt_Cliente;
        InputFecha.SelectedDate = venta.Fecha_Pedido;
        Estado.Text = venta.Estado;
        MontoTotal.Text = Math.Round(venta.Costo_Total, 3).ToString("0.###"); 
        Pago.Text = Math.Round(venta.Monto_Total, 3).ToString("0.###");

        lista_Detalle.Clear();
        foreach(Cls_DetalleVenta d in venta.DetalleVenta.ToList())
        {
            lista_Detalle.Add(d);
        }
        TablaDetalle.ItemsSource = null;
        TablaDetalle.ItemsSource = lista_Detalle;

        Calcular_Precio_Total();

        if(venta.Estado == "Pendiente")
        {
            btn_Cancelar.IsVisible = true;
        }
        else
        {
            btn_Cancelar.IsVisible = false;
        }
    }

    private void Calcular_Precio_Total()
    {
        float MontoTotal = 0;
        foreach (Cls_DetalleVenta d in lista_Detalle)
        {
            float descuento = d.Descuento ?? 0f;
            MontoTotal += d.Precio_Unitario * d.Cantidad * (1 - descuento);
        }

        Monto.Text = MontoTotal.ToString();
    }

    private async void Cancelar_Venta(object sender, EventArgs e)
    {
        try
        {
            ClsVenta.Estado = "Cancelado";
            await Client.State_VentaAsync(ClsVenta);
            lblError.Text = "Venta cancelada exitosamente";
            lblError.TextColor = Colors.Blue;
            Cargar_Pedido();
        }
        catch (Exception ex)
        {
            lblError.Text = ex.Message;
            lblError.TextColor= Colors.Red;
        }
        finally
        {
            lblError.IsVisible = true;
        }
    }
}