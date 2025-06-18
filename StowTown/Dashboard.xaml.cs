using StowTown.Pages.DJ;
using StowTown.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace StowTown
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Dashboard: ContentPage
    {
        public static Stack<ContentView> _viewHistory = new();

        public Dashboard()
        {
            InitializeComponent();
         
        }

        
        private void OnRadioStationClicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("//RadioStationManagement");
        }

        private void OnDashboardClicked(object sender, EventArgs e)
        {
            // Shell.Current.GoToAsync("//DashboardPage");
            Shell.Current.GoToAsync("//HomeDashboard");
        }

        private void OnProducerClicked(object sender, EventArgs e)
        {
            // Wrap the NavigationPage in a ContentView to resolve the type mismatch  
            //var navPage = new NavigationPage(new PojectProducerManagement());
            //var contentView = new ContentView();

            //// Use the CurrentPage directly as the Content property does not exist on Page  
            //if (navPage.CurrentPage is ContentPage contentPage)
            //{
            //    contentView.Content = contentPage.Content;
            //}
            //else
            //{
            //    throw new InvalidOperationException("The CurrentPage is not a ContentPage and does not have a Content property.");

            //}

            //MainContent.Content = contentView;
            // Create a new instance of PojectProducerManagement wrapped in a NavigationPage
            // Create a new instance of PojectProducerManagement wrapped in a NavigationPage
            // var pojectProducerPage = new PojectProducerManagement();
            // var navPage = new NavigationPage(pojectProducerPage);

            // Since NavigationPage does not expose Content, we need to assign the Content of the ContentPage
            // MainContent.Content = pojectProducerPage.Content;  // Set Content of the page to MainContent
            // Directly create page
            //var pojectProducerPage = new PojectProducerManagement();

            //// Set the Content of MainContent
            //MainContent.Content = pojectProducerPage.Content;
            var createProducerPage = new PojectProducerManagement(MainContent);
            // MainContent.Content = createProducerPage.Content;

            // Ensure the Content is wrapped in a ContentView to match the expected type  
            var contentView = new ContentView { Content = createProducerPage.Content };

            MainContent.Content = contentView;

            // Push the ContentView to the history stack  
            _viewHistory.Push(contentView);

           // var nav = new  NavigableContentView();
           
           // contentView.nav.NavigateTo = NavigateTo;
           // contentView.NavigateBack = NavigateBack;
           //// _navigationStack.Push(page1);
           // MainContent.Content = page1;
        }
        public void GoBack()
        {
            if (_viewHistory.Count > 0)
            {
                var previousView = _viewHistory.Pop();
                Console.WriteLine(previousView.GetType().Name);
                Console.WriteLine(previousView.Content != null ? "Has Content" : "No Content");
                var createProducerPage = new PojectProducerManagement(MainContent);
                // MainContent.Content = createProducerPage.Content;

                // Ensure the Content is wrapped in a ContentView to match the expected type  
                var contentView = new ContentView { Content = createProducerPage.Content };

                MainContent.Content = contentView;
                //MainContent.Content = previousView;
            }
           
        }
        private void OnDJClicked(object sender, EventArgs e)
        {
            //Shell.Current.GoToAsync("//DjManagement");
            var page = new ContentView { Content = new DjManagement().Content };
            MainContent.Content = page;
        }

        private void OnArtistClicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("//ArtistManagement");
        }

        private void OnReportingClicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("//Graph");

        }

        private void OnCallHistoryClicked(object sender, EventArgs e)
        {

        }

        private void OnSongListClicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("//SongManagement");    
        }

        private void OnMonthlySongListClicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("//MonthlySongManagement");
        }

        private void OnCallSchedulerClicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("//CallScheduleList");
        }

        private void ShowContextMenu(object sender, EventArgs e)
        {

        }

        private void OnAdminMenuClicked(object sender, EventArgs e)
        {

        }

        private async void NavigateToChildPage_Clicked(object sender, EventArgs e)
        {
            //  await Navigation.PushAsync(new HomeDashboard());
            await Shell.Current.GoToAsync(nameof(HomeDashboard));
        }

        internal void Push(ContentView contentView)
        {
            throw new NotImplementedException();
        }
    }
}