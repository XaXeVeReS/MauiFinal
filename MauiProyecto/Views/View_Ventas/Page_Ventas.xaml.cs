using WCF_Apl_Dis;
namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Ventas;

public partial class Page_Ventas : ContentPage
{
    private readonly Service1Client Client = APP_MAUI_Apl_Dis_2025_II.Services.WCFConfig.CreateWCFClient();
    public List<Cls_Ventas> ListaPedidos { get; set; } = new();
    public Cls_Ventas Pedido { get; set; } = new();

    public Page_Ventas()
	{
		InitializeComponent();
        BindingContext = this;

        _ = Cargar_Pedidos();
    }
    private async Task Cargar_Pedidos()
    {
        string estado = picker_Estado.SelectedItem?.ToString() ?? "Pendiente";
        DateTime fecha = InputFecha.SelectedDate;
        try
        {
            var pedidos = await Client.Get_VentasAsync(fecha, estado);
            ListaPedidos.Clear();
            ListaPedidos.AddRange(pedidos);

            TablaPedidos.ItemsSource = null;
            TablaPedidos.ItemsSource = ListaPedidos;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void picker_Estado_SelectedIndexChanged(object sender, EventArgs e)
    {
        await Cargar_Pedidos();
    }

    private async void InputFecha_SelectedDateChanged(object sender, DateTime fecha)
    {
        await Cargar_Pedidos();
    }

    private async void ModtrarDetalle_Clicked(object sender, EventArgs e)
    {
        var boton = (ImageButton)sender;
        var venta = (Cls_Ventas)boton.BindingContext;

        await Shell.Current.GoToAsync($"Page_DetalleVenta?Id_Pedido={venta.Id_Venta}");
    }
}

