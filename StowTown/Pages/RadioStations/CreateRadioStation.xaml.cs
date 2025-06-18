using StowTown.ViewModels;
using StowTown.Services;
using Microsoft.EntityFrameworkCore;
using StowTown.Models;
using System.Reflection.Emit;
using static System.Collections.Specialized.BitVector32;
using StowTown.Services.SaveImageService;
using System.ComponentModel;

namespace StowTown.Pages.RadioStations;

public partial class CreateRadioStation : ContentPage,IQueryAttributable,INotifyPropertyChanged
{
   
    private RadioStationViewModel _radioStationViewModel;
    private string _actionType;
    public int Id { get; set; }
    public string Type { get; set; }
    private string SelectedImagePath { get; set; }
    private string ImageName { get; set; }

    private bool _canEditIsActive = true;

    public bool CanEditIsActive
    {
        get => _canEditIsActive;
        set
        {
            if (_canEditIsActive != value)
            {
                _canEditIsActive = value;
                OnPropertyChanged(nameof(CanEditIsActive));
            }
        }
    }
    public CreateRadioStation()
    {
        InitializeComponent();
   
        SetupImageTapGesture();
    }
    private void SetupImageTapGesture()
    {
        var tapGestureRecognizer = new TapGestureRecognizer();
        tapGestureRecognizer.Tapped += async (s, e) => await PickImage();
        ImagePickerGrid.GestureRecognizers.Add(tapGestureRecognizer);

        // Make sure the ImagePickerGrid is initially set up with the default content
        //SetDefaultImagePickerContent(); // Call this method to set the default view
    }

    private async Task PickImage()
    {
        try
        {
            var result = await Application.Current.MainPage.DisplayActionSheet("Select Image Source", "Cancel", null, "Camera", "Gallery");

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
                    //  await ProcessImage(photo);

                        var imageName = await ImageFilesService.UploadImageToServer(photo, "RadioStationImages");
                        if (!String.IsNullOrEmpty(imageName))
                        {
                            var imagePath = ImageFilesService.GetImageUrl("RadioStationImages", imageName);
                            imagePicker.Source = imagePath;
                            ImageName = imageName;

                        }
 
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to pick image: {ex.Message}", "OK");
        }
    }

    private async Task ProcessImage(FileResult photo)
    {
        try
        {
            //string baseDirectory = @"C:\Users\joshi\source\repos\StowTown\StowTown\StowTown\assets\Images"; // Correct path
            //string radioStationDirectory = Path.Combine(baseDirectory, "RadioStation");

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Set the correct images directory inside the project assets folder
            string radioStationDirectory = Path.Combine(baseDirectory, "assets", "Images", "RadioStation");


            // 2. Create the directories if they don't exist
            if (!Directory.Exists(baseDirectory))
            {
                Directory.CreateDirectory(baseDirectory);
            }
            if (!Directory.Exists(radioStationDirectory))
            {
                Directory.CreateDirectory(radioStationDirectory);
            }



            // 3. Generate a unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
            var newFile = Path.Combine(radioStationDirectory, fileName);

            // 4. Copy the file
            using (var stream = await photo.OpenReadAsync())
            using (var newStream = File.OpenWrite(newFile))
            {
                await stream.CopyToAsync(newStream);
            }

            SelectedImagePath = newFile;
            ImageName = fileName;

            await UpdateImageDisplay(newFile);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to process image: {ex.Message}", "OK");
        }
    }

    private async Task UpdateImageDisplay(string imagePath)
    {
        try
        {
            //ImagePickerGrid.Children.Clear(); // Clear existing content

            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                //var image = new Image
                //{
                //    Source = ImageSource.FromFile(imagePath),
                //    Aspect = Aspect.AspectFit,
                //    HeightRequest = 200,
                //    WidthRequest = 250
                //};
                imagePicker.Source = ImageSource.FromFile(imagePath); // Update the image source
                //ImagePickerGrid.Children.Add(image);
            }
            else
            {
               // SetDefaultImagePickerContent(); // Reset to default view
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to display image: {ex.Message}", "OK");
        }
    }


