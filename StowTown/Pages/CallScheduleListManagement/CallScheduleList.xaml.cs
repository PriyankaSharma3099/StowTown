using DocumentFormat.OpenXml.Bibliography;
using StowTown.Models;
using StowTown.Services.SaveImageService;
using System.Collections.ObjectModel;

namespace StowTown.Pages.CallScheduleListManagement;

public partial class CallScheduleList : ContentPage
{
    private readonly StowTownDbContext _dbContext;
    public ObservableCollection<RadioStation> RadioStations { get; set; }
    private Button _selectedMonthButton; // Define the field

    private ActivityIndicator _loader;
    private Button _loadButton;
    private int _currentMonth;

    public CallScheduleList()
	{
		InitializeComponent();
        _dbContext = new StowTownDbContext();
        RadioStations = new ObservableCollection<RadioStation>();

        LoadRadioStations();
        BindingContext = this;
        MessagingCenter.Subscribe<CreateCallTimingsList>(this, "RefreshList", (sender) =>
        {
            LoadRadioStations();
        });
        _loader = new ActivityIndicator
        {
            IsVisible = false,
            IsRunning = false,
            Color = Colors.Blue,
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.CenterAndExpand,
            VerticalOptions = LayoutOptions.CenterAndExpand,
            Margin = new Thickness(0, 0, 0, 0),
        };

        // Add loader to your existing layout
        ContentGrid.Add(_loader);
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        LoadRadioStations(); // Refresh data every time the page appears
        if (_currentMonth > 0)
        {
            await RefreshCalendarForMonth(_currentMonth);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Unsubscribe from the message when the page disappears
        MessagingCenter.Unsubscribe<CreateCallTimingsList>(this, "RefreshList");
    }
    private void LoadRadioStations()
    {
        string parentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string imagesDirectory = System.IO.Path.Combine(parentDirectory, "assets", "Images", "RadioStation");

        var stations = _dbContext.RadioStations
            .Where(r =>  r.IsDeleted !=true && r.IsActive == true)
            .Select(r => new RadioStation
            {
                Id = r.Id,
                Name = r.Name,
                //Image = r.Image != null ? System.IO.Path.Combine(imagesDirectory, r.Image) : null
                Image=r.Image !=null? ImageFilesService.GetImageUrl("RadioStationImages", r.Image):null,
            })
            .ToList();

        RadioStations.Clear();
        foreach (var station in stations)
        {
            RadioStations.Add(station);
        }

    }
    private async void MonthButton_Clicked(object sender, EventArgs e)
    {


        //if (sender is Button button)
        //{
        //      try
        //    {
        //        // Reset the previous selection
        //        if (_selectedMonthButton != null)
        //        {
        //            _selectedMonthButton.BackgroundColor = Colors.White; // Or your default color
        //        }

        //        // Set the new selection
        //        button.BackgroundColor = Colors.LightGrey;
        //        _selectedMonthButton = button;
        //        string monthText = button.Text;
        //        int month = DateTime.ParseExact(monthText, "MMMM", null).Month;

        //        //   GenerateCalendar(month);
        //        //  Task.Delay(1000); // Simulate a delay
        //        //MainThread.BeginInvokeOnMainThread(() => GenerateContentGrid(month)); // Update UI



        //        // Optional: short delay to ensure UI updates
        //        await Task.Delay(50);

        //        // Run work in background
        //        await Task.Run(() =>
        //        {

        //            GenerateCalendar(month);

        //        });

        //        // Update UI on main thread
        //        GenerateContentGrid(month);
        //    }
        //    finally
        //    {
        //        // Hide the loader
        //        _loader.IsRunning = false;
        //        _loader.IsVisible = false;
        //    }
        //}

        if (sender is Button button)
        {
           

            try
            {
                // Button selection visual logic
                _selectedMonthButton?.SetValue(BackgroundColorProperty, Colors.White);
                button.SetValue(BackgroundColorProperty, Colors.LightGrey);
                _selectedMonthButton = button;

                int month = DateTime.ParseExact(button.Text, "MMMM", null).Month;

                _currentMonth = month;
                // Clear existing content
                //MainLayout.Children.Clear();

                // UI changes on main thread
                await MainThread.InvokeOnMainThreadAsync(() => GenerateCalendar(month));

                // Load data and build UI
                await GenerateContentGrid(month);
            }
            catch (Exception ex)
            {
                // optionally log or show error
                Console.WriteLine($"Error generating calendar: {ex.Message}");
            }
            finally
            {
                _loader.IsRunning = false;
                _loader.IsVisible = false;
            }
        }


    }


    private async Task GenerateContentGrid(int month)
    {
        try
        {
            // Show loader before doing anything
            _loader.IsVisible = true;
            _loader.IsRunning = true;

            // Let UI catch up before heavy rendering
            await Task.Delay(50);

            DynamicContentGrid.Clear();
            int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, month);

            var headerGrid = new Grid();
            for (int i = 0; i < daysInMonth; i++)
            {
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                headerGrid.Background = Colors.White;
            }

            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime currentDate = new DateTime(DateTime.Now.Year, month, day);
                var dateLabel = new Label
                {
                    Text = currentDate.Day.ToString(),
                    FontSize = 18,
                    HorizontalTextAlignment = TextAlignment.Center,
                    BackgroundColor = Colors.White,
                    HeightRequest = 25

                };
                var dayLabel = new Label
                {
                    Text = currentDate.ToString("ddd"),
                    FontSize = 18,
                    HorizontalTextAlignment = TextAlignment.Center,
                    TextColor = Colors.Red,
                    BackgroundColor = Colors.White,
                    HeightRequest = 25,
                };
                var boxview = new BoxView
                {
                    BackgroundColor = Colors.LightGray,

                    HeightRequest = 1,

                    HorizontalOptions = LayoutOptions.FillAndExpand,

                    VerticalOptions = LayoutOptions.End
                };

                var headerStack = new VerticalStackLayout
                {
                    Children = { dateLabel, dayLabel, boxview }
                };

                headerGrid.Add(headerStack, day - 1, 0);
            }

            DynamicContentGrid.Add(headerGrid);

            foreach (var station in RadioStations) //Assuming RadioStations is a list of your data type
            {
                var stationGrid = new Grid();
                for (int i = 0; i < daysInMonth; i++)
                {
                    stationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                }

                var workingDaysWithTimings = await Task.Run(() =>
                {
                    var weekDayData = _dbContext.WeekRadioTimes
                        .Where(wrt => wrt.FkRadio == station.Id)
                        .Select(wrt => new
                        {
                            fk_timing = wrt.FkTiming,
                            WeekDay = wrt.WeekDay,
                            StartTime = wrt.FkTimingNavigation.StartTime,
                            EndTime = wrt.FkTimingNavigation.EndTime
                        })
                        .ToList();

                    //return weekDayData
                    //    .Where(wd => wd.WeekDay.HasValue)
                    //    .Select(wd => new
                    //    {
                    //        fk_timing = wd.fk_timing,
                    //        DayOfWeek = (DayOfWeek)wd.WeekDay.Value,
                    //        StartTime = wd.StartTime,
                    //        EndTime = wd.EndTime
                    //    })
                    //    .ToList();

                    // Normalize: DB Sunday = 7 .NET Sunday = 0
                    return weekDayData
                        .Where(wd => wd.WeekDay.HasValue)
                        .Select(wd => new
                        {
                            fk_timing = wd.fk_timing,
                            DayOfWeek = (DayOfWeek)(wd.WeekDay.Value % 7),
                            StartTime = wd.StartTime,
                            EndTime = wd.EndTime
                        })
                        .ToList();
                });


                for (int day = 1; day <= daysInMonth; day++)
                {
                    DateTime currentDate = new DateTime(DateTime.Now.Year, month, day);
                    DayOfWeek dayOfWeek = currentDate.DayOfWeek;

                    var workingDay = workingDaysWithTimings.FirstOrDefault(w => w.DayOfWeek == dayOfWeek);
                    bool isWorkingDay = workingDay != null;

                    View cellContent;

                    if (isWorkingDay)
                    {
                        var displayLabel = new Label
                        {
                            Text = $"Available {workingDay.StartTime:HH:mm} - {workingDay.EndTime:HH:mm}",
                            FontSize = 10,
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            VerticalOptions = LayoutOptions.FillAndExpand
                        };

                        var hiddenLabel = new Label
                        {
                            Text = workingDay.fk_timing.ToString(),
                            IsVisible = false
                        };

                        var tapPanel = new StackLayout
                        {
                            Children = { displayLabel, hiddenLabel }
                        };

                        var tapGesture = new TapGestureRecognizer();
                        tapGesture.Tapped += async (s, e) =>
                        {
                            if (s is StackLayout tapped)
                            {
                                var hiddenTiming = tapped.Children.OfType<Label>().FirstOrDefault(l => !l.IsVisible);
                                if (hiddenTiming != null && int.TryParse(hiddenTiming.Text, out int timingId))
                                {
                                    await Shell.Current.GoToAsync($"{nameof(CreateCallTimingsList)}?SelectedId={timingId}&Type=Edit");
                                }
                            }
                        };
                        tapPanel.GestureRecognizers.Add(tapGesture);
                        cellContent = tapPanel;
                    }
                    else
                    {
                        cellContent = new Label
                        {
                            Text = "",
                            FontSize = 10,
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            VerticalOptions = LayoutOptions.FillAndExpand
                        };
                    }

                    var contentBorder = new Border
                    {
                        Background = isWorkingDay ? Color.FromRgba(0.2f, 0.5f, 0.5f, 0.1f) : Colors.White,
                        Stroke = isWorkingDay ? Color.FromRgba(0.2f, 0.5f, 0.5f, 0.5f) : Colors.White,
                        StrokeThickness = 1,
                        Padding = new Thickness(0, 5, 0, 0),
                        Margin = new Thickness(1),
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        WidthRequest = 80,
                        HeightRequest = 60,
                        Content = cellContent
                    };

                    stationGrid.Add(contentBorder, day - 1, 0);
                }

                MainThread.BeginInvokeOnMainThread(() => DynamicContentGrid.Add(stationGrid));
            }
        }

