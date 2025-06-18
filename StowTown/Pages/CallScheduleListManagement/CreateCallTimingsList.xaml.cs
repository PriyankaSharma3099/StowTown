using DocumentFormat.OpenXml.Presentation;
using Microsoft.EntityFrameworkCore;
using StowTown.Models;
using StowTown.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using static StowTown.ViewModels.WeekDaysViewModel;

namespace StowTown.Pages.CallScheduleListManagement;

public partial class CreateCallTimingsList : ContentPage, IQueryAttributable
{
    public event PropertyChangedEventHandler PropertyChanged;

    private ObservableCollection<RadioStation> Radios { get; set; } = new ObservableCollection<RadioStation>();
    private ObservableCollection<WeekDaysViewModel> _weekDays;
    public ObservableCollection<WeekDaysViewModel> WeekDays
    {
        get => _weekDays;
        set { _weekDays = value; OnPropertyChanged(); }
    }

    private int? TimingId { get; set; }
    private string Type { get; set; }

    int Djid;

    public CreateCallTimingsList()
    {
        InitializeComponent();
        BindRadioStation();
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateUI();
    }


    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        ClearForm();

        if (query.TryGetValue("SelectedId", out var id) && id != null && int.TryParse(id.ToString(), out int parsedId))
        {
            TimingId = parsedId;
        }
        else
        {
            TimingId = 0;
        }