    private void SetDefaultImagePickerContent()
    {
        ImagePickerGrid.Children.Clear(); // Clear anything that's currently there

        var defaultStack = new VerticalStackLayout // Use VerticalStackLayout consistently
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };

        defaultStack.Children.Add(new Image
        {
            Source = "imagepicker.png",
            HeightRequest = 40,
            WidthRequest = 40,
            Aspect = Aspect.AspectFit
        });

        //defaultStack.Children.Add(new Label // Now this Label will be added
        //{
        //    Text = "NO THUMB",
        //    TextColor = Color.FromArgb("#666666"),
        //    HorizontalOptions = LayoutOptions.Center,
        //    Margin = new Thickness(0, 10, 0, 0)
        //});

        
        ImagePickerGrid.Children.Add(defaultStack);
    }


    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        // Log all received query parameters
        System.Diagnostics.Debug.WriteLine("ApplyQueryAttributes called with parameters:");
        foreach (var param in query)
        {
            System.Diagnostics.Debug.WriteLine($"Key: {param.Key}, Value: {param.Value}");
        }

        // Extract SelectedId
        if (query.TryGetValue("SelectedId", out var id) && id != null)
        {
            if (int.TryParse(id.ToString(), out int parsedId))
            {
                Id = parsedId;
                System.Diagnostics.Debug.WriteLine($"Parsed SelectedId: {Id}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Failed to parse SelectedId: {id}");
                Id = 0;
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("No SelectedId found in query parameters");
            Id = 0;
        }

        // Extract Type
        if (query.TryGetValue("Type", out var typeObj) && typeObj != null)
        {
            ActionType = typeObj.ToString();
            System.Diagnostics.Debug.WriteLine($"ActionType set to: {ActionType}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("No Type found in query parameters");
            ActionType = "Create";
        }

        // Load radio station data
        LoadRadioStationData();
    }

    private async void LoadRadioStationData()
    {
        try
        {
           
            if (Id > 0)
            {
                // Load existing radio station
                System.Diagnostics.Debug.WriteLine($"Attempting to load radio station with ID: {Id}");
                await LoadRadioStation(Id);
                
                // Additional logging to verify data
                if (_radioStationViewModel != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Loaded Radio Station - Name: {_radioStationViewModel.Name}, Code: {_radioStationViewModel.Code}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Radio Station ViewModel is null after loading");
                }
            }
            else
            {
                // Create new radio station
                System.Diagnostics.Debug.WriteLine("Creating new Radio Station ViewModel");
                _radioStationViewModel = new RadioStationViewModel();
            }

            // Bind the view model
            BindingContext = _radioStationViewModel;

            // Update UI based on action type
            UpdateUI();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in LoadRadioStationData: {ex}");
            await DisplayAlert("Error", $"Failed to load radio station: {ex.Message}", "OK");
            _radioStationViewModel = new RadioStationViewModel();
            BindingContext = _radioStationViewModel;
        }
    }

    private async Task LoadRadioStation(int selectedId)
    {
        try 
        {
            using (var context = new StowTownDbContext())
            {
                System.Diagnostics.Debug.WriteLine($"Querying database for radio station with ID: {selectedId}");
                var radioStation = await context.RadioStations.FirstOrDefaultAsync(r => r.Id == selectedId && r.IsDeleted==false);
                

                if (radioStation != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Radio station found - Name: {radioStation.Name}");


                    //string projectRoot = AppDomain.CurrentDomain.BaseDirectory;


                    //// Now, set the correct Images directory path
                    //string imagesDirectory = Path.Combine(projectRoot, "assets", "Images", "RadioStation");

                    //// Construct the full image path dynamically
                    //string imagePath = !string.IsNullOrEmpty(radioStation.Image)
                    //    ? Path.Combine(imagesDirectory, radioStation.Image)
                    //    : Path.Combine(imagesDirectory, "default_radio_station.png");

                    //// Ensure the file exists, otherwise use the default image
                    //if (!File.Exists(imagePath))
                    //{
                    //    System.Diagnostics.Debug.WriteLine($"Image not found at: {imagePath}, using default.");
                    //    imagePath = Path.Combine(imagesDirectory, "default_radio_station.png");
                    //}

                    _radioStationViewModel = new RadioStationViewModel
                    {
                        Id = radioStation.Id,
                        Name = radioStation.Name,
                        Code = radioStation.Code,
                        Email = radioStation.Email,
                        Telephone = radioStation.Telephone,
                        MailingAddress = radioStation.MailingAddress,
                        PhysicalAddress = radioStation.PhysicalAddress,
                        PhyCity = radioStation.PhyCity,
                        PhyState = radioStation.PhyState,
                        PhyZip = radioStation.PhyZip,
                        MailCity = radioStation.MailCity,
                        MailState = radioStation.MailState,
                        MailZip = radioStation.MailZip,
                        Notes= radioStation.Notes,
                        Image= string.IsNullOrEmpty(radioStation?.Image) ? "null"  : ImageFilesService.GetImageUrl("RadioStationImages", radioStation.Image),
                        IsActive=radioStation.IsActive
                    };
                    // imagePicker.Source =radioStation.Image != null ? ImageSource.FromFile(imagePath) : ImageSource.FromFile("default_radio_station.png");
                    imagePicker.Source = string.IsNullOrEmpty(radioStation?.Image) ? "null" : ImageFilesService.GetImageUrl("RadioStationImages", radioStation.Image);

                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"No radio station found with ID: {selectedId}");
                    await DisplayAlert("Error", "Radio Station not found.", "OK");
                    _radioStationViewModel = new RadioStationViewModel();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in LoadRadioStation: {ex}");
            await DisplayAlert("Error", $"Failed to load radio station: {ex.Message}", "OK");
            _radioStationViewModel = new RadioStationViewModel();
        }
    }

    private void UpdateUI()
    {
        // Configure UI based on action type
        switch (ActionType)
        {
            case "View":
                SetFieldsReadOnly(true);
              //  SaveButton.IsEnabled = false; // Disable save button in View mode
                //SaveButton.Text = "View";
                SaveButton.IsVisible = false;
                HeaderName.Text = "View Radio Station";
                ImagePickerGrid.IsEnabled = false;
                IsActiveEntry.IsEnabled=true;
                CanEditIsActive = false;
                break;
            case "Edit":
                SetFieldsReadOnly(false);
                HeaderName.Text = "Update Radio Station";
                SaveButton.IsEnabled = true;
                SaveButton.Text = "Update";
                break;
            case "Create":
                SetFieldsReadOnly(false);
                HeaderName.Text = "Create Radio Station";
                SaveButton.IsEnabled = true;
                SaveButton.Text = "Create";
                break;
            default:
                
                break;
        }
    }

    private void SetFieldsReadOnly(bool isReadOnly)
    {
        // Ensure fields are read-only when in View mode
        NameEntry.IsReadOnly = isReadOnly;
        CodeEntry.IsReadOnly = isReadOnly;
        EmailEntry.IsReadOnly = isReadOnly;
        TelephoneEntry.IsReadOnly = isReadOnly;
        MailingAddressEntry.IsReadOnly = isReadOnly;
        PhysicalAddressEntry.IsReadOnly = isReadOnly;
        PhysicalCityEntry.IsReadOnly = isReadOnly;
        PhysicalStateEntry.IsReadOnly = isReadOnly;
        PhysicalZipEntry.IsReadOnly = isReadOnly;
        MailingCityEntry.IsReadOnly = isReadOnly;
        MailingStateEntry.IsReadOnly = isReadOnly;
        MailingZipEntry.IsReadOnly = isReadOnly;
        NotesEntry.IsReadOnly = isReadOnly;
        


        // Disable input interactions when in View mode
        NameEntry.IsEnabled = !isReadOnly;
        CodeEntry.IsEnabled = !isReadOnly;
        EmailEntry.IsEnabled = !isReadOnly;
        TelephoneEntry.IsEnabled = !isReadOnly;
        MailingAddressEntry.IsEnabled = !isReadOnly;
        PhysicalAddressEntry.IsEnabled = !isReadOnly;
        PhysicalCityEntry.IsEnabled = !isReadOnly;
        PhysicalStateEntry.IsEnabled = !isReadOnly;
        PhysicalZipEntry.IsEnabled = !isReadOnly;
        MailingCityEntry.IsEnabled = !isReadOnly;
        MailingStateEntry.IsEnabled = !isReadOnly;
        MailingZipEntry.IsEnabled = !isReadOnly;
        NotesEntry.IsEnabled = !isReadOnly;
    }

    private string ActionType
    {
        get => _actionType;
        set
        {
            _actionType = value;
            UpdateUI();
        }
    }


    private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            try
            {
                using (var context = new StowTownDbContext())
                {
                    if (_actionType == "Edit" && _radioStationViewModel.Id > 0)
                    {
                        // Update existing radio station
                        var existingStation = await context.RadioStations
                            .FirstOrDefaultAsync(r => r.Id == _radioStationViewModel.Id && r.IsDeleted== false );
                    //// Check if another station has the same email
                    //var emailExists = await context.RadioStations
                    //    .AnyAsync(r => r.Email == _radioStationViewModel.Email && r.Id != _radioStationViewModel.Id);

                    //if (emailExists)
                    //{
                    //    await DisplayAlert("Error", "Email already exists. Please use a unique email.", "OK");
                    //    return;
                    //}


                    if (existingStation != null)
                    {
                      var oldImageName = existingStation.Image; // Store old image name for deletion if needed
                        existingStation.Name = _radioStationViewModel.Name;
                            existingStation.Code = _radioStationViewModel.Code;
                            existingStation.Email = _radioStationViewModel.Email;
                            existingStation.Telephone = _radioStationViewModel.Telephone;
                           
                            existingStation.PhysicalAddress = _radioStationViewModel.PhysicalAddress;
                            existingStation.PhyCity   = _radioStationViewModel.PhyCity;
                            existingStation.PhyState = _radioStationViewModel.PhyState;
                            existingStation.PhyZip = _radioStationViewModel.PhyZip;

                        
                            existingStation.MailingAddress = _radioStationViewModel.MailingAddress;
                            existingStation.MailCity = _radioStationViewModel.MailCity;
                            existingStation.MailState = _radioStationViewModel.MailState;
                            existingStation.MailZip = _radioStationViewModel.MailZip;
                            existingStation.Notes = _radioStationViewModel.Notes;
                            existingStation.UpdatedAt = DateTime.UtcNow;

                        existingStation.Image = ImageName??existingStation.Image;
                        existingStation.IsActive = _radioStationViewModel.IsActive;
                        existingStation.IsDeleted = false;

                        var emailExists = await context.RadioStations
                     .AnyAsync(r => r.Email == existingStation.Email && r.Id != existingStation.Id);

                        if (emailExists)
                        {
                            await DisplayAlert("Error", "Email already exists. Please use a unique email.", "OK");
                            return;
                        }
                        // If new image name is different, delete old image
                        if (!string.IsNullOrEmpty(ImageName) && oldImageName != ImageName)
                        {

                            if (ImageFilesService.DeleteFtpImage("RadioStationImages", oldImageName))
                            {
                                // Handle success case if needed
                                Console.WriteLine("Image deleted successfully.");
                            }
                            else
                            {
                                // Handle failure case if needed
                               // await Application.Current.MainPage.DisplayAlert("Error", "Failed to delete the image.", "OK");
                            }

                        }
                        await context.SaveChangesAsync();
                            await DisplayAlert("Success", "Radio Station updated successfully.", "OK");
                        }
                    }
                    else
                    {
                    var emailExists = await context.RadioStations
                        .AnyAsync(r => r.Email == _radioStationViewModel.Email);

                    if (emailExists)
                    {
                        await DisplayAlert("Error", "Email already exists. Please use a unique email.", "OK");
                        return;
                    }
                    // Create new radio station
                    var newStation = new RadioStation
                        {
                            Name = _radioStationViewModel.Name,
                            Code = _radioStationViewModel.Code,
                            Email = _radioStationViewModel.Email,
                            Telephone = _radioStationViewModel.Telephone,
                            MailingAddress = _radioStationViewModel.MailingAddress,
                            PhysicalAddress = _radioStationViewModel.PhysicalAddress,
                            PhyCity = _radioStationViewModel.PhyCity,
                            PhyState = _radioStationViewModel.PhyState,
                            PhyZip = _radioStationViewModel.PhyZip,
                            MailCity = _radioStationViewModel.MailCity,
                            MailState = _radioStationViewModel.MailState,
                            MailZip = _radioStationViewModel.MailZip,
                            Notes = _radioStationViewModel.Notes,
                            Image=ImageName,
                            CreatedAt = DateTime.UtcNow,

                            UpdatedAt = DateTime.UtcNow,
                            IsActive=true,
                            IsDeleted=false
                        };

                        context.RadioStations.Add(newStation);
                        await context.SaveChangesAsync();
                        await DisplayAlert("Success", "Radio Station created successfully.", "OK");
                    }

                    // Navigate back to the management page
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to save radio station: {ex.Message}", "OK");
            }
        }

        private bool ValidateInputs()
        {
            // Add your validation logic here
            if (string.IsNullOrWhiteSpace(_radioStationViewModel.Name))
            {
                DisplayAlert("Validation Error", "Please enter a name for the radio station.", "OK");
                return false ;
            }

       

        // Validate Email
        if (string.IsNullOrWhiteSpace(_radioStationViewModel.Email) || !IsValidEmail(EmailEntry.Text))
        {
            DisplayAlert("Validation Error", "Please enter a valid email address.","Ok");
            return false;
        }

        // Validate Telephone (you can add a more specific regex if needed)
        if (string.IsNullOrWhiteSpace(_radioStationViewModel.Telephone))
        {
            DisplayAlert("Validation Error", "Please enter station telephone number.","Ok");
            return false;
        }

        // Mobile number format validation (simple digits-only check, 10–15 digits)
        var mobilePattern = @"^\d{10,15}$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(_radioStationViewModel.Telephone, mobilePattern))
        {
            DisplayAlert("Error", "Invalid mobile number. Only digits allowed (10–15 characters).", "Ok");
            return false;
        }
        //// Validate Station Code
        if (string.IsNullOrWhiteSpace(_radioStationViewModel.Code))
        {
            DisplayAlert("Validation Error", "Please enter a station code.","Ok");
            return false;
        }

        //// Validate Mailing Address (optional, based on your requirements)
        if (string.IsNullOrWhiteSpace(_radioStationViewModel.MailZip) || MailingZipEntry.Text.Length > 10)
        {
            DisplayAlert("Validation Error", "Please enter mail zip code and zip code length should be more than 10 or equal 5.","Ok");
            return false;
        }

        //// Validate Physical Address (optional, based on your requirements)
        if (string.IsNullOrWhiteSpace(_radioStationViewModel.PhyZip) || PhysicalZipEntry.Text.Length > 10)
        {
            DisplayAlert("Validation Error", "Please enter physical zip code and zip code length should be more than 10 or equal 5.","Ok");
            return false;
        }

        // Add more validation as needed
        return true;
        }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private void OnlyNumericTextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = sender as Entry;

        // Only allow numeric input by checking if the new text is a valid number
        if (!string.IsNullOrEmpty(entry.Text) && !entry.Text.All(char.IsDigit))
        {
            // If it's not a valid number, clear the invalid input or give feedback
            entry.Text = e.OldTextValue;
            DisplayAlert("Invalid Input", "Please enter only numeric values", "OK");
        }
    }
    private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

    private void OnBackClicked(object sender, EventArgs e)
    {

    }

    private void SelectImage_Clicked(object sender, EventArgs e)
    {

    }

    private void SelectImageButton_Clicked(object sender, EventArgs e)
    {

    }
}
