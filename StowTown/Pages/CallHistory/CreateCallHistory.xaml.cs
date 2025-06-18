using CommunityToolkit.Maui.Views;
using Microsoft.EntityFrameworkCore;
using StowTown.Models;
using StowTown.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Net;


namespace StowTown.Pages.CallHistory;

public partial class CreateCallHistory : ContentPage, IQueryAttributable, INotifyPropertyChanged
{


    public event PropertyChangedEventHandler PropertyChanged;
    private ObservableCollection<RadioStation> Radios { get; set; } = new ObservableCollection<RadioStation>();
    private ObservableCollection<WeekDaysViewModel> _weekDays;

    private int? CallTimingId { get; set; }
    private string Type { get; set; }

    int Djid;

    public CreateCallHistory()
    {
        InitializeComponent();
        // UpdateUI();
        BindRadioStation();
        BindingContext = this;
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
            CallTimingId = parsedId;
        }
        else
        {
            CallTimingId = 0;
        }

        Type = query.TryGetValue("Type", out var typeObj) && typeObj != null ? typeObj.ToString() : "Create";
        UpdateUI();
    }



    private void UpdateUI()
    {
        if (CallTimingId > 0 && Type == "Create")
        {
            HeaderName.Text = "Create Call History";
            SaveButton.Text = "Save";
            EnableControls();
            BindRadioStation();
            StartMinutes_txt.Text = DateTime.UtcNow.ToString("h:mm:ss tt");
            EndMinutes_txt.Text = DateTime.UtcNow.ToString("h:mm:ss tt");
            // ClearForm();
        }
        else if (CallTimingId > 0 && Type == "Edit")
        {
            HeaderName.Text = "Update Call History";
            SaveButton.Text = "Update";
            EnableControls();
            delete_btn.IsVisible = true; // Show the delete button
            BindRadioStation();
            LoadTimingDetails(CallTimingId.Value);
        }
        else if (CallTimingId > 0 && Type == "View")
        {
            HeaderName.Text = "View  Call History";
            LoadTimingDetails(CallTimingId.Value);
            DisableControls();
            SaveButton.IsVisible = false;  // Hide the SaveButton
            delete_btn.IsVisible = true;
        }
    }

    private async Task BindRadioStation()
    {
        try
        {
            using (var context = new StowTownDbContext())
            {
                // Fetch artist from the database  
                var radio = await context.RadioStations.Where(s => s.IsDeleted != true).ToListAsync();

                // Create ObservableCollection  
                Radios = new ObservableCollection<RadioStation>(radio);

                // Set the ItemsSource for the dropdown  
                Radio_Stations.ItemsSource = Radios;
                if(CallTimingId!=null)
                {
                            Radio_Stations.SelectedItem = Radios.FirstOrDefault(r => r.Id == CallTimingId);
                    //         var timing = context.WeekRadioTimes
                    //.Where(r => r.FkRadio == CallTimingId && r.FkTimingNavigation != null)
                    //.Select(r => r.FkTimingNavigation.StartTime)
                    //.FirstOrDefault();

                    //         StartMinutes_txt.Text = timing?.ToString() ?? string.Empty;
                    
                    var timing = context.WeekRadioTimes
    .Where(r => r.FkRadio == CallTimingId && r.FkTimingNavigation != null)
    .Select(r => new
    {
        Start = r.FkTimingNavigation.StartTime,
        End = r.FkTimingNavigation.EndTime
    })
    .FirstOrDefault();

                    if (timing != null)
                    {
                        // StartMinutes_txt.Text = timing.Start?.ToString("h:mm:ss tt") ?? string.Empty;
                        // EndMinutes_txt.Text = timing.End?.ToString("h:mm:ss tt") ?? string.Empty;
                       
                    }
                    else
                    {
                       // StartMinutes_txt.Text = string.Empty;
                        //EndMinutes_txt.Text = string.Empty;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
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
            }
        }
    }

    private async void LoadTimingDetails(int TimingId)
    {
        try
        {
            using (var context = new StowTownDbContext())
            {
                var callrec = context.CallRecords
.Include(c => c.FkRadioStationNavigation)
.Include(c => c.FkDjNavigation)
.FirstOrDefault(c => c.Id == TimingId);

                if (callrec != null)
                {
                    var radio = await context.RadioStations.Where(s => s.IsDeleted != true).ToListAsync();

                    // Create ObservableCollection  
                    Radios = new ObservableCollection<RadioStation>(radio);

                    // Set the ItemsSource for the dropdown  
                    Radio_Stations.ItemsSource = Radios;

                    Radio_Stations.SelectedItem = Radios.FirstOrDefault(r => r.Id == callrec.FkRadioStation);

                    //Radio_Stations.SelectedItem = callrec.FkRadioStationNavigation;

                    // Assuming DJs.ItemsSource is already set to List<Dj>
                    DJ_txt.Text = callrec.FkDjNavigation.FirstName + " " + callrec.FkDjNavigation.LastName;

                    string startddate = callrec.StartTime.ToString();
                    string endddate = callrec.EndTime.ToString();
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
                    Notes.Text = callrec.Notes;
                    //Label_txt.Text = callrec.Label;

                }




            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", "Error loading timing data:  + ex.Message", "Ok");
        }
    }
    private void ClearForm()
    {
        CallTimingId = 0;
        Type = "Create";

        DJ_txt.Text = string.Empty;
        StartMinutes_txt.Text = string.Empty;
        EndMinutes_txt.Text = string.Empty;

        if (Radio_Stations != null)
            Radio_Stations.SelectedIndex = -1;

        EnableControls();
    }

    private void EnableControls()
    {
        DJ_txt.IsEnabled = false;
        StartMinutes_txt.IsEnabled = true;
        EndMinutes_txt.IsEnabled = true;
        Radio_Stations.IsEnabled = false;
        Notes.IsEnabled = true; // Updated to use the renamed property  
    }

    private void DisableControls()
    {
        DJ_txt.IsEnabled = false;
        StartMinutes_txt.IsEnabled = false;
        EndMinutes_txt.IsEnabled = false;
        Radio_Stations.IsEnabled = false;
        Notes.IsEnabled = false; // Updated to use the renamed property  
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        //await Navigation.PopAsync();
        OnBackNavigation();


    }

    private  async Task  OnBackNavigation()
    {
        //await Navigation.PopAsync();
        //  await Shell.Current.GoToAsync("..");
        var navigationStack = Shell.Current.Navigation.NavigationStack;
        if (navigationStack.Count > 1)
        {
            var previousPage = navigationStack[navigationStack.Count - 2];
            if (previousPage != null)
            {
                string previousPageName = previousPage.GetType().Name;

                if (previousPageName == nameof(HomeDashboard)) // or "HomeDashboardPage"
                {
                    await Shell.Current.GoToAsync(nameof(HomeDashboard)); // or use the appropriate route
                }
            }
            else
            {
                await Shell.Current.GoToAsync("..");
            }
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
            await DisplayAlert("Error", "Please enter a valid Start time in the format: hh:mm:ss tt.", "Ok");
            StartMinutes_txt.Focus();
            return; // Exit if parsing fails
        }

        if (DateTime.TryParseExact(EnddateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var endTime))
        {
            enddate = DateTime.Today.Add(endTime.TimeOfDay);
        }
        else
        {
            await DisplayAlert("Error", "Please enter a valid End time in the format: hh:mm:ss tt.", "Ok");
            EndMinutes_txt.Focus();
            return; // Exit if parsing fails
        }
        // Validate that End time is after Start time
        if (enddate <= startdate)
        {
            await DisplayAlert("Error", "End time must be after Start time.", "Ok");
            StartMinutes_txt.Focus();
            return; // Exit if end time is not valid
        }

        // Validate Radio Station Selection
        if (Radio_Stations.SelectedItem == null)
        {
            await DisplayAlert("Error", "Please select a radio station.", "Ok");
            Radio_Stations.Focus();
            return; // Exit if no radio station is selected
        }
        try
        {
            using (var context = new StowTownDbContext())
            {
                var callrecord = await context.CallRecords.FirstOrDefaultAsync(d => d.Id == CallTimingId);

                if (callrecord != null)
                {
                    // Update existing record
                    callrecord.FkRadioStation = (Radio_Stations.SelectedItem as RadioStation)?.Id;
                    callrecord.FkDj = Djid;
                    callrecord.StartTime = startdate;
                    callrecord.EndTime = enddate;
                    callrecord.Notes = Notes.Text;
                    callrecord.IsChecked = true;
                    callrecord.UpdatedAt = DateTime.UtcNow;

                    context.CallRecords.Update(callrecord);
                    await context.SaveChangesAsync();
                    await DisplayAlert("Success", "Call History updated successfully!", "Ok");
                }
                else
                {
                    // Create new record
                    var callhistory = new CallRecord
                    {
                        FkRadioStation = (Radio_Stations.SelectedItem as RadioStation)?.Id,
                        FkDj = Djid,
                        StartTime = startdate,
                        EndTime = enddate,
                        Notes = Notes.Text,
                        IsChecked = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.CallRecords.Add(callhistory);
                    await context.SaveChangesAsync();
                    await DisplayAlert("Success", "Call History saved successfully!", "Ok");
                }

                // await Navigation.PopAsync();
                await OnBackNavigation();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Error saving call history: " + ex.Message, "Ok");
        }



        //if (CallTimingId == 0 || CallTimingId == null)
        //{
        //    // Create the CallRecord object
        //    var callhistory = new CallRecord()
        //    {
        //        FkRadioStation = (Radio_Stations.SelectedItem as RadioStation)?.Id,
        //        FkDj = Djid,
        //        StartTime = startdate,
        //        EndTime = enddate,
        //        Notes = Notes.Text,
        //        IsChecked = true,
        //        CreatedAt = DateTime.UtcNow
        //    };

        //    try
        //    {
        //        using (var context = new StowTownDbContext())
        //        {
        //            context.CallRecords.Add(callhistory);
        //            await context.SaveChangesAsync();
        //            await DisplayAlert("Success", "Call History saved successfully!", "Ok");
        //            await Navigation.PopAsync();

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await DisplayAlert("Error", "Error saving Call History: " + ex.Message, "Ok");
        //    }
        //}
        //else
        //{
        //    try
        //    {
        //        using (var context = new StowTownDbContext())
        //        {
        //            var callrecord = await context.CallRecords.FirstOrDefaultAsync(d => d.Id == CallTimingId);
        //            {
        //                callrecord.FkRadioStation = (Radio_Stations.SelectedItem as RadioStation)?.Id;
        //                callrecord.FkDj = Djid;
        //                callrecord.StartTime = startdate;
        //                callrecord.EndTime = enddate;
        //                callrecord.Notes = Notes.Text;
        //                callrecord.IsChecked = true;
        //                callrecord.UpdatedAt = DateTime.UtcNow;
        //            }
        //            if (callrecord == null)
        //            {
        //                // Create the CallRecord object
        //                var callhistory = new CallRecord()
        //                {
        //                    FkRadioStation = (Radio_Stations.SelectedItem as RadioStation)?.Id,
        //                    FkDj = Djid,
        //                    StartTime = startdate,
        //                    EndTime = enddate,
        //                    Notes = Notes.Text,
        //                    IsChecked = true,
        //                    CreatedAt = DateTime.UtcNow
        //                };

        //                try
        //                {
        //                    using (var context = new StowTownDbContext())
        //                    {
        //                        context.CallRecords.Add(callhistory);
        //                        await context.SaveChangesAsync();
        //                        await DisplayAlert("Success", "Call History saved successfully!", "Ok");
        //                        await Navigation.PopAsync();

        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    await DisplayAlert("Error", "Error saving Call History: " + ex.Message, "Ok");
        //                }
        //            }
        //            context.CallRecords.Update(callrecord);
        //            await context.SaveChangesAsync();
        //            await DisplayAlert("Success", "Call History Updated successfully!", "Ok");
        //            await Navigation.PopAsync();

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await DisplayAlert("Error", "Error saving song: " + ex.Message, "Ok");

        //    }
        //}


    }


    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        bool isConfirmed = await Application.Current.MainPage.DisplayAlert(
            "Confirm Delete",
            "Are you sure you want to delete this item?",
            "Yes",
            "No");

        if (isConfirmed)
        {
            try
            {
                var button = sender as Button;
                var callRecrow = button?.BindingContext as CallRecordViewModel;

                if (callRecrow != null)
                {
                    using (var context = new StowTownDbContext())
                    {
                        var getrowtodelete = context.CallRecords.Find(Convert.ToInt32(callRecrow.Id));
                        if (getrowtodelete != null)
                        {
                            getrowtodelete.IsDeleted = true;
                            context.SaveChanges();

                            await Application.Current.MainPage.DisplayAlert("Success", "Call History Deleted Successfully.", "OK");
                            await Navigation.PopAsync(); // Make sure this reloads the UI list
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




}