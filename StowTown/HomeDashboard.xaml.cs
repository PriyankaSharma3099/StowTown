namespace StowTown;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using StowTown.HelperService;
using StowTown.Models;
using StowTown.Pages.CallHistory;
using StowTown.Services.SaveImageService;
using StowTown.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class HomeDashboard : ContentPage, INotifyPropertyChanged
{

    // Add the missing field declaration for 'IsCallingChecked' to resolve CS0103.  
    private bool _isCallingChecked;
    public List<RadioStation> radioStation { get; set; }

    public ObservableCollection<MonthlySongViewModel> MonthlyTopSongs { get; set; } = new ObservableCollection<MonthlySongViewModel>();
    public ObservableCollection<MonthlySongViewModel> MonthlyTopArtist { get; set; } = new ObservableCollection<MonthlySongViewModel>();
    public ObservableCollection<SongPositionViewModel> MusicList { get; set; } = new ObservableCollection<SongPositionViewModel>();

    public ObservableCollection<CallRecordViewModel> CallList { get; set; } = new ObservableCollection<CallRecordViewModel>();

    public static bool isInitializing = false;

    private bool _ischecked;
    public bool isChecked
    {
        get => _ischecked;
        set
        {
            if (_ischecked != value)
            {
                _ischecked = value;
                OnPropertyChanged(nameof(isChecked));
            }
        }
    }
    //private bool _enabled;
    //public bool Enabled
    //{
    //    get => _enabled;
    //    set
    //    {
    //        if (_enabled != value)
    //        {
    //            _enabled = value;
    //          //  OnPropertyChanged(nameof(Enabled));
    //        }
    //    }
    //}

    private bool isInternalCheckChange = true;
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    //bool _suppressCheckedEvents = false;

    private readonly StowTownDbContext _context;

    public HomeDashboard()
    {
        isInitializing = true;

        InitializeComponent();
        // When the page appears, start loading
        this.Loaded += HomeDashboard_Loaded;
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StowTownLogs");
        Directory.CreateDirectory(folder);

        string filePath = Path.Combine(folder, $"startup-error-{DateTime.Now:yyyyMMdd-HHmmss}.log");

        // Check if file exists
        if (File.Exists(filePath))
        {
            File.AppendAllText(filePath, "Set Up home Page Success" + Environment.NewLine);
        }
        else
        {
            File.WriteAllText(filePath, "Set Up home Page Success" + Environment.NewLine);
        }

    }
    private async void HomeDashboard_Loaded(object sender, EventArgs e)
    {
        // Simulate some loading work
        await Task.Delay(2000); // Replace with real data load

        // After loading, show content and hide loader
        LoaderOverlay.IsVisible = false;
        MainContent.IsVisible = true;
    }


    //public HomeDashboard(StowTownDbContext context)
    //{
    //    InitializeComponent();
    //    _context = context;
    //}
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        CurrentUser();
        TodayCallList();
        MonthlySongList();
        CallList = new ObservableCollection<CallRecordViewModel>();
        BindingContext = new HomeDashboardViewModel();
        //var currentDate = DateTime.UtcNow;
        //var lastMonthStart = new DateTime(currentDate.Year, currentDate.Month, 1);
        var currentDate = DateTime.UtcNow;
        var lastMonth = currentDate.AddMonths(-1);
        var lastMonthStart = new DateTime(lastMonth.Year, lastMonth.Month, 1);
        LoadMonthlyTopData(lastMonthStart);
        // isInternalCheckChange = false; // <-- End suppression AFTER data is loaded
        // GetRadioStationsForToday();
        BindingContext = this.BindingContext;
        //  var callmodel = new CallRecordViewModel();
        //callmodel._suppressCheckedEvents =  true;
        GetRadioStationsForToday();
        // isInitializing = false;
        var app = new AppShell();
        app.NavigatedFrom += OnAppNavigatedFrom; // Correctly subscribe to the event  
    }

    private void OnAppNavigatedFrom(object? sender, NavigatedFromEventArgs e)
    {
        base.OnNavigatedFrom(e);

        string route = Shell.Current.CurrentItem?.Route ?? "";

        switch (route)
        {
            case "HomeDashboard":
                ShellHelper.UpdateFlyoutItemTitle("HomeDashboard", "Home", "assets/Home.png");
                break;
            case "RadioStationManagement":
                ShellHelper.UpdateFlyoutItemTitle("RadioStationManagement", "Radio Station", "assets/music.png");
                break;
            case "DjManagement":
                ShellHelper.UpdateFlyoutItemTitle("DjManagement", "Dj", "assets/headphones.png");
                break;
            case "ArtistManagement":
                ShellHelper.UpdateFlyoutItemTitle("ArtistManagement", "Artist", "assets/user.png");
                break;
            case "Graph":
                ShellHelper.UpdateFlyoutItemTitle("Graph", "Reporting", "assets/Icons.png");
                break;
            case "SongManagement":
                ShellHelper.UpdateFlyoutItemTitle("SongManagement", "Song List", "assets/music.png");
                break;
            case "MonthlySongManagement":
                ShellHelper.UpdateFlyoutItemTitle("MonthlySongManagement", "Monthly Song List", "assets/filemusic.png");
                break;
            case "CallScheduleList":
                ShellHelper.UpdateFlyoutItemTitle("CallScheduleList", "Call Schedule", "assets/Icons.png");
                break;
            case "PojectProducerManagement":
                ShellHelper.UpdateFlyoutItemTitle("PojectProducerManagement", "Producer", "assets/radio.png");
                break;
            case "ManualSongSpins":
                ShellHelper.UpdateFlyoutItemTitle("ManualSongSpins", "Manual Spin Tracker", "assets/music.png");
                break;
        }
    }

    private void TodayCallList()
    {
        using (var context = new StowTownDbContext())
        {
            var callList = context.CallRecords.Where(c => c.CreatedAt == DateTime.Now.Date).ToList();
            if (callList != null)
            {

            }
        }
    }

    private void MonthlySongList()
    {
        using (var context = new StowTownDbContext())
        {
            var currentDate = DateTime.UtcNow;
            var lastMonth = currentDate.AddMonths(-1);
            var monthlylist = context.MonthlySongLists
                .Where(m => m.IsDeleted != true && m.Date.HasValue && m.Date.Value.Month == lastMonth.Month)
                .ToList();
            if (monthlylist != null)
            {
                MonthlySongCount.Text = monthlylist.Count.ToString();
            }
        }
    }

    public List<CallRecordViewModel> GetRadioStationsForToday()
    {
        //isInternalCheckChange = true;
        int todayDayNumber = (int)DateTime.Now.DayOfWeek; // Today's day of the week
        DateTime startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1); // First day of the current month
        DateTime todayDate = DateTime.Now.Date;

        using (var context = new StowTownDbContext())
        {
            string parentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string imagesDirectory = System.IO.Path.Combine(parentDirectory, "assets", "Images", "RadioStation");

            var radioStations =
                from rs in context.RadioStations
                join wr in context.WeekRadioTimes on rs.Id equals wr.FkRadio
                join t in context.Timings on wr.FkTiming equals t.Id
                where wr.WeekDay == todayDayNumber && rs.IsActive == true && rs.IsDeleted != true
                select new
                {
                    rs.Id,
                    rs.Name,
                    rs.Image,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime,
                    LastCallInMonth = context.CallRecords
                                         .Where(cr => cr.FkRadioStation == rs.Id && cr.CreatedAt >= todayDate && cr.IsDeleted != true)
                                         .OrderByDescending(cr => cr.CreatedAt)
                                         .FirstOrDefault()
                };

            var radioactivelist = radioStations.Select(rs => new CallRecordViewModel
            {
                Id = rs.LastCallInMonth != null ? rs.LastCallInMonth.Id : rs.Id,
                RadioStationName = rs.Name,
              //  Image = rs.Image != null ? Path.Combine(imagesDirectory, rs.Image) : null,
                Image = rs.Image != null ?  ImageFilesService.GetImageUrl("RadioStationImages", rs.Image) : null,
                Timing = $"{rs.StartTime:hh\\:mm tt} - {rs.EndTime:hh\\:mm tt}",
                Checked = rs.LastCallInMonth != null ? rs.LastCallInMonth.IsChecked.Value : false, // Checked if a call exists in the current month
                                                                                                   //            IsEnabled = rs.LastCallInMonth != null
                                                                                                   //? !rs.LastCallInMonth.IsChecked.Value   // invert the original value
                                                                                                   //: false, // Enabled only if no call exists in the current month
                                                                                                   // _suppressCheckedEvents = true,

                IsEnabled = rs.LastCallInMonth != null ? !rs.LastCallInMonth.IsChecked.Value   : true, 

            }).ToList();

            int callcount = radioactivelist.Count(r => !r.Checked == true);
            CallCount.Text = callcount.ToString();


            CallList.Clear();
            foreach (var item in radioactivelist)
            {
                CallList.Add(item); //  UI update hoga
            }
            CallListControl.ItemsSource = CallList;


            //isInternalCheckChange = false;
            //isInternalCheckChange = true;


            return radioactivelist;

        }
    }
    private void LoadMonthlyTopData(DateTime month)
    {
        using (var context = new StowTownDbContext())
        {
            var songPosition = context.SongPossitions.ToList();

            GetMonthlyTopSongs(month);
            GetMonthlyTopArtists(month);
            GetSongsSortedBySpinsForCurrentMonth(month);
            //var songs = GetSongsSortedBySpinsForCurrentMonth(DateTime.Now);

            //MusicList.Clear();
            //foreach (var song in songs)
            //{
            //    MusicList.Add(song);
            //}
            //SongGrid.ItemsSource = GetSongsSortedBySpinsForCurrentMonth(month);
            //CallList.ItemsSource = GetRadioStationsForToday();
        }

    }
    public List<SongPositionViewModel> GetMonthlyTopSongs(DateTime month)
    {
        using (var context = new StowTownDbContext())
        {
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string imagesDirectory = Path.Combine(exeDirectory, "assets", "SongImages");

            var songList = context.MonthlySongLists
            .Where(msl => msl.Date.HasValue && msl.Date.Value.Month == month.Month && msl.Date.Value.Year == month.Year)
            .SelectMany(msl => msl.SongPossitions)
            .GroupBy(sp => new
            {
                sp.FkSong,
                sp.FkSongNavigation.Name,
                sp.FkSongNavigation.Image,
                sp.FkSongNavigation.FkArtistNavigation.GroupTitle
            })
            .Select(g => new SongPositionViewModel
            {
                FkSong = g.Key.FkSong.Value,
                SongName = g.Key.Name,
              //  SongImage = g.Key.Image != null ? Path.Combine(imagesDirectory, g.Key.Image) : null, // Full path to the image,
                 SongImage=g.Key.Image !=null?ImageFilesService.GetImageUrl("SongsImages",g.Key.Image):null,
                ArtistName = g.Key.GroupTitle,
                Spins = g.Sum(sp => sp.Spins ?? 0),
                Date = month.ToString("MMMM yyyy"),
                Possition = g.OrderBy(sp => sp.Possition).FirstOrDefault().Possition
            })

            .OrderByDescending(song => song.Spins)
            .Take(5) // Top 5 songs
            .ToList();
            if (songList != null)
            {
                string imagePath = string.Empty;
                songList.ForEach(song =>
                {
                    if (song.SongImage != null)
                    {
                        if (File.Exists(song.SongImage))
                        {
                            imagePath = song.SongImage;
                        }
                    }
                });
            }
            MonthlyTopSongsControl.ItemsSource = songList;
            return songList;
        }
    }

    public List<SongPositionViewModel> GetMonthlyTopArtists(DateTime month)
    {
        using (var context = new StowTownDbContext())
        {

            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string imagesDirectory = Path.Combine(exeDirectory, "assets", "ArtistImages");

            var artistList = context.MonthlySongLists
           .Where(msl => msl.Date.HasValue && msl.Date.Value.Month == month.Month && msl.Date.Value.Year == month.Year && msl.IsDeleted != true)
           .SelectMany(msl => msl.SongPossitions)
           .GroupBy(sp => new
           {
               sp.FkSongNavigation.FkArtistNavigation.Id,
               sp.FkSongNavigation.FkArtistNavigation.Name,
               sp.FkSongNavigation.FkArtistNavigation.GroupPicture
           })
           .Select(g => new SongPositionViewModel
           {
               FkSong = g.Key.Id,
               ArtistName = g.Key.Name,
               // ArtistImage = g.Key.GroupPicture != null ? Path.Combine(imagesDirectory, g.Key.GroupPicture) : null, // Full path to the image,
               ArtistImage = g.Key.GroupPicture != null ? ImageFilesService.GetImageUrl("ArtistImages",g.Key.GroupPicture) : null,
               Date = month.ToString("MMMM yyyy"),
               Spins = g.Sum(sp => sp.Spins ?? 0)
           })
           .OrderByDescending(artist => artist.Spins)
           .Take(3) // Top 3 artists
           .ToList();
            MonthlyTopArtistControl.ItemsSource = artistList;
            return artistList;
        }

    }

    public List<SongPositionViewModel> GetSongsSortedBySpinsForCurrentMonth(DateTime month)
    {
        using (var context = new StowTownDbContext())
        {

            var songs = context.SongPossitions
                .Include(sp => sp.FkSongNavigation) // Include Song details
                .Include(sp => sp.FkRadioStationNavigation) // Include Radio Station details if needed
                .Where(sp => sp.FkMonthlySongListNavigation != null &&
                             sp.FkMonthlySongListNavigation.Date.HasValue &&
                             sp.FkMonthlySongListNavigation.Date.Value.Month == month.Month &&
                             sp.FkMonthlySongListNavigation.Date.Value.Year == month.Year) // Filter by current month and year
                .OrderByDescending(sp => sp.Spins) // Sort by spins in descending order
                .Select(sp => new SongPositionViewModel
                {
                    SerialNumber = 0, // Serial number will be set later
                    FkSong = sp.FkSong,
                    FkRadioStation = sp.FkRadioStation,
                    FkMonthlySongList = sp.FkMonthlySongList,
                    Spins = sp.Spins,
                    SongName = sp.FkSongNavigation.Name, // Assuming `Name` is the property for the song title
                    ArtistName = sp.FkSongNavigation.FkArtistNavigation.Name // Assuming Song -> Artist relation
                                                                             //Label = sp.FkSongNavigation?.Label // Assuming `Label` is part of the Song entity
                })
                .ToList();

            // Add serial numbers
            for (int i = 0; i < songs.Count; i++)

            {
                songs[i].SerialNumber = i + 1;
            }

            MonthlyMusicControl.ItemsSource = songs;
            return songs;
        }
    }



    private void OnSettingsClicked(object sender, EventArgs e)
    {

    }

    private  void OnChecked_TodayCall(object sender, CheckedChangedEventArgs e)
    {
        //if (_suppressCheckedEvents)
        //    return; // Ignore changes made by the code itself

        if (sender is CheckBox checkBox && checkBox.BindingContext is CallRecordViewModel station)
        {
            System.Diagnostics.Debug.WriteLine((sender as CheckBox)?.BindingContext?.GetType().Name);
            if (station._suppressCheckedEvents == false)
                return;
            var cb = (CheckBox)sender;
            int selectedId = station.Id; //  Get the ID from the data item

             Shell.Current.GoToAsync($"{nameof(CreateCallHistory)}?SelectedId={selectedId}&Type=Create");
            GetRadioStationsForToday();

        }
        GetRadioStationsForToday();
        // Check if unchecked

    }
    // Optional: track selected items using selected
 


  

    public void ReloadRadioStations()
    {
        GetRadioStationsForToday();

        
    }

    private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        //if (isInternalCheckChange)
        //{
        //    isInternalCheckChange = false;
        //    return;
        //}
        if (isInitializing ==true)
        {
            return;
        }
            if (sender is CheckBox checkBox )
        {
            //if (GetRadioStationsForToday() != null)
            //{
            var station = (CallRecordViewModel)checkBox.BindingContext;
            System.Diagnostics.Debug.WriteLine((sender as CheckBox)?.BindingContext?.GetType().Name);
            //if (station._suppressCheckedEvents == false)
            //    return;
            var cb = (CheckBox)sender;
            int selectedId = station.Id; //  Get the ID from the data item
            Shell.Current.GoToAsync($"{nameof(CreateCallHistory)}?SelectedId={selectedId}&Type=Create");
        }
        //GetRadioStationsForToday();



        }



    private void CurrentUser()
    {
        if (GlobalUserInfo.CurrentUser != null)
        {
            var user = GlobalUserInfo.CurrentUser.Email;
            if (user != null)
            {
                using (var context = new StowTownDbContext())
                {
                    var userInfo = context.Users.FirstOrDefault(u => u.UserName == user && u.IsDeleted==false);
                    if (userInfo != null)
                    {
                        // Update the UI element with the user's name
                      CurrentUserLabel.Text = $"Welcome {userInfo.FirstName} {userInfo.LastName}";
                    }
                }
            }
        }
    }

    private void OnCheckBoxTapped(object sender, TappedEventArgs e)
    {
        if (sender is CheckBox checkBox)
        {
            // Optionally toggle manually if needed:
            checkBox.IsChecked = !checkBox.IsChecked;

            if (checkBox.BindingContext is CallRecordViewModel station)
            {
                int selectedId = station.Id;
                Shell.Current.GoToAsync($"{nameof(CreateCallHistory)}?SelectedId={selectedId}&Type=Create");
            }
        }
    
    }
}