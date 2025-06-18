using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using StowTown.Models;
using StowTown.Services.SaveImageService;
using StowTown.ViewModels;
using System.Collections.ObjectModel;

namespace StowTown.Pages.RadioStations;

public partial class RadioStationManagement : ContentPage
{
    private ObservableCollection<RadioStationViewModel> _allRadioStations;
    public ObservableCollection<RadioStationViewModel> _currentPageRadioStations { get; set; }
    private const int PageSize = 12;
    private int _currentPage = 1;
    private int _totalPages = 1;
   
    public RadioStationManagement()
    {
        InitializeComponent();
        _currentPageRadioStations = new ObservableCollection<RadioStationViewModel>();
        BindingContext = this;
        LoadData();

        // Subscribe to the refresh message
        MessagingCenter.Subscribe<CreateRadioStation>(this, "RefreshProducerList", (sender) =>
        {
            LoadData();
        });

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
        MessagingCenter.Unsubscribe<CreateRadioStation>(this, "RefreshProducerList");
    }

    public void LoadData()
    {
        try
            {
            using (var context = new StowTownDbContext())
            {
                var stations = context.RadioStations.Where(r => r.IsDeleted != true && r.IsDeleted !=null).ToList();
                var radioStations = new List<RadioStationViewModel>();

                if (stations.Count > 0)
                {
                    string parentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string imagesDirectory = System.IO.Path.Combine(parentDirectory, "assets", "Images", "RadioStation");

                    foreach (var station in stations)
                    {
                        var radio = new RadioStationViewModel
                        {
                            Id = station.Id,
                            Telephone = station.Telephone,
                            Name = station.Name,
                            MailingAddress = station.MailingAddress,
                            PhysicalAddress = station.PhysicalAddress,
                            MailCity = station.MailCity,
                            MailState = station.MailState,
                            MailZip = station.MailZip,
                            Code = station.Code,
                            Email = station.Email,
                            PhyCity = station.PhyCity,
                            PhyState = station.PhyState,
                            PhyZip = station.PhyZip,
                            Notes = station.Notes,
                            //Image = !string.IsNullOrEmpty(station.Image) 
                            //    ? System.IO.Path.Combine(imagesDirectory, station.Image) 
                            //    : "default_radio_station.png"
                            Image= string.IsNullOrEmpty(station.Image) ? "null" : ImageFilesService.GetImageUrl("RadioStationImages", station.Image)
                        };

                        //var dj = context.Djs.FirstOrDefault(d => d.FkRadioStation == radio.Id);
                        //if (dj != null)
                        //{
                        //    radio.DjName = $"{dj.FirstName} {dj.LastName}";
                        //}

                        radioStations.Add(radio);
                    }

                    // Sort radio stations in descending order by Id
                    var sortedRadioList = radioStations.OrderByDescending(a => a.Id).ToList();

                    //// Clear and repopulate _currentPageRadioStations
                    //_currentPageRadioStations.Clear();
                    //foreach (var station in sortedRadioList)
                    //{
                    //    _currentPageRadioStations.Add(station);
                    //}

                    //// Update UI on main thread
                    //MainThread.BeginInvokeOnMainThread(() =>
                    //{
                    //    RadioStationCollectionView.ItemsSource = _currentPageRadioStations;
                    //});
                    _allRadioStations = new ObservableCollection<RadioStationViewModel>(sortedRadioList);
                    _totalPages = (int)Math.Ceiling((double)_allRadioStations.Count / PageSize);
                    _currentPage = 1;

                    LoadCurrentPage();
                }
                else
                {
                    // Optional: Show a message when no radio stations are found
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        DisplayAlert("No Data", "No radio stations found.", "OK");
                    });
                }
            }
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DisplayAlert("Error", $"Error loading radio stations: {ex.Message}", "OK");
            });
        }
    }

    private void LoadCurrentPage()
    {
        //_currentPageRadioStations.Clear();
        //var pageItems = _currentPageRadioStations
        //    .Skip((_currentPage - 1) * PageSize)
        //    .Take(PageSize)
        //    .ToList();

        //foreach (var item in pageItems)
        //{
        //    _currentPageRadioStations.Add(item);
        //}
        //RadioStationCollectionView.ItemsSource = _currentPageRadioStations;
        //UpdatePaginationControls();
        _currentPageRadioStations.Clear();
        var pageItems = _allRadioStations
            .Skip((_currentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        foreach (var item in pageItems)
        {
            _currentPageRadioStations.Add(item);
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            RadioStationCollectionView.ItemsSource = null;
            RadioStationCollectionView.ItemsSource = _currentPageRadioStations;
            UpdatePaginationControls();
        });
    }

    private void UpdatePaginationControls()
    {
        PreviousButton.IsEnabled = _currentPage > 1;
        NextButton.IsEnabled = _currentPage < _totalPages;
        PageInfo.Text = $"Page {_currentPage} of {_totalPages}";
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
        string searchText = e.NewTextValue?.ToLower() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(searchText))
        {
            // Reset to first page and show all records
            _currentPage = 1;
            // LoadCurrentPage();
            LoadData();
           // IsDisplayItemNotFound.IsVisible = false;
            //IsDisplayItemNotFound.Text = "";
            PreviousButton.IsVisible = true;
            NextButton.IsVisible = true;
            PageInfo.IsVisible = true;



        }
        else
        {
            // Filter all records and update pagination
            var filteredResults = _currentPageRadioStations.Where(radio =>
                radio.Name.ToLower().Contains(searchText) 
                
            ).ToList();

            // Update serial numbers
            for (int i = 0; i < filteredResults.Count; i++)
            {
                filteredResults[i].Id = i + 1;
            }

            _currentPageRadioStations.Clear();
            var pageItems = filteredResults
                .Skip((_currentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            foreach (var item in pageItems)
            {
                _currentPageRadioStations.Add(item);
            }

            if(pageItems.Count == 0)
            {
                PreviousButton.IsVisible = false;
                NextButton.IsVisible = false;
                PageInfo.IsVisible = false;
                await DisplayAlert("No Results", "No radio stations found matching your search criteria.", "OK");
               // IsDisplayItemNotFound.IsVisible = true;
                //IsDisplayItemNotFound.Text = "No Data Available.";
                

               
            }
            else
            {
                //IsDisplayItemNotFound.IsVisible = false;
                //IsDisplayItemNotFound.Text = "";
            }
           
            _totalPages = (int)Math.Ceiling(filteredResults.Count / (double)PageSize);
            RadioStationCollectionView.ItemsSource = _currentPageRadioStations;
            UpdatePaginationControls();
           
        }
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        using (var context = new StowTownDbContext())
        {
            var navigationQuery = new Dictionary<string, object>
                {
                    { "SelectedId", 0 },
                    { "Type", "Create" }
                };
            await Shell.Current.GoToAsync(nameof(CreateRadioStation), navigationQuery);

        }
    }

    public async void OnManageClicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Manage Button Clicked"); // Add this    
        Button manageButton = sender as Button;
        var radioStation = manageButton?.BindingContext as RadioStationViewModel;

        if (radioStation == null)
        {
            await DisplayAlert("Error", "Could not find the associated radio station.", "OK");
            return;
        }

        // Log the details of the radio station being managed
        System.Diagnostics.Debug.WriteLine($"Manage Clicked - Radio Station: ID={radioStation.Id}, Name={radioStation.Name}");

        await Task.Delay(50); // Small delay (adjust as needed)

        await Dispatcher.DispatchAsync(async () =>
        {
            var action = await DisplayActionSheet(
                "Manage Radio Station",
                "Cancel",
                null,
                "View Details",
                "Edit Station",
                "Delete Station"
            );

            // Log the selected action
            System.Diagnostics.Debug.WriteLine($"Selected Action: {action}");

            // Handle the selected action
            if (action == "View Details")
            {
                // Use Shell navigation with query parameters
                var navigationQuery = new Dictionary<string, object>
                {
                    { "SelectedId", radioStation.Id },
                    { "Type", "View" }
                };

                System.Diagnostics.Debug.WriteLine($"Navigating to CreateRadioStation with ID: {radioStation.Id}, Type: View");
                
                await Shell.Current.GoToAsync(nameof(CreateRadioStation), navigationQuery);
            }
            else if (action == "Edit Station")
            {
                // Use Shell navigation with query parameters
                var navigationQuery = new Dictionary<string, object>
                {
                    { "SelectedId", radioStation.Id },
                    { "Type", "Edit" }
                };

                System.Diagnostics.Debug.WriteLine($"Navigating to CreateRadioStation with ID: {radioStation.Id}, Type: Edit");
                
                await Shell.Current.GoToAsync(nameof(CreateRadioStation), navigationQuery);
            }
            else if (action == "Delete Station")
            {
                await DeleteRadioStation(radioStation);
            }
        });
    }

    private async Task SaveRadioStationChanges(RadioStationViewModel radioStation)
    {
        try
        {
            using (var context = new StowTownDbContext())
            {
                var stationToUpdate = context.RadioStations.FirstOrDefault(r => r.Id == radioStation.Id);
                
                if (stationToUpdate != null)
                {
                    // Update fields as needed
                    stationToUpdate.Name = radioStation.Name;
                    stationToUpdate.Code = radioStation.Code;
                    stationToUpdate.Email = radioStation.Email;
                    stationToUpdate.Telephone = radioStation.Telephone;
                    stationToUpdate.MailingAddress = radioStation.MailingAddress;
                    stationToUpdate.PhysicalAddress = radioStation.PhysicalAddress;

                    context.SaveChanges();

                    // Refresh data
                    LoadData();

                    await DisplayAlert("Success", "Radio station updated successfully.", "OK");
                    
                    // Pop the edit page
                    await Navigation.PopAsync();
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not update radio station: {ex.Message}", "OK");
        }
    }

    private async Task DeleteRadioStation(RadioStationViewModel radioStation)
    {
        // Confirm deletion using MAUI's built-in DisplayAlert  
        bool confirm = await DisplayAlert("Confirm Delete",
            $"Are you sure you want to delete the radio station '{radioStation.Name}'?",
            "Yes", "No");

        if (confirm)
        {
            try
            {
                using (var context = new StowTownDbContext())
                {
                    // Find the radio station in the database  
                    var stationToDelete = context.RadioStations.FirstOrDefault(r => r.Id == radioStation.Id);

                    if (stationToDelete != null)
                    {
                        var imageFileName = stationToDelete.Image;
                        // If the station has an image, delete it from the server  
                        if (!string.IsNullOrEmpty(imageFileName))
                        {
                            // Delete the image file from the server  
                            bool isDeleted = ImageFilesService.DeleteFtpImage("RadioStationImages", imageFileName);
                            if (!isDeleted)
                            {
                               // await DisplayAlert("Error", "Failed to delete the image file from the server.", "OK");
                                return;
                            }
                        }

                        // Soft delete  
                        stationToDelete.IsDeleted = true;
                        context.SaveChanges();

                        // Refresh the data  
                        LoadData();

                        await DisplayAlert("Success", "Radio station deleted successfully.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not delete radio station: {ex.Message}", "OK");
            }
        }
    }
    private async void UploadFile_Clicked(object sender, EventArgs e)
    {
        try
        {
            var button = sender as Button;
            var radiorow = button.BindingContext as RadioStationViewModel;

            if (radiorow == null)
            {
                await DisplayAlert("Error", "Please select a radio station first.", "OK");
                return;
            }
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                                {
                                    { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } }, // .xlsx MIME type
                                    { DevicePlatform.iOS, new[] { "com.microsoft.excel.xlsx" } }, // UTI
                                    { DevicePlatform.macOS, new[] { "com.microsoft.excel.xlsx" } },
                                    { DevicePlatform.WinUI, new[] { ".xlsx" } } // File extension for Windows
                                });


            var fileResult = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select a Worksheet",
                FileTypes = customFileType
            });

            if (fileResult != null)
            {
                SaveWorksheetData(radiorow.Id, fileResult.FullPath);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
   
    private async void SaveWorksheetData(int radioStationId, string filePath)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets[0];

            int rowCount = 0;

            // Count meaningful rows
            for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
            {
                bool isRowNonEmpty = false;

                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, col]?.Text))
                    {
                        isRowNonEmpty = true;
                        break;
                    }
                }

                if (isRowNonEmpty)
                {
                    rowCount = row;
                }
            }

            //var getCell = worksheet.Cells["A3"];
            //var getId =0;

            //if (getCell.Value is double doubleId)
            //{
            //    getId = Convert.ToInt32(doubleId);
            //}


            var getCell = worksheet.Cells["A3"];
            var cellText = getCell.Text;

            if (int.TryParse(cellText, out int getId))
            {


                if (getId == radioStationId)
                {
                    using (var context = new StowTownDbContext())
                    {
                        var songPositions = new List<SongPossition>();

                        for (int row = 5; row <= rowCount; row++)
                        {
                            var songName = worksheet.Cells[row, 1]?.Text ?? string.Empty;
                            var artistName = worksheet.Cells[row, 2]?.Text ?? string.Empty;
                            var duration = worksheet.Cells[row, 3]?.Text ?? string.Empty;
                            var date = worksheet.Cells[row, 4]?.Text ?? string.Empty;

                            var song = context.Songs.FirstOrDefault(s => s.Name == songName);

                            if (song != null)
                            {
                                var position = new SongPossition
                                {
                                    FkSong = song.Id,
                                    FkMonthlySongList = int.TryParse(worksheet.Cells[row, 8]?.Text, out var monthlyId) ? monthlyId : 0,
                                    FkRadioStation = radioStationId,
                                   // Spins = int.TryParse(worksheet.Cells[row, 5]?.Text, out var spins) ? spins : (int?)null,
                                    //Possition = int.TryParse(worksheet.Cells[row, 6]?.Text, out var pos) ? pos : (int?)null,
                                    //RotationNotes = worksheet.Cells[row, 7]?.Text ?? string.Empty
                                     Spins = int.TryParse(worksheet.Cells[row, 4]?.Text, out var spins) ? spins : (int?)null,
                                    Possition = int.TryParse(worksheet.Cells[row, 5]?.Text, out var pos) ? pos : (int?)null,
                                    RotationNotes = worksheet.Cells[row, 6]?.Text ?? string.Empty
                                };

                                // Delete existing records
                                var existingRecords = context.SongPossitions
                                    .Where(sp => sp.FkSong == position.FkSong &&
                                                 sp.FkMonthlySongList == position.FkMonthlySongList &&
                                                 sp.FkRadioStation == position.FkRadioStation)
                                    .ToList();

                                if (existingRecords.Any())
                                {
                                    context.SongPossitions.RemoveRange(existingRecords);
                                }

                                songPositions.Add(position);
                            }
                            else
                            {
                                await MainThread.InvokeOnMainThreadAsync(async () =>
                                {
                                    await Application.Current.MainPage.DisplayAlert("Error", $"Song '{songName}' not found.", "OK");
                                });
                                return;
                            }
                        }

                        if (songPositions.Count > 0)
                        {
                            context.SongPossitions.AddRange(songPositions);
                            context.SaveChanges();

                            await MainThread.InvokeOnMainThreadAsync(async () =>
                            {
                                await Application.Current.MainPage.DisplayAlert("Success", "Records uploaded successfully.", "OK");
                            });
                        }
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Wrong File", "You selected the wrong file.", "OK");
                }
            }
            else
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Wrong File", "You selected the wrong file.", "OK");
                });
            }
        }
    }

    private void OnUploadFileTapped(object sender, TappedEventArgs e)
    {

    }

    private async void OnTestClick(object sender, EventArgs e)
    {
        await DisplayAlert("Debug", "Test button clicked inside CollectionView", "OK");
    }
}