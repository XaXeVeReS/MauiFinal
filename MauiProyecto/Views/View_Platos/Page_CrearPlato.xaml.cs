using WCF_Apl_Dis;

namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Platos;

[QueryProperty(nameof(Id_Plato), "Id_Plato")]
public partial class Page_CrearPlato : ContentPage
{
    byte[] VersionPlato;
    Service1Client servicio;
    List<RecetaItem> receta = new List<RecetaItem>();

    public int Id_Plato { get; set; }

    byte[] imagenBytes = null;        
    string nombreImagen = "";        
    string direc_Imagen = null;       

    public Page_CrearPlato()
    {
        InitializeComponent();
        servicio = APP_MAUI_Apl_Dis_2025_II.Services.WCFConfig.CreateWCFClient();
        CargarInsumos();
        Shell.SetNavBarIsVisible(this, false);
    }

    // --------------------------- MODELO PARA LISTA DE RECETA ---------------------------
    public class RecetaItem
    {
        public int Id_Insumo { get; set; }
        public string Insumo { get; set; }
        public string Cantidad { get; set; }
    }

    // --------------------------- CARGAR INSUMOS ---------------------------
    private async void CargarInsumos()
    {
        var lista = await servicio.Get_InsumosAsync();
        pickerInsumos.ItemsSource = lista;
        pickerInsumos.ItemDisplayBinding = new Binding("Nombre");
    }

