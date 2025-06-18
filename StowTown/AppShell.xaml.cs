using Microsoft.Maui.ApplicationModel.Communication;
using StowTown.HelperService;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using StowTown.Models;
using StowTown.Pages.ArtistsManagements;
using StowTown.Pages.CallHistory;
using StowTown.Pages.CallScheduleListManagement;
using StowTown.Pages.DJ;
using StowTown.Pages.ManualSpinTracker;

//using StowTown.Pages.ManualSpinTracker;
using StowTown.Pages.MonthlySong;
using StowTown.Pages.RadioStations;
using StowTown.Pages.Reports;
using StowTown.Pages.Songs;
using StowTown.Pages.UserSettingInfo;
using StowTown.Services;

namespace StowTown
{
    public partial class AppShell : Shell
    {
        private Dictionary<FlyoutItem, string> originalFlyoutItemTitles = new Dictionary<FlyoutItem, string>();
        private static string Email; // Declare 'email' variable  
        private string password; // Declare 'password' variable  
                                 // public UserInfoViewModel UserInfo { get; } = new UserInfoViewModel();
        public UserInfoViewModel UserInfo { get; set; }
        public static string loginuser { get; set; } // Static property to hold the logged-in user's name
        public AppShell()
        {
            InitializeComponent();
            BindingContext = UserInfo;

            // Store original titles
            StoreOriginalFlyoutTitles();

            Dispatcher.Dispatch(async () =>
            {
                if (Shell.Current.CurrentPage is MainPage)
                {

                    Shell.Current.FlyoutIsPresented = false;

                    var mainPageShellContent = Shell.Current.Items.FirstOrDefault();

                    // Hide from Flyout after app starts  
                    if (mainPageShellContent != null)
                    {
                        mainPageShellContent.FlyoutItemIsVisible = false;
                        HomeDashboard.isInitializing = true;
                    }

                    else
                    {
                        Shell.Current.FlyoutIsPresented = true;

                    }
                }


            });
           
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage)); // Optional, used only if you navigate later  
            Routing.RegisterRoute("DashboardPage", typeof(DashboardPage));
            //Routing.RegisterRoute("HomeDashboard", typeof(StowTown.HomeDashboard));
            Routing.RegisterRoute(nameof(HomeDashboard), typeof(HomeDashboard)); // only once
            //Routing.RegisterRoute(nameof(HomeDashboard), new  <HomeDashboard>()); //  uses DI

