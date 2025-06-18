using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Maui;
using StowTown.ViewModels;
using System.Collections.ObjectModel;
using StowTown.Models;
using System.Globalization;
using System.Collections.Specialized;
using OfficeOpenXml;
using System.Net.Mail;
using System.Net;
using OfficeOpenXml.Style;
using StowTown.HelperService;
using MailKit.Security;
using MimeKit;
using Color = Microsoft.Maui.Graphics.Color;
using StowTown.Services.SaveImageService;

namespace StowTown.Pages.MonthlySong;

public partial class MonthlySongManagement : ContentPage
{
    private ObservableCollection<MonthlySongViewModel> _allSongs;
    private ObservableCollection<MonthlySongViewModel> _filteredSongs;
    private const int PageSize = 10;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private ObservableCollection<MonthlySongViewModel> _filteredArtists;  // New collection for filtered results
    private string _searchText = string.Empty;  // Store the search query

    public ObservableCollection<string> Years { get; set; }
    public ObservableCollection<string> Months { get; set; }
    public string SelectedYear { get; set; }
    public string SelectedMonth { get; set; }

    public int SelectedYearIndex { get; set; }

    public EmailService EmailService { get; set; }

    public MonthlySongManagement()
	{
		InitializeComponent();
      
        _filteredSongs = new ObservableCollection<MonthlySongViewModel>();
        _allSongs = new ObservableCollection<MonthlySongViewModel>();
    


        // Subscribe to refresh message
        MessagingCenter.Subscribe<CreateMonthlySongList>(this, "RefreshArtistList", (sender) =>
        {
            LoadData();
           // FilterSongsByDate();
        });
        SongList.ItemsSource = _filteredSongs; // Assuming ListView ID is 'ArtistList'
        
        BindingContext = this;

        LoadMonthPicker();
        LoadYearPicker();
        //  Auto-select the current year and month
        //YearPicker.SelectedIndex = Years.IndexOf(DateTime.Now.Year.ToString());
        //MonthPicker.SelectedIndex = DateTime.Now.Month - 1;
        //OnYearSelected(YearPicker, EventArgs.Empty);
        //OnMonthSelected(MonthPicker, EventArgs.Empty);
        //FilterSongsByDate();
        LoadData();
        EmailService = new EmailService();
        settingbutton();
    }

    public void settingbutton()
    {
        PreviousButton.BackgroundColor = Color.FromArgb("#63CF6C");
        PreviousButton.TextColor = Colors.White;
        NextButton.BackgroundColor = Color.FromArgb("#63CF6C");
        NextButton.TextColor = Colors.White;
        PreviousButton.WidthRequest = 100;
        NextButton.WidthRequest = 100;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        // LoadData(); // Refresh data every time the page appears
        if (YearPicker.SelectedItem == null)
        {
            YearPicker.SelectedIndex = YearPicker.Items.IndexOf(DateTime.Now.Year.ToString());
        }

        if (MonthPicker.SelectedItem == null)
        {
            MonthPicker.SelectedIndex = DateTime.Now.Month - 1;
        }
        //YearPicker.SelectedIndex = Years.IndexOf(DateTime.Now.Year.ToString());
        //MonthPicker.SelectedIndex = DateTime.Now.Month - 1;

        //FilterSongsByDate(); // Load initial data based on current month & year
        LoadData();
        EmailService = new EmailService();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Unsubscribe from the message when the page disappears
        MessagingCenter.Unsubscribe<CreateMonthlySongList>(this, "RefreshSongList");
    }


    private void LoadYearPicker()
    {
      Years = new ObservableCollection<string>();
            int startYear = DateTime.Now.Year - 1; // Start from last year
            int endYear = startYear + 10; //  Up to 10 years ahead

            for (int i = startYear; i <= endYear; i++)
            {
                Years.Add(i.ToString());
            }

            // Set default selected year (index where current year is found)
            SelectedYearIndex = Years.IndexOf(DateTime.Now.Year.ToString());

            OnPropertyChanged(nameof(Years));
            OnPropertyChanged(nameof(SelectedYearIndex));
    }

