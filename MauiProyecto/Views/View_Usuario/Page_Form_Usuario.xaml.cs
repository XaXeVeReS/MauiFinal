using WCF_Apl_Dis;

namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Usuario;

public partial class Page_Form_Usuario : ContentPage, IQueryAttributable
{
    Service1Client Client;

    public int Id_Usuario { get; set; }

    public Page_Form_Usuario()
	{
		InitializeComponent();
        Client = APP_MAUI_Apl_Dis_2025_II.Services.WCFConfig.CreateWCFClient();
	}

    private void Switch_Activo(object sender, ToggledEventArgs e)
    {
        if (e.Value)
            lblActivo.Text = "Usuario: Activo";
        else
            lblActivo.Text = "Usuario: Suspendido";
    }

    private async void btn_Guardar_Usuario(object sender, EventArgs e)
    {
        Cls_Usuarios user = new Cls_Usuarios();


        if (contrasena.Text == confirmar.Text)
        {
            try
            {
                user.Nombre = nombre_usuario.Text;
                user.Email = email.Text;
                user.Telefono = telefono.Text;
                user.Password = contrasena.Text;
                user.Activo = Activo_switch.IsToggled;
                user.Rol = picker_Rol.SelectedItem?.ToString();

                await Client.Insert_UserAsync(user);
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlert("Error", ex.Message, "OK");
                });
            }
        }
        else
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await DisplayAlert("Error", "Contrasena no valida", "OK");
            });
        }
    }

    private async void Cargar_usuario()
    {
        try
        {
            Cls_Usuarios user = await Client.Search_UserAsync(Id_Usuario);

            nombre_usuario.Text = user.Nombre;
            email.Text = user.Email;
            telefono.Text = user.Telefono;
            picker_Rol.SelectedItem = user.Rol;
            Activo_switch.IsToggled = user.Activo;
            Fecha.SelectedDate = user.Fecha_Registro;

            btn_Guardar.Text = "Actualizar";
            btn_Guardar.Clicked -= btn_Guardar_Usuario;
            btn_Guardar.Clicked += btn_Actualizar_Usuario;
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await DisplayAlert("Error", ex.Message, "OK");
            });
        }
    }

    private async void btn_Actualizar_Usuario(object sender, EventArgs e)
    {
        Cls_Usuarios user = new Cls_Usuarios();

        if (contrasena.Text == confirmar.Text)
        {
            try
            {
                user.Id_Usuario = Id_Usuario;
                user.Nombre = nombre_usuario.Text;
                user.Email = email.Text;
                user.Telefono = telefono.Text;
                user.Password = contrasena.Text;
                user.Activo = Activo_switch.IsToggled;
                user.Rol = picker_Rol.SelectedItem?.ToString();

                await Client.Update_UserAsync(user);
                await DisplayAlert("Exito","usuario actualizado exitozamente", "OK");
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {                  
                    await DisplayAlert("Error", ex.Message, "OK");
                });
            }
        }
        else
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await DisplayAlert("Error", "Contrasena no valida", "OK");
            });
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("Id_Usuario"))
        {
            Id_Usuario = int.Parse(s: query["Id_Usuario"].ToString());
            Cargar_usuario();
        }
    }
}