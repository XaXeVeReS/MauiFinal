using WCF_Apl_Dis;
namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Usuario;

public partial class Page_Usuarios : ContentPage
{
    Service1Client Client;

    public List<Cls_Usuarios> ListaUsuarios { get; set; } = new();
    public Page_Usuarios()
	{
		InitializeComponent();
        Client = APP_MAUI_Apl_Dis_2025_II.Services.WCFConfig.CreateWCFClient();
        Cargar_Usuarios();
    }


    private async void btn_Agregar_Usuario(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Page_Form_Usuario");
    }

    private async void Cargar_Usuarios()
    {
        try
        {
            var lista = await Client.Get_UsersAsync ();
            ListaUsuarios.Clear();
            ListaUsuarios.AddRange(lista);

            TablaUsuarios.ItemsSource = null;
            TablaUsuarios.ItemsSource = ListaUsuarios;
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await DisplayAlert("Error", ex.Message, "OK");
            });
        }
    }

    private async void btnEditarUsuario_Clicked(object sender, EventArgs e)
    {
        var boton = (ImageButton)sender;
        var user = (Cls_Usuarios)boton.BindingContext;

        await Shell.Current.GoToAsync($"Page_Form_Usuario?Id_Usuario={user.Id_Usuario}");
    }
}