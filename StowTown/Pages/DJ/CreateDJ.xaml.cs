using Microsoft.EntityFrameworkCore;
using Microsoft.Maui;
using NSubstitute.Core;
using StowTown.Models;
using StowTown.Pages.RadioStations;
using StowTown.Services.SaveImageService;
using StowTown.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace StowTown.Pages.DJ;

public partial class CreateDJ : ContentPage, IQueryAttributable,INotifyPropertyChanged
{
    private int? DjId { get; set; }
    private string Type { get; set; }
    private string SelectedImagePath { get; set; }

    private ObservableCollection<RadioStationViewModel> _radioStations;
    private ObservableCollection<RadioStationViewModel> RadioStations { get; set; } = new ObservableCollection<RadioStationViewModel>();

    private RadioStationViewModel _selectedRadioStation;

    public event PropertyChangedEventHandler? PropertyChanged;

    public RadioStationViewModel SelectedRadioStation
    {
        get => _selectedRadioStation;
        set
        {
            if (_selectedRadioStation != value)
            {
                _selectedRadioStation = value;
                OnPropertyChanged(nameof(SelectedRadioStation));
            }
        }
    }

    public void OnPropertyChanged(string propertyName)
    {
        if (Application.Current?.Dispatcher != null)
        {
            Application.Current.Dispatcher.Dispatch(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }
        else
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    private string ImageName;
    public CreateDJ()
    {
        InitializeComponent();
        //SetupImageTapGesture();

        this.BindingContext = this; // Set in constructor or OnAppearing


    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        BindRadioStation(); // Load data when page appears
    }


    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        ClearForm();

        if (query.TryGetValue("SelectedId", out var id) && id != null && int.TryParse(id.ToString(), out int parsedId))
        {
            DjId = parsedId;
        }
        else
        {
            DjId = 0;
        }

        Type = query.TryGetValue("Type", out var typeObj) && typeObj != null ? typeObj.ToString() : "Create";
        UpdateUI();
    }


    private void ClearForm()
    {
        DjId = 0;
        Type = "Create";

        FName_txt.Text = string.Empty;
        LName_txt.Text = string.Empty;
        DOB_txt.Date = DateTime.Now;
        Contact_txt.Text = string.Empty;
        Email_txt.Text = string.Empty;
        Personal_txt.Text = string.Empty;
        SelectedImagePath = null;
        EnableControls();
    }

    private void EnableControls()
    {
        FName_txt.IsEnabled = true;
        LName_txt.IsEnabled = true;
        RadioStation_DD.IsEnabled = true;
        Email_txt.IsEnabled = true;
        Contact_txt.IsEnabled = true;
        Personal_txt.IsEnabled = true;
        DOB_txt.IsEnabled = true;
    }

    private void UpdateUI()
    {
        if (DjId == 0 || Type == "Create")
        {
            HeaderName.Text = "Create DJ";
            SaveButton.Text = "Save";
            EnableControls();
            ClearForm();
        }
        else if (DjId > 0 && Type == "Edit")
        {
            HeaderName.Text = "Update DJ";
            SaveButton.Text = "Update";
            EnableControls();
            BindRadioStation();
            BindDJ(DjId.Value);
        }
        else if (DjId > 0 && Type == "View")
        {
            HeaderName.Text = "View DJ";
            BindRadioStation();
            BindDJ(DjId.Value);
            DisableControls();
            SaveButton.IsVisible = false;  // Hide the SaveButton
        }
    }

    private async void BindDJ(int djId)
    {
        using (var context = new StowTownDbContext())
        {

           
            var dj = await context.Djs.Where(d=>d.Id ==djId && d.IsDeleted ==false ).FirstOrDefaultAsync();
            if (dj != null)
            {
                FName_txt.Text = dj.FirstName;
                LName_txt.Text = dj.LastName;
                // RadioStation_DD.SelectedItem = dj.FkRadioStation.HasValue ? dj.FkRadioStation.Value : 0;

                if (dj.FkRadioStation.HasValue && _radioStations != null)
                {
                    var selectedStation = _radioStations
                        .FirstOrDefault(s => s.Id == dj.FkRadioStation.Value);

                    RadioStation_DD.SelectedItem = selectedStation;
                }
                Email_txt.Text = dj.Email;
                Contact_txt.Text = dj.MobileNumber;
                Personal_txt.Text = dj.PersonalData;
                if (DateTime.TryParse(dj.Birthday, out DateTime birthday))
                {
                    DOB_txt.Date = birthday;
                }
                Spouse_txt.Text=dj.Spouse;
                TShirt_Size_txt.Text = dj.TshirtSize;
                MailAdd_txt.Text = dj.MailingAddress;
                MailCity_txt.Text = dj.MailCity;
                MailState_txt.Text = dj.MailState;
                MailZip_txt.Text = dj.MailZip;
                PhyAdd_txt.Text = dj.PhysicalAddress;
                PhyCity_txt.Text = dj.PhyCity;
                PhyState_txt.Text = dj.PhyState;
                ZipCode_txt.Text = dj.PhyZipCode;
                Children_txt.Text = dj.Children;
                PrayerReq_txt.Text = dj.PrayerRequest;
                Notes_txt.Text = dj.NotesOfCall;
                IsActive_Chb.IsChecked = dj.IsActive ?? false;
                HomeAdd_txt.Text = dj.HomeAddress;
                HomeCity_txt.Text = dj.HomeCity;
                HomeState_txt.Text = dj.HomeState;
                HomeZipCode_txt.Text = dj.HomeZipCode;
                //SelectedImagePath = dj.Image;

                //SelectedImagePath = GetImagePath(dj.Image); // Convert filename to full path
                //await UpdateImageDisplay(SelectedImagePath); // Update UI
                SelectedImageView.Source = dj.Image != null ? ImageFilesService.GetImageUrl("DjImages", dj.Image) : null;

            }
        }
    }

    private void DisableControls()
    {
        FName_txt.IsEnabled = false;
        LName_txt.IsEnabled = false;
        RadioStation_DD.IsEnabled = false;
        Email_txt.IsEnabled = false;
        Contact_txt.IsEnabled = false;
        Personal_txt.IsEnabled = false;
        DOB_txt.IsEnabled = false;
        Spouse_txt.IsEnabled = false;
        TShirt_Size_txt.IsEnabled = false;
        MailAdd_txt.IsEnabled = false;
        MailCity_txt.IsEnabled = false;
        MailState_txt.IsEnabled = false;
        MailZip_txt.IsEnabled = false;
        PhyAdd_txt.IsEnabled = false;
        PhyCity_txt.IsEnabled = false;
        PhyState_txt.IsEnabled = false;
        ZipCode_txt.IsEnabled = false;
        Children_txt.IsEnabled = false;
        PrayerReq_txt.IsEnabled = false;
        Notes_txt.IsEnabled = false;
        HomeAdd_txt.IsEnabled = false;
        HomeCity_txt.IsEnabled = false;
        HomeState_txt.IsEnabled = false;
        HomeZipCode_txt.IsEnabled = false;

    }


    //private async void BindRadioStation()
    //{
    //    try
    //    {
    //        List<RadioStation> radioStations = new List<RadioStation>();

    //        using (var context = new StowTownDbContext()) // Ensure DbContext is properly instantiated
    //        {
    //            // Fetch all selected radio station IDs from DJs
    //            var selectedRadioStationIds = await context.Djs
    //                .Where(d => d.IsDeleted == false && d.FkRadioStation != null)
    //                .Select(d => d.FkRadioStation)
    //                .ToListAsync();

    //            if (DjId == 0 || DjId == null) // Creating a new DJ
    //            {
    //                // Fetch all available radio stations (only unselected ones)
    //                radioStations = await context.RadioStations
    //                    .Where(s => s.IsDeleted == false && !selectedRadioStationIds.Contains(s.Id))
    //                    .ToListAsync();
    //            }
    //            else // Editing an existing DJ
    //            {
    //                // Get the current DJ's selected radio station ID
    //                var currentDjRadioStationId = await context.Djs
    //                    .Where(d => d.Id == DjId && d.IsDeleted == false)
    //                    .Select(d => d.FkRadioStation)
    //                    .FirstOrDefaultAsync();

    //                // Fetch all radio stations (including unselected ones and the currently assigned station)
    //                //radioStations = await context.RadioStations
    //                //    .Where(s => s.IsDeleted == false &&
    //                //                (!selectedRadioStationIds.Contains(s.Id) || s.Id == currentDjRadioStationId))
    //                //    .ToListAsync();
    //                //RadioStation_DD.ItemsSource = radioStations;
    //                if (currentDjRadioStationId != null)
    //                {
    //                    radioStations = await context.RadioStations
    //                        .Where(s => s.IsDeleted == false &&
    //                                    (!selectedRadioStationIds.Contains(s.Id) || s.Id == currentDjRadioStationId.Value))
    //                        .ToListAsync();
    //                }
    //                else
    //                {
    //                    radioStations = await context.RadioStations
    //                        .Where(s => s.IsDeleted == false && !selectedRadioStationIds.Contains(s.Id))
    //                        .ToListAsync();
    //                }
    //            }
    //        }

    //        //// **Ensure UI Binding Happens in the Main Thread**
    //        Device.BeginInvokeOnMainThread(() =>
    //        {
    //            RadioStations.Clear();
    //            foreach (var station in radioStations)
    //            {
    //                RadioStations.Add(station);
    //            }

    //            // Bind the updated list to the Picker
    //            RadioStation_DD.ItemsSource = RadioStations;
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        await DisplayAlert("Error", "Failed to load radio stations: " + ex.Message, "OK");
    //    }
    //}

    //private async void BindRadioStation()
    //{
    //    try
    //    {
    //        using (var context = new StowTownDbContext())
    //        {
    //            var selectedRadioStationIds = await context.Djs
    //                .Where(d => d.IsDeleted != true)
    //                .Select(d => d.FkRadioStation)
    //                .ToListAsync();

    //            //List<RadioStationViewModel> radioStations;

    //            //if (DjId == 0 || DjId == null)
    //            //{
    //            //    radioStations = await context.RadioStations
    //            //        .Where(s => s.IsDeleted != true && !selectedRadioStationIds.Contains(s.Id))
    //            //        .ToListAsync();
    //            //}
    //            //else
    //            //{
    //            //    var currentDjRadioStationId = await context.Djs
    //            //        .Where(d => d.Id == DjId && d.IsDeleted != true)
    //            //        .Select(d => d.FkRadioStation)
    //            //        .FirstOrDefaultAsync();

    //            //    radioStations = await context.RadioStations
    //            //        .Where(s => s.IsDeleted != true &&
    //            //                    (!selectedRadioStationIds.Contains(s.Id) || s.Id == currentDjRadioStationId))
    //            //        .ToListAsync();
    //            //}

    //            //RadioStation_DD.ItemsSource = new ObservableCollection<RadioStationViewModel>(radioStations);



    //            List<RadioStationViewModel> radioStations;
    //            int? currentDjRadioStationId = null;

    //            if (DjId != null && DjId != 0)
    //            {
    //                currentDjRadioStationId = await context.Djs
    //                    .Where(d => d.Id == DjId && d.IsDeleted !=true)
    //                    .Select(d => d.FkRadioStation)
    //                    .FirstOrDefaultAsync();
    //            }

    //            radioStations = await context.RadioStations
    //                .Where(s => s.IsDeleted !=true &&
    //                            (!selectedRadioStationIds.Contains(s.Id) || s.Id == currentDjRadioStationId))
    //                .Select(s => new RadioStationViewModel
    //                {
    //                    Id = s.Id,
    //                    Name = s.Name,
    //                    // Map other necessary properties from `s` to your view model
    //                })
    //                .ToListAsync();

    //            RadioStation_DD.ItemsSource = new ObservableCollection<RadioStationViewModel>(radioStations);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        await DisplayAlert("Error", ex.Message, "OK");
    //    }
    //}


    private async Task BindRadioStation()
    {
        using (var context = new StowTownDbContext())
        {
            var stations = await context.RadioStations
                .Where(s => s.IsDeleted !=true && s.IsActive == true)
                .Select(s => new RadioStationViewModel
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .ToListAsync();

            _radioStations = new ObservableCollection<RadioStationViewModel>(stations);
            RadioStation_DD.ItemsSource = _radioStations;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            using (var context = new StowTownDbContext())
            {
                Dj dj = DjId == 0 ? new Dj { CreatedAt = DateTime.Now, IsActive = true, IsDeleted = false } : await context.Djs.FindAsync(DjId);

                if (dj == null) return;

                // Required fields

                //ValidInputs();
                if (! await ValidInputs())
                    return;
                var oldImageName = dj.Image; // Store old image name for deletion if needed
                dj.FirstName = FName_txt.Text;
                dj.LastName = LName_txt.Text;
                //dj.FkRadioStation = (RadioStation_DD.SelectedItem as RadioStation)?.Id;
                dj.FkRadioStation = (RadioStation_DD.SelectedItem as RadioStationViewModel)?.Id;
                dj.Email = Email_txt.Text;
                dj.MobileNumber = Contact_txt.Text;
                dj.PersonalData = Personal_txt.Text;
                dj.Birthday = DOB_txt.Date.ToString("yyyy-MM-dd");
                dj.UpdatedAt = DateTime.Now;

                  dj.Spouse= Spouse_txt.Text ;
                 dj.TshirtSize= TShirt_Size_txt.Text ;
                 dj.MailingAddress= MailAdd_txt.Text ;
                dj.MailCity= MailCity_txt.Text;
                 dj.MailState= MailState_txt.Text;
                 dj.MailZip= MailZip_txt.Text;
                 dj.PhysicalAddress= PhyAdd_txt.Text ;
                 dj.PhyCity= PhyCity_txt.Text ;
                 dj.PhyState= PhyState_txt.Text ;
                 dj.PhyZipCode= ZipCode_txt.Text ;
                 dj.Children= Children_txt.Text;
                dj.PrayerRequest = PrayerReq_txt.Text ;
                 dj.NotesOfCall=Notes_txt.Text ;
                dj.Image = ImageName ?? dj.Image;
                dj.IsActive = IsActive_Chb.IsChecked;
                dj.IsDeleted = false;
                dj.HomeAddress = HomeAdd_txt.Text;
                dj.HomeCity = HomeCity_txt.Text;
                dj.HomeState = HomeState_txt.Text;
                dj.HomeZipCode = HomeZipCode_txt.Text;
                // If new image name is different, delete old image
                if (!string.IsNullOrEmpty(ImageName) && oldImageName != ImageName)
                {

                    if (ImageFilesService.DeleteFtpImage("DjImages", oldImageName))
                    {
                        // Handle success case if needed
                        Console.WriteLine("Image deleted successfully.");
                    }
                    else
                    {
                        // Handle failure case if needed
                        //await Application.Current.MainPage.DisplayAlert("Error", "Failed to delete the image.", "OK");
                    }

                }
                if (DjId == 0) context.Djs.Add(dj);
                await context.SaveChangesAsync();

                await DisplayAlert("Success", DjId == 0 ? "DJ created successfully!" : "DJ updated successfully!", "OK");
                await Navigation.PopAsync();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }

    }

    private void OnBackClicked(object sender, EventArgs e)
    {

    }

    private void OnImagePickerClicked(object sender, EventArgs e)
    {

    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {

    }

    private  async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }  // Handle Image Tap Gesture (Popup for Camera/Gallery)
    private async void OnPickImageClicked(object sender, EventArgs e)
    {
        await PickImage();
    }

    // Pick Image from Camera or Gallery
    private async Task PickImage()
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() => { });


            var result = await DisplayActionSheet("Select Image Source", "Cancel", null, "Camera", "Gallery");

            if (result == "Camera")
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    var photo = await MediaPicker.Default.CapturePhotoAsync();
                    if (photo != null)
                    {
                        await ProcessImage(photo);
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Camera is not supported on this device", "OK");
                }
            }
            else if (result == "Gallery")
            {
                var photo = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Select an Image",
                    FileTypes = FilePickerFileType.Images
                });

