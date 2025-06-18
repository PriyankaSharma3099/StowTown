using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using StowTown.Models;
using StowTown.Services.SaveImageService;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace StowTown.Pages.MonthlySong;

public partial class CreateMonthlySongList : ContentPage, IQueryAttributable, INotifyPropertyChanged
{
    public static int songId { get; set; }
    private string Type { get; set; }
    private ObservableCollection<Song> _songs;
    public ObservableCollection<Song> Songs
    {
        get => _songs;
        set
        {
            _songs = value;
            OnPropertyChanged(nameof(Songs));
        }
    }

    private Song _selectedArtist;
    private string selectedImagePath1;
    private string? selectedImagePath;

    public Song SelectedArtist
    {
        get => _selectedArtist;
        set
        {
            if (_selectedArtist != value)
            {
                _selectedArtist = value;
                OnPropertyChanged(nameof(SelectedArtist));

                // Clear the title after the initial selection
                if (value != null)
                {
                    Application.Current.Dispatcher.Dispatch(() =>
                    {
                        var picker = Application.Current.MainPage.FindByName<Picker>("Artist_DD");
                        if (picker != null)
                        {
                            picker.Title = null; // Or ""
                        }
                    });
                }
            }
            }
    }
    private string SelectedImagePath { get; set; }

