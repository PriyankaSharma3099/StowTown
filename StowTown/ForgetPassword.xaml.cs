using Microsoft.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using StowTown.ViewModels;
using StowTown.Models;
using Microsoft.EntityFrameworkCore;
using StowTown.HelperService;

namespace StowTown;

public partial class ForgetPassword : ContentPage
{
    private readonly string connectionString;
    public string loginuser { get; set; }

    public EmailService emailhelper { get; set; }
    public ForgetPassword()
	{
		InitializeComponent();

        emailhelper = new EmailService();

        connectionString = "Data Source=DESKTOP-RJ\\SQLEXPRESS02;Initial Catalog=StowTown;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30";
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

    }

    private static readonly Random random = new();

    public static string GenerateRandomPassword(int length)
    {
        const string uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string specialChars = "!@#$%^&*()-_=+[]{};:,.<>?";

        string allChars = uppercaseChars + lowercaseChars + digits + specialChars;

        StringBuilder password = new(length);
        password.Append(uppercaseChars[random.Next(uppercaseChars.Length)]);
        password.Append(lowercaseChars[random.Next(lowercaseChars.Length)]);
        password.Append(digits[random.Next(digits.Length)]);
        password.Append(specialChars[random.Next(specialChars.Length)]);

        for (int i = 4; i < length; i++)
        {
            password.Append(allChars[random.Next(allChars.Length)]);
        }

        return Shuffle(password.ToString());
    }

    private static string Shuffle(string password)
    {
        char[] array = password.ToCharArray();
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
        return new string(array);
    }

    private string smtpServer = "smtp.gmail.com"; // Replace with your SMTP server
    private int smtpPort = 587;
    private string senderEmail = UserSession.UserName; // Replace with your email
    private string senderPassword = UserSession.EmailPassword; // Replace with your email password

    public void SendPasswordEmail(string recipientEmail, string password)
    {
        try
        {
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail),
                Subject = "Your Generated Password",
                Body = $"Your generated password is: {password}",
                IsBodyHtml = false
            };
            mailMessage.To.Add(recipientEmail);

            using var smtpClient = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true
            };
            smtpClient.Send(mailMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email: {ex.Message}");
        }
    }

    public string HashPassword(string password)
    {
        using SHA1 sha = SHA1.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(password);
        byte[] hashBytes = sha.ComputeHash(bytes);
        StringBuilder sb = new();
        foreach (byte b in hashBytes)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }

    private async void OnSendButtonClicked(object sender, EventArgs e)
    {
        int passwordLength = 8;
        string randomPassword = GenerateRandomPassword(passwordLength);
        string hashedPassword = HashPassword(randomPassword);

        string recipientEmail = Emailtxt_txt.Text;

        SendPasswordEmail(recipientEmail, randomPassword);

        using SqlConnection conn = new(connectionString);
        conn.Open();
        using SqlCommand cmd = new("UPDATE Users SET Password = @Password, UpdatedAt = @UpdatedAt WHERE UserName = @UserName", conn);
        cmd.Parameters.AddWithValue("@Password", hashedPassword);
        cmd.Parameters.AddWithValue("@UserName", recipientEmail);
        cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

        try
        {
            cmd.ExecuteNonQuery();
            Emailtxt_txt.Text = string.Empty;
            await DisplayAlert("Success", "Password updated successfully", "OK");
            await Navigation.PopAsync(); // Navigate back to the login page
        }
        catch (SqlException ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

   
    

    private async void Button_Clicked(object sender, EventArgs e)
    {
        int passwordLength = 8;
        string randomPassword = GenerateRandomPassword(passwordLength);
        string hashedPassword = HashPassword(randomPassword);

        string recipientEmail = Emailtxt_txt.Text;

       

        if (string.IsNullOrWhiteSpace(recipientEmail) ||  !IsValidEmail(recipientEmail))
        {
            await Application.Current.MainPage.DisplayAlert("Warning", "Please enter the Email.", "OK");
            Emailtxt_txt.Focus();
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
                var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == recipientEmail && u.IsDeleted == false);
                if (user != null)
                {
                    if (user.UserName == recipientEmail)
                    {
                    
                       bool isSuceess =  await emailhelper.SendPasswordEmail(recipientEmail, randomPassword);
                        if (isSuceess)
                        {
                            // SendPasswordEmail(recipientEmail, randomPassword);
                            user.Password = randomPassword;
                            user.UpdatedAt = DateTime.UtcNow;
                            context.Users.Update(user);
                            await context.SaveChangesAsync();

                            await Application.Current.MainPage.DisplayAlert("Success", "Password Updated Successfully!", "OK");
                            Application.Current.MainPage = new MainPage();

                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Error", $"Invalid Credentails", "OK");
                        }
                       
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Warning", "Email does not match.", "OK");
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
    private void navigateToLogin(object sender, TappedEventArgs e)
    {
        Application.Current.MainPage = new MainPage();
    }
}