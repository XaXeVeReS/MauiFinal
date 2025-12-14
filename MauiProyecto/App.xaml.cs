using APP_MAUI_Apl_Dis_2025_II.Core;
namespace APP_MAUI_Apl_Dis_2025_II
{
    public partial class App : Application
    {
        private readonly AlertDetector alertDetector = new AlertDetector();
        public App()
        {
            InitializeComponent();
            Task.Run(async () =>
            {
                await alertDetector.EjecutarDeteccionAsync();
            });

            // Timer para actualizar cada 30 segundos
            var timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(30);
            timer.IsRepeating = true;

            timer.Tick += async (s, e) =>
            {
                await alertDetector.EjecutarDeteccionAsync();
            };

            timer.Start();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}