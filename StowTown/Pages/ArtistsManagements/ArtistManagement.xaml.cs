using Microsoft.Maui;
using System.Collections.ObjectModel;
using StowTown.ViewModels;
using StowTown.Models;
using StowTown.Services.SaveImageService;

namespace StowTown.Pages.ArtistsManagements;

public partial class ArtistManagement : ContentPage
{
    private ObservableCollection<ArtistViewModel> _allArtists;
    private ObservableCollection<ArtistViewModel> _currentPageArtists;
    private const int PageSize = 10;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private ObservableCollection<ArtistViewModel> _filteredArtists;  // New collection for filtered results
    private string _searchText = string.Empty;  // Store the search query
    public ArtistManagement()
	{
        InitializeComponent();
        _currentPageArtists = new ObservableCollection<ArtistViewModel>();
        _filteredArtists = new ObservableCollection<ArtistViewModel>();
        ArtistList.ItemsSource = _currentPageArtists; // Assuming ListView ID is 'ArtistList'
        LoadData();

        // Subscribe to refresh message
        MessagingCenter.Subscribe<CreateArtistManagement>(this, "RefreshArtistList", (sender) =>
        {
            LoadData();
        });
        settingbutton();
    }

    public void settingbutton()
    {
        PreviousButton.BackgroundColor = Color.FromArgb("#63CF6C");
        PreviousButton.TextColor = Colors.White;
        NextButton.BackgroundColor = Color.FromArgb("#63CF6C");
        NextButton.TextColor = Colors.White;
        PreviousButton.WidthRequest = 100;
        NextButton.WidthRequest = 100;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadData(); // Refresh data every time the page appears
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Unsubscribe from the message when the page disappears
        MessagingCenter.Unsubscribe<CreateArtistManagement>(this, "RefreshArtistList");
    }
    public void LoadData()
    {
        try
        {
            using (var context = new StowTownDbContext())
            {
                var artistList = context.ArtistGroups
                    .Where(a => a.IsDeleted != true )
                    .OrderByDescending(a => a.Id)
                    .ToList();

                var artistViewList = new List<ArtistViewModel>();
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imagesDirectory = Path.Combine(exeDirectory, "assets", "ArtistImages");

                for (int i = 0; i < artistList.Count; i++)
                {
                    var artist = artistList[i];

                    //  Get the count of members directly without loading them
                    int noOfMembers = context.ArtistGroupMembers.Count(d => d.FkArtistGroup == artist.Id && d.IsDeleted == false);

                    // Get the count of songs directly without loading them
                    int noOfSongs = context.Songs.Count(s => s.FkArtist == artist.Id && s.IsDeleted == false);

                    //  Add to artistViewList
                    artistViewList.Add(new ArtistViewModel
                    {
                        Id = artist.Id,
                        Name = artist.Name,
                        IsActive = artist.IsActive,
                        // GroupPicture = artist.GroupPicture != null ? Path.Combine(imagesDirectory, artist.GroupPicture) : null,
                        GroupPicture = artist.GroupPicture != null ? ImageFilesService.GetImageUrl("ArtistGroupImages", artist.GroupPicture) : null,
                        GroupTitle = artist.GroupTitle,
                        GroupWebsite = artist.GroupWebsite,
                        GroupAccomplishment = artist.GroupAccomplishment,
                        IsDeleted = artist.IsDeleted,
                        CreatedAt = artist.CreatedAt,
                        UpdatedAt = artist.UpdatedAt,
                        SerialNumber = i + 1,
                        NoOfMembers = noOfMembers,  // Assigning count directly
                        NoOfSongs = noOfSongs      // Assigning count directly
                    });
                }

                _allArtists = new ObservableCollection<ArtistViewModel>(artistViewList);
                _filteredArtists = new ObservableCollection<ArtistViewModel>(_allArtists);
                _totalPages = (int)Math.Ceiling(_allArtists.Count / (double)PageSize);
                LoadCurrentPage();
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void LoadCurrentPage()
    {
        _currentPageArtists.Clear();
        var pageItems = _filteredArtists
            .Skip((_currentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        foreach (var item in pageItems)
        {
            _currentPageArtists.Add(item);
        }

        UpdatePaginationControls();
    }

    private void UpdatePaginationControls()
    {
        PreviousButton.IsEnabled = _currentPage > 1;
        NextButton.IsEnabled = _currentPage < _totalPages;
        PageInfo.Text = $"Page {_currentPage} of {_totalPages}";
      //  NoResultsLabel.IsVisible = _filteredArtists.Count == 0;
    }

    private void PreviousButton_Clicked(object sender, EventArgs e)
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            LoadCurrentPage();
        }
    }

    private void NextButton_Clicked(object sender, EventArgs e)
    {
        if (_currentPage < _totalPages)
        {
            _currentPage++;
            LoadCurrentPage();
        }
    }

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _searchText = e.NewTextValue.ToLower();  // Capture the search text and convert it to lowercase

        // Filter the artist list based on the search text
        var filteredList = _allArtists
            .Where(a => a.Name.ToLower().Contains(_searchText))  // Filter based on name or any other properties you want
            .ToList();

        // Update the filtered collection
        _filteredArtists = new ObservableCollection<ArtistViewModel>(filteredList);

        if(_filteredArtists.Count == 0)
        {
            await DisplayAlert("No Results", "No Artists found matching your search criteria", "Ok");
            PreviousButton.IsVisible = false;
            NextButton.IsVisible = false;
            PageInfo.IsVisible = false;
            IsEmptyPageInfo.Text = "No Data Available";
            IsEmptyPageInfo.IsVisible = true;
        }
        else
        {

            PreviousButton.IsVisible = true;
            NextButton.IsVisible = true;
            PageInfo.IsVisible = true;
            IsEmptyPageInfo.Text = string.Empty;
            IsEmptyPageInfo.IsVisible = false;

        }
        // Reset to the first page after search
        _currentPage = 1;
        _totalPages = (int)Math.Ceiling(_filteredArtists.Count / (double)PageSize);
        LoadCurrentPage();

    }

    private async void OnViewButtonClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.CommandParameter is int selectedId)
        {
            await Shell.Current.GoToAsync($"{nameof(CreateArtistManagement)}?SelectedId={selectedId}&Type=View");       
        }
    }

