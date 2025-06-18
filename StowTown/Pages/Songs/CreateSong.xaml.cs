using DocumentFormat.OpenXml.Bibliography;
using Microsoft.EntityFrameworkCore;
using StowTown.Models;
using StowTown.Services.SaveImageService;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace StowTown.Pages.Songs;

public partial class CreateSong : ContentPage, IQueryAttributable
{
    public static int songId { get; set; }
    private string Type { get; set; }
    private ObservableCollection<ArtistGroup> _artists;
    public ObservableCollection<ArtistGroup> Artists
    {
        get => _artists;
        set
        {
            _artists = value;
            OnPropertyChanged(nameof(Artists));
        }
    }

    private ArtistGroup _selectedArtist;
    private string selectedImagePath1;
    private string? selectedImagePath;

    public ArtistGroup SelectedArtist
    {
        get => _selectedArtist;
        set
        {
            _selectedArtist = value;
            OnPropertyChanged(nameof(SelectedArtist));
        }
    }
    private string SelectedImagePath { get; set; }

    private string ImageName { get; set; }


    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    public CreateSong()
    {
        InitializeComponent();
        _artists = new ObservableCollection<ArtistGroup>();
        LoadArtists();
        Artist_DD.ItemsSource = _artists;
    }
    private async void LoadArtists()
    {
        try
        {
                using (var context = new StowTownDbContext())
                {
                    // Fetch artist from the database
                    var artistGroups = await context.ArtistGroups.Where(s => s.IsDeleted != true && s.IsActive == true).ToListAsync();


                Artists = new ObservableCollection<ArtistGroup>(artistGroups);
                // Set the ItemsSource for the dropdown
                Artist_DD.ItemsSource = new ObservableCollection<ArtistGroup>(artistGroups);
                }
            
           
        }
        catch (Exception ex)
        {
            //DisplayAlert(ex.Message);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // BindSongs(); // Load data when page appears
        if (songId == 0)
        {
            HeaderName.Text = "Create Song";
            SaveButton.Text = "Save";
            EnableControls();
            ClearForm();
        }

    }


    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        // ClearForm();

        if (query.TryGetValue("SelectedId", out var id) && id != null && int.TryParse(id.ToString(), out int parsedId))
        {
            songId = parsedId;
        }
        else
        {
            songId = 0;
        }

        Type = query.TryGetValue("Type", out var typeObj) && typeObj != null ? typeObj.ToString() : "Create";
        if (songId == 0)
        {
            HeaderName.Text = "Create Song";
            SaveButton.Text = "Save";
            EnableControls();
            ClearForm();
        }

        if (songId > 0 && Type == "Edit")
        {
            HeaderName.Text = "Update Song";
            BindSongs();
            //BindSong(songId);
            SaveButton.Text = "Update";
        }
        else if (songId > 0 && Type == "View")
        {
            HeaderName.Text = "View Song";
            BindSongs();
            DisableControls();
            SaveButton.IsVisible = false;  // Hide the SaveButton
            SelectImageButton.IsVisible = false;
        }
    }

   

