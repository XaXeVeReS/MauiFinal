using APP_MAUI_Apl_Dis_2025_II.Views.View_Platos;
using APP_MAUI_Apl_Dis_2025_II.Views.View_Usuario;
using APP_MAUI_Apl_Dis_2025_II.Views.View_Pedidos;
using APP_MAUI_Apl_Dis_2025_II.Views.View_Promociones;
using APP_MAUI_Apl_Dis_2025_II.Views.View_Insumos;
using APP_MAUI_Apl_Dis_2025_II.Views.View_Ventas;
using APP_MAUI_Apl_Dis_2025_II.Views.View_Metricas;
using APP_MAUI_Apl_Dis_2025_II.Views.View_Alertas;
using APP_MAUI_Apl_Dis_2025_II.Views.View_Compras;
using APP_MAUI_Apl_Dis_2025_II.Views.View_Comentarios;
namespace APP_MAUI_Apl_Dis_2025_II
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("Page_Pedidos", typeof(Page_Pedidos));
            Routing.RegisterRoute("Page_Form_Pedidos", typeof(Page_Form_Pedidos));
            Routing.RegisterRoute("Page_Form_Pago", typeof(Page_Form_Pago));

            Routing.RegisterRoute("Page_Platos", typeof(Page_Platos));
            Routing.RegisterRoute("Page_CrearPlato", typeof(Page_CrearPlato));

            Routing.RegisterRoute("Page_Usuarios", typeof(Page_Usuarios));
            Routing.RegisterRoute("Page_Form_Usuario", typeof(Page_Form_Usuario));

            Routing.RegisterRoute("Page_Promociones", typeof(Page_Promociones));
            Routing.RegisterRoute("Page_CrearPromociones", typeof(Page_CrearPromociones));

            Routing.RegisterRoute("Page_Insumos", typeof(Page_Insumos));
            Routing.RegisterRoute("Page_Form_Stock", typeof(Page_Form_Stock));
            Routing.RegisterRoute("Page_Form_Insumo", typeof(Page_Form_Insumo));
            Routing.RegisterRoute("Page_Cardex", typeof(Page_Cardex));

            Routing.RegisterRoute("Page_Compras", typeof(Page_Compras));
            Routing.RegisterRoute("Page_Form_Compra", typeof(Page_Form_Compra));
            Routing.RegisterRoute("Page_Detalle_Compra", typeof(Page_Detalle_Compra));

            Routing.RegisterRoute("Page_Comentarios", typeof(Page_Comentarios));

            Routing.RegisterRoute("Page_Ventas", typeof(Page_Ventas));
            Routing.RegisterRoute("Page_DetalleVenta", typeof(Page_DetalleVenta));

            Routing.RegisterRoute("Page_Metricas", typeof(Page_Metricas));

            Routing.RegisterRoute("Page_Alertas", typeof(Page_Alertas));

            Routing.RegisterRoute("alertas_page", typeof(APP_MAUI_Apl_Dis_2025_II.Views.View_Alertas.Page_Alertas));

        }
        private void CerrarFlyout(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = false;
        }

    }
}