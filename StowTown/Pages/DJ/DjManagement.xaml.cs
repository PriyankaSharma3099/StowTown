using Microsoft.IdentityModel.Tokens;
using StowTown.Models;
using StowTown.Services.SaveImageService;
using StowTown.ViewModels;
using System.Collections.ObjectModel;

namespace StowTown.Pages.DJ;

public partial class DjManagement : ContentPage
{
    private ObservableCollection<DjViewModel> _djCollection = new ObservableCollection<DjViewModel>();
    private ObservableCollection<DjViewModel> _filteredCollection = new ObservableCollection<DjViewModel>();
    private int _currentPage = 1;
    private const int PageSize = 10; // Number of items per page
    private int _totalPages = 1;
    private List<DjViewModel> _allDjs = new List<DjViewModel>(); // Holds all DJ records


    public DjManagement()
	{
		InitializeComponent();
        LoadData();
        // Subscribe to the refresh message
        MessagingCenter.Subscribe<CreateDJ>(this, "RefreshList", (sender) =>
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
        MessagingCenter.Unsubscribe<CreateDJ>(this, "RefreshList");
    }
    private async void LoadData()
    {
        try
        {
            using (var context = new StowTownDbContext())
            {
                var djList = context.Djs.Where(d => d.IsDeleted != true).ToList();
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imagesDirectory = Path.Combine(exeDirectory, "Images", "DJ");

                var sortedDjList = djList
    .OrderByDescending(dj => dj.Id) //  Sorting first
    .ToList();

                var djViewModelList = sortedDjList
                    .Select((dj, index) => new DjViewModel
                    {
                        Id = dj.Id,
                        FirstName = dj.FirstName,
                        RadioStationName = context.RadioStations.FirstOrDefault(r => r.Id == dj.FkRadioStation)?.Name,
                        Email = dj.Email,
                        MobileNumber = dj.MobileNumber,
                     //   Image = GetImagePath(dj.Image) != null ? GetImagePath(dj.Image): null,
                         Image = dj.Image != null ? ImageFilesService.GetImageUrl("DjImages", dj.Image) : null,
                        SerialNumber = index + 1 // Assigning serial number after sorting
                    })
                    .ToList();

                //var djViewModelList = djList.Select((dj, index) => new DjViewModel
                //{
                //    Id = dj.Id,
                //    FirstName = dj.FirstName,
                //    RadioStationName = context.RadioStations.FirstOrDefault(r => r.Id == dj.FkRadioStation)?.Name,
                //    Email = dj.Email,
                //    MobileNumber = dj.MobileNumber,
                //    Image = dj.Image != null ? $"Images/DJ/{dj.Image}" : null,
                //    SerialNumber = index + 1
                //}).OrderByDescending(a => a.Id).ToList();

                _djCollection = new ObservableCollection<DjViewModel>(djViewModelList);
                DjCollectionView.ItemsSource = _djCollection;
                //await DisplayAlert("Count ", $"Total DJs: {_djCollection.Count}", "OK");

                _allDjs = djViewModelList; // Store all DJs for search and pagination

                _totalPages = (int)Math.Ceiling((double)_allDjs.Count / PageSize); // Calculate total pages

                _currentPage = 1; // Reset to first page when loading new data
                ApplyPagination();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnCreateDJ(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CreateDJ());
    }

    private async void OnViewDJ(object sender, EventArgs e)
    {
        var dj = (sender as ImageButton)?.BindingContext as DjViewModel;
        if (dj != null)
        {
            //await Navigation.PushAsync(new CreateDJ(dj.Id, "View"));
        }
    }

    private async void OnEditDJ(object sender, EventArgs e)
    {
        var dj = (sender as ImageButton)?.BindingContext as DjViewModel;
        if (dj != null)
        {
          //  await Navigation.PushAsync(new CreateDJPage(dj.Id, "Edit"));
        }
    }

    private async void OnDeleteDJ(object sender, EventArgs e)
    {
        var dj = (sender as ImageButton)?.BindingContext as DjViewModel;
        if (dj != null)
        {
            var confirm = await DisplayAlert("Confirm Delete", "Are you sure you want to delete this DJ?", "Yes", "No");
            if (confirm)
            {
                using (var context = new StowTownDbContext())
                {
                    var getrowtodelete = context.Djs.Find(dj.Id);
                    if (getrowtodelete != null)
                    {
                        var imageFileName = getrowtodelete.Image;
                        // If the station has an image, delete it from the server  
                        if (!string.IsNullOrEmpty(imageFileName))
                        {
                            // Delete the image file from the server  
                            bool isDeleted = ImageFilesService.DeleteFtpImage("DjImages", imageFileName);
                            if (!isDeleted)
                            {
                              //  await DisplayAlert("Error", "Failed to delete the image file from the server.", "OK");
                                return;
                            }
                        }
                        getrowtodelete.IsDeleted = true;
                        getrowtodelete.IsActive = false;
                        context.SaveChanges();
                        await DisplayAlert("Success", "DJ Deleted Successfully", "OK");
                        LoadData();
                    }
                }
            }
        }
    }


    private void UpdatePaginationControls()
    {
        PreviousButton.IsEnabled = _currentPage > 1;
        NextButton.IsEnabled = _currentPage < _totalPages;
        PageInfo.Text = $"Page {_currentPage} of {_totalPages}";
    }
    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string filter = SearchEntry.Text?.Trim().ToLower() ?? string.Empty;

        if (string.IsNullOrEmpty(filter))
        {
            _allDjs = _djCollection.ToList(); // Reset to full data
            PreviousButton.IsVisible = true;
            NextButton.IsVisible = true;
            PageInfo.IsVisible = true;
            IsEmptyPageInfo.Text =string.Empty;
            IsEmptyPageInfo.IsVisible = false;
        }
        else
        {
            _allDjs = _djCollection.Where(a =>
                a.FirstName.ToLower().Contains(filter) ||
                a.RadioStationName?.ToLower().Contains(filter) == true ||
                a.Email.ToLower().Contains(filter) ||
                a.MobileNumber.ToString().Contains(filter)).ToList();
            if(!_allDjs.Any())
            {
               await DisplayAlert("No Results", "No DJs found matching your search criteria.", "OK");
                PageInfo.Text = "No Data  Available";
                PreviousButton.IsVisible = false;
                NextButton.IsVisible = false;
                PageInfo.IsVisible = false;
                IsEmptyPageInfo.Text = "No DJ Available";
                IsEmptyPageInfo.IsVisible = true;

            }
        }

        _totalPages = (int)Math.Ceiling((double)_allDjs.Count / PageSize);
        _currentPage = 1; // Reset to first page when searching
        ApplyPagination();
    }

    private async  void OnViewButtonClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.CommandParameter is int selectedId)
        {
            await Shell.Current.GoToAsync($"{nameof(CreateDJ)}?SelectedId={selectedId}&Type=View");
        }
    }

