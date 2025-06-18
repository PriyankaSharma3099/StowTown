using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using Microsoft.EntityFrameworkCore;
using StowTown.Models;
using System.Collections.ObjectModel;
using Microcharts;
using SkiaSharp;
using static StowTown.Pages.Reports.Graph;
using Microcharts.Maui;
using Microsoft.Maui.Controls;
using SkiaSharp.Views.Maui;
using ClosedXML.Excel;

namespace StowTown.Pages.Reports;

public partial class Graph : ContentPage
{
    public List<RadioStation> radioStations { get; set; }
    public List<string> Labels { get; private set; }
    public ISeries[] Series { get; private set; } // Add this property

   // public ObservableCollection<SongData> Songs { get; set; } = new ObservableCollection<SongData>();

    private ObservableCollection<SongData> _songs;
    public ObservableCollection<SongData> Songs
    {
        get => _songs;
        set
        {
            _songs = value;
            OnPropertyChanged();
        }
    }
    public class SongData
    {
        public string SongName { get; set; }
        public int SpinCount { get; set; }
        public string RotationNotes { get; set; }
    }

    private ObservableCollection<Song> _songsList;
    public ObservableCollection<Song> SongsList
    {
        get => _songsList;
        set
        {
            _songsList = value;
            OnPropertyChanged();
        }
    }

    public string SelectedFilterValue { get; set; }
    public string SelectedFilterModel { get; set; }
    public Graph()
    {
        InitializeComponent();
        BindRadioStation();
     //   ChartCanvas.InvalidateSurface(); // Force canvas redraw  
        Songs = new ObservableCollection<SongData>();
        SongsList = new ObservableCollection<Song>(); // Initialize SongsList  
        LoadSongs();
        BindingContext = this;
        SelectDataTypePicker.SelectedIndex = 0; // Set default selection to "Radio Station"
        SelectedFilterValue = "Radio Station"; // Set the initial filter value
       
    }

