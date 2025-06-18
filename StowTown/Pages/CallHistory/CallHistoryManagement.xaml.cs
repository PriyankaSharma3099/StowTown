using StowTown.Models;
using StowTown.Services.SaveImageService;
using StowTown.ViewModels;
using System.Collections.ObjectModel;

namespace StowTown.Pages.CallHistory;

public partial class CallHistoryManagement : ContentPage
{
    private ObservableCollection<CallRecordViewModel> _allcallhisory;
    private ObservableCollection<CallRecordViewModel> _filteredcallhisory;
    private const int PageSize = 10;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private ObservableCollection<CallRecordViewModel> _filteredcall;  // New collection for filtered results
    private string _searchText = string.Empty;  // Store the search query


    public CallHistoryManagement()
	{
		InitializeComponent();
        _filteredcallhisory = new ObservableCollection<CallRecordViewModel>();
        _allcallhisory = new ObservableCollection<CallRecordViewModel>();

        LoadData();

        // Subscribe to refresh message
        MessagingCenter.Subscribe<CreateCallHistory>(this, "RefreshArtistList", (sender) =>
        {
            LoadData();
        });
        CallList.ItemsSource = _filteredcallhisory; // Assuming ListView ID is 'ArtistList'
        settingbutton();
        HomeDashboard.isInitializing = false;
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
        MessagingCenter.Unsubscribe<CreateCallHistory>(this, "RefreshSongList");
    }

    private async void  LoadData()
    {
        try
        {
            using (var context = new StowTownDbContext())
            {
                var callHisList = context.CallRecords
                    .Where(d => d.IsDeleted != true)
                    .OrderByDescending(d => d.Id)
                    .ToList();

                var callHistoryViewModelList = new List<CallRecordViewModel>();
                string parentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imagesDirectory = Path.Combine(parentDirectory, "assets", "Images", "RadioStation");

                for (int i = 0; i < callHisList.Count; i++)
                {
                    var callhis = callHisList[i];
                    var viewModel = new     CallRecordViewModel
                    {
                        Id = callhis.Id,
                        FkDj = callhis.FkDj,
                        FkRadioStation = callhis.FkRadioStation,
                        StartTime = callhis.StartTime,
                        EndTime = callhis.EndTime,
                        Notes = callhis.Notes,
                        Label = callhis.Label,
                        CreatedAt = callhis.CreatedAt,
                        UpdatedAt = callhis.UpdatedAt
                    };

                    var radioStation = context.RadioStations.FirstOrDefault(r => r.Id == callhis.FkRadioStation);
                    if (radioStation != null)
                    {
                        viewModel.RadioStationName = radioStation.Name;
                       // viewModel.Image = !string.IsNullOrEmpty(radioStation.Image) ? Path.Combine(imagesDirectory, radioStation.Image) : null;

                        viewModel.Image = !string.IsNullOrEmpty(radioStation.Image) ? ImageFilesService.GetImageUrl("RadioStationImages", radioStation.Image) : null;
                    }

                    var dj = context.Djs.FirstOrDefault(d => d.Id == callhis.FkDj);
                    if (dj != null)
                    {
                        viewModel.DjName = $"{dj.FirstName} {dj.LastName}";
                    }

                    callHistoryViewModelList.Add(viewModel);
                }

                // Assign serial numbers after sorting
                var sortedList = callHistoryViewModelList.OrderByDescending(a => a.Id).ToList();
                for (int i = 0; i < sortedList.Count; i++)
                {
                    sortedList[i].SerialNumber = i + 1;
                }

                _allcallhisory = new ObservableCollection<CallRecordViewModel>(sortedList);
                _filteredcallhisory = new ObservableCollection<CallRecordViewModel>(_allcallhisory);
                _totalPages = (int)Math.Ceiling(_allcallhisory.Count / (double)PageSize);
                LoadCurrentPage();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed : {ex.Message}", "OK");

        }
    }
    private void LoadCurrentPage()
    {
        _filteredcallhisory.Clear();
        var pageItems = _allcallhisory
            .Skip((_currentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        foreach (var item in pageItems)
        {
            _filteredcallhisory.Add(item);
        }
        CallList.ItemsSource = _filteredcallhisory; // Always update ListView
        UpdatePaginationControls();
    }

    private void UpdatePaginationControls()
    {
        PreviousButton.IsEnabled = _currentPage > 1;
        NextButton.IsEnabled = _currentPage < _totalPages;
        PageInfo.Text = $"Page {_currentPage} of {_totalPages}";
       // NoResultsLabel.IsVisible = _filteredcallhisory.Count == 0;
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
            _filteredcallhisory = new ObservableCollection<CallRecordViewModel>(_allcallhisory);
            PreviousButton.IsVisible = true;
            NextButton.IsVisible = true;
            IsEmptyPageInfo.IsVisible = false;
            PageInfo.IsVisible = true;
        }
        else
        {
            // Perform search
            var filteredList = _allcallhisory
                .Where(s => s.RadioStationName.ToLower().Contains(_searchText) || s.DjName.ToLower().Contains(_searchText)) // Search by Artist Name
                .ToList();

            _filteredcallhisory.Clear(); //  Ensure old data is cleared
            foreach (var item in filteredList)
            {
                _filteredcallhisory.Add(item);
            }
        }

        // Reset to first page
        _currentPage = 1;
        _totalPages = (int)Math.Ceiling(_filteredcallhisory.Count / (double)PageSize);
        if(_filteredcallhisory.Count == 0)
        {
            PreviousButton.IsVisible = false;
            NextButton.IsVisible = false;
            PageInfo.IsVisible = false;
            IsEmptyPageInfo.Text = "No Data Availabel";
            IsEmptyPageInfo.IsVisible = true;
            await DisplayAlert("No Results", "Not found matching your search criteria", "Ok");
          
        }

        // 
        CallList.ItemsSource = null;
        CallList.ItemsSource = _filteredcallhisory;

        //  Load the current page (pagination)
        //LoadCurrentPage();

        // Show "No Results Found" message only when no results exist
        //NoResultsLabel.IsVisible = _filteredcallhisory.Count == 0;
    }

    private async void OnViewButtonClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.CommandParameter is int selectedId)
        {
            //await Shell.Current.GoToAsync($"{nameof(CreateSong)}?SelectedId={selectedId}&Type=View");
            await Shell.Current.GoToAsync($"{nameof(CreateCallHistory)}?SelectedId={selectedId}&Type=View");
        }

    }

    private  async void OnEditButtonClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.CommandParameter is int selectedId)
        {
            //await Shell.Current.GoToAsync($"{nameof(CreateSong)}?SelectedId={selectedId}&Type=View");
            await Shell.Current.GoToAsync($"{nameof(CreateCallHistory)}?SelectedId={selectedId}&Type=Edit");
        }
    }

    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        bool isConfirmed = await Application.Current.MainPage.DisplayAlert(
            "Confirm Delete",
            "Are you sure you want to delete this item?",
            "Yes",
            "No");

        if (isConfirmed)
        {
            try
            {
                var button = sender as Button;
                var callRecrow = button?.BindingContext as CallRecordViewModel;

                if (callRecrow != null)
                {
                    using (var context = new StowTownDbContext())
                    {
                        var getrowtodelete = context.CallRecords.Find(Convert.ToInt32(callRecrow.Id));
                        if (getrowtodelete != null)
                        {
                            getrowtodelete.IsDeleted = true;
                            context.SaveChanges();

                            await Application.Current.MainPage.DisplayAlert("Success", "Call History Deleted Successfully.", "OK");
                            await Navigation.PopAsync(); // Make sure this reloads the UI list
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
