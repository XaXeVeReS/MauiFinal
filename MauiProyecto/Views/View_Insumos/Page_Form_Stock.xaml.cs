using System.Collections.ObjectModel;
using WCF_Apl_Dis;
namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Insumos;

public partial class Page_Form_Stock : ContentPage
{
    Service1Client Client;

    public List<Cls_Insumos> ListaInsumos = new();

    public ObservableCollection<Cls_Insumos> ItemsFiltrados = new();

    public Page_Form_Stock()
    {
        InitializeComponent();

        Client = APP_MAUI_Apl_Dis_2025_II.Services.WCFConfig.CreateWCFClient();

        ListaFiltrada.ItemsSource = ItemsFiltrados;

        CargarInsumos();
    }

    private async void CargarInsumos()
    {
        var lista = await Client.Get_InsumosAsync();
        ListaInsumos = lista.ToList();
    }

    // CUANDO EL USUARIO ESCRIBE
    private void EntryBusqueda_TextChanged(object sender, TextChangedEventArgs e)
    {
        string texto = e.NewTextValue?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(texto))
        {
            ListaFiltrada.IsVisible = false;
            ItemsFiltrados.Clear();
            return;
        }

        var resultados = ListaInsumos
            .Where(x => x.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase))
            .ToList();

        ItemsFiltrados.Clear();

        foreach (var item in resultados)
            ItemsFiltrados.Add(item);

        ListaFiltrada.IsVisible = ItemsFiltrados.Any();
    }

    // CUANDO SE SELECCIONA UN INSUMO
    private void ListaFiltrada_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0) return;

        var seleccionado = (Cls_Insumos)e.CurrentSelection[0];

        // Mostrar en el Entry
        EntryBusqueda.Text = seleccionado.Nombre;
        lblUnidMedida.Text = seleccionado.Unidad_Medida;

        // Ocultar desplegable
        ListaFiltrada.IsVisible = false;

        // Limpiar selecci�n
        ((CollectionView)sender).SelectedItem = null;
    }

    // CUANDO EL USUARIO PRESIONA ENTER (opcional)
    private void EntryBusqueda_Completed(object sender, EventArgs e)
    {
        ListaFiltrada.IsVisible = false;
    }

    private void btnAgregarInsumo_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EntryBusqueda.Text))
        { mensage_alerta("Seleccione un insumo"); return; }

        if (string.IsNullOrWhiteSpace(InputStock.Text) || !decimal.TryParse(InputStock.Text, out decimal stock))
        { mensage_alerta("Ingrese un stock valido"); return; }


        // Buscar el insumo seleccionado
        var insumo = ListaInsumos.FirstOrDefault(x => x.Nombre.Equals(EntryBusqueda.Text, StringComparison.OrdinalIgnoreCase));

        // Verificar si ya est� agregado
        var listaActual = (ObservableCollection<Cls_Insumos>)TablaInsumos.ItemsSource ?? new ObservableCollection<Cls_Insumos>();

        if (listaActual.Any(x => x.Id_Insumo == insumo.Id_Insumo))
        { mensage_alerta("el insumo ya existe en la lista"); return; }


        // Crear una copia del modelo para no alterar la lista original
        var nuevo = new Cls_Insumos
        {
            Id_Insumo = insumo.Id_Insumo,
            Nombre = insumo.Nombre,
            Unidad_Medida = insumo.Unidad_Medida,
            Stock_Disponible = (float)stock
        };

        listaActual.Add(nuevo);

        TablaInsumos.ItemsSource = listaActual;

        // Limpiar campos
        EntryBusqueda.Text = "";
        InputStock.Text = "";
        ItemsFiltrados.Clear();
        ListaFiltrada.IsVisible = false;

        lblError.IsVisible = false;
    }
    private async void btn_ActualizarStock_Clicked(object sender, EventArgs e)
    {
        if (TablaInsumos.ItemsSource is null)
        { mensage_alerta("La lista debe contener al menos un registro"); return; }

        var listaTabla = (ObservableCollection<Cls_Insumos>)TablaInsumos.ItemsSource;

        List<Cls_Insumos> listaActualizar = new();

        foreach (var item in listaTabla)
        {
            listaActualizar.Add(new Cls_Insumos
            {
                Id_Insumo = item.Id_Insumo,
                Stock_Disponible = item.Stock_Disponible
            });
        }

        // Aquí llamas a tu servicio WCF
        try
        {
            await Client.Stock_InsumosAsync(listaActualizar, Global.Id_Usuario);

            lblError.Text = "Srock actualizado exitosamente";
            lblError.TextColor = Colors.Green;
            lblError.IsVisible = true;
        }
        catch (Exception ex)
        {
            mensage_alerta(ex.Message);
        }
    }

    public void mensage_alerta(string mensage)
    {
        lblError.Text = mensage;
        lblError.TextColor =Colors.Red;
        lblError.IsVisible =true;
    }

    private void EntryStock_TextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = (Entry)sender;

        string newText = e.NewTextValue;
        string oldText = e.OldTextValue;

        if (string.IsNullOrEmpty(newText))
            return;

        if (newText == "." || newText.EndsWith("."))
            return;

        if (newText.EndsWith(".") && decimal.TryParse(newText.TrimEnd('.'), out _))
            return;

        if (!decimal.TryParse(newText, out decimal value))
        {
            entry.Text = oldText; 
            return;
        }

        decimal min = 0m;
        decimal max = 9999.999m;

        if (value > max)
        {
            entry.Text = oldText;  
            return;
        }

        value = Math.Round(value, 3);

        string corrected = value.ToString("0.###");

        if (newText != corrected)
            entry.Text = corrected;

        if (entry.BindingContext is Cls_Insumos insumo)
            insumo.Stock_Disponible = (float)value;
    }
    private async void btn_quitar_clicked(object sender, EventArgs e)
    {
        var boton = (ImageButton)sender;
        var detalle = (Cls_Insumos)boton.BindingContext;

        var listaActual = (ObservableCollection<Cls_Insumos>)TablaInsumos.ItemsSource;

        if (listaActual == null) return;

        var item = listaActual.FirstOrDefault(x => x.Id_Insumo == detalle.Id_Insumo);
        if (item != null)
            listaActual.Remove(item);
    }

}