    private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
    {
        var surface = args.Surface;
        var canvas = surface.Canvas;
        var info = args.Info;

        try
        {
            DateTime startDate = StartDatePicker.Date;
            DateTime endDate = EndDatePicker.Date;
            // RadioStation selectedRadioStation = RadioStationPicker.SelectedItem as RadioStation??null;

            int filterId = 0;
            if (endDate < startDate)
            {
                DisplayAlert("Error", "End date cannot be earlier than start date.", "OK");
                return;
            }

          
            using (var context = new StowTownDbContext())
            {
                var songPositionsQuery = from sp in context.SongPossitions
                                         join msl in context.MonthlySongLists on sp.FkMonthlySongList equals msl.Id
                                         join song in context.Songs on sp.FkSong equals song.Id
                                         where msl.Date >= startDate && msl.Date <= endDate
                                         select new
                                         {
                                             Spins = sp.Spins ?? 0,
                                             SongName = song.Name,
                                             RotationNotes = sp.RotationNotes,
                                             RadioStationId = sp.FkRadioStation,
                                             RadioStationName= sp.FkRadioStationNavigation.Name, // Assuming you have a navigation property for RadioStation

                                            SongId = song.Id
                                         };

                List<SongData> songData = new List<SongData>();
                if (SelectedFilterValue != null)
                {
                    if (string.Equals(SelectedFilterValue?.ToString(), "Radio Station", StringComparison.Ordinal))
                    {
                        if (RadioStationPicker.SelectedItem != null)
                        {
                            int getselectedRadioStationId = ((RadioStation)RadioStationPicker.SelectedItem).Id;
                            if (getselectedRadioStationId > 0)
                            {
                                var songSpins = songPositionsQuery
                                    .Where(sp => sp.RadioStationId == getselectedRadioStationId)
                                    .ToList(); // Execute the query and bring data into memory  

                                songData = songSpins
                                     .GroupBy(item => item.SongName)
                                     .Select(group => new SongData
                                     {
                                         SongName = group.Key,
                                         SpinCount = group.Sum(x => x.Spins),
                                     })
                                     .ToList();
                            }
                            else
                            {
                                var songSpins = songPositionsQuery
                                    .ToList(); // Execute the query and bring data into memory  
                                songData = songSpins
                                    .GroupBy(item => item.SongName)
                                    .Select(group => new SongData
                                    {
                                        SongName = group.Key,
                                        SpinCount = group.Sum(x => x.Spins),
                                    })
                                    .ToList();
                            }
                        }
                    }
                    else if (string.Equals(SelectedFilterValue?.ToString(), "Songs", StringComparison.Ordinal))
                    {
                        if (RadioStationPicker.SelectedItem != null)
                        {
                            var getselectedSongId = ((Song)RadioStationPicker.SelectedItem).Id;
                            if (getselectedSongId >0)
                            {
                                var songSpins = songPositionsQuery
                                    .Where(sp => sp.SongId == getselectedSongId)
                                    .ToList(); // Execute the query and bring data into memory  

                                songData = songSpins
                                     .GroupBy(item => item.RadioStationName)
                                     .Select(group => new SongData
                                     {
                                         SongName = group.FirstOrDefault()?.RadioStationName ?? null,
                                         SpinCount = group.Sum(x => x.Spins),
                                     })
                                     .ToList();
                            }
                            else
                            {
                                var songSpins = songPositionsQuery
                                    .ToList(); // Execute the query and bring data into memory  
                                songData = songSpins
                                    .GroupBy(item => item.RadioStationName)
                                    .Select(group => new SongData
                                    {
                                        SongName = group.FirstOrDefault()?.RadioStationName ?? null,
                                        SpinCount = group.Sum(x => x.Spins),
                                    })
                                    .ToList();
                            }
                        }
                    }

                }
                //var songSpins = songPositionsQuery
                //    .GroupBy(item => item.SongName)
                //    .Select(group => new
                //    {
                //        SongName = group.Key,
                //        TotalSpins = group.Sum(item => item.Spins)
                //    })
                //    .ToList();

                //List<SongData> songData = songSpins
                //    .Select(item => new SongData
                //    {
                //        SongName = item.SongName,
                //        SpinCount = item.TotalSpins
                //    })
                //    .ToList();

                if (songData.Count == 0)
                {
                    canvas.Clear(SKColors.White);
                    using (var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 30, IsAntialias = true })
                    {
                        canvas.DrawText("No data available.", info.Width / 2f - 100, info.Height / 2f, textPaint);
                    }
                    return;
                }

                canvas.Clear(SKColors.White);

                float barHeight = Math.Min(30, info.Height / (songData.Count + 1));

                float spacing = Math.Max(10, barHeight / 6);
                //float maxBarWidth = info.Width * 0.5f;
                //float startX = info.Width * 0.4f;
                float maxBarWidth = info.Width * 0.5f; // Increase bar width range
                float startX = 120f; // Shift bars to left
                float startY = spacing;

                int maxValue = songData.Max(sd => sd.SpinCount);

                using (var textPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    TextSize = barHeight / 2,
                    IsAntialias = true
                })
                using (var barPaint = new SKPaint
                {
                    Color = SKColors.Blue,
                    IsAntialias = true
                })
                {
                    ////Fix Y-Axis Label (Songs)
                    //string yAxisLabel = "Songs";
                    //using (var labelPaint = new SKPaint { Color = SKColors.Black, TextSize = 30, IsAntialias = true })
                    //{
                    //    SKRect yAxisLabelBounds = new SKRect();
                    //    labelPaint.MeasureText(yAxisLabel, ref yAxisLabelBounds);
                    //    float yAxisLabelX = 20;
                    //    float yAxisLabelY = info.Height / 2f;

                    //    canvas.Save();
                    //    canvas.RotateDegrees(-90, yAxisLabelX, yAxisLabelY);
                    //    canvas.DrawText(yAxisLabel, yAxisLabelX, yAxisLabelY, labelPaint);
                    //    canvas.Restore();
                    //}

                    // Fix X-Axis Label (Spin Count)
                    //   canvas.DrawText("Spin Count", info.Width / 5f - 10, info.Height - 20, textPaint);
                 
                    foreach (var song in songData)
                    {
                        float barWidth = (song.SpinCount / (float)maxValue) * maxBarWidth;

                        // Dynamic Song Name Handling (Truncation)
                        string truncatedSongName = song.SongName;
                        SKRect songNameBounds = new SKRect();
                        textPaint.MeasureText(truncatedSongName, ref songNameBounds);

                        while (textPaint.MeasureText(truncatedSongName + "...", ref songNameBounds) > startX - 40 && truncatedSongName.Length > 3)
                        {
                            truncatedSongName = truncatedSongName.Substring(0, truncatedSongName.Length - 1);
                        }

                        if (truncatedSongName.Length > 3)
                        {
                            truncatedSongName += "...";
                        }

                        //  Fix Song Name Placement (Align to Left)
                        float textX = startX - 20;
                        float textY = startY + (barHeight / 2) + (textPaint.TextSize / 4);
                        textPaint.TextAlign = SKTextAlign.Right;
                        canvas.DrawText(truncatedSongName, textX, textY, textPaint);

                        //  Draw Bar
                            canvas.DrawRect(startX, startY, barWidth, barHeight, barPaint);

                        //  Fix Spin Count Label Placement
                        float spinCountX = startX + barWidth + 10;
                        float spinCountY = startY + (barHeight / 2) + (textPaint.TextSize / 4);
                        textPaint.TextAlign = SKTextAlign.Left;
                        canvas.DrawText(song.SpinCount.ToString(), spinCountX, spinCountY, textPaint);

                        startY += barHeight + spacing;
                    }
                    float labelY = startY + (barHeight / 2) + (textPaint.TextSize / 4) + spacing + 10; // Adjust 10 for more padding if needed.

                    // Draw the "Spin Count" label below the last bar's spin count
                    canvas.DrawText("Spin Count", info.Width / 5f - 10, labelY, textPaint);

                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnCanvasViewPaintSurface: {ex}");
            canvas.Clear(SKColors.White);
            using (var textPaint = new SKPaint { Color = SKColors.Red, TextSize = 20, IsAntialias = true })
            {
                canvas.DrawText($"An error occurred: {ex.Message}", 10, 30, textPaint);
            }
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Alternatively, you can call it here if you need to refresh data when the page appears
        // BindRadioStation();
    }

    // ...

    protected async void BindRadioStation()
    {
        try
        {
            using (var context = new StowTownDbContext())
            {
                var radioStations = await context.RadioStations.Where(s => s.IsDeleted != true && s.IsActive == true).ToListAsync();
                radioStations.Insert(0, new RadioStation { Id = 0, Name = "All" });

                // Fix the error by using a loop to add items to the ObservableCollection
                foreach (var station in radioStations)
                {
                    Songs.Add(new SongData { SongName = station.Name, SpinCount = 0,RotationNotes=station.Notes});
                }

                RadioStationPicker.ItemsSource = new ObservableCollection<RadioStation>(radioStations);
                RadioStationPicker.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK"); // Use DisplayAlert for user-friendly error messages
        }
    }

    protected async void BindSongList()
    {
        try
        {
            using (var context = new StowTownDbContext())
            {
                var songs = await context.Songs.Where(s => s.IsDeleted != true).ToListAsync();
                songs.Insert(0, new Song { Id = 0, Name = "All" });
                SongsList.Clear(); // Clear SongsList before adding new items  
                foreach (var song in songs)
                {
                    SongsList.Add(song);
                }
                RadioStationPicker.ItemsSource = new ObservableCollection<Song>(songs); // Fix CS0246 and CS1526  
                RadioStationPicker.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
    //private void ShowChart(object sender, EventArgs e)
    //{

    //    // Get the values from the input fields
    //    DateTime startDate = StartDatePicker.Date;
    //    DateTime endDate = EndDatePicker.Date;
    //    RadioStation selectedRadioStation = RadioStationPicker.SelectedItem as RadioStation;
    //    string selectedViewType = ViewTypePicker.SelectedItem as string;

    //    List<SongData> songData = new List<SongData>
    //    {
    //        new SongData { SongName = "Song1", SpinCount = 60 },
    //        new SongData { SongName = "Song2", SpinCount = 25 },
    //        new SongData { SongName = "Song3", SpinCount = 50 },
    //        new SongData { SongName = "Song4", SpinCount = 40 },
    //        new SongData { SongName = "Song5", SpinCount = 30 }
    //    };
    //    // Create Microcharts entries
    //    List<ChartEntry> entries = songData.Select(sd => new ChartEntry(sd.SpinCount)
    //    {
    //        Label = sd.SongName,
    //        ValueLabel = sd.SpinCount.ToString(),
    //        Color = SKColor.Parse("#3498db") // Example color
    //    }).ToList();

    //    // Create the bar chart
    //    var chart = new BarChart { Entries = entries };

    //    // Display the chart in your UI
    //    ChartDisplay.Chart = chart; // Assuming ChartDisplay is your Microcharts.Maui ChartView
    //}


    private async void ShowChart(object sender, EventArgs e)
    {
        // Existing code...

        var selectedView = ViewTypePicker.SelectedItem?.ToString();
        var selectedFilterType = SelectDataTypePicker.SelectedIndex;
        if (selectedView == "BarChart View")
        {
            ChartSection.IsVisible = true;
            GridSection.IsVisible = false;
            ChartCanvas.InvalidateSurface();
        }
        else
        {
            ChartSection.IsVisible = false;
            GridSection.IsVisible = true;
            LoadSongs();
        }

        // Correct method call to invalidate the canvas
        //ChartCanvas.InvalidateSurface();
        //LoadSongs();
    }

    //private async void ShowChart(object sender, EventArgs e)
    //{
    //    try
    //    {
    //        DateTime startDate = StartDatePicker.Date;
    //        DateTime endDate = EndDatePicker.Date;
    //        RadioStation selectedRadioStation = RadioStationPicker.SelectedItem as RadioStation;
    //        string selectedViewType = ViewTypePicker.SelectedItem as string;

    //        if (endDate < startDate)
    //        {
    //            await DisplayAlert("Error", "End date cannot be earlier than start date.", "OK");
    //            return;
    //        }

    //        int selectedRadioStationId = selectedRadioStation.Id;

    //        using (var context = new StowTownDbContext())
    //        {
    //            var songPositionsQuery = from sp in context.SongPossitions
    //                                     join msl in context.MonthlySongLists on sp.FkMonthlySongList equals msl.Id
    //                                     join song in context.Songs on sp.FkSong equals song.Id
    //                                     where msl.Date >= startDate && msl.Date <= endDate
    //                                     select new
    //                                     {
    //                                         Spins = sp.Spins ?? 0,
    //                                         SongName = song.Name,
    //                                         RotationNotes = sp.RotationNotes,
    //                                         RadioStationId = sp.FkRadioStation
    //                                     };

    //            // Remove the Radio Station Filter Condition
    //            // The query will now include all radio stations by default

    //            // Group and Aggregate Data
    //            var songSpins = songPositionsQuery
    //                .GroupBy(item => item.SongName)
    //                .Select(group => new
    //                {
    //                    SongName = group.Key,
    //                    TotalSpins = group.Sum(item => item.Spins)
    //                })
    //                .ToList();

    //            // Create SongData Objects
    //            List<SongData> songData = songSpins
    //                .Select(item => new SongData
    //                {
    //                    SongName = item.SongName,
    //                    SpinCount = item.TotalSpins
    //                })
    //                .ToList();

    //            // Create Microcharts Entries
    //            List<ChartEntry> entries = songData.Select(sd => new ChartEntry(sd.SpinCount)
    //            {
    //                Label = sd.SongName,
    //                ValueLabel = sd.SpinCount.ToString(),
    //                Color = SKColor.Parse("#3498db") // Example color
    //            }).ToList();

    //            // Create the bar chart
    //            var chart = new BarChart { Entries = entries };

    //            // Display the chart in your UI
    //            ChartDisplay.Chart = chart;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        await DisplayAlert("Error", ex.Message, "OK");
    //    }
    //}

    private async void ExportToExcel_Click(object sender, EventArgs e)
    {
        try
        {
            // Load songs data first
            LoadSongs();

            // Define the file path to save the Excel file
            //var filePath = Path.Combine(FileSystem.AppDataDirectory, "SongsReport.xlsx");

            //// If the file already exists, delete it
            //if (File.Exists(filePath))
            //{
            //    File.Delete(filePath);
            //}


            // Get the path to the OneDrive Documents folder
            string oneDriveDocumentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OneDrive", "Documents");

            // Define the path for the new folder "Radio Station Excel Data"
            string folderPath = Path.Combine(oneDriveDocumentsPath, "Radio Station Excel Data");

            // Check if the folder exists, if not, create it
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Define the file path to save the Excel file
            var filePath = Path.Combine(folderPath, "SongsReport.xlsx");

            // If the file already exists, delete it
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Create a new Excel workbook
            using (var workbook = new XLWorkbook())
            {
                // Create a worksheet
                var worksheet = workbook.AddWorksheet("Songs");

                // Define header row
                worksheet.Cell(1, 1).Value = "Song Name";
                worksheet.Cell(1, 2).Value = "Spin Count";
                worksheet.Cell(1, 3).Value = "Rotation Notes";

                // Populate the worksheet with song data
                int row = 2; // Start from row 2 (below the header)
                foreach (var song in Songs)
                {
                    worksheet.Cell(row, 1).Value = song.SongName;
                    worksheet.Cell(row, 2).Value = song.SpinCount;
                    worksheet.Cell(row, 3).Value = song.RotationNotes;
                    row++;
                }

                // Auto-size columns for better readability
                worksheet.Columns().AdjustToContents();

                // Save the workbook to the file system
                workbook.SaveAs(filePath);

                // Notify the user that the file is saved
                await DisplayAlert("Success", "Songs data exported to Excel successfully.", "OK");

                // Optionally, open the file (if supported by platform)
                await OpenFile(filePath);
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions
            await DisplayAlert("Error", $"An error occurred while exporting: {ex.Message}", "OK");
        }
    }

    // Method to open the file (if applicable)
    private async Task OpenFile(string filePath)
    {
        // For Android/iOS/MacCatalyst
        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.MacCatalyst)
        {
            await Launcher.OpenAsync(filePath);
        }
        else
        {
            // For Windows, try to open with default app
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });
        }
    }

    private async void LoadSongs()
    {

        DateTime startDate = StartDatePicker.Date;
        DateTime endDate = EndDatePicker.Date;
        RadioStation selectedRadioStation = RadioStationPicker.SelectedItem as RadioStation;
        string selectedViewType = ViewTypePicker.SelectedItem as string;

        if (endDate < startDate)
        {
            DisplayAlert("Error", "End date cannot be earlier than start date.", "OK");
            return;
        }
        using (var context = new StowTownDbContext())
        {
            //var songPositionsQuery = from sp in context.SongPossitions
            //                         join msl in context.MonthlySongLists on sp.FkMonthlySongList equals msl.Id
            //                         join song in context.Songs on sp.FkSong equals song.Id
            //                         where msl.Date >= DateTime.Now.AddMonths(-1) && msl.Date <= DateTime.Now
            //                         select new
            //                         {
            //                             Spins = sp.Spins ?? 0,
            //                             SongName = song.Name,
            //                             RotationNotes = sp.RotationNotes
            //                         };


            var songPositionsQuery = from sp in context.SongPossitions
                                     join msl in context.MonthlySongLists on sp.FkMonthlySongList equals msl.Id
                                     join song in context.Songs on sp.FkSong equals song.Id
                                     where msl.Date >= startDate && msl.Date <= endDate
                                     select new
                                     {
                                         Spins = sp.Spins ?? 0,
                                         SongName = song.Name,
                                         RotationNotes = sp.RotationNotes,
                                         RadioStationId = sp.FkRadioStation,
                                         RadionStationName = sp.FkRadioStationNavigation.Name, // Assuming you have a navigation property for RadioStation
                                         SongId = song.Id
                                     };
            if (SelectedFilterValue != null)
            {
                if (string.Equals(SelectedFilterValue?.ToString(), "Radio Station", StringComparison.Ordinal))
                {
                    if (RadioStationPicker.SelectedItem != null)
                    {
                        int getselectedRadioStationId = ((RadioStation)RadioStationPicker.SelectedItem).Id;
                        if (getselectedRadioStationId > 0)
                        {
                            songPositionsQuery = songPositionsQuery
                                .Where(sp => sp.RadioStationId == getselectedRadioStationId);

                            var Spins = songPositionsQuery
                              .GroupBy(item => item.SongName)
                              .Select(group => new SongData
                              {
                                  SongName = group.FirstOrDefault() != null ? group.FirstOrDefault().SongName : null,
                                  SpinCount = group.Sum(item => item.Spins),
                                  RotationNotes = group.Select(item => item.RotationNotes).FirstOrDefault() ?? string.Empty // Fix for CS8072  
                              })
                              .ToList();
                            if (Spins.Count == 0)
                            {
                                await MainThread.InvokeOnMainThreadAsync(() =>
                                {
                                    DisplayAlert("No Records", "No song data found for the selected filters.", "OK");
                                });
                            
                                GridSection.IsVisible = false;
                            }
                            // UI must be updated on the main thread
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                Songs.Clear();
                                foreach (var song in Spins)
                                {
                                    Songs.Add(song);
                                }
                            });
                        }
                        else
                        {
                            var songSpins = songPositionsQuery
               .GroupBy(item => item.SongName)
               .Select(group => new SongData
               {
                   SongName = group.Key,
                   SpinCount = group.Sum(item => item.Spins),
                   RotationNotes = group.Select(item => item.RotationNotes).FirstOrDefault() ?? string.Empty // Fix for CS8072
               })
               .ToList();
                            if (songSpins.Count == 0)
                            {
                                await MainThread.InvokeOnMainThreadAsync(() =>
                                {
                                    DisplayAlert("No Records", "No song data found for the selected filters.", "OK");
                                });

                                GridSection.IsVisible = false;
                            }
                            // UI must be updated on the main thread
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                Songs.Clear();
                                foreach (var song in songSpins)
                                {
                                    Songs.Add(song);
                                }
                            });
                        }
                    }
                }
                else if (string.Equals(SelectedFilterValue?.ToString(), "Songs", StringComparison.Ordinal))
                {
                    if (RadioStationPicker.SelectedItem != null)
                    {
                        var getselectedSongId = ((Song)RadioStationPicker.SelectedItem).Id;
                        if (getselectedSongId > 0)
                        {
                            songPositionsQuery = songPositionsQuery
                                .Where(sp => sp.SongId == getselectedSongId);
                            var Spins = songPositionsQuery
                               .GroupBy(item => item.RadionStationName)
                               .Select(group => new SongData
                               {
                                   SongName = group.FirstOrDefault() != null ? group.FirstOrDefault().RadionStationName : null,
                                   SpinCount = group.Sum(item => item.Spins),
                                   RotationNotes = group.Select(item => item.RotationNotes).FirstOrDefault() ?? string.Empty // Fix for CS8072  
                               })
                               .ToList();
                            if(Spins.Count == 0)
                            {
                                await MainThread.InvokeOnMainThreadAsync(() =>
                                {
                                    DisplayAlert("No Records", "No song data found for the selected filters.", "OK");
                                });
                                GridSection.IsVisible = false;
                            }
                            // UI must be updated on the main thread
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                Songs.Clear();
                                foreach (var song in Spins)
                                {
                                    Songs.Add(song);
                                }
                            });
                        }
                        
                        else
                        {
                            // If no song is selected, fetch all songs
                            var Spins = songPositionsQuery
                                .GroupBy(item => item.RadionStationName)
                                .Select(group => new SongData
                                {
                                    SongName = group.FirstOrDefault() != null ? group.FirstOrDefault().RadionStationName : null,
                                    SpinCount = group.Sum(item => item.Spins),
                                    RotationNotes = group.Select(item => item.RotationNotes).FirstOrDefault() ?? string.Empty // Fix for CS8072  
                                })
                                .ToList();
                            if (Spins.Count == 0)
                            {
                                await MainThread.InvokeOnMainThreadAsync(() =>
                                {
                                    DisplayAlert("No Records", "No song data found for the selected filters.", "OK");
                                });
                                GridSection.IsVisible = false;
                            }
                                // UI must be updated on the main thread
                                MainThread.BeginInvokeOnMainThread(() =>
                            {
                                Songs.Clear();
                                foreach (var song in Spins)
                                {
                                    Songs.Add(song);
                                }
                            });
                        }
                        return;

                    }
                }
            }

            //var songSpins = songPositionsQuery
            //    .GroupBy(item => item.SongName)
            //    .Select(group => new SongData
            //    {
            //        SongName = group.Key,
            //        SpinCount = group.Sum(item => item.Spins),
            //        RotationNotes = group.Select(item => item.RotationNotes).FirstOrDefault() ?? string.Empty // Fix for CS8072
            //    })
            //    .ToList();
            //// UI must be updated on the main thread
            //MainThread.BeginInvokeOnMainThread(() =>
            //{
            //    Songs.Clear();
            //    foreach (var song in songSpins)
            //    {
            //        Songs.Add(song);
            //    }
            //});
        } 
    }

    private void OnViewTypeChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        var selectedView = picker.SelectedItem?.ToString();

        // Show the correct section based on the selected view type
        if (selectedView == "BarChart View")
        {
            ChartSection.IsVisible = true;
            GridSection.IsVisible = false;
        }
        else if (selectedView == "Grid View")
        {
            ChartSection.IsVisible = false;
            GridSection.IsVisible = true;
        }
    }

    private  async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();

    }

    private void OnSelectDataTypeChanged(object sender, EventArgs e)
    {
        var selectedItem = SelectDataTypePicker.SelectedItem?.ToString();

        if (selectedItem == "Radio Station")
        {
            PickerLabel.Text = "Radio Station *";
            //  RadioStationPicker.ItemsSource = radioStations;
            BindRadioStation();
            RadioStationPicker.ItemDisplayBinding = new Binding("Name");
            SelectedFilterValue = "Radio Station"; // Set the selected filter value
        }
        else if (selectedItem == "Songs")
        {
            PickerLabel.Text = "Songs *";
            //  RadioStationPicker.ItemsSource = songs;
            BindSongList();
            RadioStationPicker.ItemDisplayBinding = new Binding("Name");
            SelectedFilterValue = "Songs"; // Set the selected filter value
        }

        //RadioStationPicker.SelectedItem = null; // Optional: reset selection
    }
}
