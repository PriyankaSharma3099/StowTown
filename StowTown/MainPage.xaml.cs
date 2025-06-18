using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StowTown.Models;
using StowTown.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace StowTown
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private readonly string connectionString = "Data Source=DESKTOP-RJ\\SQLEXPRESS02;Initial Catalog=StowTown;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30";
        public MainPage()
        {
            InitializeComponent();
            //Shell.Current.FlyoutIsPresented = false;
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StowTownLogs");
            Directory.CreateDirectory(folder);

            string filePath = Path.Combine(folder, $"startup-error-{DateTime.Now:yyyyMMdd-HHmmss}.log");
            File.WriteAllText(filePath, "Set Up Main Page Success");
        }
        private async void Button_Clicked(object sender, EventArgs e)
        {
            try
            {
                string email = Email_txt.Text; // Get email from Entry
                string password = Pass_txt.Text; // Get password from Entry

                User user = await GetUserByEmail(email); // Retrieve user by email
                if (user != null)
                {
                    string hashedPassword = HashPassword(password); // Hash the password
                    if (user.UserName == email && user.Password == hashedPassword)
                    {
                        //Application.Current.MainPage = new AppShell();
                        // Navigate to Dashboard
                        // Shell.Current.FlyoutIsPresented = true;
                        // Set up the ViewModel with user info
                        var vm = new UserInfoViewModel
                        {
                            Name = user.FirstName,
                            Email = user.UserName,
                            // ImageUrl = "C:\\Users\\joshi\\source\\repos\\StowTown_Stable\\stowtown\\StowTown\\assets\\avatar.png" // optional
                            ImageUrl = "assets/avatar.png"
                        };
                        GlobalUserInfo.CurrentUser = vm;
                        // Assign AppShell with ViewModel as BindingContext
                        var shell = new AppShell
                        {
                            BindingContext = vm
                        };

                        Application.Current.MainPage = shell;





                        //await MainThread.InvokeOnMainThreadAsync(async () =>
                        //{
                        //    await Task.Delay(1000); // non-blocking delay
                        //    await Shell.Current.GoToAsync("//HomeDashboard");
                        //});

                        await Shell.Current.GoToAsync(nameof(HomeDashboard));
                        //  await Shell.Current.GoToAsync("//HomeDashboard");
                        HomeDashboard.isInitializing = false;
                    }
                    else
                    {

                        // Invalid username or password
                        await Application.Current.MainPage.DisplayAlert("Error", "Invalid username or password.", "OK");
                    }
                }
                else
                {
                    // User not found
                    await Application.Current.MainPage.DisplayAlert("Error", "User not found.", "OK");
                }
            }
            catch (Exception ex)
            {
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StowTownLogs");
                Directory.CreateDirectory(folder);

                string filePath = Path.Combine(folder, $"startup-error-{DateTime.Now:yyyyMMdd-HHmmss}.log");

                // Check if file exists
                if (File.Exists(filePath))
                {
                    File.AppendAllText(filePath, "Set Up Main Page Success" + Environment.NewLine+ex);
                }
                else
                {
                    File.WriteAllText(filePath, "Set Up Main Page Success" + Environment.NewLine);
                }
            }

        }

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

        public async Task<User> GetUserByEmail(string email)
        {
            // User user = null;

            //// Ensure connection string is set properly
            //using (SqlConnection conn = new SqlConnection(connectionString))
            //{
            //    await conn.OpenAsync();

            //    // Use parameterized query to prevent SQL injection
            //    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Users WHERE UserName = @UserName", conn))
            //    {
            //        cmd.Parameters.AddWithValue("@UserName", email);

            //        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
            //        {
            //            if (await reader.ReadAsync())
            //            {
            //                // Map the data from the database to the User object
            //                user = new User
            //                {
            //                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
            //                    UserName = reader["UserName"].ToString(),
            //                    Password = reader["Password"].ToString(),
            //                    FirstName = reader["FirstName"].ToString(),
            //                    LastName = reader["LastName"].ToString(),
            //                    UserImage = reader["UserImage"].ToString(),
            //                    _2stepVerifPassword = reader["2StepVerifPassword"].ToString(),
            //                    NotificationDate = reader["NotificationDate"] == DBNull.Value
            //                        ? DateTime.Now
            //                        : reader.GetDateTime(reader.GetOrdinal("NotificationDate"))
            //                };

            //                // Update session details
            //                UserSession.FirstName = user.FirstName;
            //                UserSession.LastName = user.LastName;
            //                UserSession.UserImage = user.UserImage;
            //                UserSession.UserName = user.UserName;
            //                UserSession.EmailPassword = user._2stepVerifPassword;
            //                //Noti_date = user.NotificationDate;
            //            }
            //        }
            //    }
            //}

            // return user;

            User user = null;

            using (var context = new StowTownDbContext())
            {
                user = await context.Users
                    .Where(u => u.UserName == email)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    // Update session details
                    UserSession.FirstName = user.FirstName;
                    UserSession.LastName = user.LastName;
                    UserSession.UserImage = user.UserImage;
                    UserSession.UserName = user.UserName;
                    UserSession.EmailPassword = user._2stepVerifPassword;
                    // Noti_date = user.NotificationDate; // Uncomment if needed
                }
            }

            return user;
        }

        private async void navigateToForgetPassword(object sender, TappedEventArgs e)
        {

            //await Shell.Current.GoToAsync("ForgetPassword");
            Application.Current.MainPage = new ForgetPassword(); // Navigate without Flyout
        }
    }

}
