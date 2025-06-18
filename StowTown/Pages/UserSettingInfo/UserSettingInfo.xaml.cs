
using CommunityToolkit.Maui.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui;
using StowTown.Models;
using StowTown.Services.SaveImageService;

namespace StowTown.Pages.UserSettingInfo;

public partial class UserSettingInfo : ContentPage,IQueryAttributable
{
    public static int songId { get; set; }
    private string Type { get; set; }
    public string loginemail { get; set; } // Static property to hold the logged-in user's name

    public string imageName { get; set; }
    public string savedimagePath { get; set; }
    public UserSettingInfo()
    {
        InitializeComponent();
        if (GlobalUserInfo.CurrentUser != null)
        {
            loginemail = GlobalUserInfo.CurrentUser.Email;
        }
        else
        {
            loginemail = string.Empty;
        }
        loadData();
        //LoadSavedImage();



    }
    public void loadData()
    {
        try
        {
            using (var context = new StowTownDbContext())
            {
                var user = context.Users.FirstOrDefault(u => u.UserName == loginemail && u.IsDeleted==false);
                if (user != null)
                {
                    // Assuming you have Entry controls named first_name, last_name, email_txt, and Password in your XAML  
                    first_name.Text = user.FirstName;
                    last_name.Text = user.LastName;
                    email_txt.Text = user?.UserName ?? ""; // Fix: Assign the string value to the Text property of the Entry control  
                    password_txt.Text = user._2stepVerifPassword;

                    // Fix: Assign the DateTime value to the Date property of the DatePicker control  
                    if (user.NotificationDate.HasValue)
                    {
                        notification_txt.Date = user.NotificationDate.Value;
                    }
                    else
                    {
                        notification_txt.Date = DateTime.Now; // Default value if NotificationDate is null  
                    }
                    if (!string.IsNullOrWhiteSpace(user.UserImage))
                    {
                        //var fullPath = GetImageFullPath(user.UserImage);

                        //if (!string.IsNullOrEmpty(fullPath))
                        //{
                        //    SelectedImageView.Source = ImageSource.FromFile(fullPath);
                        //    NoImageLabel.IsVisible = false;
                        //}
                        //else
                        //{
                        //    NoImageLabel.IsVisible = true;
                        //}
                        var getImagePath = ImageFilesService.GetImageUrl("UserProfileImages", user.UserImage);
                        if (getImagePath != null)
                        {
                            SelectedImageView.Source = getImagePath;
                            NoImageLabel.IsVisible = false;
                        }
                    }
                    else
                    {
                        NoImageLabel.IsVisible = true;
                    }

                }
                else
                {
                    // Handle case where user is not found  
                }
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions  
        }
    }
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        var navStack = Navigation.NavigationStack;

        if (navStack.Count >= 2)
        {
            var previousPage = navStack[navStack.Count - 2];

            if (previousPage is HomeDashboard) // or your actual class name
            {
                await Shell.Current.GoToAsync("//HomeDashboard");
                return;
            }
        }

        await Navigation.PopAsync();

        //await Shell.Current.GoToAsync("//HomeDashboard");

    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(first_name.Text))
        {
            await DisplayAlert("Validation", "Please enter First Name.", "OK");
            first_name.Focus();
            return;
        }
        if (string.IsNullOrWhiteSpace(last_name.Text))
        {
            await DisplayAlert("Validation", "Please enter Last Name.", "OK");
            first_name.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(email_txt.Text) || !IsValidEmail(email_txt.Text))
        {
            await DisplayAlert("Validation", "Please enter a valid Email.", "OK");
            email_txt.Focus();
            return;
        }

        if (notification_txt.Date >= DateTime.Now.Date)
        {
            await DisplayAlert("Validation", "Please select a future notification date.", "OK");
            notification_txt.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(password_txt.Text))
        {
            await DisplayAlert("Validation", "Please enter Email Password.", "OK");
            password_txt.Focus();
            return;
        }

        try
        {
            using var context = new StowTownDbContext();

            int userId = 1; // Replace with dynamic user ID if needed
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.UserName== loginemail && u.IsDeleted==false);