    private string ImageName { get; set; }


    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public CreateMonthlySongList()
	{
		InitializeComponent();
        
        _songs = new ObservableCollection<Song>();
       
        Artist_DD.ItemsSource = _songs;

    }
 

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BindSongs(); // Load data when page appears
    }


    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        // ClearForm();

        if (query.TryGetValue("SelectedId", out var id) && id != null && int.TryParse(id.ToString(), out int parsedId))
        {
            songId = parsedId;
        }
        else
        {
            songId = 0;
        }

        Type = query.TryGetValue("Type", out var typeObj) && typeObj != null ? typeObj.ToString() : "Create";
        if (songId == 0)
        {
            HeaderName.Text = "Create Monthly Song";
            SaveButton.Text = "Save";
            EnableControls();
            ClearForm();
        }

        if (songId > 0 && Type == "Edit")
        {
            HeaderName.Text = "Update Monthly Song";
            BindSongs(); 
            SaveButton.Text = "Update";
        }
        else if (songId > 0 && Type == "View")
        {
            HeaderName.Text = "View Monthly Song";
            BindSongs();
            DisableControls();
            SaveButton.IsVisible = false;  // Hide the SaveButton

        }
    }



    private async Task BindSongs()
    {
        try
        {
            using (var Context = new StowTownDbContext())
            {
             

                var artistGroups = await Context.Songs.Where(s => s.IsDeleted != true).ToListAsync();


                Songs = new ObservableCollection<Song>(artistGroups);
                // Set the ItemsSource for the dropdown
                Artist_DD.ItemsSource = new ObservableCollection<Song>(artistGroups);

                var msong = Context.MonthlySongLists.FirstOrDefault(d => d.Id == songId);
                if (msong != null)
                {
                    var selectedSong = Context.Songs.FirstOrDefault(a => a.Id == msong.FkSong);

                    Artist_DD.SelectedItem = Context.Songs.FirstOrDefault(a => a.Id == msong.FkSong);

                    ReleaseDatePicker.Date = msong.Date ?? DateTime.Now; // Corrected line


                }
                //MainThread.BeginInvokeOnMainThread(() =>
                //{
                //    Artist_DD.SelectedItem = Context.Songs.FirstOrDefault(a => a.Id == msong.FkSong);
                //});
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }


    private void DisableControls()
    {
       
        Artist_txt.IsEnabled = false;
        Artist_DD.IsEnabled = false; 
        ReleaseDatePicker.IsEnabled = false;
        SelectedImageView.IsEnabled = false;

    }

    private void EnableControls()
    {
       
        Artist_txt.IsEnabled = true;
        Artist_DD.IsEnabled = true; 
        ReleaseDatePicker.IsEnabled = true;
        SelectedImageView.IsEnabled = true;


    }

    private void ClearForm()
    {
        songId = 0;
        Type = "Create";
        Artist_txt.Text = string.Empty;
        ReleaseDatePicker.Date = DateTime.Now;
        Artist_DD.SelectedItem = string.Empty;
        EnableControls();
    }

    private  async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

 

   

    private async void Song_DD_SelectionChanged(object sender, EventArgs e)
    {
        if (Artist_DD.SelectedItem is Song selectedSong)
        {
            using (var context = new StowTownDbContext())
            {
                // Check for duplicates (if not Edit or View mode)
                if (Type != "Edit" && Type != "View")
                {
                    var currentDate = DateTime.Now;
                    var check = await context.MonthlySongLists
                        .FirstOrDefaultAsync(s => s.FkSong == selectedSong.Id && s.Date.HasValue
                                                 && s.Date.Value.Month == currentDate.Month
                                                 && s.Date.Value.Year == currentDate.Year);

                    if (check != null)
                    {
                        DisplayAlert("Error", "This Song has already been added to the list", "Ok");
                        Artist_DD.SelectedItem = null; // Clear the selection
                        return;
                    }
                }

                // Fetch the artist
                var artist = await context.ArtistGroups.FirstOrDefaultAsync(x => x.Id == selectedSong.FkArtist && x.IsDeleted==false);

                if (artist != null)
                {
                    Artist_txt.Text = artist.Name;
                    Artist_txt.IsEnabled = false;

                    //selectedImagePath1 = selectedSong.Image;

                    //if (selectedImagePath1 != null)
                    //{
                    //    string projectRoot = AppDomain.CurrentDomain.BaseDirectory;

                    //    string fullPath = Path.Combine(projectRoot, "assets", "SongImages");

                    //    if (!Directory.Exists(fullPath))
                    //    {
                    //        Directory.CreateDirectory(fullPath);
                    //    }
                    //    selectedImagePath1 = Path.Combine(fullPath, Path.GetFileName(selectedSong.Image));
                    //    SelectedImageView.Source = ImageSource.FromFile(selectedImagePath1);
                    //}
                    if (!string.IsNullOrEmpty(selectedSong.Image))
                    {
                        selectedImagePath1 = ImageFilesService.GetImageUrl("SongsImages", selectedSong.Image);
                        SelectedImageView.Source = selectedImagePath1;

                    }
                    else
                    {
                        SelectedImageView.Source = null; // Clear the image if none is set
                    }




                }
                else
                {
                    Artist_txt.Text = "Artist not found"; // Handle case where artist is not found
                    Artist_txt.IsEnabled = false;
                }
            }
        }
        else
        {
            Artist_txt.Text = string.Empty; // Clear Artist_txt if no song is selected
            Artist_txt.IsEnabled = true; // Enable the Artist_txt if nothing is selected.
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Validate Song Selection
        if (Artist_DD.SelectedItem == null)
        {
            DisplayAlert("Error","Please select a song.","Ok");
            Artist_DD.Focus();
            return; // Exit early if no song is selected
        }

        if (ReleaseDatePicker.Date == default(DateTime))
        {
            DisplayAlert("Error","Please Enter Date.","Ok");
            return;
        }

        if (songId == 0 || songId == null)
        {

            if (Artist_DD.SelectedItem is Song selectedSong)
            {
                int selectedSongId = selectedSong.Id;
                int selectedArtistId = (int)selectedSong.FkArtist;

                var monthlysong = new MonthlySongList
                {
                    FkSong = selectedSongId,
                    FkArtist = selectedArtistId,
                    Date = ReleaseDatePicker.Date,
                    CreatedAt = DateTime.Now,
                    IsDeleted=false,
                };
                try
                {
                    using (var context = new StowTownDbContext())
                    {
                        context.MonthlySongLists.Add(monthlysong);
                        await context.SaveChangesAsync();
                        DisplayAlert("Success", "Monthly Song saved successfully!", "Ok");
                        await Navigation.PopAsync();
                    }
                }


                catch (Exception ex)
                {
                    //MessageBox.Show("Error saving song: " + ex.Message);

                }
            }
        }
        else
        {
            try
            {

                if (Artist_DD.SelectedItem is Song selectedSong)
                {
                    int selectedSongId = selectedSong.Id;
                    int selectedArtistId = (int)selectedSong.FkArtist;

                    using (var context = new StowTownDbContext())
                    {
                        var msong = await context.MonthlySongLists.FirstOrDefaultAsync(d => d.Id == songId && d.IsDeleted == false);
                        {
                            msong.FkSong = selectedSongId;
                            msong.FkArtist = selectedArtistId;
                            msong.Date = ReleaseDatePicker.Date;
                            msong.UpdatedAt = DateTime.Now;
                            msong.IsDeleted = false; // Ensure IsDeleted is set to false when updating
                        }
                        context.MonthlySongLists.Update(msong);
                        await context.SaveChangesAsync();

                        DisplayAlert("Success", "Songs Updated successfully!", "Ok");
                        await Navigation.PopAsync();
                    }
                }
            }
            catch (Exception ex)
            {
               // DisplayAlert("Error","Error saving song: " + ex.Message","Ok");

            }
        }
    }
}