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
            Debug.WriteLine("Window Activated event fired. Attempting to refresh AppShell layout via Padding.");

            // Ensure execution on the main UI thread.
            // While Activated event should be on UI thread, explicit dispatching is safer for UI manipulations.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Application.Current?.MainPage is AppShell appShell)
                {
                    var originalPadding = appShell.Padding;
                    // Apply a tiny, likely imperceptible change to Padding to trigger a layout update.
                    // Adding to Left; any component or a new Thickness object would work.
                    appShell.Padding = new Thickness(originalPadding.Left + 0.001, originalPadding.Top, originalPadding.Right, originalPadding.Bottom);
                    // Immediately revert to the original padding.
                    appShell.Padding = originalPadding;
                    Debug.WriteLine("AppShell.Padding temporarily modified and reverted to trigger UI refresh.");
                }
                else
                {
                    Debug.WriteLine("AppShell instance not found on MainPage for Padding modification.");
                }
            });
        }
    }
}