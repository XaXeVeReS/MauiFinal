using Microcharts;
using SkiaSharp; // Necesario para los colores de Microcharts
using WCF_Apl_Dis;
using ChartEntry = Microcharts.ChartEntry;
using System.Globalization;

namespace APP_MAUI_Apl_Dis_2025_II.Views.View_Metricas;

public partial class Page_Metricas : ContentPage
{
    private readonly Service1Client servicio;
    // Paleta de colores para gráficos
    private readonly string[] coloresGraficos = new[] { "#2E86C1", "#27AE60", "#E67E22", "#8E44AD", "#C0392B", "#F1C40F", "#16A085" };

    public Page_Metricas()
    {
        InitializeComponent();
        servicio = new Service1Client();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Solo cargamos si es la primera vez o si las fechas no están seteadas
        if (dpFin.Date == dpInicio.Date)
        {
            dpFin.Date = DateTime.Today;
            dpInicio.Date = DateTime.Today.AddDays(-30);
            await CargarDashboard();
        }
    }

    private async void BtnGenerar_Clicked(object sender, EventArgs e)
    {
        await CargarDashboard();
    }

    private void SetLoading(bool isLoading)
    {
        // Muestra u oculta el overlay y deshabilita el botón
        loadingOverlay.IsVisible = isLoading;
        btnGenerar.IsEnabled = !isLoading;
    }