            Routing.RegisterRoute(nameof(CreateProjectProducer), typeof(CreateProjectProducer));
            Routing.RegisterRoute(nameof(RadioStationManagement), typeof(RadioStationManagement));
            Routing.RegisterRoute(nameof(CreateRadioStation), typeof(CreateRadioStation));
            Routing.RegisterRoute(nameof(CreateDJ), typeof(CreateDJ));
            Routing.RegisterRoute(nameof(ArtistManagement), typeof(ArtistManagement));
            Routing.RegisterRoute(nameof(CreateArtistManagement), typeof(CreateArtistManagement));
            Routing.RegisterRoute(nameof(Graph), typeof(Graph));
            Routing.RegisterRoute(nameof(CreateSong), typeof(CreateSong));
            Routing.RegisterRoute(nameof(CreateMonthlySongList), typeof(CreateMonthlySongList));
            Routing.RegisterRoute(nameof(EmailService), typeof(EmailService));
            Routing.RegisterRoute(nameof(CreateCallTimingsList), typeof(CreateCallTimingsList));
            Routing.RegisterRoute(nameof(CreateCallHistory), typeof(CreateCallHistory));
            //Routing.RegisterRoute("", typeof(UserInfoPage));
            Routing.RegisterRoute(nameof(UserSettingInfo), typeof(UserSettingInfo));
            Routing.RegisterRoute(nameof(ChangePasswordPopup), typeof(ChangePasswordPopup));
            Routing.RegisterRoute(nameof(ForgetPassword), typeof(ForgetPassword));
            Routing.RegisterRoute(nameof(ManualSongSpins), typeof(ManualSongSpins));
            //InitializeAsync();  
            HomeDashboard.isInitializing = false;
        }

        private async Task InitializeAsync()
        {
            var loginPage = new MainPage();
            loginPage.BindingContext = loginPage;

            if (!string.IsNullOrEmpty(Email)) // Ensure 'email' is not null or empty  
            {
                User user = await loginPage.GetUserByEmail(Email); // Retrieve user by email  
                if (user != null)
                {
                    string hashedPassword = loginPage.HashPassword(password); // Hash the password  
                    if (user.UserName == Email && user.Password == hashedPassword)
                    {
                        // Authentication successful logic here  
                        loginuser = user.UserName; // Set the logged-in user's name
                    }
                }
            }
        }

        private void OnSettingsClicked(object sender, EventArgs e)
        {
            ////var page = new UserInfoPage();
            //if (GlobalUserInfo.CurrentUser != null)
            //{
            //    loginuser = GlobalUserInfo.CurrentUser.Email; // Set the logged-in user's name
            //}

            //Shell.Current.GoToAsync($"{nameof(page)}?SelectedId=0&Type=View"); // Navigate to UserInfoPage
            // Shell.Current.GoToAsync($"{nameof(CreateSong)}?SelectedId={selectedId}&Type=View");  
            Shell.Current.GoToAsync($"UserSettingInfo?SelectedId=123&Type=loginuser");
        }

        private void OnLogoutClicked(object sender, EventArgs e)
        {
            //Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            Application.Current.MainPage = new MainPage();
        }

        private void OnFlyoutHeaderTapped(object sender, TappedEventArgs e)
        {
            FlyoutDropdown.IsVisible = !FlyoutDropdown.IsVisible;
        }

        protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
        {
            base.OnNavigatedFrom(args);

            string route = Shell.Current.CurrentItem?.Route ?? "";

            switch (route)
            {
                case "HomeDashboard":
                    ShellHelper.UpdateFlyoutItemTitle("HomeDashboard", "Home", "assets/Home.png");
                    break;
                case "RadioStationManagement":
                    ShellHelper.UpdateFlyoutItemTitle("RadioStationManagement", "Radio Station", "assets/music.png");
                    break;
                case "DjManagement":
                    ShellHelper.UpdateFlyoutItemTitle("DjManagement", "Dj", "assets/headphones.png");
                    break;
                case "ArtistManagement":
                    ShellHelper.UpdateFlyoutItemTitle("ArtistManagement", "Artist", "assets/user.png");
                    break;
                case "Graph":
                    ShellHelper.UpdateFlyoutItemTitle("Graph", "Reporting", "assets/Icons.png");
                    break;
                case "SongManagement":
                    ShellHelper.UpdateFlyoutItemTitle("SongManagement", "Song List", "assets/music.png");
                    break;
                case "MonthlySongManagement":
                    ShellHelper.UpdateFlyoutItemTitle("MonthlySongManagement", "Monthly Song List", "assets/filemusic.png");
                    break;
                case "CallScheduleList":
                    ShellHelper.UpdateFlyoutItemTitle("CallScheduleList", "Call Schedule", "assets/Icons.png");
                    break;
                case "PojectProducerManagement":
                    ShellHelper.UpdateFlyoutItemTitle("PojectProducerManagement", "Producer", "assets/radio.png");
                    break;
                case "ManualSongSpins":
                    ShellHelper.UpdateFlyoutItemTitle("ManualSongSpins", "Manual Spin Tracker", "assets/music.png");
                    break;
            }
        }

        private void StoreOriginalFlyoutTitles()
        {
            originalFlyoutItemTitles.Clear(); // Clear if it were ever called multiple times, though unlikely for constructor
            foreach (var item in this.Items.OfType<FlyoutItem>())
            {
                if (item != null && !string.IsNullOrEmpty(item.Title))
                {
                    originalFlyoutItemTitles[item] = item.Title;
                    Debug.WriteLine($"Stored original title for FlyoutItem: '{item.Title}'");
                }
            }
            // Also handle ShellContent items that act as flyout items if they are not FlyoutItem typed but have titles
            // For this specific app structure, FlyoutItems are explicitly defined.
            // The "Call History" item is a FlyoutItem without a route. This loop should catch it.
        }

        public Dictionary<FlyoutItem, string> GetOriginalFlyoutItemTitles()
        {
            // Ensure it's populated if called before constructor fully finishes or if items change (though not expected here)
            // For safety, could call StoreOriginalFlyoutTitles() here if originalFlyoutItemTitles is empty,
            // but for this plan, constructor population should be sufficient.
            return new Dictionary<FlyoutItem, string>(originalFlyoutItemTitles); // Return a copy
        }
    

    }
}

