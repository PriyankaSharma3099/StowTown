using Microsoft.Maui.Controls;
using System.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using StowTown.Models;
using Microsoft.EntityFrameworkCore;
using StowTown.Pages.RadioStations;
using Microsoft.Maui.Controls.Internals;
using StowTown.Services.SaveImageService;

namespace StowTown;

public partial class CreateProjectProducer : ContentPage, IQueryAttributable
{
    public static int ProducerId { get; set; }
    public static string Type { get; set; }
    private string SelectedImagePath { get; set; }
    private string ImageName { get; set; }

    ContentView mainContent;
    ContentPage mainPage;
    public CreateProjectProducer()
    {
        try
        {


            InitializeComponent();
            DOB_txt.Date = DateTime.Now;
            SetupImageTapGesture();
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error initializing CreateProjectProducer: {ex.Message}");
            // Optionally, you can display an alert or log the error
            Application.Current.MainPage.DisplayAlert("Error", "Failed to initialize the page. Please try again.", "OK");

            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StowTownLogs");
            Directory.CreateDirectory(folder);

            string filePath = Path.Combine(folder, $"startup-error-{DateTime.Now:yyyyMMdd-HHmmss}.log");

            // Check if file exists
            if (File.Exists(filePath))
            {
                File.AppendAllText(filePath, "Issue producer page" + Environment.NewLine + ex);
            }
            else
            {
                File.WriteAllText(filePath, "Issue producer page " + Environment.NewLine+ex);
            }
        }

    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateUI();
    }
    //public CreateProjectProducer(int producerId, string type, Dashboard dashboard)
    //{
    //    InitializeComponent();

    //    ProducerId = producerId;
    //    Type = type;
    //    DOB_txt.Date = DateTime.Now;
    //    SetupImageTapGesture();
    //    UpdateUI();
    //    mainPage = dashboard;
    //}


    private void SetupImageTapGesture()
    {
        var tapGestureRecognizer = new TapGestureRecognizer();
        tapGestureRecognizer.Tapped += async (s, e) => await PickImage();
        ImagePickerGrid.GestureRecognizers.Add(tapGestureRecognizer);
    }