    private async Task BindSongs()
    {
        try
        {
            using (var Context = new StowTownDbContext())
            {
                var song = await Context.Songs.FirstOrDefaultAsync(d => d.Id == songId);

                if (song != null)
                {
                    Title_txt.Text = song.Name ?? string.Empty;
                    Minutes_txt.Text = song.Minutes.ToString() ?? string.Empty;
                    Seconds_txt.Text = song.Seconds.ToString() ?? string.Empty;
                  //  Artist_DD.SelectedItem = song.FkArtist; // Assuming FkArtist is a string or compatible type
                    ReleaseDatePicker.Date = song.ReleaseDate ?? DateTime.Now; // Corrected line

                    // selectedImagePath = song.Image;
                    // Set the selected artist
                    Artist_DD.SelectedItem = Artists.FirstOrDefault(a => a.Id == song.FkArtist);

                    //selectedImagePath1 = song.Image;

                    //if (selectedImagePath1 != null)
                    //{
                    //    string projectRoot = AppDomain.CurrentDomain.BaseDirectory;

                    //    string fullPath = Path.Combine(projectRoot, "assets", "SongImages");

                    //    if (!Directory.Exists(fullPath))
                    //    {
                    //        Directory.CreateDirectory(fullPath);
                    //    }
                    //    selectedImagePath1 = Path.Combine(fullPath, Path.GetFileName(song.Image));
                    //    SelectedImageView.Source = ImageSource.FromFile(selectedImagePath1);
                    //}
                   
                        if (!String.IsNullOrEmpty(song.Image))
                        {
                            var imagePath = ImageFilesService.GetImageUrl("SongsImages", song.Image);
                            SelectedImageView.Source = imagePath;                            
                            NoImageLabel.IsVisible = false; // Hide "NO THUMB" label
                        }


                    


                }
                
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }


    private void DisableControls()
    {
        Title_txt.IsEnabled = false;
        Minutes_txt.IsEnabled = false;
        Artist_DD.IsEnabled = false;
        Seconds_txt.IsEnabled = false;
        ReleaseDatePicker.IsEnabled = false;
       // SelectedImageView.IsEnabled = true;
        //SelectImageButton.IsEnabled = false;
        //SelectImageButton.IsEnabled = false;
    }

    private void EnableControls()
    {
        Title_txt.IsEnabled = true;
        Minutes_txt.IsEnabled = true;
        Artist_DD.IsEnabled = true;
        Seconds_txt.IsEnabled = true;
        ReleaseDatePicker.IsEnabled = true;
        SelectedImageView.IsEnabled = true;
        SelectImageButton.IsEnabled = true;

    }

    private void ClearForm()
    {
        songId = 0;
        Type = "Create";
        Title_txt.Text = string.Empty;
        Minutes_txt.Text = string.Empty;
        ReleaseDatePicker.Date = DateTime.Now;
        Artist_DD.SelectedItem = string.Empty;
        Seconds_txt.Text = string.Empty;
        //SelectedImagePath = null;
        EnableControls();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            // Validate Title
            if (string.IsNullOrWhiteSpace(Title_txt.Text))
            {
                await DisplayAlert("Error", "Please enter a title for the song.", "OK");
                Title_txt.Focus();
                return;
            }

            // Validate Artist Selection
            if (Artist_DD.SelectedItem == null)
            {
                await DisplayAlert("Error", "Please select an artist.", "OK");
                Artist_DD.Focus();
                return;
            }

            // Validate ReleaseDate
            if (ReleaseDatePicker.Date == null)
            {
                await DisplayAlert("Error", "Please select a Release Date.", "OK");
                return;
            }

            // Validate Minutes
            if (string.IsNullOrWhiteSpace(Minutes_txt.Text) || !int.TryParse(Minutes_txt.Text, out int minutes) || minutes < 0 || minutes > 59)
            {
                await DisplayAlert("Error", "Please enter valid minutes (0-59).", "OK");
                Minutes_txt.Focus();
                return;
            }

            // Validate Seconds
            if (string.IsNullOrWhiteSpace(Seconds_txt.Text))
            {
                Seconds_txt.Text = "00";
            }
            if (!int.TryParse(Seconds_txt.Text, out int seconds) || seconds < 0 || seconds > 59)
            {
                await DisplayAlert("Error", "Please enter valid seconds (0-59).", "OK");
                Seconds_txt.Focus();
                return;
            }

            // Extract selected artist
            var selectedArtist = Artist_DD.SelectedItem as ArtistGroup;
            if (selectedArtist == null)
            {
                await DisplayAlert("Error", "Invalid artist selection.", "OK");
                return;
            }

            using (var context = new StowTownDbContext())
            {
                if (songId == 0) // **Create New Song**
                {
                    var newSong = new Song
                    {
                        Name = Title_txt.Text,
                        FkArtist = selectedArtist.Id,  // **Use Artist ID**
                        Minutes = minutes,
                        Seconds = seconds,
                        ReleaseDate = ReleaseDatePicker.Date, // **Use .Date**
                        Image = ImageName,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted=false
                    };

                    context.Songs.Add(newSong);
                    await context.SaveChangesAsync();
                    await DisplayAlert("Success", "Song saved successfully!", "OK");
                }
                else // **Update Existing Song**
                {
                    var song = await context.Songs.FirstOrDefaultAsync(d => d.Id == songId);
                    if (song == null)
                    {
                        await DisplayAlert("Error", "Song not found!", "OK");
                        return;
                    }
                    var oldImageName = song.Image; // Store old image name for deletion later
                    // **Update properties**
                    song.Name = Title_txt.Text;
                    song.FkArtist = selectedArtist.Id;
                    song.Minutes = minutes;
                    song.Seconds = seconds;
                    song.ReleaseDate = ReleaseDatePicker.Date;
                    song.Image = ImageName??song.Image;
                    song.UpdatedAt = DateTime.UtcNow;
                    song.IsDeleted = false;
                    // If new image name is different, delete old image
                    if (!string.IsNullOrEmpty(ImageName) && oldImageName != ImageName)
                    {

                        if (ImageFilesService.DeleteFtpImage("SongsImages", oldImageName))
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
                    await DisplayAlert("Success", "Song updated successfully!", "OK");
                }
            }

            await Navigation.PopAsync(); // **Go back after saving**
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Error saving song: " + ex.Message, "OK");
        }
    }


    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    // Pick Image from Camera or Gallery
    private async Task PickImage(bool isArtist)
    {
       try
        {


            // Still use a small delay
            await Task.Delay(100);
            Device.BeginInvokeOnMainThread(async () =>
            {
                var result = await DisplayActionSheet("Select Image Source", "Cancel", null as string, "Camera", "Gallery");

                if (result == "Camera")
                {
                    if (MediaPicker.Default.IsCaptureSupported)
                    {
                        bool hasPermission = await CheckAndRequestPermissions();
                        if (!hasPermission)
                        {
                            await DisplayAlert("Permission Denied", "Camera access is required.", "OK");
                            return;
                        }

                        var photo = await MediaPicker.Default.CapturePhotoAsync();
                        if (photo != null)
                        {
                            await ProcessImage(photo, isArtist);
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Camera is not supported on this device", "OK");
                    }
                }
                else if (result == "Gallery")
                {
                    bool hasPermission = await CheckAndRequestPermissions();
                    if (!hasPermission)
                    {
                        await DisplayAlert("Permission Denied", "Gallery access is required.", "OK");
                        return;
                    }

                    var photo = await FilePicker.Default.PickAsync(new PickOptions
                    {
                        PickerTitle = "Select an Image",
                        FileTypes = FilePickerFileType.Images
                    });

                    if (photo != null)
                    {
                        // await ProcessImage(photo, isArtist);
                       
                            var imageName = await ImageFilesService.UploadImageToServer(photo, "SongsImages");
                            if (!String.IsNullOrEmpty(imageName))
                            {
                                var imagePath = ImageFilesService.GetImageUrl("SongsImages", imageName);
                                SelectedImageView.Source = imagePath;
                                ImageName = imageName;
                                NoImageLabel.IsVisible = false; // Hide "NO THUMB" label
                            }


                        
                    }
                }
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to pick image: {ex.Message}", "OK");
        }

    }
    private async Task<bool> CheckAndRequestPermissions()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
        }

        var storageStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
        if (storageStatus != PermissionStatus.Granted)
        {
            storageStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
        }

        return status == PermissionStatus.Granted && storageStatus == PermissionStatus.Granted;
    }


    // Process & Save Image
    private async Task ProcessImage(FileResult photo, bool isArtist)
    {
        try
        {
            string imagesDir = GetDynamicImagePath(isArtist); // Get the correct path
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

            if (isArtist)
            {
                selectedImagePath1 = newFilePath;
                await UpdateImageDisplay(newFilePath);
            }
            

        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to process image: {ex.Message}", "OK");
        }
    }


    // Get Dynamic Image Path (Inside assets/DJ)
    private string GetDynamicImagePath(bool isArtist)
    {
        string projectRoot = AppDomain.CurrentDomain.BaseDirectory;
        string folderName = isArtist ? "SongImages" : "Images"; // Separate folders
        string fullPath = Path.Combine(projectRoot, "assets", folderName);

        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        return fullPath;
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

            MainThread.BeginInvokeOnMainThread(() =>
            {
                SelectedImageView.Source = ImageSource.FromFile(imagePath);
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to display image: {ex.Message}", "OK");
        }
    }

    private  async void SelectImageButton_Clicked(object sender, EventArgs e)
    {
         await PickImage(true);
    }
}