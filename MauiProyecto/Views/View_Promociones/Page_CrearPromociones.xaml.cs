using WCF_Apl_Dis;

namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Promociones;

[QueryProperty(nameof(Id_Promocion), "Id_Promocion")]
public partial class Page_CrearPromociones : ContentPage
{
    Service1Client servicio;
    byte[] VersionPromociones;
    public int Id_Promocion { get; set; }   // ID recibido para EDITAR

    public Page_CrearPromociones()
    {
        InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);

        servicio = APP_MAUI_Apl_Dis_2025_II.Services.WCFConfig.CreateWCFClient();
        CargarPlatos();
    }

   
    private async void CargarPlatos()
    {
        try
        {
            var platos = await servicio.Get_PlatosAsync();
            pickerPlatos.ItemsSource = platos;
            pickerPlatos.ItemDisplayBinding = new Binding("Nombre");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
    private bool ValidarCampos()
    {
        if (pickerPlatos.SelectedItem == null)
        {
            DisplayAlert("Error", "Seleccione un plato", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            DisplayAlert("Error", "Ingrese un nombre de promoción", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtCantidad.Text))
        {
            DisplayAlert("Error", "Ingrese la cantidad aplicable", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtDescuento.Text))
        {
            DisplayAlert("Error", "Ingrese el descuento", "OK");
            return false;
        }

        return true;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (Id_Promocion != 0)
        {
            btnAgregar.IsVisible = false;
            btnEditar.IsVisible = true;

            CargarDatosParaEditar();
        }
        else
        {
            btnAgregar.IsVisible = true;
            btnEditar.IsVisible = false;
        }
    }


    private async void CargarDatosParaEditar()
    {
        try
        {
            var promo = await servicio.Search_PromocionAsync(Id_Promocion);

            // Plato
            var listaPlatos = await servicio.Get_PlatosAsync();
            pickerPlatos.ItemsSource = listaPlatos;
            pickerPlatos.ItemDisplayBinding = new Binding("Nombre");

            var platoSeleccionado = listaPlatos.FirstOrDefault(p => p.Id_Plato == promo.Id_Plato);
            pickerPlatos.SelectedItem = platoSeleccionado;

            // Nombre
            txtNombre.Text = promo.Nombre;
            txtCantidad.Text = promo.Cantidad_Aplicable.ToString();
            txtDescuento.Text = promo.Descuento.ToString();

            // Fechas
            pickerInicioFecha.Date = promo.Fecha_Inicio.Date;
            pickerInicioHora.Time = promo.Fecha_Inicio.TimeOfDay;

            pickerFinFecha.Date = promo.Fecha_Fin.Date;
            pickerFinHora.Time = promo.Fecha_Fin.TimeOfDay;

            VersionPromociones = promo.Version;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }


    private async void btnAgregar_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (!ValidarCampos()) return;

            var plato = (Cls_Platos)pickerPlatos.SelectedItem;

            var fechaInicio = pickerInicioFecha.Date.Add(pickerInicioHora.Time);
            var fechaFin = pickerFinFecha.Date.Add(pickerFinHora.Time);

            var nuevaPromo = new Cls_Promociones
            {
                Id_Plato = plato.Id_Plato,
                Nombre = txtNombre.Text,
                Cantidad_Aplicable = int.Parse(txtCantidad.Text),
                Descuento = float.Parse(txtDescuento.Text),
                Fecha_Inicio = fechaInicio,
                Fecha_Fin = fechaFin,
                Activo = true
            };

            await servicio.Insert_PromocionAsync(nuevaPromo);

            await DisplayAlert("¡Éxito!", "Promoción creada correctamente", "OK");
            await Shell.Current.GoToAsync("Page_Promociones");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void btnEditar_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (!ValidarCampos()) return;

            var plato = (Cls_Platos)pickerPlatos.SelectedItem;

            var fechaInicio = pickerInicioFecha.Date.Add(pickerInicioHora.Time);
            var fechaFin = pickerFinFecha.Date.Add(pickerFinHora.Time);

            var promoEditada = new Cls_Promociones
            {
                Id_Promocion = Id_Promocion,
                Id_Plato = plato.Id_Plato,
                Nombre = txtNombre.Text,
                Cantidad_Aplicable = int.Parse(txtCantidad.Text),
                Descuento = float.Parse(txtDescuento.Text),
                Fecha_Inicio = fechaInicio,
                Fecha_Fin = fechaFin,
                Activo = true,
                Version = VersionPromociones
            };

            await servicio.Update_PromocionAsync(promoEditada);

            await DisplayAlert("OK", "Promoción actualizada correctamente", "Cerrar");
            await Shell.Current.GoToAsync("Page_Promociones");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void btnCancelar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
