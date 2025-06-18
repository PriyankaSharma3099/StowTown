using Microsoft.Maui.Controls;
using StowTown;
using System.Diagnostics;

namespace StowTown
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                InitializeComponent();
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StowTownLogs");
                Directory.CreateDirectory(folder);

                string filePath = Path.Combine(folder, $"startup-error-{DateTime.Now:yyyyMMdd-HHmmss}.log");
                File.WriteAllText(filePath, "Set Up Success");
                MainPage = new MainPage();

                Routing.RegisterRoute("CreateProjectProducer", typeof(CreateProjectProducer));
            }
            catch (Exception ex)
            {
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StowTownLogs");
                Directory.CreateDirectory(folder);

                string filePath = Path.Combine(folder, $"startup-error-{DateTime.Now:yyyyMMdd-HHmmss}.log");
                File.WriteAllText(filePath, ex.ToString());


            }


        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = base.CreateWindow(activationState);

            window.Activated += OnWindowActivated;
            // Optionally, you can also subscribe to Deactivated if needed for logging or other logic
            // window.Deactivated += OnWindowDeactivated;

            return window;
        }

        private void OnWindowActivated(object sender, EventArgs e)
        {
            Debug.WriteLine("Window Activated event fired.");
            if (Application.Current?.MainPage is AppShell appShell)
            {
                appShell.RefreshAllFlyoutTitles();
            }
            else
            {
                Debug.WriteLine("AppShell instance not found on MainPage for refreshing titles.");
            }
        }

        // Optional: Handler for Deactivated event if you added it
        // private void OnWindowDeactivated(object sender, EventArgs e)
        // {
        //     Debug.WriteLine("Window Deactivated event fired.");
        // }
    }
}