    // --------------------------- AGREGAR IMAGEN ---------------------------
    private async void btnAgregarImagen_Clicked(object sender, EventArgs e)
    {
        var pick = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Selecciona una imagen",
            FileTypes = FilePickerFileType.Images
        });

        if (pick == null)
            return;

        nombreImagen = pick.FileName;  // nombre de archivo
        direc_Imagen = pick.FileName;  // para UPDATE

        using var stream = await pick.OpenReadAsync();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);

        imagenBytes = ms.ToArray();

        imgPreviewBtn.Source = ImageSource.FromStream(() => new MemoryStream(imagenBytes));
        imgPreviewBtn.IsVisible = true;

        btnAgregarImagen.IsVisible = false; // OCULTAR EL +
    }

    // --------------------------- AGREGAR INSUMO ---------------------------
    private void btnAgregarInsumo_Clicked(object sender, EventArgs e)
    {
        if (pickerInsumos.SelectedItem == null)
        {
            DisplayAlert("Error", "Seleccione un insumo", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(txtCantidad.Text))
        {
            DisplayAlert("Error", "Ingrese la cantidad", "OK");
            return;
        }

        var insumo = (Cls_Insumos)pickerInsumos.SelectedItem;

        receta.Add(new RecetaItem
        {
            Id_Insumo = insumo.Id_Insumo,
            Insumo = insumo.Nombre,
            Cantidad = txtCantidad.Text
        });

        cvReceta.ItemsSource = null;
        cvReceta.ItemsSource = receta;
    }

    // --------------------------- CREAR PLATO ---------------------------
    private async void btnCrear_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (imagenBytes == null)
            {
                await DisplayAlert("Error", "Seleccione una imagen.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtDescripcion.Text) ||
                string.IsNullOrWhiteSpace(txtPrecio.Text) ||
                string.IsNullOrWhiteSpace(txtTiempo.Text))
            {
                await DisplayAlert("Error", "Complete todos los campos.", "OK");
                return;
            }

            var nuevoPlato = new Cls_Platos
            {
                Nombre = txtNombre.Text,
                Descripcion = txtDescripcion.Text,
                Precio = float.Parse(txtPrecio.Text),
                Tiempo_Preparacion = int.Parse(txtTiempo.Text),
                Activo = true,

                Imagen = imagenBytes,
                txt_Nombre_Imagen = nombreImagen,
                Direc_Imagen = nombreImagen,

                Recetario = receta.Select(r => new Cls_Recetario
                {
                    Id_Insumo = r.Id_Insumo,
                    Cantidad_Necesaria = float.Parse(r.Cantidad)
                }).ToList()
            };

            await servicio.Insert_PlatoAsync(nuevoPlato);

            await DisplayAlert("OK", "Plato guardado correctamente", "Cerrar");
            await Shell.Current.GoToAsync("Page_Platos");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // --------------------------- CANCELAR ---------------------------
    private async void btnCancelar_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    // --------------------------- MODO EDITAR ---------------------------
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (Id_Plato != 0)
        {
            btnCrear.IsVisible = false;
            btnEditar.IsVisible = true;

            btnAgregarImagen.IsVisible = false;
            imgPreviewBtn.IsVisible = true;

            CargarDatosParaEditar();
        }
        else
        {
            btnCrear.IsVisible = true;
            btnEditar.IsVisible = false;

            btnAgregarImagen.IsVisible = true;
            imgPreviewBtn.IsVisible = false;
        }
    }

    // --------------------------- CARGAR PLATO EN EDITAR ---------------------------
    private async void CargarDatosParaEditar()
    {
        try
        {
            var plato = await servicio.Search_PlatoAsync(Id_Plato);

            txtNombre.Text = plato.Nombre;
            txtDescripcion.Text = plato.Descripcion;
            txtPrecio.Text = plato.Precio.ToString();
            txtTiempo.Text = plato.Tiempo_Preparacion.ToString();
            VersionPlato = plato.Version;

            // Imagen
            if (plato.Imagen != null && plato.Imagen.Length > 0)
            {
                imagenBytes = plato.Imagen;
                nombreImagen = plato.txt_Nombre_Imagen;
                direc_Imagen = plato.Direc_Imagen;

                lblUrl.Text = direc_Imagen;

                imgPreviewBtn.Source =
                    ImageSource.FromStream(() => new MemoryStream(imagenBytes));
                imgPreviewBtn.IsVisible = true;
                btnAgregarImagen.IsVisible = false;
            }

            // Receta
            receta = plato.Recetario.Select(r => new RecetaItem
            {
                Id_Insumo = r.Id_Insumo,
                Insumo = r.txt_Insumo,
                Cantidad = r.Cantidad_Necesaria.ToString()
            }).ToList();

            cvReceta.ItemsSource = receta;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // --------------------------- EDITAR PLATO ---------------------------
    private async void btnEditar_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtDescripcion.Text) ||
                string.IsNullOrWhiteSpace(txtPrecio.Text) ||
                string.IsNullOrWhiteSpace(txtTiempo.Text))
            {
                await DisplayAlert("Error", "Complete todos los campos.", "OK");
                return;
            }

            // Si NO eligió nueva imagen: usamos la actual
            if (imagenBytes == null)
            {

                var platoBD = await servicio.Search_PlatoAsync(Id_Plato);

                imagenBytes = platoBD.Imagen;
                nombreImagen = platoBD.txt_Nombre_Imagen;
                direc_Imagen = platoBD.Direc_Imagen;
            }

            var plato = new Cls_Platos
            {
                Id_Plato = Id_Plato,
                Nombre = txtNombre.Text,
                Descripcion = txtDescripcion.Text,
                Precio = float.Parse(txtPrecio.Text),
                Tiempo_Preparacion = int.Parse(txtTiempo.Text),
                Activo = true,

                Direc_Imagen = direc_Imagen,
                Imagen = imagenBytes,
                txt_Nombre_Imagen = nombreImagen,

                Version = VersionPlato,

                Recetario = receta.Select(r => new Cls_Recetario
                {
                    Id_Insumo = r.Id_Insumo,
                    Cantidad_Necesaria = float.Parse(r.Cantidad)
                }).ToList()
            };

            await servicio.Update_PlatoAsync(plato);

            await DisplayAlert("OK", "Plato actualizado correctamente", "Cerrar");
            await Shell.Current.GoToAsync("Page_Platos");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
    private void btnQuitarInsumo_Clicked(object sender, EventArgs e)
    {
        var boton = sender as ImageButton;
        var item = boton?.CommandParameter as RecetaItem;

        if (item != null)
        {
            receta.Remove(item);

            cvReceta.ItemsSource = null;
            cvReceta.ItemsSource = receta;
        }
    }
}