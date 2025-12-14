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

                mostrar_mensage("Usuario ingresado exitozamente", true);
            }
            catch (Exception ex)
            {
                mostrar_mensage(ex.Message, false);
            }
        }
        else
        {
            mostrar_mensage("Contrasena no valida", false);
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
        catch 
        {
            mostrar_mensage("Usario no encontrado", false);
        }
    }

    private void mostrar_mensage(string mensage,bool error)
    {
        lblError.Text = mensage;
        lblError.TextColor = error ?Colors.Green: Colors.Red;
        lblError.Opacity = 1;
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
                mostrar_mensage("Usuario actualizado exitozamente", true);
            }
            catch (Exception ex)
            {
                mostrar_mensage(ex.Message, false);
            }
        }
        else
        {
            mostrar_mensage("Contrasena no valida", false);
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

    private async void btn_Cancelar_Guardado(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Page_Usuarios");
    }
}