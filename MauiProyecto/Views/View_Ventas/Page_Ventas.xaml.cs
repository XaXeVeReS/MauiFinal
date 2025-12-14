using System.Collections.ObjectModel;
using WCF_Apl_Dis;
namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Ventas;

public partial class Page_Ventas : ContentPage
{
    private readonly Service1Client Client = new();

    PeriodicTimer _timer;
    CancellationTokenSource _cts;
    Cls_Ventas _itemVisible;

    public ObservableCollection<Cls_Ventas> ListaPedidos { get; } = new();
    public Cls_Ventas Pedido { get; set; } = new();

    public Page_Ventas()
	{
		InitializeComponent();
        BindingContext = this;

        _ = Cargar_Pedidos();
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
    private async Task Cargar_Pedidos()
    {
        string estado = picker_Estado.SelectedItem?.ToString() ?? "Pendiente";
        DateTime fecha = InputFecha.SelectedDate;
        try
        {
            var pedidos = await Client.Get_VentasAsync(fecha, estado);

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