    private void LoadMonthPicker()
    {
        Months = new ObservableCollection<string>();

        for (int i = 1; i <= 12; i++)
        {
            Months.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i));
        }

        SelectedMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month); // 
        OnPropertyChanged(nameof(Months));
        OnPropertyChanged(nameof(SelectedMonth));
    }

    private void OnYearSelected(object sender, EventArgs e)
    {
        string selectedYear = YearPicker.SelectedItem?.ToString();
        if (!string.IsNullOrEmpty(selectedYear))
        {
            Console.WriteLine($"Selected Year: {selectedYear}");
            //FilterSongsByDate();
            LoadData();
        }
    }
    private void OnMonthSelected(object sender, EventArgs e)
    {
        string selectedMonth = MonthPicker.SelectedItem?.ToString();
        if (!string.IsNullOrEmpty(selectedMonth))
        {
            Console.WriteLine($"Selected Month: {selectedMonth}");
            //FilterSongsByDate();
            LoadData();
        }
    }


    private void FilterSongsByDate()
    {
        try
        {
            if (YearPicker.SelectedItem == null || MonthPicker.SelectedItem == null)
                return; // Ensure both year and month are selected

            int selectedYear = int.Parse(YearPicker.SelectedItem.ToString());
            int selectedMonth = DateTime.ParseExact(MonthPicker.SelectedItem.ToString(), "MMMM", CultureInfo.InvariantCulture).Month;

            using (var context = new StowTownDbContext())
            {
                var filteredList = context.Songs
                    .Where(s => s.IsDeleted != true &&
                                s.ReleaseDate.HasValue &&
                                s.ReleaseDate.Value.Year == selectedYear &&
                                s.ReleaseDate.Value.Month == selectedMonth)
                    .OrderByDescending(s => s.Id)
                    .Select(s => new
                    {
                        Song = s,
                        ArtistName = context.ArtistGroups
                            .Where(a => a.Id == s.FkArtist)
                            .Select(a => a.Name)
                            .FirstOrDefault()
                    })
                    .ToList();

                var songViewList = new List<SongViewModel>();
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imagesDirectory = Path.Combine(exeDirectory, "assets", "SongImages");


                for (int i = 0; i < filteredList.Count; i++)
                {
                    var songData = filteredList[i];

                    //songViewList.Add(new MonthlySongViewModel
                    //{
                    //    SerialNumber = i + 1,
                    //    Id = songData.Song.Id,
                    //   // Name = songData.Song.Name,
                    //    FkArtist = songData.Song.FkArtist,
                    //    IsDeleted = songData.Song.IsDeleted,
                    //    Duration = $"{songData.Song.Minutes} min {songData.Song.Seconds} sec",
                    //    //ReleaseDate = songData.Song.ReleaseDate?.ToString("dd-MM-yyyy"),
                    //    CreatedAt = songData.Song.CreatedAt,
                    //    UpdatedAt = songData.Song.UpdatedAt,
                    //    Image = !string.IsNullOrEmpty(songData.Song.Image) ? Path.Combine(imagesDirectory, songData.Song.Image) : null,
                    //    ArtistName = songData.ArtistName
                    //});
                }

               // _filteredSongs = new ObservableCollection<SongViewModel>(songViewList);
               // _allSongs = new ObservableCollection<MonthlySongViewModel>(songViewList);
               // _filteredSongs = new ObservableCollection<MonthlySongViewModel>(_allSongs);
                _totalPages = (int)Math.Ceiling(_allSongs.Count / (double)PageSize);
                LoadCurrentPage(); // Refresh the UI
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", ex.Message, "OK");
        }
    }

    //public void LoadData()
    //{
    //    try
    //    {
    //        using (var context = new StowTownDbContext())
    //        {
    //            var songList = context.Songs
    //                .Where(s => s.IsDeleted != true)
    //                .OrderByDescending(s => s.Id)
    //                .Select(s => new
    //                {
    //                    Song = s,
    //                    ArtistName = context.ArtistGroups
    //                        .Where(a => a.Id == s.FkArtist)
    //                        .Select(a => a.Name)
    //                        .FirstOrDefault()
    //                })
    //                .ToList();

    //            var songViewList = new List<SongViewModel>();
    //            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
    //            string imagesDirectory = Path.Combine(exeDirectory, "assets", "SongImages");

    //            for (int i = 0; i < songList.Count; i++)
    //            {
    //                var songData = songList[i];

    //                songViewList.Add(new SongViewModel
    //                {
    //                    SerialNumber = i + 1,
    //                    Id = songData.Song.Id,
    //                    Name = songData.Song.Name,
    //                    FkArtist = songData.Song.FkArtist,
    //                    IsDeleted = songData.Song.IsDeleted,
    //                    Duration = $"{songData.Song.Minutes} min {songData.Song.Seconds} sec",
    //                    ReleaseDate = songData.Song.ReleaseDate?.ToString("dd-MM-yyyy"), // Formatting date safely
    //                    CreatedAt = songData.Song.CreatedAt,
    //                    UpdatedAt = songData.Song.UpdatedAt,
    //                    Image = !string.IsNullOrEmpty(songData.Song.Image) ? Path.Combine(imagesDirectory, songData.Song.Image) : null,
    //                    ArtistName = songData.ArtistName // Fetched in the query
    //                });
    //            }

    //            _allSongs = new ObservableCollection<SongViewModel>(songViewList);
    //            _filteredSongs = new ObservableCollection<SongViewModel>(_allSongs);
    //            _totalPages = (int)Math.Ceiling(_allSongs.Count / (double)PageSize);
    //            LoadCurrentPage();
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        DisplayAlert("Error", ex.Message, "OK");
    //    }
    //}



    private void LoadData()
    {
        try
        {
        
            if (YearPicker.SelectedItem == null || MonthPicker.SelectedItem == null)
                return; // Ensure both year and month are selected

            int selectedYear = int.Parse(YearPicker.SelectedItem.ToString());
            int selectedMonth = DateTime.ParseExact(MonthPicker.SelectedItem.ToString(), "MMMM", CultureInfo.InvariantCulture).Month;


            using (var context = new StowTownDbContext())
            {
                // Fetch Monthly Song List based on the selected month and year
                var monthlySongs = context.MonthlySongLists
                    .Where(ms => ms.IsDeleted  != true && ms.Date.HasValue
                                 && ms.Date.Value.Month == selectedMonth
                                 && ms.Date.Value.Year == selectedYear)
                    .ToList();

                // Fetch all song and artist data in a single query to avoid multiple database hits
                var songIds = monthlySongs.Select(ms => ms.FkSong).Distinct().ToList();
                var artistIds = monthlySongs.Select(ms => ms.FkArtist).Distinct().ToList();

                var songs = context.Songs
                    .Where(s => songIds.Contains(s.Id))
                    .ToDictionary(s => s.Id);

                var artists = context.ArtistGroups
                    .Where(a => artistIds.Contains(a.Id))
                    .ToDictionary(a => a.Id);

                var msongViewModelList = new List<MonthlySongViewModel>();
                string imagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "SongImages");

               // string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                //string imagesDirectory = Path.Combine(exeDirectory, "assets", "SongImages");

                for (int i = 0; i < monthlySongs.Count; i++)
                {
                    var msong = monthlySongs[i];
                    string formattedDate = msong.Date?.ToString("dd-MM-yyyy");

                    msongViewModelList.Add(new MonthlySongViewModel
                    {
                        SerialNumber = i + 1,
                        Id = msong.Id,
                        FkSong = msong.FkSong,
                        FkArtist = msong.FkArtist,
                        IsDeleted = msong.IsDeleted,
                        Date = formattedDate,
                        CreatedAt = msong.CreatedAt,
                        UpdatedAt = msong.UpdatedAt,
                        SongName = songs.ContainsKey((int)msong.FkSong) ? songs[(int)msong.FkSong].Name : "Unknown",
                        Duration = songs.ContainsKey((int)msong.FkSong) ? $"{songs[(int)msong.FkSong].Minutes} min {songs[(int)msong.FkSong].Seconds} sec" : "N/A",
                        //Image = songs.ContainsKey((int)msong.FkSong) && !string.IsNullOrEmpty(songs[(int)msong.FkSong].Image)
                        // ? Path.Combine(imagesDirectory, songs[(int)msong.FkSong].Image)
                        //: null,
                        // Image=!string.IsNullOrEmpty(songs[(int)msong.FkSong].Image) ? Path.Combine(imagesDirectory, songs[(int)msong.FkSong].Image) : null,
                        Image = !string.IsNullOrEmpty(songs[(int)msong.FkSong].Image) ?  ImageFilesService.GetImageUrl("SongsImages", songs[(int)msong.FkSong].Image) : null, 
                        ArtistName = artists.ContainsKey((int)msong.FkArtist) ? artists[(int)msong.FkArtist].Name : "Unknown"
                    });
                }

                // Sort list in descending order based on ID
                var sortedMSongList = msongViewModelList.OrderByDescending(a => a.Id).ToList();

                // Assign SerialNumber in ascending order based on sorted list
                for (int j = 0; j < sortedMSongList.Count; j++)
                {
                    sortedMSongList[j].SerialNumber = j + 1;
                }

                //_monthlysongCollection = new ObservableCollection<MonthlySongViewModel>(sortedMSongList);
                //dataGrid.ItemsSource = _monthlysongCollection;


                _allSongs = new ObservableCollection<MonthlySongViewModel>(sortedMSongList);
                _filteredSongs = new ObservableCollection<MonthlySongViewModel>(_allSongs);
                _totalPages = (int)Math.Ceiling(_allSongs.Count / (double)PageSize);
                LoadCurrentPage();
            }
        }
        catch (Exception ex)
        {
           // MessageBox.Show("Error: " + ex.Message);
        }
    }


    private void LoadCurrentPage()
    {
        _filteredSongs.Clear();
        var pageItems = _allSongs
            .Skip((_currentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        foreach (var item in pageItems)
        {
            _filteredSongs.Add(item);
        }
        SongList.ItemsSource = _filteredSongs; // Always update ListView
        UpdatePaginationControls();
    }

    private void UpdatePaginationControls()
    {
        PreviousButton.IsEnabled = _currentPage > 1;
        NextButton.IsEnabled = _currentPage < _totalPages;
        PageInfo.Text = $"Page {_currentPage} of {_totalPages}";
      //  NoResultsLabel.IsVisible = _filteredSongs.Count == 0;
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

    private  async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _searchText = e.NewTextValue?.ToLower().Trim(); // Handle null & trim spaces

        if (string.IsNullOrWhiteSpace(_searchText))
        {
            // Reset search -> Show all songs
            _filteredSongs = new ObservableCollection<MonthlySongViewModel>(_allSongs);
            PreviousButton.IsVisible = true;
            NextButton.IsVisible = true;
            PageInfo.IsVisible = true;
            IsEmptyPageInfo.IsVisible = false;
        }
        else
        {
            // Perform search
            var filteredList = _allSongs
                .Where(s => s.SongName.ToLower().Contains(_searchText) ||
                            (!string.IsNullOrEmpty(s.ArtistName) && s.ArtistName.ToLower().Contains(_searchText))) // Search by Artist Name
                .ToList();

            _filteredSongs.Clear(); // Ensure old data is cleared
            foreach (var item in filteredList)
            {
                _filteredSongs.Add(item);
            }
        }

        // Reset to first page
        _currentPage = 1;
        _totalPages = (int)Math.Ceiling(_filteredSongs.Count / (double)PageSize);

        if(_filteredSongs.Count == 0)
        {
            PreviousButton.IsVisible = false;
            NextButton.IsVisible = false;
            PageInfo.IsVisible = false;
            IsEmptyPageInfo.Text = "No Data Availabel";
            IsEmptyPageInfo.IsVisible = true;
            await Application.Current.MainPage.DisplayAlert("No Results", "No Songs found matching your search criteria", "Ok");
        }

        // Update ListView
        SongList.ItemsSource = null;
        SongList.ItemsSource = _filteredSongs;

        // Show "No Results Found" message only when no results exist
       // NoResultsLabel.IsVisible = _filteredSongs.Count == 0;
    }

    private async void OnViewButtonClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.CommandParameter is int selectedId)
        {
            await Shell.Current.GoToAsync($"{nameof(CreateMonthlySongList)}?SelectedId={selectedId}&Type=View");
        }
    }

    private  async void OnEditButtonClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.CommandParameter is int selectedId)
        {
            await Shell.Current.GoToAsync($"{nameof(CreateMonthlySongList)}?SelectedId={selectedId}&Type=Edit");
        }
    }

    private async void OnDeleteButtonClicked(object sender, EventArgs e)
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
                using (var context = new StowTownDbContext())
                {
                    if (sender is Button button && button.BindingContext is MonthlySongViewModel msongrow)
                    {
                        if (msongrow != null)
                        {
                            var getrowtodelete = await context.MonthlySongLists.FindAsync(msongrow.Id); // Assuming Id is the primary key
                            if (getrowtodelete != null)
                            {

                                getrowtodelete.IsDeleted = true;
                                await context.SaveChangesAsync();
                                await Application.Current.MainPage.DisplayAlert("Success", "Song Deleted Successfully..", "OK");
                                LoadData(); // Assuming this reloads your data
                            }
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

    private  async void OnCreateClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CreateMonthlySongList());
    }

    private async void SendMail_Click(object sender, EventArgs e)
    {
        try
        {
            // Fetch email recipients
            var recipients = GetRadioStationEmails();
            if (recipients.Count == 0)
            {
                await Shell.Current.DisplayAlert("Error", "No email addresses found.", "OK");
                return;
            }

            // Collect all email addresses in a list
            List<string> recipientEmails = recipients.Select(r => r.Email).ToList();

            //// Define the storage path in MAUI
            //string saveDirectory = FileSystem.Current.AppDataDirectory;
            //string fileName = $"{Guid.NewGuid()}_MonthlySongs.xlsx";
            //string filePath = Path.Combine(saveDirectory, fileName);

            // Export list data to Excel
            //ExportListToExcel(_filteredSongs, filePath);

            // await ExportDataGridToExcel(_filteredSongs, filePath, 0);

            //await SendEmailViaSMTP(recipientEmails, "Monthly Songs Report", "Please find the attached report.", filePath);


            //foreach (var recipient in recipients)
            //{
            //    // Define the storage path in MAUI with a unique filename for each recipient
            //    string saveDirectory = FileSystem.Current.AppDataDirectory;
            //    string fileName = $"{Guid.NewGuid()}_MonthlySongs_Recipient{recipient.Id}.xlsx";
            //    string filePath = Path.Combine(saveDirectory, fileName);

            //    // Export list data to Excel, including the recipient's ID
            //    await ExportDataGridToExcel(_filteredSongs, filePath, recipient.Id);

            //    // Send the email with the attachment to the current recipient
            //    await SendEmailViaSMTP(recipient.Email, "Monthly Songs Report", "Please find the attached report.", filePath);

            //}
            //await Shell.Current.DisplayAlert("Success", "Email sent successfully!", "OK");

            int successCount = 0;

            foreach (var recipient in recipients)
            {
                try
                {
                    string saveDirectory = FileSystem.Current.AppDataDirectory;
                    string fileName = $"{Guid.NewGuid()}_MonthlySongs_Recipient{recipient.Id}.xlsx";
                    string filePath = Path.Combine(saveDirectory, fileName);

                    await ExportDataGridToExcel(_filteredSongs, filePath, recipient.Id);
                   // await SendEmailViaSMTP(recipient.Email, "Monthly Songs Report", "Please find the attached report.", filePath);

                    EmailService.SendEmailViaSMTP(recipient.Email, "Monthly Songs Report", "Please find the attached report.", filePath);
                    successCount++;
                }
                catch (Exception ex)
                {
                    // Optional: log individual recipient error
                    Console.WriteLine($"Failed to send email to {recipient.Email}: {ex.Message}");
                }
            }

            await Shell.Current.DisplayAlert("Success", $"{successCount} of {recipients.Count} Emails sent Successfully.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }



    public async Task ExportDataGridToExcel(ObservableCollection<MonthlySongViewModel> items, string filePath, int id)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Use "Commercial" if it's for business
        using (ExcelPackage package = new ExcelPackage())
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Monthly Songs");

            // Leave first row empty and add heading in the second row
            var headingCell = worksheet.Cells["A2"];
            headingCell.Value = "Please click on Enable Editing to modify data in the cells.";
            headingCell.Style.Font.Size = 14;
            headingCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            headingCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            headingCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headingCell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);

            worksheet.Cells[2, 1, 2, 6].Merge = true; // Adjusted merge range

            // Hidden identifier in third row
            var idCell = worksheet.Cells["A3"];
            idCell.Value = id;
            idCell.Style.Font.Color.SetColor(System.Drawing.Color.White);

            // Define column headers
            string[] columnHeaders = { "Song Name", "Artist", "Year", "Spin", "Position", "Rotation/Notes" };
            for (int i = 0; i < columnHeaders.Length; i++)
            {
                worksheet.Cells[4, i + 1].Value = columnHeaders[i];
                worksheet.Cells[4, i + 1].Style.Font.Bold = true;
                worksheet.Cells[4, i + 1].Style.Font.Size = 11;
            }

            using (var context = new StowTownDbContext())
            {
                for (int i = 0; i < items.Count; i++)
                {
                    var row = items[i];
                    if (row == null) continue;

                    var rowId = row.Id;
                    var existingData = context.SongPossitions.FirstOrDefault(sp => sp.FkMonthlySongList == rowId);
                    var propertiesToInclude = new[] { "Id" };
                    for (int j = 0; j < propertiesToInclude.Length; j++)
                    {
                        var prop = row.GetType().GetProperty(propertiesToInclude[j]);
                        if (prop != null)
                        {
                            var value = prop.GetValue(row);
                            worksheet.Cells[i + 5, 8].Value = value;
                            worksheet.Cells[i + 5, 8].Style.Font.Color.SetColor(System.Drawing.Color.Black);

                            // Color styling for first column (Id)
                            if (j == 0)
                            {
                                worksheet.Cells[i + 5, j + 1].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            }
                        }
                    }


                    // worksheet.Cells[i + 5, 1].Value = row.SongName;
                    // worksheet.Cells[i + 5, 2].Value = row.ArtistName;
                    //// worksheet.Cells[i + 5, 3].Value = row.Duration;
                    // worksheet.Cells[i + 5, 4].Value = row.Date;

                    // if (existingData != null)
                    // {
                    //     worksheet.Cells[i + 5, 5].Value = existingData.Spins;
                    //     worksheet.Cells[i + 5, 6].Value = existingData.Possition;
                    //     worksheet.Cells[i + 5, 7].Value = existingData.RotationNotes;
                    // }

                    worksheet.Cells[i + 5, 1].Value = row.SongName;
                    worksheet.Cells[i + 5, 2].Value = row.ArtistName;
                    // worksheet.Cells[i + 5, 3].Value = row.Duration;
                    worksheet.Cells[i + 5, 3].Value = row.Date;

                    if (existingData != null)
                    {
                        worksheet.Cells[i + 5, 4].Value = existingData.Spins;
                        worksheet.Cells[i + 5, 5].Value = existingData.Possition;
                        worksheet.Cells[i + 5, 6].Value = existingData.RotationNotes;
                    }
                }
            }

            // Lock all cells
            worksheet.Cells.Style.Locked = true;

            // Unlock Spin, Position, and Rotation/Notes columns
            for (int i = 1; i <= items.Count; i++)
            {
                //worksheet.Cells[i + 4, 5].Style.Locked = false;
                //worksheet.Cells[i + 4, 6].Style.Locked = false;
                //worksheet.Cells[i + 4, 7].Style.Locked = false;
                worksheet.Cells[i + 4, 4].Style.Locked = false;
                worksheet.Cells[i + 4, 5].Style.Locked = false;
                worksheet.Cells[i + 4, 6].Style.Locked = false;
            }

            worksheet.Protection.IsProtected = true;
            worksheet.Protection.SetPassword("ncbl bwnv xzxf kxdj");

            FileInfo fileInfo = new FileInfo(filePath);
            package.SaveAs(fileInfo);
        }
    }

    


   
    private List<RadioStation> GetRadioStationEmails()
    {
        using (var context = new StowTownDbContext())
        {
            // Fetch emails and IDs from the RadioStation table
            var emailList = context.RadioStations
                .Where(rs => !string.IsNullOrEmpty(rs.Email) && rs.IsActive == true)
                .Select(rs => new RadioStation
                {
                    Email = rs.Email,
                    Id = rs.Id
                })
                .ToList();

            return emailList;
        }
    }
    

    private readonly string _smtpServer = "smtp.gmail.com";  // Change as needed
    private readonly int _smtpPort = 587;
    private readonly string _smtpUser = "Sharmapriya3099@gmail.com";
    private readonly string _smtpPassword = "ncbl bwnv xzxf kxdj";  // Use App Password for security

    public async Task OpenEmailClient(string recipient, string subject, string body, string filePath = null)
    {
        try
        {
            var message = new EmailMessage
            {
                Subject = subject,
                Body = body,
                To = new List<string> { recipient }
            };

            // Attach file if it exists
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                message.Attachments.Add(new EmailAttachment(filePath));
            }

            await Email.Default.ComposeAsync(message);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    /// <summary>
    /// Sends an email using SMTP with an attachment
    /// </summary>
    public async Task SendEmailViaSMTP(string recipient, string subject, string body, string filePath = null)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Your App", _smtpUser));
            email.To.Add(new MailboxAddress("Recipient", recipient));
            email.Subject = subject;

            var textBody = new TextPart("plain") { Text = body };

            var multipart = new Multipart("mixed") { textBody };

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                var attachment = new MimePart("application", "octet-stream")
                {
                    Content = new MimeContent(File.OpenRead(filePath), ContentEncoding.Default),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = Path.GetFileName(filePath)
                };
                multipart.Add(attachment);
            }

            email.Body = multipart;

            using (var smtpClient = new MailKit.Net.Smtp.SmtpClient())
            {
                await smtpClient.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_smtpUser, _smtpPassword);
                await smtpClient.SendAsync(email);
                await smtpClient.DisconnectAsync(true);
            }

            //await Application.Current.MainPage.DisplayAlert("Success", "Email Sent Successfully!", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }


    public async Task SendEmailViaSMTP(List<string> recipients, string subject, string body, string filePath = null)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Your App", _smtpUser));

            //  Add Multiple Recipients
            foreach (var recipient in recipients)
            {
                email.To.Add(new MailboxAddress(recipient, recipient));
            }

            email.Subject = subject;

            var textBody = new TextPart("plain") { Text = body };

            var multipart = new Multipart("mixed") { textBody };

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                var attachment = new MimePart("application", "octet-stream")
                {
                    Content = new MimeContent(File.OpenRead(filePath), ContentEncoding.Default),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = Path.GetFileName(filePath)
                };
                multipart.Add(attachment);
            }

            email.Body = multipart;

            using (var smtpClient = new MailKit.Net.Smtp.SmtpClient())
            {
                await smtpClient.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_smtpUser, _smtpPassword);
                await smtpClient.SendAsync(email);
                await smtpClient.DisconnectAsync(true);
            }

            await Application.Current.MainPage.DisplayAlert("Success", "Email Sent Successfully!", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }


}