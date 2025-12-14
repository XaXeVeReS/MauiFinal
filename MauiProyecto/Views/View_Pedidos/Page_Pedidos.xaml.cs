using System.Globalization;
using WCF_Apl_Dis;

namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Pedidos;

public partial class Page_Pedidos : ContentPage
{
    private readonly Service1Client Client = APP_MAUI_Apl_Dis_2025_II.Services.WCFConfig.CreateWCFClient();
    public List<Cls_Ventas> ListaPedidos { get; set; } = new();

    public Cls_Ventas Pedido { get; set; } = new() ;
    public Page_Pedidos()
    {
        InitializeComponent();
        BindingContext = this;

        _ = Cargar_Pedidos();
    }
    private async Task Cargar_Pedidos()
    {

        try
        {
            var pedidos = await Client.Get_VentasAsync(DateTime.Now, null);
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
    private async void TablaPedidos_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Cls_Ventas item)
        {
            Mostrar_detalle_pedido(item.Id_Venta);
            lblMensage.Text = "";
            lblMensage.IsVisible = false;
        }
    }
    private async void AgregarPedido_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Page_Form_Pedidos");
    }
    private async void Actualizar_Estado(object sender, EventArgs e)
    {
        try
        {
            await Client.State_VentaAsync(Pedido);
            _ = Cargar_Pedidos();
            lblMensage.Text = "Estado Actualizado Existosamente";
            lblMensage.TextColor = Colors.Blue;
            Mostrar_detalle_pedido(Pedido.Id_Venta);
        }
        catch (Exception ex)
        {
            lblMensage.Text = ex.Message;
            lblMensage.TextColor = Colors.Red;
        }
        finally { lblMensage.IsVisible = true; }
    }
    private async void Mostrar_detalle_pedido(int id_venta)
    {

        Cls_Ventas venta = await Client.Search_VentaAsync(id_venta);
        Pedido = venta;

        lblCliente.Text = $"Cliente: {venta.txt_Cliente}";
        lblHora.Text = $"Fecha: {venta.Fecha_Pedido.ToString("dd MMMM HH:mm")}";
        lblEstado.Text = $"Estado: {venta.Estado}";

        string detalle = "Pedido:\r\nCantidad\tPlato\r\n";

        foreach (Cls_DetalleVenta d in venta.DetalleVenta.ToList())
        {
            detalle += $"{d.Cantidad}\t\t{d.txt_Plato}\r\n";
        }
        lblPedido.Text = detalle.TrimEnd('\r', '\n');

        switch (venta.Estado)
        {
            case "Pendiente":
                btn_Estado.Text = "Procesando";
                btn_Estado.BackgroundColor = Color.FromArgb("#FFD633");
                Pedido.Estado = "Procesando";
                btn_Estado.IsVisible = true;
                break;
            case "Procesando":
                btn_Estado.Text = "Entregado";
                btn_Estado.BackgroundColor = Colors.Green;
                Pedido.Estado = "Entregado";
                btn_Estado.IsVisible = true;
                break;
            case "Entregado":
                btn_Estado.IsVisible = false;
                break;
        }
    }
}

public class EstadoColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return Colors.Gray;

        return value.ToString() switch
        {
            "Entregado" => Color.FromArgb("#7ED957"),
            "Procesando" => Color.FromArgb("#F4C542"),
            "Pendiente" => Color.FromArgb("#CCCCCC"),
            "Cancelado" => Colors.Red,
            _ => Colors.Gray
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}