    private async Task CargarDashboard()
    {
        // 1. Validaciones básicas
        if (dpInicio.Date > dpFin.Date)
        {
            await DisplayAlert("Atención", "La fecha de inicio no puede ser mayor a la fecha fin.", "OK");
            return;
        }

        try
        {
            SetLoading(true); // Activar Spinner

            DateTime? fi = dpInicio.Date;
            DateTime? ff = dpFin.Date;

            // Llamada asíncrona al servicio
            var data = await servicio.Get_KPI_DinamicoAsync(fi, ff, null, null, null);

            if (data == null)
            {
                await DisplayAlert("Info", "No se recibieron datos del servidor.", "OK");
                return;
            }

            // ==============================================================================
            // BLOQUE DE SEGURIDAD
            // 1. Inicializamos listas en caso de que vengan nulas del servidor
            // ==============================================================================
            data.TopPlatos ??= new List<Cls_KPI_PlatoMasVendido>();
            data.InsumosUsados ??= new List<Cls_KPI_InsumoUsado>();
            data.ClientesFrecuentes ??= new List<Cls_KPI_ClienteFrecuente>();
            data.InsumosCriticos ??= new List<Cls_KPI_InsumoCritico>();
            data.PlatosMenosVendidos ??= new List<Cls_KPI_PlatoMenosVendido>();
            data.VentasPorDia ??= new List<Cls_KPI_VentaDia>();
            data.VentasPorPlato ??= new List<Cls_KPI_VentaPlato>();
            // ==============================================================================

            // =============================
            // 2. Llenado de KPIs (Labels)
            // =============================
            lblTicketPromedio.Text = data.PromedioDeVentas.ToString("C", new CultureInfo("es-PE"));
            lblInventario.Text = data.TotalInventario.ToString("C", new CultureInfo("es-PE"));

            // Plato top
            if (data.TopPlatos.Any())
            {
                var top = data.TopPlatos.First();
                lblPlatoTop.Text = top.Nombre;
                lblPlatoTopCant.Text = $"{top.CantidadVendida} un.";
            }
            else { lblPlatoTop.Text = "-"; lblPlatoTopCant.Text = ""; }

            // Insumo top
            if (data.InsumosUsados.Any())
            {
                var ins = data.InsumosUsados.First();
                lblInsumoTop.Text = ins.Nombre;
                lblInsumoTopCant.Text = $"{ins.CantidadUsada:N1} un.";
            }
            else { lblInsumoTop.Text = "-"; lblInsumoTopCant.Text = ""; }

            // Clientes frecuentes
            if (data.ClientesFrecuentes.Any())
            {
                var lista = data.ClientesFrecuentes.Take(3)
                    .Select(c => $"• {c.Nombre} ({c.Compras})");
                lblClientesFrecuentes.Text = string.Join("\n", lista);
            }
            else lblClientesFrecuentes.Text = "Sin datos recientes.";

            // Insumos críticos
            if (data.InsumosCriticos.Any())
            {
                var lista = data.InsumosCriticos
                    .Select(i => $"• {i.Nombre} (Disp: {i.Stock_Disponible} / Min: {i.Stock_Minimo})");
                lblInsumosCriticos.Text = string.Join("\n", lista);
                lblInsumosCriticos.TextColor = Colors.DarkRed;
            }
            else
            {
                lblInsumosCriticos.Text = "Todo el inventario está saludable. ✅";
                lblInsumosCriticos.TextColor = Colors.Green;
            }

            // Platos menos vendidos
            if (data.PlatosMenosVendidos.Any())
            {
                var lista = data.PlatosMenosVendidos.Take(3)
                    .Select(p => $"• {p.Nombre} ({p.Vendidos})");
                lblPlatosMenos.Text = string.Join("\n", lista);
            }
            else lblPlatosMenos.Text = "Sin datos.";


            // =============================
            // 3. Configuración de Gráficos (CON PROTECCIÓN ANTI-CRASH)
            // =============================

            // GRÁFICO 1: LINEAL (Ventas)
            // Solo creamos el gráfico si hay datos. Si la lista está vacía, ocultamos el control.
            if (data.VentasPorDia.Any())
            {
                var entriesVentas = new List<ChartEntry>();
                foreach (var v in data.VentasPorDia)
                {
                    entriesVentas.Add(new ChartEntry((float)v.Total)
                    {
                        Label = v.Fecha.ToString("dd/MM"),
                        ValueLabel = v.Total.ToString("0"),
                        Color = SKColor.Parse("#2E86C1") // Azul corporativo
                    });
                }

                chartVentasPorDia.Chart = new LineChart
                {
                    Entries = entriesVentas,
                    LineMode = LineMode.Straight,
                    LineSize = 8,
                    PointMode = PointMode.Circle,
                    PointSize = 18,
                    LabelTextSize = 25,
                    BackgroundColor = SKColors.Transparent
                };
                chartVentasPorDia.IsVisible = true; // Mostrar si hay datos
            }
            else
            {
                chartVentasPorDia.IsVisible = false; // Ocultar para evitar división por cero interna
            }


            // GRÁFICO 2: BARRAS (Platos)
            if (data.VentasPorPlato.Any())
            {
                var entriesPlatos = new List<ChartEntry>();
                int colorIndex = 0;

                foreach (var p in data.VentasPorPlato.Take(5))
                {
                    string colorHex = coloresGraficos[colorIndex % coloresGraficos.Length];
                    entriesPlatos.Add(new ChartEntry(p.CantidadVendida)
                    {
                        Label = p.Nombre.Length > 10 ? p.Nombre.Substring(0, 10) + "..." : p.Nombre,
                        ValueLabel = p.CantidadVendida.ToString(),
                        Color = SKColor.Parse(colorHex)
                    });
                    colorIndex++;
                }

                chartPlatosTop.Chart = new BarChart
                {
                    Entries = entriesPlatos,
                    LabelTextSize = 25,
                    ValueLabelOrientation = Orientation.Horizontal,
                    LabelOrientation = Orientation.Horizontal
                };
                chartPlatosTop.IsVisible = true;
            }
            else
            {
                chartPlatosTop.IsVisible = false;
            }


            // GRÁFICO 3: DONUT (Insumos)
            if (data.InsumosUsados.Any())
            {
                var entriesInsumos = new List<ChartEntry>();
                int colorIndex = 0;

                foreach (var i in data.InsumosUsados.Take(5))
                {
                    string colorHex = coloresGraficos[(colorIndex + 2) % coloresGraficos.Length];
                    entriesInsumos.Add(new ChartEntry((float)i.CantidadUsada)
                    {
                        Label = i.Nombre,
                        ValueLabel = i.CantidadUsada.ToString("0"),
                        Color = SKColor.Parse(colorHex)
                    });
                    colorIndex++;
                }

                chartInsumos.Chart = new DonutChart
                {
                    Entries = entriesInsumos,
                    LabelTextSize = 25,
                    HoleRadius = 0.4f
                };
                chartInsumos.IsVisible = true;
            }
            else
            {
                chartInsumos.IsVisible = false;
            }

        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un problema: {ex.Message}", "OK");
        }
        finally
        {
            SetLoading(false); // Desactivar Spinner siempre
        }
    }
}