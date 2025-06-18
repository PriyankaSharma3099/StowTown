using CommunityToolkit.Maui.Views;
using Microsoft.EntityFrameworkCore;
using StowTown.Models;
using System.Security.Cryptography;
using System.Text;

namespace StowTown.Pages.UserSettingInfo;

public partial class ChangePasswordPopup : Popup
{
    public string NewPassword { get; private set; }
    public string loginuser { get; set; } // Static property to hold the logged-in user's name  
    public bool IsChangePassword { get; private set; } // Added this property to fix CS0103  

    public ChangePasswordPopup()
    {
        InitializeComponent();
    }

    public void InitializePopup()
    {
        System.Diagnostics.Debug.WriteLine("ChangePasswordPopup: Popup initialized");
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        System.Diagnostics.Debug.WriteLine("ChangePasswordPopup: OnHandlerChanged called");
    }

    private async void OnChangeClicked(object sender, EventArgs e)
    {
        string oldPasswordEntry = OldPasswordEntry.Text;
        string newPasswordEntry = NewPasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(oldPasswordEntry))
        {
            await Application.Current.MainPage.DisplayAlert("Warning", "Please enter the Old Password.", "OK");
            OldPasswordEntry.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(newPasswordEntry))
        {
            await Application.Current.MainPage.DisplayAlert("Warning", "Please enter the New Password.", "OK");
            NewPasswordEntry.Focus();
            return;
        }

        try
        {
            using (var context = new StowTownDbContext())
            {
                if (GlobalUserInfo.CurrentUser != null)
                {
                    loginuser = GlobalUserInfo.CurrentUser.Email;
                }
                var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == loginuser && u.IsDeleted == false);
                if (user != null)
                {
                    var oldPasswordHashed = HashPassword(oldPasswordEntry);
                    if (user.Password == oldPasswordHashed)
                    {
                        var newPasswordHashed = HashPassword(newPasswordEntry);
                        NewPassword = newPasswordHashed;
                        IsChangePassword = true;
                        user.Password = NewPassword;
                        user.UpdatedAt = DateTime.UtcNow;
                        context.Users.Update(user);
                        await context.SaveChangesAsync();

                        await Application.Current.MainPage.DisplayAlert("Success", "Password Updated Successfully!", "OK");
                        Close(NewPassword); // Pass the new hashed password back  
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Warning", "Old Password does not match.", "OK");
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "User not found.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error changing password: {ex}");
            await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        this.Close(null);
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        this.Close(null);
    }

    //private string HashPassword(string password)
    //{
    //    using (var sha256 = SHA256.Create())
    //    {
    //        var bytes = Encoding.UTF8.GetBytes(password);
    //        var hashBytes = sha256.ComputeHash(bytes);
    //        return Convert.ToBase64String(hashBytes);
    //    }
    //}
    public string HashPassword(string password)
    {
        using (SHA1 lSHA = SHA1.Create())
        {
            byte[] Byte = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = lSHA.ComputeHash(Byte);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