        Type = query.TryGetValue("Type", out var typeObj) && typeObj != null ? typeObj.ToString() : "Create";
        UpdateUI();
    }



    private void UpdateUI()
    {
        if (TimingId == 0 || Type == "Create")
        {
            HeaderName.Text = "Create Call Timing";
            SaveButton.Text = "Save";
            EnableControls();
            BindRadioStation();
            LoadWeekdays();
            StartMinutes_txt.Text = DateTime.UtcNow.ToString("h:mm:ss tt");
            EndMinutes_txt.Text = DateTime.UtcNow.ToString("h:mm:ss tt");
           // ClearForm();
        }
        else if (TimingId > 0 && Type == "Edit")
        {
            HeaderName.Text = "Edit Call Timing";
            SaveButton.Text = "Update";
            EnableControls();
            delete_btn.IsVisible = true; // Show the delete button
            BindRadioStation();
            LoadWeekdays();
            LoadTimingDetails(TimingId.Value);
        }
        else if (TimingId > 0 && Type == "View")
        {
            HeaderName.Text = "View  Call Timing";
            LoadTimingDetails(TimingId.Value);
            DisableControls();
            SaveButton.IsVisible = false;  // Hide the SaveButton
        }
    }

    private void ClearForm()
    {
        TimingId = 0;
        Type = "Create";

        DJ_txt.Text = string.Empty;
        StartMinutes_txt.Text = string.Empty;
        EndMinutes_txt.Text = string.Empty;

        if (Radio_Stations != null)
            Radio_Stations.SelectedIndex = -1;

        if (WeekDay != null) // Updated to use the renamed property  
            WeekDay.SelectedIndex = -1;// Clear the ObservableCollection instead of using ItemsSource  

        EnableControls();
    }

    private void EnableControls()
    {
        DJ_txt.IsEnabled = true;
        StartMinutes_txt.IsEnabled = true;
        EndMinutes_txt.IsEnabled = true;
        Radio_Stations.IsEnabled = true;
        WeekDay.IsEnabled = true; // Updated to use the renamed property  
    }

    private void DisableControls()
    {
        DJ_txt.IsEnabled = false;
        StartMinutes_txt.IsEnabled = false;
        EndMinutes_txt.IsEnabled = false;
        Radio_Stations.IsEnabled = false;
        WeekDay.IsEnabled = false; // Updated to use the renamed property  
    }



    private async Task BindRadioStation()
    {
        try
        {
            using (var context = new StowTownDbContext())
            {
                // Fetch artist from the database  
                var radio = await context.RadioStations.Where(s => s.IsDeleted != true && s.IsActive == true).ToListAsync();

                // Create ObservableCollection  
                Radios = new ObservableCollection<RadioStation>(radio);

                // Set the ItemsSource for the dropdown  
                Radio_Stations.ItemsSource = Radios;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void LoadWeekdays()
    {
        var weekdays = Enum.GetValues(typeof(Weekday))
       .Cast<Weekday>()
       .Select(w => new WeekDaysViewModel
       {
           Id = (int)w,
           Name = w.ToString()
       })
       .ToList();

        WeekDays = new ObservableCollection<WeekDaysViewModel>(weekdays);
        WeekDay.ItemsSource = WeekDays;


    }
    //private async void BindCallTiming(int djId)
    //{
    //    using (var context = new StowTownDbContext())
    //    {


    //        var dj = await context.Djs.FindAsync(djId);
    //        if (dj != null)
    //        {
    //            FName_txt.Text = dj.FirstName;
    //            LName_txt.Text = dj.LastName;
    //            RadioStation_DD.SelectedItem = dj.FkRadioStation.HasValue ? dj.FkRadioStation.Value : 0;
    //            Email_txt.Text = dj.Email;
    //            Contact_txt.Text = dj.MobileNumber;
    //            Personal_txt.Text = dj.PersonalData;


    //        }
    //    }
    //}





    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();

    }


    private void WeekDay_SelectionChanged(object sender, EventArgs e)
    {

    }

    private async void Radio_SelectionChanged(object sender, EventArgs e)
    {
        DJ_txt.Text = string.Empty;
        Djid = 0; // Reset Djid

        if (Radio_Stations.SelectedItem is RadioStation selectedRadioStation)
        {
            using (var context = new StowTownDbContext())
            {
                var getDj = await context.Djs.FirstOrDefaultAsync(x => x.FkRadioStation == selectedRadioStation.Id);

                if (getDj != null)
                {
                    Djid = getDj.Id;
                    DJ_txt.Text = getDj.FirstName + " " + getDj.LastName;
                    DJ_txt.IsEnabled = false;
                }
                else
                {
                    // Handle case where no DJ is found for the selected radio station
                    DJ_txt.IsEnabled = true; // Allow enabling if no DJ is found
                }
                // Refresh the page by navigating to it again
                //NavigationService.Navigate(new CreateCallTimingsList());

            }
        }
    }

    private async void LoadTimingDetails(int TimingId)
    {
        try
        {
            using (var context = new StowTownDbContext())
            {
                var timing = context.Timings.FirstOrDefault(t => t.Id == TimingId);
                var weekradio = context.WeekRadioTimes.FirstOrDefault(w => w.FkTiming == TimingId);
                if (timing != null && weekradio != null)
                {
                    // Bind only the time part to the text fields  

                    string startddate = timing.StartTime.ToString();
                    string endddate = timing.EndTime.ToString();
                    DateTime releaseDate;
                    if (DateTime.TryParse(startddate, out releaseDate))
                    {
                        string formattedDate = releaseDate.ToString("h:mm:ss tt");
                        StartMinutes_txt.Text = formattedDate;
                    }
                    if (DateTime.TryParse(endddate, out releaseDate))
                    {
                        string formattedDate = releaseDate.ToString("h:mm:ss tt");
                        EndMinutes_txt.Text = formattedDate;
                    }
                    // Set other fields if required    
                    if (weekradio.FkRadio.HasValue && Radios != null)
                    {
                        Console.WriteLine(Radios.Count());
                        await BindRadioStation();
                        var selectedRadioStation = Radios.FirstOrDefault(r => r.Id == weekradio.FkRadio.Value);
                        if (selectedRadioStation != null)
                        {
                            Radio_Stations.SelectedItem = selectedRadioStation;
                        }
                    }
                    var selectedWeekDay = WeekDays.FirstOrDefault(w => w.Id == weekradio.WeekDay);
                    if (selectedWeekDay != null)
                    {
                        WeekDay.SelectedItem = selectedWeekDay;
                    }
                }
                else
                {
                    DisplayAlert("Error", "Timing not found for the given ID.", "ok");
                }
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", "Error loading timing data:  + ex.Message", "Ok");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        string StartdateString = StartMinutes_txt.Text; // Get the text from the TextBox
        string EnddateString = EndMinutes_txt.Text;
        DateTime startdate;
        DateTime enddate;

        string format = "h:mm:ss tt"; // The format for your date string

        // Use today's date combined with the time string
        if (DateTime.TryParseExact(StartdateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var startTime))
        {
            startdate = DateTime.Today.Add(startTime.TimeOfDay);
        }
        else
        {
            DisplayAlert("Error", "Please enter a valid Start time in the format: hh:mm:ss tt.", "Ok");
            StartMinutes_txt.Focus();
            return; // Exit if parsing fails
        }

        if (DateTime.TryParseExact(EnddateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var endTime))
        {
            enddate = DateTime.Today.Add(endTime.TimeOfDay);
        }
        else
        {
            DisplayAlert("Error", "Please enter a valid End time in the format: hh:mm:ss tt.", "Ok");
            EndMinutes_txt.Focus();
            return; // Exit if parsing fails
        }
        // Validate that End time is after Start time
        if (enddate <= startdate)
        {
            DisplayAlert("Error", "End time must be after Start time.", "Ok");
            EndMinutes_txt.Focus();
            return; // Exit if end time is not valid
        }

        
        // Create the CallRecord object
        int? selectedWeekDayId = (WeekDay.SelectedItem as WeekDaysViewModel)?.Id;
        int? selectedRadioId = (Radio_Stations.SelectedItem as RadioStation)?.Id;

        // Validate Radio Station Selection
        if (selectedRadioId ==null ||selectedRadioId < 0)
        {
            DisplayAlert("Error", "Please select a radio station.", "Ok");
            Radio_Stations.Focus();
            return; // Exit if no radio station is selected
        }
        if (selectedWeekDayId == null || selectedWeekDayId < 0)
        {
            DisplayAlert("Error", "Please select a valid week day.", "Ok");
            WeekDay.Focus();
            return; // Exit if no week day is selected
        }
        if (TimingId == null || TimingId == 0)
        {
            // To resolve the ambiguity between 'StowTown.Models.Timing' and 'DocumentFormat.OpenXml.Presentation.Timing',
            // explicitly qualify the 'Timing' type with its namespace where it is used.

            var timelist = new StowTown.Models.Timing();
           
            if(timelist != null)
            {
                timelist.StartTime = startdate;
                timelist.EndTime = enddate;
                   
                 
            }

            try
            {
                // Get selected values using safe casting


                using (var context = new StowTownDbContext())
                {
                    
                    var checkdayforradio = context.WeekRadioTimes.FirstOrDefault(w => w.WeekDay == Convert.ToInt32(selectedWeekDayId)
                    && w.FkRadio == Convert.ToInt32(selectedRadioId));
                    if (checkdayforradio == null)
                    {
                        context.Timings.Add(timelist);
                        await context.SaveChangesAsync();


                        var calltiming = new WeekRadioTime()
                        {
                            FkRadio = Convert.ToInt32(selectedRadioId),
                            FkDj = Djid,
                            FkTiming = timelist.Id,
                            WeekDay = Convert.ToInt32(selectedWeekDayId)

                        };

                       
                  

                        context.WeekRadioTimes.Add(calltiming);
                        await context.SaveChangesAsync();
                        DisplayAlert("Success", "Call Timing saved successfully!", "Ok");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        DisplayAlert("Error", "You have already set the timing for this day.", "Ok");
                    }
                }


            }
            catch (Exception ex)
            {
                DisplayAlert("Error", "Error saving Call Timing:  + ex.Message", "Ok");
            }
        }
        else
        {
            try
            {
                using (var context = new StowTownDbContext())
                {
                    // Create a new instance of the ArtistGroup
                    var calltiming = context.Timings.FirstOrDefault(a => a.Id == TimingId);
                    if (calltiming != null)
                    {
                        calltiming.StartTime = startdate;
                        calltiming.EndTime = enddate;
                    }
                    context.Timings.Update(calltiming);

                    var weekcalltime = context.WeekRadioTimes.FirstOrDefault(w => w.FkTiming == TimingId);
                    if (weekcalltime != null)
                    {
                        weekcalltime.FkDj = Djid;
                        weekcalltime.FkRadio = Convert.ToInt32(selectedRadioId);
                        weekcalltime.FkTiming = TimingId;
                        weekcalltime.WeekDay = Convert.ToInt32(selectedWeekDayId);
                    }
                    context.WeekRadioTimes.Update(weekcalltime);
                    context.SaveChanges();
                    DisplayAlert("Success", "Call Timing updated successfully!", "Ok");
                    await Navigation.PopAsync();

                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", "Error updating Call Timing:  + ex.Message", "Ok");

            }
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        bool result = await DisplayAlert("Alert", "Are you sure you want to delete this item?", "Confirm Delete", "Cancel");

        if (!result)
            return;

        try
        {
            using (var context = new StowTownDbContext())
            {
                if (TimingId != 0)
                {
                    var getRowToDelete = context.WeekRadioTimes.FirstOrDefault(w => w.FkTiming == TimingId);
                    if (getRowToDelete != null)
                    {
                        context.WeekRadioTimes.Remove(getRowToDelete);
                    }

                    var getTiming = context.Timings.FirstOrDefault(t => t.Id == TimingId);
                    if (getTiming != null)
                    {
                        context.Timings.Remove(getTiming);
                    }

                    context.SaveChanges();

                    await DisplayAlert("Success", "Call Timing deleted successfully!", "Ok");
                    await Navigation.PopAsync();
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error deleting Call Timing: {ex.Message}", "Ok");
        }
    }

}