        finally
        {
            // Hide loader after everything is done
            _loader.IsRunning = false;
            _loader.IsVisible = false;
        }
    }
    private void GenerateCalendar(int month)    
    {
        DateGrid.Children.Clear();

        int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, month);
        DateTime startDate = new DateTime(DateTime.Now.Year, month, 1);

        for (int day = 1; day <= daysInMonth; day++)
        {
            DateTime currentDay = new DateTime(startDate.Year, startDate.Month, day);

            var dateFrame = new Frame
            {
                BorderColor = Colors.LightGray,
                BackgroundColor = Colors.White,
                CornerRadius = 5,
                Padding = 5,
                Content = new StackLayout
                {
                    Children =
                        {
                            //new Label { Text = currentDay.Day.ToString(), FontSize = 16, HorizontalOptions = LayoutOptions.Center },
                            //new Label { Text = currentDay.ToString("ddd"), FontSize = 14, HorizontalOptions = LayoutOptions.Center, TextColor = Colors.Red }
                        }
                }
            };

            DateGrid.Children.Add(dateFrame);
           

        }
    }

    private async void Create_Click(object sender, EventArgs e)
    {
        //await Navigation.PushAsync(new CreateCallTimingsList());
        await Shell.Current.GoToAsync($"{nameof(CreateCallTimingsList)}?SelectedId={0}&Type=Create");
    }

    private async void OnShowLoader()
    {
        _loader = new ActivityIndicator
        {
            IsVisible = false,
            IsRunning = false,
            Color = Colors.Blue,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            //VerticalAlignment = LayoutAlignment.Center,
           // HorizontalAlignment = LayoutAlignment.Center
        };
        // Example button to trigger loading
        _loadButton = new Button
        {
            Text = "Start Loading",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        _loadButton.Clicked += async (s, e) =>
        {
            await ShowLoaderAsync(async () =>
            {
                // Simulate long-running task
                await Task.Delay(3000);
            });
        };

        // Create the layout
        Content = new Grid
        {
            Children =
            {
               
                _loader
            }
        };
    }

    private async Task ShowLoaderAsync(Func<Task> taskToRun)
    {
        _loader.IsVisible = true;
        _loader.IsRunning = true;

        try
        {
            await taskToRun();
        }
        finally
        {
            _loader.IsRunning = false;
            _loader.IsVisible = false;
        }
    }
    private async Task RefreshCalendarForMonth(int month)
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() => GenerateCalendar(month));
            await GenerateContentGrid(month);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating calendar: {ex.Message}");
        }
        finally
        {
            _loader.IsRunning = false;
            _loader.IsVisible = false;
        }
    }

}