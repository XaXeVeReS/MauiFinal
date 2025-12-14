using System.Collections.ObjectModel;
using System.Globalization;
using WCF_Apl_Dis;

namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Pedidos;
public partial class Page_Pedidos : ContentPage
{
    private readonly Service1Client Client = new();

    PeriodicTimer _timer;
    CancellationTokenSource _cts;
    Cls_Ventas _itemVisible;

    public ObservableCollection<Cls_Ventas> ListaPedidos { get; } = new();
    public Cls_Ventas Pedido { get; set; } = new();
    public Page_Pedidos()
    {
        InitializeComponent();
        BindingContext = this;
        Cargar_Pedidos();
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        _cts?.Cancel();

        _cts = new CancellationTokenSource();
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        try
        {
            while (await _timer.WaitForNextTickAsync(_cts.Token))
            {
                await Cargar_Pedidos();
            }
        }
        catch (OperationCanceledException)
        {
            // Cancelación normal al cerrar la app/emulador
        }
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
    private async Task Cargar_Pedidos()
    {
        try
        {
            var pedidos = await Client.Get_VentasAsync(DateTime.Now, null);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                bool hayCambios = false;

                // Guardar referencia e índice visible
                var visibleItem = _itemVisible;
                int visibleIndex = visibleItem != null
                    ? ListaPedidos.IndexOf(visibleItem)
                    : -1;

                // -----------------------------
                // Detectar eliminados
                // -----------------------------
                for (int i = ListaPedidos.Count - 1; i >= 0; i--)
                {
                    if (!pedidos.Any(p => p.Id_Venta == ListaPedidos[i].Id_Venta))
                    {
                        hayCambios = true;
                        break;
                    }
                }

                // -----------------------------
                // Detectar agregados o modificados
                // -----------------------------
                if (!hayCambios)
                {
                    foreach (var pedido in pedidos)
                    {
                        var existente = ListaPedidos.FirstOrDefault(x => x.Id_Venta == pedido.Id_Venta);

                        if (existente == null || !SonIguales(existente, pedido))
                        {
                            hayCambios = true;
                            break;
                        }
                    }
                }

                // Si no hay cambios  salir
                if (!hayCambios)
                    return;

                // ============================
                // ACTUALIZAR COLECCIÓN
                // ============================

                // Eliminar
                for (int i = ListaPedidos.Count - 1; i >= 0; i--)
                {
                    if (!pedidos.Any(p => p.Id_Venta == ListaPedidos[i].Id_Venta))
                        ListaPedidos.RemoveAt(i);
                }

                // Agregar / actualizar SIN reemplazar objetos
                foreach (var pedido in pedidos)
                {
                    var existente = ListaPedidos.FirstOrDefault(x => x.Id_Venta == pedido.Id_Venta);

                    if (existente == null)
                        ListaPedidos.Add(pedido);
                    else
                        ActualizarPedido(existente, pedido);
                }

                // ============================
                // RESTAURAR SCROLL
                // ============================

                if (ListaPedidos.Count == 0)
                    return;

                Cls_Ventas targetItem = null;

                //  Si el item visible aún existe  usarlo
                if (visibleItem != null)
                {
                    targetItem = ListaPedidos.FirstOrDefault(x => x.Id_Venta == visibleItem.Id_Venta);
                }

                // Si ya no existe usar el anterior
                if (targetItem == null && visibleIndex > 0)
                {
                    int newIndex = Math.Min(visibleIndex - 1, ListaPedidos.Count - 1);
                    targetItem = ListaPedidos[newIndex];
                }

                // Último recurso mismo índice
                if (targetItem == null && visibleIndex >= 0 && visibleIndex < ListaPedidos.Count)
                {
                    targetItem = ListaPedidos[visibleIndex];
                }

                // Scroll SOLO si hay un item válido
                if (targetItem != null)
                {
                    TablaPedidos.ScrollTo(
                        item: targetItem,
                        position: ScrollToPosition.Start,
                        animate: false);
                }
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
    private void TablaPedidos_Scrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        if (ListaPedidos.Count == 0)
            return;

        int firstVisibleIndex = e.FirstVisibleItemIndex;

        if (firstVisibleIndex >= 0 && firstVisibleIndex < ListaPedidos.Count)
            _itemVisible = ListaPedidos[firstVisibleIndex];
    }
    private bool SonIguales(Cls_Ventas a, Cls_Ventas b)
    {
        return a.Id_Venta == b.Id_Venta
            && a.Estado == b.Estado;
    }
    private void ActualizarPedido(Cls_Ventas destino, Cls_Ventas origen)
    {
        if (destino.Estado == origen.Estado)
            return;

        destino.Estado = origen.Estado;

        int index = ListaPedidos.IndexOf(destino);
        if (index >= 0)
        {
            // Fuerza refresco SIN cambiar objeto
            ListaPedidos[index] = ListaPedidos[index];
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