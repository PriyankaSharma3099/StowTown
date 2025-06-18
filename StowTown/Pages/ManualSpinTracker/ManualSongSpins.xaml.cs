using Microsoft.EntityFrameworkCore;
using StowTown.Models;
using StowTown.Services.SaveImageService;
using StowTown.ViewModels;
using System.Collections.ObjectModel;
using System.Globalization;


namespace StowTown.Pages.ManualSpinTracker;

public partial class ManualSongSpins : ContentPage
{
    private ObservableCollection<RadioStationViewModel> _radioStations;
    private ObservableCollection<SongPositionViewModel> _songList;
    public ObservableCollection<SongPositionViewModel> MonthlySongs { get; set; } = new ObservableCollection<SongPositionViewModel>();

    private int _currentPage = 1;
    private int _itemsPerPage = 10; // Customize this as needed

    public ObservableCollection<SongPositionViewModel> PagedSongs { get; set; } = new ObservableCollection<SongPositionViewModel>();
    public string SearchQuery { get; set; } = string.Empty;
    public ManualSongSpins()
	{
		InitializeComponent();
        BindingContext = this;

    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Perform any additional setup or data loading here
        // For example, you can load the list of manual spins from a database or API
        LoadManualSpins();
    }
    private void LoadManualSpins()
    {
        int currentYear = DateTime.Now.Year;
        int startYear = currentYear - 1;
        int endYear = currentYear + 10;

        // Calculate count correctly: (end - start + 1)
        var years = Enumerable.Range(startYear, endYear - startYear + 1).ToList();

        YearPicker.ItemsSource = years;
        YearPicker.SelectedItem = currentYear; // Optional default selection

        // Example: Populate Months
        var months = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames
                          .Where(m => !string.IsNullOrEmpty(m))
                          .ToList();
        MonthPicker.ItemsSource = months;

        // Auto-select current month
        string currentMonth = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.UtcNow.Month);
        MonthPicker.SelectedItem = currentMonth;