            if (user != null)
            {
                var oldImageName = user.UserImage;
                user.FirstName = first_name.Text;
                user.LastName = last_name.Text;
                user.UserName = email_txt.Text;
                user._2stepVerifPassword = password_txt.Text;
                user.NotificationDate = notification_txt.Date;
                user.UpdatedAt = DateTime.UtcNow;
                user.IsDeleted = false;

                //if (!string.IsNullOrWhiteSpace(savedimagePath))
                //{
                //    var fileName = SaveImageAndGetFileName(savedimagePath); // Save new image
                //    user.UserImage = fileName;
                //}

                var profileimage = imageName;
                if (!string.IsNullOrWhiteSpace(profileimage))
                {
                    
                    user.UserImage = profileimage ?? user.UserImage;
                    // If new image name is different, delete old image
                    if (!string.IsNullOrEmpty(profileimage) && oldImageName != profileimage)
                    {

                        if (ImageFilesService.DeleteFtpImage("UserProfileImages", oldImageName))
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
                }

                //if (IsChangePassword && !string.IsNullOrWhiteSpace(NewPassword))
                //{
                //    user.Password = NewPassword; // Assume already hashed
                //}

                context.Users.Update(user);
                await context.SaveChangesAsync();

                await DisplayAlert("Success", "User updated successfully!", "OK");

                await Shell.Current.GoToAsync("//HomeDashboard");


            }
            else
            {
                await DisplayAlert("Error", "User not found.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error updating user: {ex.Message}", "OK");
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
    
        if (query.TryGetValue("SelectedId", out var id) && id != null && int.TryParse(id.ToString(), out int parsedId))
        {
            songId = parsedId;
        }
        else
        {
            songId = 0;
        }

        Type = query.TryGetValue("Type", out var typeObj) && typeObj != null ? typeObj.ToString() : "Create";
        
    }

    private async  void  SelectImageButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select an image",
                FileTypes = FilePickerFileType.Images
            });
            if (result != null) {

                
                    //  await ProcessImage(photo);

                    var userImageName = await ImageFilesService.UploadImageToServer(result, "UserProfileImages");
                    if (!String.IsNullOrEmpty(userImageName))
                    {
                        var imagePath = ImageFilesService.GetImageUrl("UserProfileImages", userImageName);

                        SelectedImageView.Source = ImageSource.FromUri(new Uri(imagePath));
                        NoImageLabel.IsVisible = false;
                        imageName = userImageName;
                    }

                
            }

            //var result = "";

            //MainThread.BeginInvokeOnMainThread(async () =>
            //{
            //    var result = await Application.Current.MainPage.DisplayActionSheet("Select Image Source", "Cancel", null, "Camera", "Gallery");


            //});
            
            //if (result == "Camera")
            //{
            //    if (MediaPicker.Default.IsCaptureSupported)
            //    {
            //        var photo = await MediaPicker.Default.CapturePhotoAsync();
            //        if (photo != null)
            //        {
            //            //await ProcessImage(photo);
            //        }
            //    }
            //    else
            //    {
            //        await DisplayAlert("Error", "Camera is not supported on this device", "OK");
            //    }
            //}
            //else if (result == "Gallery")
            //{
            //    var photo = await FilePicker.Default.PickAsync(new PickOptions
            //    {
            //        PickerTitle = "Select an Image",
            //        FileTypes = FilePickerFileType.Images
            //    });

            //    if (photo != null)
            //    {
            //        //  await ProcessImage(photo);

            //        var imageName = await ImageFilesService.UploadImageToServer(photo, "UserProfileImages");
            //        if (!String.IsNullOrEmpty(imageName))
            //        {
            //            var imagePath = ImageFilesService.GetImageUrl("UserProfileImages", imageName);

            //            SelectedImageView.Source = ImageSource.FromUri(new Uri(imagePath));
            //            NoImageLabel.IsVisible = false;
            //            imageName = imageName;
            //        }

            //    }

            //}


            //if (result != null)
            //{
            //    var extension = Path.GetExtension(result.FileName);
            //    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            //    var destinationPath = Path.Combine(FileSystem.AppDataDirectory, uniqueFileName);

            //    using var stream = await result.OpenReadAsync();
            //    using (var fileStream = File.Create(destinationPath))
            //    {
            //        await stream.CopyToAsync(fileStream);
            //    }

            //    // Show new image
            //    SelectedImageView.Source = ImageSource.FromFile(destinationPath);
            //    NoImageLabel.IsVisible = false;

            //    // Save path
            //    Preferences.Set("SavedImagePath", destinationPath);


            //    imageName = uniqueFileName;
            //    savedimagePath = destinationPath;

            //    // Optional: Delete old images
            //    // DeleteOldImagesExcept(destinationPath);
            //}
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Image load failed: {ex.Message}", "OK");
        }
    }

    private string SaveImageAndGetFileName(string originalPath)
    {
        var extension = Path.GetExtension(originalPath);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var destinationPath = Path.Combine(FileSystem.AppDataDirectory, uniqueFileName);

        File.Copy(originalPath, destinationPath, true);
        return uniqueFileName;
    }

    private string GetImageFullPath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        var fullPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
        return File.Exists(fullPath) ? fullPath : null;
    }
    private void LoadSavedImage()
    {
        var fileName = Preferences.Get("SavedImageFileName", null);
        var fullPath = GetImageFullPath(fileName);

        if (!string.IsNullOrEmpty(fullPath))
        {
            SelectedImageView.Source = ImageSource.FromFile(fullPath);
            NoImageLabel.IsVisible = false;
        }
        else
        {
            NoImageLabel.IsVisible = true;
        }
    }

    //private void LoadSavedImage()
    //{
    //    var savedPath = Preferences.Get("SavedImagePath", null);
    //    if (!string.IsNullOrWhiteSpace(savedPath) && File.Exists(savedPath))
    //    {
    //        SelectedImageView.Source = ImageSource.FromFile(savedPath);
    //        NoImageLabel.IsVisible = false;
    //    }
    //    else
    //    {
    //        NoImageLabel.IsVisible = true;
    //    }
    //}

    private void DeleteOldImagesExcept(string keepPath)
    {
        try
        {
            var directory = FileSystem.AppDataDirectory;
            var files = Directory.GetFiles(directory);

            foreach (var file in files)
            {
                if (file != keepPath && IsImageFile(file))
                {
                    File.Delete(file);
                }
            }
        }
        catch
        {
            // Silent fail
        }
    }

    private bool IsImageFile(string path)
    {
        string ext = Path.GetExtension(path).ToLower();
        return ext == ".jpg" || ext == ".jpeg" || ext == ".png";
    }

    private void DeletePreviousImage(string newImagePath)
    {
        try
        {
            var oldImagePath = Preferences.Get("SavedImagePath", null);

            if (!string.IsNullOrEmpty(oldImagePath) &&
                File.Exists(oldImagePath) &&
                oldImagePath != newImagePath)
            {
                File.Delete(oldImagePath);
            }
        }
        catch
        {
            // Optional: Log error
        }
    }
    public bool IsValidEmail(string email)
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

    private async void OnChangedPasswordClicked(object sender, EventArgs e)
    {
        try
        {
            // Assuming ChangePasswordPopup is a custom class derived from Popup  
           Popup popup = new ChangePasswordPopup();
            // this.ShowPopup(popup); // Fix: Use a class that inherits from CommunityToolkit.Maui.Views.Popup  
            Shell.Current.CurrentPage.ShowPopupAsync(popup);
            //// Show the popup and await the result  
            //var result = await Application.Current.MainPage.ShowPopupAsync(popup);

            //if (result is string newPassword)
            //{
            //    await DisplayAlert("Success", "Password changed successfully!", "OK");
            //    password_txt.Text = newPassword;
            //}
            //else if (result is string error && error.StartsWith("Please"))
            //{
            //    await DisplayAlert("Warning", error, "OK");
            //}
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error showing popup: {ex}");
            await DisplayAlert("Error", "An error occurred while trying to change the password.", "OK");
        }
    }
}