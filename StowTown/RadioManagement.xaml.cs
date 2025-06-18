using OfficeOpenXml;
using StowTown.Models;
using StowTown.ViewModels;
using System.Collections.ObjectModel;

namespace StowTown;

public partial class RadioManagement : ContentPage
{
    private ObservableCollection<ViewModels.RadioStationViewModel> _radiostationCollection; // Original data
    private ObservableCollection<ViewModels.RadioStationViewModel> _filteredCollection; // Filtered data

    public RadioManagement()
	{
        InitializeComponent();
        BindRadio();
        //nameTextBox.Focus();
    }

    protected async void BindRadio()
    {
        try
        {
            // Use async and await to handle database access
            await Task.Run(() =>
            {
                using (var context = new StowTownDbContext())
                {
                    var stations = context.RadioStations.Where(r => r.IsDeleted != true).ToList();
                    List<ViewModels.RadioStationViewModel> radioStations = new List<ViewModels.RadioStationViewModel>();

                    if (stations.Count > 0)
                    {
                        string parentDirectory = FileSystem.Current.AppDataDirectory; // Replace AppDomain.BaseDirectory for MAUI
                        string imagesDirectory = System.IO.Path.Combine(parentDirectory, "Images", "RadioStation");

                        foreach (var station in stations)
                        {
                            var radio = new ViewModels.RadioStationViewModel
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
                                Image = station.Image != null ? System.IO.Path.Combine(imagesDirectory, station.Image) : null // Use a placeholder image or default path
                            };

                            // Fetch DJ information
                            //var dj = context.Djs.FirstOrDefault(d => d.FkRadioStation == radio.Id);
                            //if (dj != null)
                            //{
                            //    radio.DjName = $"{dj.FirstName} {dj.LastName}";
                            //}

                            radioStations.Add(radio);
                        }

                        // Sort the list in descending order by Id
                        var sortedRadioList = radioStations.OrderByDescending(a => a.Id).ToList();

                        // Bind to ObservableCollection for UI
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            _radiostationCollection = new ObservableCollection<ViewModels.RadioStationViewModel>(sortedRadioList);
                            //itemsControl.ItemsSource = _radiostationCollection; // Ensure 'itemsControl' is your CollectionView
                        });
                    }
                }
            });
        }
        catch (Exception ex)
        {
            // Use appropriate error handling for MAUI
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void UploadFile_Click(object sender, EventArgs e)
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

            var fileResult = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select a Worksheet",
                FileTypes = FilePickerFileType.Pdf
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

    private void SaveWorksheetData(int radioStationId, string filePath)
    {
        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension.End.Row;

            if (worksheet.Cells[3, 1]?.Value is double radioId && (int)radioId == radioStationId)
            {
                using (var context = new StowTownDbContext())
                {
                    var songPositions = new List<SongPossition>();

                    for (int row = 5; row <= rowCount; row++)
                    {
                        var songName = worksheet.Cells[row, 3]?.Text;
                        if (!string.IsNullOrEmpty(songName))
                        {
                            var song = context.Songs.FirstOrDefault(s => s.Name == songName);

                            if (song != null)
                            {
                                songPositions.Add(new SongPossition
                                {
                                    FkSong = song.Id,
                                    FkRadioStation = radioStationId
                                    //SongName = songName,
                                    // Add other necessary fields
                                });
                            }
                        }
                    }

                    if (songPositions.Count > 0)
                    {
                        context.SongPossitions.AddRange(songPositions);
                        context.SaveChanges();
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            DisplayAlert("Success", "Records uploaded successfully.", "OK");
                        });
                    }
                }
            }
        }
    }

    private void ManageButton_Click(object sender, EventArgs e)
    {

        if(sender is not Button button)
        return;

        // Create a MenuFlyout
        var menuFlyout = new MenuFlyout();

        // Add the 'View' menu item
        var viewItem = new MenuFlyoutItem { Text = "View" };
        viewItem.Clicked += (s, args) => View_Clicked(button, args);
        menuFlyout.Add(viewItem);

        // Add the 'Edit' menu item
        var editItem = new MenuFlyoutItem { Text = "Edit" };
        editItem.Clicked += (s, args) => Edit_Clicked(button, args);
        menuFlyout.Add(editItem);

        // Add the 'Delete' menu item
        var deleteItem = new MenuFlyoutItem { Text = "Delete" };
        deleteItem.Clicked += (s, args) => Delete_Clicked(button, args);
        menuFlyout.Add(deleteItem);

        // Assign the MenuFlyout to the button
        //button.ContextFlyout = menuFlyout;
    }

    private  async void Delete_Clicked(Button button, EventArgs e)
    {
        bool result = await Application.Current.MainPage.DisplayAlert(
       "Confirm Delete",
       "Are you sure you want to delete this item?",
       "Yes",
       "No");

        if (result)
        {
            try
            {
               // var button = sender as Button;
                var radiorow = button?.BindingContext as RadioStationViewModel;
                if (radiorow != null)
                {
                    using (var context = new StowTownDbContext())
                    {
                        var station = context.RadioStations.Find(radiorow.Id);
                        if (station != null)
                        {
                            station.IsDeleted = true;
                            station.IsActive = false;
                            context.SaveChanges();

                            await Application.Current.MainPage.DisplayAlert("Success", "Radio Station Deleted Successfully.", "OK");
                            BindRadio();
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

    private  async void Edit_Clicked(Button button, EventArgs e)
    {
         try
        {
            //var button = sender as Button;
            var radiorow = button?.BindingContext as RadioStationViewModel;
            if (radiorow != null)
            {
               // await Navigation.PushAsync(new CreateRadioStationPage(radiorow.Id, "Edit"));
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void View_Clicked(Button button, EventArgs e)
    {
        try
        {
           // var button = sender as Button;
            var radiorow = button?.BindingContext as RadioStationViewModel;
            if (radiorow != null)
            {
               // await Navigation.PushAsync(new CreateRadioStationPage(radiorow.Id, "View"));
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void OnSearchTextChanged(object sender, Microsoft.Maui.Controls.TextChangedEventArgs e)
    {
        string filter = e.NewTextValue?.Trim().ToLower();

        if (string.IsNullOrEmpty(filter))
        {
            //itemsControl.ItemsSource = _radiostationCollection;
        }
        else
        {
            _filteredCollection = new ObservableCollection<RadioStationViewModel>(_radiostationCollection?.Where(a =>
                (a.Code?.ToLower().Contains(filter) ?? false) 
                //||
                //(a.DjName?.ToLower().Contains(filter) ?? false))
                ));

           // itemsControl.ItemsSource = _filteredCollection;
        }

    }

    private void CreateRadio_Click(object sender, EventArgs e)
    {

    }
}