    private async void OnEditButtonClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.CommandParameter is int selectedId)
        {
            await Shell.Current.GoToAsync($"{nameof(CreateArtistManagement)}?SelectedId={selectedId}&Type=Edit");
        }
    }

    private  async void OnDeleteButtonClicked(object sender, EventArgs e)
    {

        bool answer = await Application.Current.MainPage.DisplayAlert(
         "Confirm Delete",
         "Are you sure you want to delete this item?",
         "Yes",
         "No");

        if (!answer)
            return;

        try
        {
            if (sender is ImageButton button && button.CommandParameter is int selectedId)
            {
                using (var context = new StowTownDbContext())
                {
                    var getrowtodelete = context.ArtistGroups.Find(selectedId);
                    if (getrowtodelete != null)
                    {
                        getrowtodelete.IsDeleted = true;
                        getrowtodelete.IsActive = false;
                        var imageName = getrowtodelete.GroupPicture;
                        if (imageName != null) {

                            bool isDelete = ImageFilesService.DeleteFtpImage("ArtistGroupImages", imageName);
                            if (!isDelete) {
                                await DisplayAlert("Error", "Image File not found", "Ok");
                            }
                        }

                        var producers = context.ProjectProducerArtistGroups
                            .Where(p => p.FkArtist == selectedId).ToList();
                        foreach (var producer in producers)
                        {
                            producer.IsDeleted = true;
                        }

                        var members = context.ArtistGroupMembers
                            .Where(m => m.FkArtistGroup == selectedId).ToList();
                        foreach (var member in members)
                        {
                            member.IsDeleted = true;
                            var imagememberName = member.MemberPicture;
                            if (imageName != null)
                            {

                                bool isDelete = ImageFilesService.DeleteFtpImage("ArtistMemberImages", imagememberName);
                                if (!isDelete)
                                {
                                    await DisplayAlert("Error", "Image File not found", "Ok");
                                }
                            }
                        }

                        await context.SaveChangesAsync();

                        await Application.Current.MainPage.DisplayAlert("Success", "Artist and Members Deleted Successfully.", "OK");
                        LoadData();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private  async void OnCreateClicked(object sender, EventArgs e)
    {
       
        if (sender is ImageButton button && button.CommandParameter is int selectedId)
        {
            await Shell.Current.GoToAsync($"{nameof(CreateArtistManagement)}?SelectedId={0}&Type=Create");
        }
        await Shell.Current.GoToAsync($"{nameof(CreateArtistManagement)}?SelectedId={0}&Type=Create");

    }
}