        BindRadioStation();
    }

    private async Task BindRadioStation()
    {
        try             
        {
            using (var context = new StowTownDbContext())
            {
                var stations = await context.RadioStations
                    .Where(s => s.IsDeleted != true && s.IsActive == true)
                    .Select(s => new RadioStationViewModel
                    {
                        Id = s.Id,
                        Name = s.Name
                    })
                    .ToListAsync();

                _radioStations = new ObservableCollection<RadioStationViewModel>(stations);
                RadioStationPicker.ItemsSource = _radioStations;
              //  RadioStationPicker.SelectedIndex = 0; // Optionally set a default selection
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions, e.g., log the error or show an alert
            await DisplayAlert("Error", "Failed to load radio stations: " + ex.Message, "OK");
        }
    }

    private async Task BindSongOnRadioStationSelected()
    {
        try
        {
            using (var context = new StowTownDbContext())
            {
                // Validate UI selection
                if (YearPicker.SelectedItem == null || MonthPicker.SelectedItem == null || RadioStationPicker.SelectedItem == null)
                    return;

                int selectedYear = int.Parse(YearPicker.SelectedItem.ToString());
                int selectedMonth = DateTime.ParseExact(MonthPicker.SelectedItem.ToString(), "MMMM", CultureInfo.InvariantCulture).Month;

                var selectedStation = RadioStationPicker.SelectedItem as RadioStationViewModel;
                int selectedRadioId = selectedStation?.Id ?? 0;

                // 1. Get all MonthlySongList entries matching month & year
                var monthlyLists = await context.MonthlySongLists
                    .Where(s => s.IsDeleted != true &&
                                s.Date.HasValue &&
                                s.Date.Value.Year == selectedYear &&
                                s.Date.Value.Month == selectedMonth)
                    .OrderByDescending(s => s.Id)
                    .ToListAsync();

                if (!monthlyLists.Any())
                {
                    await DisplayAlert("Info", "No Monthly Song List found for selected year/month.", "OK");
                    return;
                }

                // 2. Prepare result list outside loop
                var allSongViewModels = new List<SongPositionViewModel>();

                foreach (var list in monthlyLists)
                {
                    int monthlyListId = list.Id;

                    // 3. Get all song IDs in this list
                    var songIds = await context.SongPossitions
                        .Where(p => p.FkMonthlySongList == monthlyListId)
                        .Select(p => p.FkSong)
                        .Distinct()
                        .ToListAsync();

                    // 4. Get song details
                    var songMasterList = await context.Songs
                        .Where(s => songIds.Contains(s.Id))
                        .ToListAsync();

                    // 5. Get spin data for this radio station
                    var existingPositions = await context.SongPossitions
                        .Where(p => p.FkMonthlySongList == monthlyListId && p.FkRadioStation == selectedRadioId)
                        .ToListAsync();

                    // 6. Merge spin data with song details
                    var songViewModels = songMasterList.Select(song =>
                    {
                        var match = existingPositions.FirstOrDefault(p => p.FkSong == song.Id);

                        return new SongPositionViewModel
                        {
                            FkSong = song.Id,
                            SongName = song.Name,
                            Duration = $"{song.Minutes} min {song.Seconds} sec",
                            Spins = match?.Spins,
                            Possition = match?.Possition,
                            RotationNotes = match?.RotationNotes,
                            FkRadioStation = selectedRadioId,
                            FkMonthlySongList = monthlyListId,
                            Id = match?.Id ?? 0,
                            Image = !string.IsNullOrEmpty(song.Image) ? ImageFilesService.GetImageUrl("SongsImages", song.Image) : null,
                        };
                    }).ToList();

                    // 7. Add to global list
                    allSongViewModels.AddRange(songViewModels);
                }

                if (allSongViewModels == null || allSongViewModels.Count == 0)
                {
                    await DisplayAlert("Info", "No monthly song data found for this Radio Station.", "OK");
                    return;
                }
                // Assign serial numbers
                int serial = 1;
                foreach (var song in allSongViewModels)
                {
                    song.SerialNumber = serial++;
                }

                // Bind to UI
                MonthlySongs.Clear();
                foreach (var song in allSongViewModels)
                    MonthlySongs.Add(song);
                ApplySearchAndPagination();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Failed to load songs: " + ex.Message, "OK");
        }
    }


    private async Task BindSongOnMonthSelected()
    {
        try
        {
            using (var context = new StowTownDbContext())
            {
                if (YearPicker.SelectedItem == null || MonthPicker.SelectedItem == null)
                    return; // Ensure both year and month are selected  

                int selectedYear = int.Parse(YearPicker.SelectedItem.ToString());
                int selectedMonth = DateTime.ParseExact(MonthPicker.SelectedItem.ToString(), "MMMM", CultureInfo.InvariantCulture).Month;
                RadioStation selectedStation = RadioStationPicker.SelectedItem as RadioStation;
                int selectedRadioId = 0;
                if (selectedStation != null)
                {
                    selectedRadioId = selectedStation.Id;
                }

                var selectedmonthlySongData = await context.MonthlySongLists
                    .Where(s => s.IsDeleted != true &&
                                s.Date.HasValue &&
                                s.Date.Value.Year == selectedYear &&
                                s.Date.Value.Month == selectedMonth)
                    .OrderByDescending(s => s.Id)
                    .Select(s => new SongPositionViewModel
                    {
                        Id = s.Id,
                        FkSong = s.FkSong,
                        SongName = context.Songs.Where(song => song.Id == s.FkSong).Select(song => song.Name).FirstOrDefault(),
                        Spins = 0,
                        RotationNotes = " ",
                        Possition = 0,
                        Image = context.Songs.Where(song => song.Id == s.FkSong).Select(song => song.Image).FirstOrDefault() != null
                            ? ImageFilesService.GetImageUrl("SongsImages", context.Songs.Where(song => song.Id == s.FkSong).Select(song => song.Image).FirstOrDefault())
                            : null,
                    })
                .ToListAsync();

                if (selectedmonthlySongData == null || selectedmonthlySongData.Count == 0)
                {
                    await DisplayAlert("Info", "No monthly song data found for this date.", "OK");
                    return;
                }

                //MonthlySongs.Clear();
                //foreach (var song in selectedmonthlySongData)
                //    MonthlySongs.Add(song);

                // Assign serial numbers
                int serial = 1;
                foreach (var song in selectedmonthlySongData)
                {
                    song.SerialNumber = serial++;
                }

                // Bind to UI
                MonthlySongs.Clear();
                foreach (var song in selectedmonthlySongData)
                    MonthlySongs.Add(song);

                ApplySearchAndPagination(); // Call after binding
            }

        }
        catch (Exception ex)
        {
            // Handle exceptions, e.g., log the error or show an alert  
            await DisplayAlert("Error", "Failed to load songs: " + ex.Message, "OK");
        }
    }

    private async void ApplySearchAndPagination()
    {
        var filteredSongs = string.IsNullOrWhiteSpace(SearchQuery)
            ? MonthlySongs
            : new ObservableCollection<SongPositionViewModel>(
                MonthlySongs.Where(s => s.SongName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)));

        if (filteredSongs.Count == 0)
        {
            PagedSongs.Clear();
            PageInfoNotFound.Text = "No Data Available ";
            PageInfoNotFound.TextColor = Colors.Black;
            PageInfoNotFound.IsVisible = true;
            PreviousButton.IsVisible = false;
            NextButton.IsVisible = false;
            PageInfo.Text = string.Empty; // Clear page info if no songs found
            await Application.Current.MainPage.DisplayAlert("Info", "No Songs found matching the search.", "OK");
            return;
        }
        else
        {
            PageInfoNotFound.IsVisible = false;
            PreviousButton.IsVisible = true;
            NextButton.IsVisible = true;
            PageInfoNotFound.Text = string.Empty; // Clear not found message
        }

        var totalItems = filteredSongs.Count;
        var totalPages = (int)Math.Ceiling((double)totalItems / _itemsPerPage);
        _currentPage = Math.Max(1, Math.Min(_currentPage, totalPages));

        var paged = filteredSongs
            .Skip((_currentPage - 1) * _itemsPerPage)
            .Take(_itemsPerPage)
            .ToList();

        PagedSongs.Clear();
        foreach (var item in paged)
            PagedSongs.Add(item);

        PageInfo.Text = $"Page {_currentPage} of {totalPages}";
        PreviousButton.IsEnabled = _currentPage > 1;
        NextButton.IsEnabled = _currentPage < totalPages;
    }


    private void OnCancelClicked(object sender, EventArgs e)
    {

    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            using (var context = new StowTownDbContext())
            {
                int selectedRadioId = 0;
                foreach (var song in PagedSongs)
                {
                    // Check if this is an update or a new insert
                    SongPossition existing = null;

                    if (song.Id != 0)
                    {
                        existing = await context.SongPossitions.FirstOrDefaultAsync(p => p.Id == song.Id && p.FkMonthlySongList == song.FkMonthlySongList && p.FkRadioStation== song.FkRadioStation);
                        selectedRadioId = song.FkRadioStation ?? 0; // Get the selected radio station ID
                    }

                    if (existing != null)
                    {
                        // Update existing record
                        existing.Spins = song.Spins ?? 0;
                        existing.Possition = song.Possition;
                        existing.RotationNotes = song.RotationNotes;
                    }
                    else
                    {
                        // Insert new record
                        var newEntry = new SongPossition
                        {
                            FkSong = song.FkSong,
                            FkRadioStation = song.FkRadioStation,
                            FkMonthlySongList = song.FkMonthlySongList,
                            Spins = song.Spins ?? 0,
                            Possition = song.Possition,
                            RotationNotes = song.RotationNotes,
                           
                        };
                        selectedRadioId = song.FkRadioStation ?? 0; // Get the selected radio station ID
                        context.SongPossitions.Add(newEntry);
                    }
                    // If you want to update the MonthlySongList as well, you can do it here
                    AddOrUpdateCallRecords(selectedRadioId); // Call to update or add call records for the selected radio station



                }

                await context.SaveChangesAsync();
                await DisplayAlert("Success", "Songs saved successfully.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save data: {ex.Message}", "OK");
        }
    }



    private void OnSelectedRadioChanged(object sender, EventArgs e)
    {
         BindSongOnRadioStationSelected();

    }
    private void OnSelectedMonthChanged(object sender, EventArgs e)
    {
        BindSongOnMonthSelected();

    }

    private void PreviousButton_Clicked(object sender, EventArgs e)
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            ApplySearchAndPagination();
        }
    }

    private void NextButton_Clicked(object sender, EventArgs e)
    {
        var totalPages = (int)Math.Ceiling((double)MonthlySongs.Count / _itemsPerPage);
        if (_currentPage < totalPages)
        {
            _currentPage++;
            ApplySearchAndPagination();
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {

        SearchQuery = e.NewTextValue;
        _currentPage = 1; // Reset to first page on search
        ApplySearchAndPagination();
    }
       
    private  async Task AddOrUpdateCallRecords( int selectedRadioId)
    {
        try
        {

            using (var context = new StowTownDbContext())
            {
                int todayDayNumber = (int)DateTime.Now.DayOfWeek; // Today's day of the week
                DateTime todayDate = DateTime.Now.Date;
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
                    FkRadioStation = rs.Id,
                    //  Image = rs.Image != null ? Path.Combine(imagesDirectory, rs.Image) : null,
                    Image = rs.Image != null ? ImageFilesService.GetImageUrl("RadioStationImages", rs.Image) : null,
                    Timing = $"{rs.StartTime:hh\\:mm tt} - {rs.EndTime:hh\\:mm tt}",
                    Checked = rs.LastCallInMonth != null ? rs.LastCallInMonth.IsChecked.Value : false, // Checked if a call exists in the current month
                                                                                                       //            IsEnabled = rs.LastCallInMonth != null
                                                                                                       //? !rs.LastCallInMonth.IsChecked.Value   // invert the original value
                                                                                                       //: false, // Enabled only if no call exists in the current month
                                                                                                       // _suppressCheckedEvents = true,

                    IsEnabled = rs.LastCallInMonth != null ? !rs.LastCallInMonth.IsChecked.Value : true,

                }).ToList();
                // Update the LastCallInMonth for the selected radio station

                var getcallRadioStationId = radioactivelist.Where(c => c.FkRadioStation == selectedRadioId)
                    .Select(c => c.Id)
                    .FirstOrDefault();
                if (getcallRadioStationId != 0)
                {

                    var dj =  context.Djs.Where(dj => dj.FkRadioStation == selectedRadioId && dj.IsDeleted == false && dj.IsActive == true).FirstOrDefault();

                    var getDjId = dj?.Id ?? 0; // Ensure null safety by using the null-coalescing operator.
                    var callRecord =  context.CallRecords.FirstOrDefault(c => c.Id == getcallRadioStationId);
                    if (callRecord != null)
                    {
                        callRecord.FkRadioStation = selectedRadioId;
                        callRecord.IsChecked = true; // Mark as checked
                        callRecord.CreatedAt = DateTime.UtcNow; // Update the timestamp
                        callRecord.FkDj = getDjId;
                        callRecord.StartTime = DateTime.UtcNow;
                        callRecord.EndTime = DateTime.UtcNow;
                        callRecord.Notes = "Manual Songs Spins Entry";
                    }
                    else
                    {
                        // If no existing record, create a new one
                        var newCallRecord = new CallRecord
                        {
                            FkRadioStation = selectedRadioId,
                            IsChecked = true,
                            CreatedAt = DateTime.UtcNow,
                            FkDj = getDjId,
                            StartTime = DateTime.UtcNow,
                            EndTime = DateTime.UtcNow,
                            Notes = "Manual Songs Spins Entry",
                        };
                        context.CallRecords.Add(newCallRecord);
                        context.SaveChanges();
                    }
                }

            }
        }
        catch (Exception ex)
        {
            // Handle exceptions, e.g., log the error or show an alert
            DisplayAlert("Error", "Failed to add or update call records: " + ex.Message, "OK");
        }

    }
}
