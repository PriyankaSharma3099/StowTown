using DocumentFormat.OpenXml.Bibliography;
using Microsoft.Maui;
using StowTown.Models;
using StowTown.Services.SaveImageService;
using StowTown.ViewModels;
using System.Collections.ObjectModel;

namespace StowTown.Pages.Songs;

public partial class SongManagement : ContentPage
{
    private ObservableCollection<SongViewModel> _allSongs;
    private ObservableCollection<SongViewModel> _filteredSongs;
    private const int PageSize = 10;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private ObservableCollection<SongViewModel> _filteredArtists;  // New collection for filtered results
    private string _searchText = string.Empty;  // Store the search query
    public SongManagement()
	{
		InitializeComponent();

        _filteredSongs = new ObservableCollection<SongViewModel>();
        _allSongs = new ObservableCollection<SongViewModel>();
       
        LoadData();

        // Subscribe to refresh message
        MessagingCenter.Subscribe<CreateSong>(this, "RefreshArtistList", (sender) =>
        {
            LoadData();
        });
        SongList.ItemsSource = _filteredSongs; // Assuming ListView ID is 'ArtistList'
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
        MessagingCenter.Unsubscribe<CreateSong>(this, "RefreshSongList");
    }
    public void LoadData()
    {
        try
        {
            using (var context = new StowTownDbContext())
            {
                var songList = context.Songs
                    .Where(s => s.IsDeleted !=true)
                    .OrderByDescending(s => s.Id)
                    .Select(s => new
                    {
                        Song = s,
                        ArtistName = context.ArtistGroups
                            .Where(a => a.Id == s.FkArtist)
                            .Select(a => a.Name)
                            .FirstOrDefault()
                    })
                    .ToList();

                var songViewList = new List<SongViewModel>();
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imagesDirectory = Path.Combine(exeDirectory, "assets", "SongImages");

                for (int i = 0; i < songList.Count; i++)
                {
                    var songData = songList[i];

                    songViewList.Add(new SongViewModel
                    {
                        SerialNumber = i + 1,
                        Id = songData.Song.Id,
                        Name = songData.Song.Name,
                        FkArtist = songData.Song.FkArtist,
                        IsDeleted = songData.Song.IsDeleted,
                        Duration = $"{songData.Song.Minutes} min {songData.Song.Seconds} sec",
                        ReleaseDate = songData.Song.ReleaseDate?.ToString("dd-MM-yyyy"), // Formatting date safely
                        CreatedAt = songData.Song.CreatedAt,
                        UpdatedAt = songData.Song.UpdatedAt,
                        // Image = !string.IsNullOrEmpty(songData.Song.Image) ? Path.Combine(imagesDirectory, songData.Song.Image) : null,
                        Image =  !string.IsNullOrEmpty(songData.Song.Image) ? ImageFilesService.GetImageUrl("SongsImages", songData.Song.Image):null,
                        ArtistName = songData.ArtistName // Fetched in the query
                    });
                }

                _allSongs = new ObservableCollection<SongViewModel>(songViewList);
                _filteredSongs = new ObservableCollection<SongViewModel>(_allSongs);
                _totalPages = (int)Math.Ceiling(_allSongs.Count / (double)PageSize);
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
        _filteredSongs.Clear();
        var pageItems = _allSongs
            .Skip((_currentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        foreach (var item in pageItems)
        {
            _filteredSongs.Add(item);
        }
        SongList.ItemsSource = _filteredSongs; // Always update ListView
        UpdatePaginationControls();
    }

    private void UpdatePaginationControls()
    {
        PreviousButton.IsEnabled = _currentPage > 1;
        NextButton.IsEnabled = _currentPage < _totalPages;
        PageInfo.Text = $"Page {_currentPage} of {_totalPages}";
         //NoResultsLabel.IsVisible = _filteredSongs.Count == 0;
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
        _searchText = e.NewTextValue?.ToLower().Trim(); // Handle null & trim spaces

        if (string.IsNullOrWhiteSpace(_searchText))
        {
            // Reset search -> Show all songs
            _filteredSongs = new ObservableCollection<SongViewModel>(_allSongs);
            PreviousButton.IsVisible = true;
            NextButton.IsVisible = true;
            PageInfo.IsVisible = true;
            IsEmptyPageInfo.IsVisible = false; // Hide empty page info when showing all songs
        }
        else
        {
            // Perform search
            var filteredList = _allSongs
                .Where(s => s.Name.ToLower().Contains(_searchText) ||
                            (!string.IsNullOrEmpty(s.ArtistName) && s.ArtistName.ToLower().Contains(_searchText))) // Search by Artist Name
                .ToList();

            _filteredSongs.Clear(); //  Ensure old data is cleared
            foreach (var item in filteredList)
            {
                _filteredSongs.Add(item);
            }
        }

        // Reset to first page
        _currentPage = 1;
        _totalPages = (int)Math.Ceiling(_filteredSongs.Count / (double)PageSize);

        if(_filteredSongs.Count == 0)
        {
            PreviousButton.IsVisible = false;
            NextButton.IsVisible = false;
            PageInfo.IsVisible = false;
            IsEmptyPageInfo.IsVisible = true;
            IsEmptyPageInfo.Text = "No Data Available";
            await  DisplayAlert("No Results", "No Songs found matching your search criteria", "Ok");
    
        }
        SongList.ItemsSource = null;
        SongList.ItemsSource = _filteredSongs;

        //  Load the current page (pagination)
        //LoadCurrentPage();

        // Show "No Results Found" message only when no results exist
      // NoResultsLabel.IsVisible = _filteredSongs.Count == 0;
    }




    private async void OnCreateClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CreateSong());
    }

    private async void OnViewButtonClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.CommandParameter is int selectedId)
        {
            await Shell.Current.GoToAsync($"{nameof(CreateSong)}?SelectedId={selectedId}&Type=View");
        }
    }

    private async void OnEditButtonClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.CommandParameter is int selectedId)
        {
            await Shell.Current.GoToAsync($"{nameof(CreateSong)}?SelectedId={selectedId}&Type=Edit");
        }

    }

    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        var song = (sender as ImageButton)?.BindingContext as SongViewModel;
        if (song != null)
        {
            var confirm = await DisplayAlert("Confirm Delete", "Are you sure you want to delete this DJ?", "Yes", "No");
            if (confirm)
            {
                using (var context = new StowTownDbContext())
                {
                    var getrowtodelete = context.Songs.Find(song.Id);
                    if (getrowtodelete != null)
                    {
                        var imageFileName = getrowtodelete.Image;
                        // If the station has an image, delete it from the server  
                        if (!string.IsNullOrEmpty(imageFileName))
                        {
                            // Delete the image file from the server  
                            bool isDeleted = ImageFilesService.DeleteFtpImage("SongsImages", imageFileName);
                            if (!isDeleted)
                            {
                               // await DisplayAlert("Error", "Failed to delete the image file from the server.", "OK");
                                return;
                            }
                        }

                        getrowtodelete.IsDeleted = true;
                        
                        context.SaveChanges();
                        await DisplayAlert("Success", "Song Deleted Successfully", "OK");
                        LoadData();
                    }
                }
            }
        }
    }


}