    public View GetViewContent()
    {
        return this.Content;
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
                    await Application.Current.MainPage.DisplayAlert("Error", "Camera is not supported on this device", "OK");
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
                  var imageName= await ImageFilesService.UploadImageToServer(photo, "ProducerImages");
                    if(!String.IsNullOrEmpty(imageName))
                    {
                         var imagePath=ImageFilesService.GetImageUrl("ProducerImages", imageName);
                        SelectedImageView.Source = imagePath;
                       ImageName = imageName;

                    }

                }
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to pick image: {ex.Message}", "OK");
        }
    }

    private async Task ProcessImage(FileResult photo)
    {
        try
        {
            // Create the images directory if it doesn't exist
            var imagesDir = Path.Combine(FileSystem.AppDataDirectory, "ProducerImages");
            if (!Directory.Exists(imagesDir))
            {
                Directory.CreateDirectory(imagesDir);
            }

            // Generate a unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
            var newFile = Path.Combine(imagesDir, fileName);

            // Copy the file
            using (var stream = await photo.OpenReadAsync())
            using (var newStream = File.OpenWrite(newFile))
            {
                await stream.CopyToAsync(newStream);
            }

            // Update UI and store path
            SelectedImagePath = newFile;
            ImageName = fileName;

            await UpdateImageDisplay(newFile);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to process image: {ex.Message}", "OK");
        }
    }

    private string GetImagePath(string imageName)
    {
        if (string.IsNullOrEmpty(imageName)) return null;
        
        var imagesDir = Path.Combine(FileSystem.AppDataDirectory, "ProducerImages");
        // Create directory if it doesn't exist
        if (!Directory.Exists(imagesDir))
        {
            Directory.CreateDirectory(imagesDir);
        }
        
        var fullPath = Path.Combine(imagesDir, imageName);
        return File.Exists(fullPath) ? fullPath : null;
    }

    private async Task UpdateImageDisplay(string imagePath)
    {
        try
        {
            //if (!string.IsNullOrEmpty(imagePath))
            //{
            //    var image = new Image
            //    {
            //        Source = ImageSource.FromFile(imagePath),
            //        Aspect = Aspect.AspectFit,
            //        HeightRequest = 200,
            //        WidthRequest = 250
            //    };

            //    // Clear existing content and add new image
            //    ImagePickerGrid.Children.Clear();
            //    ImagePickerGrid.Children.Add(image);
            //}
            //else
            //{
            //    // Reset to default state if no image
            //    ImagePickerGrid.Children.Clear();
            //    var defaultStack = new StackLayout
            //    {
            //        VerticalOptions = LayoutOptions.Center,
            //        HorizontalOptions = LayoutOptions.Center
            //    };

            //    defaultStack.Children.Add(new Image
            //    {
            //        Source = "imagepicker.png",
            //        HeightRequest = 40,
            //        WidthRequest = 40,
            //        Aspect = Aspect.AspectFit
            //    });

            //    defaultStack.Children.Add(new Label
            //    {
            //        Text = "NO THUMB",
            //        TextColor = Color.FromArgb("#666666"),
            //        HorizontalOptions = LayoutOptions.Center,
            //        Margin = new Thickness(0, 10, 0, 0)
            //    });

            //    ImagePickerGrid.Children.Add(defaultStack);
            //}
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to display image: {ex.Message}", "OK");
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        // Clear all fields first
       // ClearForm();

        if (query.TryGetValue("SelectedId", out var id) && id != null)
        {
            if (int.TryParse(id.ToString(), out int parsedId))
            {
                ProducerId = parsedId;
            }
        }
        else
        {
            ProducerId = 0;
        }
        
        if (query.TryGetValue("Type", out var typeObj) && typeObj != null)
        {
            Type = typeObj.ToString();
        }
        else
        {
            Type = "Create";
        }

        UpdateUI();
    }

    private void ClearForm()
    {
        ProducerId = 0;
        Type = "Create";
        
        ProducerName.Text = string.Empty;
        DOB_txt.Date = DateTime.Now;
        MobileNo.Text = string.Empty;
        Addreess.Text = string.Empty;
        City.Text = string.Empty;
        State.Text = string.Empty;
        ZipCode.Text = string.Empty;
        Email.Text = string.Empty;
        ImageName = null;
        SelectedImagePath = null;
        
        // Reset image to default state
        //ImagePickerGrid.Children.Clear();
        //var defaultStack = new StackLayout
        //{
        //    VerticalOptions = LayoutOptions.Center,
        //    HorizontalOptions = LayoutOptions.Center
        //};

        //defaultStack.Children.Add(new Image
        //{
        //    Source = "imagepicker.png",
        //    HeightRequest = 40,
        //    WidthRequest = 40,
        //    Aspect = Aspect.AspectFit
        //});

        //defaultStack.Children.Add(new Label
        //{
        //    Text = "NO THUMB",
        //    TextColor = Color.FromArgb("#666666"),
        //    HorizontalOptions = LayoutOptions.Center,
        //    Margin = new Thickness(0, 10, 0, 0)
        //});

        //ImagePickerGrid.Children.Add(defaultStack);

        // Enable all controls
        EnableControls();
    }

    private void EnableControls()
    {
        ProducerName.IsEnabled = true;
        DOB_txt.IsEnabled = true;
        MobileNo.IsEnabled = true;
        Addreess.IsEnabled = true;
        City.IsEnabled = true;
        State.IsEnabled = true;
        ZipCode.IsEnabled = true;
        Email.IsEnabled = true;
        //ImagePickerGrid.IsEnabled = true;
    }

    private void UpdateUI()
    {
        if (ProducerId == 0 || Type == "Create")
        {
            HeaderName.Text = "Create Project Producer";
            EnableControls();
            // Ensure form is clear for Create
            ClearForm();
            SaveButton.Text = "Save";
        }
        else if (ProducerId > 0 && Type == "Edit")
        {
            HeaderName.Text = "Update Project Producer";
            EnableControls();
            BindProducer(ProducerId);
            SaveButton.Text = "Update";
        }
        else if (ProducerId > 0 && Type == "View")
        {
            HeaderName.Text = "View Project Producer";
            BindProducer(ProducerId);
            DisableControls();
            SaveButton.IsVisible = false;
        }
    }

    private async void BindProducer(int producerId)
    {
        try
        {
            using (var context = new StowTownDbContext())
            {
                var producer = await context.ProjectProducers
                    .FirstOrDefaultAsync(p => p.Id == producerId && p.IsDeleted==false );

                if (producer != null)
                {
                    ProducerName.Text = producer.ProducerName;
                    if (producer.Dob.HasValue)
                    {
                        DOB_txt.Date = producer.Dob.Value;
                    }
                    MobileNo.Text = producer.MobileNo;
                    Addreess.Text = producer.Address;
                    City.Text = producer.City;
                    State.Text = producer.State;
                    ZipCode.Text = producer.ZipCode.ToString();
                    Email.Text = producer.Email;

                    //if (!string.IsNullOrEmpty(producer.ProducerImage))
                    //{
                    //    ImageName = producer.ProducerImage;
                    //    var imagePath = GetImagePath(producer.ProducerImage);
                    //    if (imagePath != null)
                    //    {
                    //        SelectedImagePath = imagePath;
                    // await UpdateImageDisplay(imagePath);
                    //    }
                    //}
                    if (!string.IsNullOrEmpty(producer.ProducerImage))
                    {
                        var imagePath = ImageFilesService.GetImageUrl("ProducerImages", producer.ProducerImage);
                        if (!string.IsNullOrEmpty(imagePath))
                        {
                           
                           // SelectedImageView.Source = ImageSource.FromUri(new Uri("https://wca.microlent.com/StowTown/assets/ProducerImages/d3799356-20ab-419b-ab69-e4326184c50b.jpg"));
                            Console.WriteLine("");
                            SelectedImageView.Source = ImageSource.FromUri(new Uri(imagePath));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Failed to load producer details: " + ex.Message, "OK");
        }
    }

    private void DisableControls()
    {
        ProducerName.IsEnabled = false;
        DOB_txt.IsEnabled = false;
        MobileNo.IsEnabled = false;
        Addreess.IsEnabled = false;
        City.IsEnabled = false;
        State.IsEnabled = false;
        ZipCode.IsEnabled = false;
        Email.IsEnabled = false;
        ImagePickerGrid.IsEnabled = false;
    }

    private async void SaveButton_Clicked(object sender, EventArgs e)
    {
        // Validate inputs before proceeding
        if (!await ValidateInputs())
        {
            return;
        }

        try
        {
            // Check if ProducerId is set (for update)
            if (ProducerId == null || ProducerId == 0)
            {
                // Adding a new producer
                var producer = new ProjectProducer
                {
                    ProducerName = ProducerName.Text,
                    MobileNo = MobileNo.Text,
                    Email = Email.Text,
                    Address = Addreess.Text,
                    City = City.Text,
                    State = State.Text,
                    ZipCode = string.IsNullOrWhiteSpace(ZipCode.Text) ? (int?)null : int.Parse(ZipCode.Text),
                    Dob = DOB_txt.Date,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    ProducerImage = ImageName,
                    IsDeleted = false
                };

                // Check if email already exists
                using (var context = new StowTownDbContext())
                {
                    var getEmail = context.ProjectProducers.Select(e => e.Email).ToList();
                    if (getEmail.Contains(Email.Text))
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "This Email Already Exists. Please Enter a Unique Email.", "OK");
                        Email.Focus();
                        return;
                    }

                    context.ProjectProducers.Add(producer);
                    await context.SaveChangesAsync();

                    // Copy image if selected
                    if (!string.IsNullOrEmpty(SelectedImagePath) && !string.IsNullOrEmpty(ImageName))
                    {
                        string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                        string targetDirectory = Path.Combine(exeDirectory, "Images", "Producer");

                        if (!Directory.Exists(targetDirectory))
                        {
                            Directory.CreateDirectory(targetDirectory);
                        }

                        string targetPath = Path.Combine(targetDirectory, ImageName);
                        File.Copy(SelectedImagePath, targetPath, true);
                    }

                    await Application.Current.MainPage.DisplayAlert("Success", "Producer created successfully!", "OK");
                }
            }
            else
            {
                // Update existing producer
                    using (var context = new StowTownDbContext())
                    {
                        var producer = await context.ProjectProducers.FirstOrDefaultAsync(d => d.Id == ProducerId);
                    var oldImageName = producer.ProducerImage;
                        if (producer != null)
                        {
                            producer.ProducerName = ProducerName.Text;
                            producer.Dob = DOB_txt.Date;
                            producer.Email = Email.Text;
                            producer.MobileNo = MobileNo.Text;
                            producer.Address = Addreess.Text;
                            producer.City = City.Text;
                            producer.State = State.Text;
                            producer.ZipCode = string.IsNullOrWhiteSpace(ZipCode.Text) ? (int?)null : int.Parse(ZipCode.Text);
                            producer.UpdatedAt = DateTime.Now;
                            producer.ProducerImage = ImageName??producer.ProducerImage;

                        // If new image name is different, delete old image
                        if (!string.IsNullOrEmpty(ImageName) && oldImageName != ImageName)
                        {

                            if (ImageFilesService.DeleteFtpImage("ProducerImages", oldImageName))
                            {
                                // Handle success case if needed
                                Console.WriteLine("Image deleted successfully.");
                            }
                            else
                            {
                                // Handle failure case if needed
                              //  await Application.Current.MainPage.DisplayAlert("Error", "Failed to delete the image.", "OK");
                            }

                        }

                        // Check if email already exists and it's not the current producer's email
                        var getEmail = context.ProjectProducers.Select(e => e.Email).ToList();
                            var existing = context.ProjectProducers.FirstOrDefault(dj => dj.Id == ProducerId);
                            if (!getEmail.Contains(Email.Text) || existing.Email == Email.Text)
                            {
                                context.ProjectProducers.Update(producer);
                                await context.SaveChangesAsync();
                           
                            await Application.Current.MainPage.DisplayAlert("Success", "Producer updated successfully!", "OK");
                                ProducerId = 0; // Reset the ProducerId for a fresh start
                            }
                            else
                            {
                                await Application.Current.MainPage.DisplayAlert("Error", "Email is already in use by another producer.", "OK");
                            }
                        }
                    }
            }

            // Navigate back to the management page  
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save producer: {ex.Message}", "OK");
        }
    }

    // Validation method that checks all fields
    private async Task<bool> ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(ProducerName.Text))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Please enter the Name.", "OK");
            ProducerName.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(MobileNo.Text))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Please enter the Mobile Number.", "OK");
            MobileNo.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(ZipCode.Text) || ZipCode.Text.Length < 5)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Please enter a valid zip code (length >= 5).", "OK");
            ZipCode.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(Email.Text) || !IsValidEmail(Email))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Please enter a valid email address.", "OK");
            Email.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(DOB_txt.ToString()) || DOB_txt.Date >= DateTime.Now)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Please enter a valid birthday (not in the future).", "OK");
            DOB_txt.Focus();
            return false;
        }

        // Validate Producer Name (only letters and spaces allowed)  
        if (string.IsNullOrWhiteSpace(ProducerName.Text))
        {
            await Application.Current.MainPage.DisplayAlert("Validation Error", "Producer name is required", "OK");
            return false;
        }
        if (!ProducerName.Text.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
        {
            await Application.Current.MainPage.DisplayAlert("Validation Error", "Producer name should only contain letters and spaces", "OK");
            return false;
        }

        // Validate Mobile Number (10 digits)  
        if (string.IsNullOrWhiteSpace(MobileNo.Text))
        {
            await Application.Current.MainPage.DisplayAlert("Validation Error", "Mobile number is required", "OK");
            return false;
        }
        if (!System.Text.RegularExpressions.Regex.IsMatch(MobileNo.Text, @"^\d{10}$"))
        {
            await Application.Current.MainPage.DisplayAlert("Validation Error", "Mobile number must be exactly 10 digits", "OK");
            return false;
        }

        return true;
        return true;
    }

    private bool IsValidEmail(Entry emailEntry)
    {
        var email = emailEntry.Text;
        return email.Contains("@") && email.Contains(".");
    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        // Ensure that the mainPage is of a type that contains _viewHistory  
        //if (mainPage is Dashboard dashboard)
        //{
        //    // Call the appropriate method to navigate back  
        //    if (Dashboard._viewHistory.Count > 0) // Use the type name to access the static member  
        //    {
        //        dashboard.GoBack();

        //        Dashboard._viewHistory.Pop();
        //    }
        //    else
        //    {
        //        await Shell.Current.GoToAsync("..");
        //    }
        //}
        //else
        //{
        //    await Application.Current.MainPage.DisplayAlert("Error", "mainPage is not of type Dashboard.", "OK");
        //}
        await Shell.Current.GoToAsync("..");
    }  
      
    

    //private async Task<bool> ValidateInputs()
    //{
    //    // Validate Producer Name (only letters and spaces allowed)  
    //    if (string.IsNullOrWhiteSpace(ProducerName.Text))
    //    {
    //        await Application.Current.MainPage.DisplayAlert("Validation Error", "Producer name is required", "OK");
    //        return false;
    //    }
    //    if (!ProducerName.Text.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
    //    {
    //        await  Application.Current.MainPage.DisplayAlert("Validation Error", "Producer name should only contain letters and spaces", "OK");
    //        return false;
    //    }

    //    // Validate Mobile Number (10 digits)  
    //    if (string.IsNullOrWhiteSpace(MobileNo.Text))
    //    {
    //        await Application.Current.MainPage.DisplayAlert("Validation Error", "Mobile number is required", "OK");
    //        return false;
    //    }
    //    if (!System.Text.RegularExpressions.Regex.IsMatch(MobileNo.Text, @"^\d{10}$"))
    //    {
    //        await Application.Current.MainPage.DisplayAlert("Validation Error", "Mobile number must be exactly 10 digits", "OK");
    //        return false;
    //    }

    //    return true;
    //}

    private void ProducerName_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.NewTextValue))
        {
            // Remove any non-letter characters except spaces
            string validText = new string(e.NewTextValue.Where(c => char.IsLetter(c) || char.IsWhiteSpace(c)).ToArray());
            if (validText != e.NewTextValue)
            {
                ProducerName.Text = validText;
            }
        }
    }

    private void MobileNo_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.NewTextValue))
        {
            // Remove any non-digit characters
            string validText = new string(e.NewTextValue.Where(char.IsDigit).ToArray());
            if (validText != e.NewTextValue)
            {
                MobileNo.Text = validText;
            }

            // Limit to 10 digits
            if (validText.Length > 10)
            {
                MobileNo.Text = validText.Substring(0, 10);
            }
        }
    }

    private void SelectImageButton_Clicked(object sender, EventArgs e)
    {

    }

    //private async void OnImageTapped(object sender, EventArgs e)
    //{
    //    await PickImage(); // Call your method when the image is tapped
    //}
    private  async void OnImageTapped(object sender, EventArgs e)
    {
        await PickImage();

    }

    //public static implicit operator View(CreateProjectProducer v)
    //{
    //    throw new NotImplementedException();
    //}
}