    private async void OnEditButtonClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.CommandParameter is int selectedId)
        {
            await Shell.Current.GoToAsync($"{nameof(CreateDJ)}?SelectedId={selectedId}&Type=Edit");
        }
    }

    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        var dj = (sender as ImageButton)?.BindingContext as DjViewModel;
        if (dj != null)
        {
            var confirm = await DisplayAlert("Confirm Delete", "Are you sure you want to delete this DJ?", "Yes", "No");
            if (confirm)
            {
                using (var context = new StowTownDbContext())
                {
                    var getrowtodelete = context.Djs.Find(dj.Id);
                    if (getrowtodelete != null)
                    {
                        getrowtodelete.IsDeleted = true;
                        getrowtodelete.IsActive = false;
                        context.SaveChanges();
                        await DisplayAlert("Success", "DJ Deleted Successfully", "OK");
                        LoadData();
                    }
                }
            }
        }
    }

    private void PreviousButton_Clicked(object sender, EventArgs e)
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            ApplyPagination();
        }
    }

    private void NextButton_Clicked(object sender, EventArgs e)
    {
        if (_currentPage < _totalPages)
        {
            _currentPage++;
            ApplyPagination();
        }
    }


    private void ApplyPagination()
    {
        //var paginatedList = _djCollection.Skip((_currentPage - 1) * _itemsPerPage).Take(_itemsPerPage).ToList();
        //DjCollectionView.ItemsSource = new ObservableCollection<DjViewModel>(paginatedList);
        Device.BeginInvokeOnMainThread(() =>
        {
            _filteredCollection.Clear();

            var paginatedList = _allDjs
                .Skip((_currentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            foreach (var item in paginatedList)
            {
                _filteredCollection.Add(item);
            }

            DjCollectionView.ItemsSource = _filteredCollection;

            UpdatePaginationControls();
        });
    }

    private string GetDynamicImagePath()
    {
        string projectRoot = AppDomain.CurrentDomain.BaseDirectory;
        string relativePath = Path.Combine("assets", "DJ");
        string fullPath = Path.Combine(projectRoot, relativePath);

        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        return fullPath;
    }

    // Retrieve Image Path for UI
    private string GetImagePath(string imageName)
    {
        if (string.IsNullOrEmpty(imageName)) return null;

        string imagesDir = GetDynamicImagePath(); // assets/DJ folder
        string fullPath = Path.Combine(imagesDir, imageName);

        return File.Exists(fullPath) ? fullPath : null;
    }





}