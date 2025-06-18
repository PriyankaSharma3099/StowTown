using Microsoft.Maui.Controls;
using StowTown;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

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
            Debug.WriteLine("Window Activated event fired. Attempting to refresh FlyoutItem titles directly.");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Application.Current?.MainPage is AppShell appShell)
                {
                    Dictionary<FlyoutItem, string> originalTitles = appShell.GetOriginalFlyoutItemTitles();
                    if (originalTitles.Count == 0)
                    {
                        Debug.WriteLine("No original FlyoutItem titles were retrieved from AppShell.");
                    }

                    bool refreshedAny = false;
                    foreach (var flyoutItemPair in originalTitles)
                    {
                        FlyoutItem flyoutItem = flyoutItemPair.Key;
                        string originalTitle = flyoutItemPair.Value;

                        if (flyoutItem != null)
                        {
                            string tempTitle = originalTitle + " "; // Add a space
                            flyoutItem.Title = tempTitle;    // Force a change
                            flyoutItem.Title = originalTitle; // Change it back to original
                            Debug.WriteLine($"Refreshed title for FlyoutItem linked to original: '{originalTitle}'");
                            refreshedAny = true;
                        }
                    }

                    if (refreshedAny)
                    {
                        Debug.WriteLine("Finished attempt to refresh FlyoutItem titles.");
                    }
                    else if (originalTitles.Any()) // Original titles were found, but items were null (unlikely)
                    {
                         Debug.WriteLine("Original titles found, but corresponding FlyoutItems were null during refresh attempt.");
                    }
                }
                else
                {
                    Debug.WriteLine("AppShell instance not found on MainPage for title refresh.");
                }
            });
        }
    }
}