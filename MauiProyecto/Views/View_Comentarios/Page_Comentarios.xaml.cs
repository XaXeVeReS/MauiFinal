using System.Collections.ObjectModel;
using WCF_Apl_Dis;

namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Comentarios;

public partial class Page_Comentarios : ContentPage
{
    Service1Client Client;
    private ObservableCollection<ComentarioViewModel> _todosComentarios;
    private ObservableCollection<ComentarioViewModel> _comentariosFiltrados;

    public Page_Comentarios()
    {
        InitializeComponent();
        Client = APP_MAUI_Apl_Dis_2025_II.Services.WCFConfig.CreateWCFClient();

        _todosComentarios = new ObservableCollection<ComentarioViewModel>();
        _comentariosFiltrados = new ObservableCollection<ComentarioViewModel>();
        collectionComentarios.ItemsSource = _comentariosFiltrados;

        pickerFiltro.SelectedIndex = 0; // Todos
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CargarComentarios();
    }

    private async void CargarComentarios()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[COMENTARIOS] Cargando comentarios...");

            var comentarios = await Client.Get_Comentarios_AllAsync();

            _todosComentarios.Clear();
            if (comentarios != null && comentarios.Count > 0)
            {
                // Ordenar por fecha descendente
                var comentariosOrdenados = comentarios.OrderByDescending(c => c.Fecha_Comentario).ToArray();

                foreach (var comentario in comentariosOrdenados)
                {
                    _todosComentarios.Add(new ComentarioViewModel(comentario));
                }

                System.Diagnostics.Debug.WriteLine($"[COMENTARIOS] ✓ {comentarios.Count} comentarios cargados");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[COMENTARIOS] No hay comentarios");
            }

            ActualizarEstadisticas();
            AplicarFiltro();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[COMENTARIOS] ERROR: {ex.Message}");
            await DisplayAlert("Error", $"Error al cargar comentarios: {ex.Message}", "OK");
        }
    }

    private void ActualizarEstadisticas()
    {
        int total = _todosComentarios.Count;
        lblTotalComentarios.Text = total.ToString();

        if (total > 0)
        {
            float promedio = (float)_todosComentarios.Average(c => c.Valoracion);
            lblPromedioValoracion.Text = promedio.ToString("F1");

            int mejor = _todosComentarios.Max(c => c.Valoracion);
            lblMejorValoracion.Text = $"{mejor} ★";
        }
        else
        {
            lblPromedioValoracion.Text = "0.0";
            lblMejorValoracion.Text = "0 ★";
        }
    }

    private void AplicarFiltro()
    {
        _comentariosFiltrados.Clear();

        var tipoSeleccionado = pickerFiltro.SelectedIndex switch
        {
            1 => "Plato",
            2 => "Venta",
            _ => null
        };

        var filtrados = tipoSeleccionado == null
            ? _todosComentarios
            : _todosComentarios.Where(c => c.Tipo_Contexto == tipoSeleccionado);

        foreach (var comentario in filtrados)
        {
            _comentariosFiltrados.Add(comentario);
        }

        System.Diagnostics.Debug.WriteLine($"[COMENTARIOS] Filtro aplicado: {_comentariosFiltrados.Count} comentarios");
    }

    private void OnFiltroChanged(object sender, EventArgs e)
    {
        AplicarFiltro();
    }

    private void Actualizar_Clicked(object sender, EventArgs e)
    {
        CargarComentarios();
    }

    private async void Eliminar_Comentario_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int comentarioId)
        {
            bool confirmar = await DisplayAlert("Confirmar",
                "¿Está seguro de eliminar este comentario? Esta acción no se puede deshacer.",
                "Eliminar", "Cancelar");

            if (confirmar)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[COMENTARIOS] Eliminando comentario ID: {comentarioId}");

                    await Client.Delete_ComentarioAsync(comentarioId);

                    System.Diagnostics.Debug.WriteLine("[COMENTARIOS] ✓ Comentario eliminado");
                    await DisplayAlert("Éxito", "Comentario eliminado correctamente", "OK");

                    CargarComentarios();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[COMENTARIOS] ERROR al eliminar: {ex.Message}");
                    await DisplayAlert("Error", $"Error al eliminar comentario: {ex.Message}", "OK");
                }
            }
        }
    }

    // ViewModel para bindeo
    public class ComentarioViewModel
    {
        public int Id_Comentario { get; set; }
        public int? Id_Plato { get; set; }
        public int? Id_Venta { get; set; }
        public string Tipo_Contexto { get; set; }
        public string Contexto_Display { get; set; }
        public string Comentario { get; set; }
        public int Valoracion { get; set; }
        public Color Color_Valoracion { get; set; }
        public DateTime Fecha_Comentario { get; set; }
        public string Fecha_Formatted { get; set; }
        public int Id_Usuario { get; set; }
        public string Usuario_Display { get; set; }
        public bool Puede_Eliminar { get; set; }

        public ComentarioViewModel(Cls_Comentarios comentario)
        {
            Id_Comentario = comentario.Id_Comentario;
            Id_Plato = comentario.Id_Plato;
            Id_Venta = comentario.Id_Venta;
            Comentario = comentario.Comentario ?? "Sin comentario";
            Valoracion = comentario.Valoracion;
            Fecha_Comentario = comentario.Fecha_Comentario;
            Id_Usuario = comentario.Id_Usuario;

            // Formatear fecha
            Fecha_Formatted = Fecha_Comentario.ToString("dd/MM/yyyy HH:mm");

            // Determinar tipo de contexto
            if (Id_Plato.HasValue)
            {
                Tipo_Contexto = "Plato";
                Contexto_Display = $"Plato ID: {Id_Plato.Value}";
            }
            else if (Id_Venta.HasValue)
            {
                Tipo_Contexto = "Venta";
                Contexto_Display = $"Venta #{Id_Venta.Value}";
            }
            else
            {
                Tipo_Contexto = "General";
                Contexto_Display = "Comentario general";
            }

            // Usuario
            Usuario_Display = $"Usuario ID: {Id_Usuario}";

            // Color según valoración
            Color_Valoracion = Valoracion switch
            {
                5 => Colors.Green,
                4 => Color.FromArgb("#90EE90"),
                3 => Colors.Orange,
                2 => Color.FromArgb("#FF6347"),
                1 => Colors.Red,
                _ => Colors.Gray
            };

            // Solo puede eliminar si es el usuario actual
            Puede_Eliminar = Id_Usuario == Global.Id_Usuario;
        }
    }
}
