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

            return window;
        }

        private void OnWindowActivated(object sender, EventArgs e)
        {
            Debug.WriteLine("Window Activated event fired. Attempting to invalidate AppShell measure.");

            // Ensure execution on the main UI thread.
            // While Activated event should be on UI thread, explicit dispatching is safer for UI manipulations.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Application.Current?.MainPage is AppShell appShell)
                {
                    appShell.InvalidateMeasure();
                    Debug.WriteLine("AppShell.InvalidateMeasure() called.");
                }
                else
                {
                    Debug.WriteLine("AppShell instance not found on MainPage for InvalidateMeasure.");
                }
            });
        }
    }
}