                if (photo != null)
                {
                    // await ProcessImage(photo);
                  
                        var imageName = await ImageFilesService.UploadImageToServer(photo, "DjImages");
                        if (!String.IsNullOrEmpty(imageName))
                        {
                            var imagePath = ImageFilesService.GetImageUrl("DjImages", imageName);
                            SelectedImageView.Source = imagePath;
                            ImageName = imageName;
                        NoImageLabel.IsVisible = false; // Hide "NO THUMB" label
                    }

                    
                }
            }
         
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to pick image: {ex.Message}", "OK");
        }
    }

    // Process & Save Image
    private async Task ProcessImage(FileResult photo)
    {
        try
        {
            string imagesDir = GetDynamicImagePath();
            if (!Directory.Exists(imagesDir))
            {
                Directory.CreateDirectory(imagesDir);
            }

            // Generate a unique filename
            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
            string newFilePath = Path.Combine(imagesDir, fileName);

            // Copy file to the project directory
            using (var stream = await photo.OpenReadAsync())
            using (var newStream = File.OpenWrite(newFilePath))
            {
                await stream.CopyToAsync(newStream);
            }

            // Store image path
            SelectedImagePath = newFilePath;
            ImageName = fileName;

            await UpdateImageDisplay(newFilePath);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to process image: {ex.Message}", "OK");
        }
    }

    // Get Dynamic Image Path (Inside assets/DJ)
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

    // Update UI with Selected Image
    private async Task UpdateImageDisplay(string imagePath)
    {
        try
        {
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                SelectedImageView.Source = ImageSource.FromFile(imagePath); // Update Image Source
                NoImageLabel.IsVisible = false; // Hide "NO THUMB" label
            }
            else
            {
                SelectedImageView.Source = "imagepicker.png"; // Default image
                NoImageLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to display image: {ex.Message}", "OK");
        }
    }


    private async void SelectImageButton_Clicked(object sender, EventArgs e)
    {
        await PickImage();
    }

    private void OnSelectedRadioStation(object sender, EventArgs e)
    {

    }

    private void OnlyNumericTextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = sender as Entry;

        // Only allow numeric input by checking if the new text is a valid number
        if (!string.IsNullOrEmpty(entry.Text) && !entry.Text.All(char.IsDigit))
        {
            // If it's not a valid number, clear the invalid input or give feedback
            entry.Text = e.OldTextValue;
            DisplayAlert("Invalid Input", "Please enter only numeric values ", "OK");
        }
    }

    public async Task<bool> ValidInputs()
    {
        if (string.IsNullOrWhiteSpace(FName_txt.Text))
        {
            await DisplayAlert("Error", "First name is required.", "Ok");
            return false;
        }
        if (string.IsNullOrWhiteSpace(Email_txt.Text))
        {
            await DisplayAlert("Error", "Email is required.", "Ok");
            return false;
        }
        // Email validation  
        if (!string.IsNullOrWhiteSpace(Email_txt.Text) && !Regex.IsMatch(Email_txt.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            await DisplayAlert("Error", "Invalid Email format.", "Ok");
            return false;
        }

        if (RadioStation_DD.SelectedItem == null)
        {
            await DisplayAlert("Error", "Please select a radio station.", "Ok");
            return false;
        }

        if (!Regex.IsMatch(Contact_txt.Text ?? "", @"^\+?\d{10,15}$"))
        {
            await DisplayAlert("Error", "Enter a valid Contact Details (10-15 digits, optional +).", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(ZipCode_txt.Text))
        {
            await DisplayAlert("Error", "Zip code is required.", "Ok");
            return false;
        }

        if (string.IsNullOrWhiteSpace(MailZip_txt.Text))
        {
            await DisplayAlert("Error", "Mail zip code is required.", "Ok");
            return false;
        }

        if (DOB_txt.Date >= DateTime.Today)
        {
            await DisplayAlert("Error", "Please enter a valid date of birth that is not in the future.", "OK");
            return false;
        }

        if (MailZip_txt.Text?.Length >10 )
        {
            await DisplayAlert("Error", "Please enter mail zip code and zip code length should be more than 10 or equal 5", "Ok");
            return false;
        }
        if (ZipCode_txt.Text?.Length > 10)
        {
            await DisplayAlert("Error", "Please enter Physical Zip code and Physical Zip code length should be more than 10 or equal 5.", "Ok");
            return false;
        }
        if(HomeZipCode_txt.Text?.Length >10)
        {
            await DisplayAlert("Error", "Please enter Home Zip code and Home Zip code length should be more than 10 or equal 5.", "Ok");
            return false;
        }

        return true;
    }
}


