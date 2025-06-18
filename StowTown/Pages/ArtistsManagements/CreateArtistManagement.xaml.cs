
using StowTown.Custom_model;
using StowTown.Models;
using StowTown.Pages.RadioStations;
using StowTown.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using static Azure.Core.HttpHeader;
using System.Net;
using System.Reflection;
using Azure.Core;
using Image = Microsoft.Maui.Controls.Image;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.InkML;
using System.Linq;
using StowTown.Services.SaveImageService;
using Microsoft.Maui;


namespace StowTown.Pages.ArtistsManagements;

public partial class CreateArtistManagement : ContentPage, INotifyPropertyChanged, IQueryAttributable
{
    private string Type { get; set; }
    private string SelectedImagePath { get; set; }

    private string ImageName { get; set; }

    private ObservableCollection<ProducerViewModel> _producers;

    //public ObservableCollection<ArtistGroupMember> Members { get; set; } // Fix for CS0618 and CS0246

    public ObservableCollection<MemberViewModel> Members { get; set; }

    public ObservableCollection<Member> members { get; set; } = new ObservableCollection<Member>();

    public bool IsSelected
    {
        get { return _isSelected; }
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }

        }
    }

    public ObservableCollection<ProducerViewModel> Producers

    {
        get => _producers;
        set
        {
            _producers = value;
            OnPropertyChanged(nameof(Producers));
        }
    }

    private string _selectedProducersText;
    public string SelectedProducersText
    {
        get => _selectedProducersText;
        set
        {
            _selectedProducersText = value;
            OnPropertyChanged(nameof(SelectedProducersText));
        }
    }
    public Dictionary<int,bool> SelectedProducerIds { get; set; }  // Holds the selected producer IDs

    private bool _isSelected;

    public static List<ArtistGroupMember>? artistmembers { get; set; }
    private string selectedImagePath1;
    private string imageSource1 { get; set; }
    private string imageSourcepath { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public static int artistId { get; set; }

    private void ShowPopup(object sender, EventArgs e)
    {

        PopupContainer.IsVisible = true;
    }

    private void ClosePopup(object sender, EventArgs e)
    {
        var popupContainer = this.FindByName<ContentView>("PopupContainer");
        if (popupContainer != null)
        {
            popupContainer.IsVisible = false;
        }
        UpdateSelectedProducers();
    }

    public void UpdateSelectedProducers()
    {
        SelectedProducersText = string.Join(", ", Producers.Where(p => p.IsSelectedProducer).Select(p => p.ProducerName));
    }
    private ArtistGroupMember _selectedMember;
    public ArtistGroupMember SelectedMember
    {
        get => _selectedMember;
        set
        {
            if (_selectedMember != value)
            {
                _selectedMember = value;
                OnPropertyChanged();
            }
        }
    }




    // List to store the ArtistForm user controls dynamically

    public CreateArtistManagement()
    {
        InitializeComponent();
      //  BindProducers();
        SelectedProducerIds = new Dictionary<int, bool>();  // Initialize empty list
        Members = new ObservableCollection<MemberViewModel>();
        BindingContext = this;  // Ensure BindingContext is set

    }


    protected override void OnAppearing()
    {
        base.OnAppearing();
        //BindProducers(); // Load data when page appears
    }


    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        // ClearForm();

        if (query.TryGetValue("SelectedId", out var id) && id != null && int.TryParse(id.ToString(), out int parsedId))
        {
            artistId = parsedId;
        }
        else
        {
            artistId = 0;
        }

        Type = query.TryGetValue("Type", out var typeObj) && typeObj != null ? typeObj.ToString() : "Create";
        if (artistId == 0)
        {
            HeaderName.Text = "Create Artist";
            BindProducers();
            NoImageLabel.Text = "NO THUMB";
        }

        if (artistId > 0 && Type == "Edit")
        {
            HeaderName.Text = "Update Artist";
            BindProducers();
            BindArtist();    
        }
        else if (artistId > 0 && Type == "View")
        {
            HeaderName.Text = "View Artist";
            BindProducers();
            BindArtist();
            DisableControls();
            //savebutton.Visibility = Visibility.Hidden;
            // backbutton.Visibility = Visibility.Visible;
            SaveButton.IsVisible = false;
            OnAddMemberClickedButton.IsVisible = false;
            MemberContainer.IsEnabled = true;
            artistId = 0;
            //Type = "";
            DisableAllMemberInputs();
        }
    }

   
    private void BindProducers()
    {
        try
        {
            using (var context = new StowTownDbContext())
            {

                var producersFromDb = context.ProjectProducers.Where(pp => pp.IsDeleted != true)
                                              .Select(p => new ProducerViewModel
                                              {
                                                  Id = p.Id,
                                                  ProducerName = p.ProducerName,
                                                  IsSelectedProducer = false
                                              })
                                              .ToList();

                // Bind the list to the ObservableCollection
                Producers = new ObservableCollection<ProducerViewModel>(producersFromDb);

               // Producer_DD.ItemsSource = Producers;
                // Subscribe to property change to update the text
                foreach (var producer in Producers)
                {
                    producer.PropertyChanged += Producer_PropertyChanged;
                }
            }
        }
        catch (Exception ex)
        {
            //MessageBox.Show(ex.Message);
        }
    }

    private void Producer_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProducerViewModel.IsSelectedProducer))
        {
            UpdateSelectedProducersText(e.PropertyName);
         
        }
    }

    private void UpdateSelectedProducersText(string name)
    {
        //SelectedProducerIds = Producers
        // .Where(p => p.IsSelectedProducer)
        // .Select(p => p.Id)
        // .ToList();

        // Maintain a dictionary of selected IDs internally  
        SelectedProducerIds = Producers
            .Where(p => p.IsSelectedProducer)
            .ToDictionary(p => p.Id, p => true); // Fix: Specify key-value pair for ToDictionary  

        // Generate a comma-separated list of names for display  
        SelectedProducersText = string.Join(", ", Producers
            .Where(p => p.IsSelectedProducer)
            .Select(p => p.ProducerName));

        name =SelectedProducerIds.FirstOrDefault().Value.ToString();
        OnPropertyChanged(nameof(SelectedProducersText));
        OnPropertyChanged(nameof(name));
    }

    protected void DisableControls()
    {
        Group_Name_txt.IsEnabled = false;
        Group_title_txt.IsEnabled = false;
        Group_Website_txt.IsEnabled = false;
        Group_Accomplishments_txt.IsEnabled = false;
        OnAddMemberClickedButton.IsEnabled = false;
        PopupContainer.IsEnabled = false;
        popupContainer.IsEnabled = false;

    }
    private void DisableAllMemberInputs()
    {
        foreach (var memberFrame in MemberContainer.Children.OfType<Microsoft.Maui.Controls.Frame>())
        {
            if (memberFrame.Content is Grid mainGrid)
            {
                foreach (var child in mainGrid.Children)
                {
                    // Handle left panel
                    if (child is VerticalStackLayout leftPanel)
                    {
                        foreach (var item in leftPanel.Children)
                        {
                            if (item is Button button)
                                button.IsEnabled = false;

                            if (item is Microsoft.Maui.Controls.Frame imageFrame && imageFrame.Content is Grid imgGrid)
                            {
                                foreach (var gridChild in imgGrid.Children.OfType<VerticalStackLayout>())
                                {
                                    foreach (var image in gridChild.Children.OfType<Image>())
                                    {
                                        image.GestureRecognizers.Clear();
                                    }
                                }
                            }
                        }
                    }

                    // Handle right form panel
                    if (child is Grid formPanel)
                    {
                        foreach (var element in formPanel.Children)
                        {
                            switch (element)
                            {
                                case Entry entry:
                                    entry.IsEnabled = false;
                                    break;
                                case DatePicker datePicker:
                                    datePicker.IsEnabled = false;
                                    break;
                                case Picker picker:
                                    picker.IsEnabled = false;
                                    break;
                                case Editor editor:
                                    editor.IsEnabled = false;
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }




    private async void BindArtist()
    {
        try
        {
            using (var Context = new StowTownDbContext())
            {
                var artist = Context.ArtistGroups.FirstOrDefault(d => d.Id == artistId );
                if (artist != null) // Fix for CS8602
                {
                    Group_Name_txt.Text = artist.Name ?? string.Empty; // Fix for CS8602
                    Group_title_txt.Text = artist.GroupTitle ?? string.Empty; // Fix for CS8602
                    Group_Website_txt.Text = artist.GroupWebsite ?? string.Empty; // Fix for CS8602
                    Group_Accomplishments_txt.Text = artist.GroupAccomplishment ?? string.Empty; // Fix for CS8602
                                                                                                 //selectedImagePath1 = artist.GroupPicture;

                    IsActiveEntry.IsChecked = artist.IsActive??false; // Assuming you have a checkbox for IsActive
                    //if (selectedImagePath1 != null)
                    //{
                    //    string projectRoot = AppDomain.CurrentDomain.BaseDirectory;

                    //    string fullPath = Path.Combine(projectRoot, "assets", "ArtistImages");

                    //    if (!Directory.Exists(fullPath))
                    //    {
                    //        Directory.CreateDirectory(fullPath);
                    //    }
                    //    selectedImagePath1 = Path.Combine(fullPath, Path.GetFileName(artist.GroupPicture));
                    //    SelectedImageView.Source = ImageSource.FromFile(selectedImagePath1);
                    //}
                    //SelectedImageView.Source = string.IsNullOrEmpty(artist.GroupPicture) ?  ImageFilesService.GetImageUrl("ArtistGroupImages", artist.GroupPicture) :null;

                    if (!string.IsNullOrEmpty(artist.GroupPicture))
                    {
                        var imagePath = ImageFilesService.GetImageUrl("ArtistGroupImages", artist.GroupPicture);
                        if (imagePath != null)
                        {
                            SelectedImageView.Source = ImageSource.FromUri(new Uri(imagePath));
                            NoImageLabel.Text = " ";
                        }



                    }
                    else
                    {
                        
                            NoImageLabel.Text = "NO THUMB";
                       

                    }
                    // Fetch selected producers for the artist
                    var selectedProducerIds = Context.ProjectProducerArtistGroups
                                                     .Where(p => p.FkArtist == artistId)
                                                     .Select(p => p.FkProjectProducer)
                                                     .ToList();

                    if (selectedProducerIds.Any())
                    {
                        //Producers.Clear();
                        foreach (var producer in Producers)
                        {
                            producer.IsSelectedProducer = selectedProducerIds.Contains(producer.Id);
                        }

                        // Update the selected producers text
                        //UpdateSelectedProducersText();
                    }


                    // Fetch members and bind to ObservableCollection
                    var artistmembers = Context.ArtistGroupMembers.Where(m => m.FkArtistGroup == artistId && m.IsDeleted==false).ToList();
                    var artistCount = artistmembers.Count();
                   // await DisplayAlert("count", artistCount.ToString(), "Ok");
                    

                    Members.Clear(); // Clear existing members

                    foreach (var memberData in artistmembers)
                    {
                        string fullPath=" ";
                        //if (memberData.MemberPicture != null)
                        //{
                        //    string parentdirectory = AppDomain.CurrentDomain.BaseDirectory;
                        //    string imagesDirectory = System.IO.Path.Combine(parentdirectory, "assets", "MemberImages");

                        //     fullPath = System.IO.Path.Combine(imagesDirectory, memberData.MemberPicture);

                        //    // Load the image from the constructed path
                        //    if (File.Exists(fullPath))
                        //    {
                        //        //var bitmap = new BitmapImage();
                                
                        //    }

                        //}
                        var member = new MemberViewModel
                        {
                            Id = memberData.Id,
                            MemberName = memberData.MemberName ?? string.Empty,
                            Position = memberData.Position ?? string.Empty,
                            Address = memberData.Address ?? string.Empty,
                            // Birthday = DateTime.TryParse(memberData.Birthday?.ToString(), out DateTime dob) ? dob : (DateTime?)null, // Nullable DateTime fix

                            Birthday = memberData.Birthday != null ? memberData.Birthday.Value : (DateTime?)null,

                            City = memberData.City ?? string.Empty,
                            State = memberData.State ?? string.Empty,
                            Zip = memberData.Zip,
                            OfficeNumber = memberData.OfficeNumber ?? string.Empty,
                            MobileNumber = memberData.MobileNumber ?? string.Empty,
                            Email = memberData.Email ?? string.Empty,
                            Facebook = memberData.Facebook ?? string.Empty,
                            Instagram = memberData.Instagram ?? string.Empty,
                            AccoplishmentsHistory = memberData.AccoplishmentsHistory ?? string.Empty,
                            Notes = memberData.Notes ?? string.Empty,
                            //MemberPicture = fullPath?? memberData.MemberPicture,
                            MemberPicture = string.IsNullOrEmpty(memberData.MemberPicture) ? null : ImageFilesService.GetImageUrl("ArtistMemberImages", memberData.MemberPicture),

                        };



                        Members.Add(member);
                      //  await DisplayAlert("Member Name", member.MemberName, "Ok");
                    }

                    var totalMember = Members.ToList().Count;
                    //await DisplayAlert("Total Members", totalMember.ToString(), "Ok");
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        //await DisplayAlert("Total Members", totalMember.ToString(), "Ok");
                    });
                }
            }
        }
        catch (Exception ex)
        {
             await DisplayAlert("Error",ex.Message,"Ok");
        }
    }

    

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            using (var context = new StowTownDbContext())
            {

                if (artistId == 0)
                {
                    if (string.IsNullOrWhiteSpace(Group_Name_txt.Text))
                    {
                      DisplayAlert("Error","Group Name is required.","Ok");
                        
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(Group_title_txt.Text))
                    {
                        DisplayAlert("Error","Group Title is required.","Ok");
                        return;
                    }
                    var artist = new ArtistGroup
                    {
                        Name = Group_Name_txt.Text,
                        GroupTitle = Group_title_txt.Text,
                        GroupWebsite = Group_Website_txt.Text,
                        GroupAccomplishment = Group_Accomplishments_txt.Text,
                        GroupPicture = ImageName,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                    };
                    context.ArtistGroups.Add(artist);
                    context.SaveChanges();
                    artistId = artist.Id;
                    foreach (var producerId in SelectedProducerIds.Keys)
                    {
                        var projectProducerArtistGroup = new ProjectProducerArtistGroup
                        {
                            FkArtist = artistId,
                            FkProjectProducer = producerId

                        };
                        context.ProjectProducerArtistGroups.Add(projectProducerArtistGroup);
                    }
                    await DisplayAlert("Success", "Artist created successfully.", "OK");
                    context.SaveChanges();
                }
                else
                {
                    var artist = context.ArtistGroups.FirstOrDefault(d => d.Id == artistId && d.IsDeleted == false );

                    if (artist != null)
                    {
                        var oldImageName = artist.GroupPicture;
                        artist.Name = Group_Name_txt.Text;
                        artist.GroupTitle = Group_title_txt.Text;
                        artist.GroupWebsite = Group_Website_txt.Text;
                        artist.GroupAccomplishment = Group_Accomplishments_txt.Text;
                        artist.GroupPicture = ImageName?? artist.GroupPicture;
                        artist.IsActive = IsActiveEntry.IsChecked; // Assuming you have a checkbox for IsActive
                        if (!string.IsNullOrEmpty(ImageName) && oldImageName != ImageName)
                        {

                            if (ImageFilesService.DeleteFtpImage("ArtistGroupImages", oldImageName))
                            {
                                // Handle success case if needed
                                Console.WriteLine("Image deleted successfully.");
                            }
                            else
                            {
                                // Handle failure case if needed
                               // await Application.Current.MainPage.DisplayAlert("Error", "Failed to delete the image.", "OK");
                            }

                        }
                        artist.UpdatedAt = DateTime.UtcNow;
                    }
                    if (artist.Id != null && artist.Id > 0)
                    {

                        var existingProducers = context.ProjectProducerArtistGroups
                       .Where(p => p.FkArtist == artist.Id)
                       .ToList();
                        context.ProjectProducerArtistGroups.RemoveRange(existingProducers);
                        foreach (var producerId in SelectedProducerIds.Keys)
                        {
                            var projectProducerArtistGroup = new ProjectProducerArtistGroup
                            {
                                FkArtist = artist.Id,
                                FkProjectProducer = producerId

                            };
                            context.ProjectProducerArtistGroups.Add(projectProducerArtistGroup);
                        }
                    }
                    await DisplayAlert("Success", "Artist updated successfully.", "OK");
                    context.SaveChanges();
                }

                // Save the selected producers (unchanged)
               
                // Save the members (modified to handle updates)
                foreach (var member in Members)
                {
                    // Normalize input
                    var newName = member.MemberName.Trim().ToLowerInvariant();
                    var newEmail = member.Email?.Trim().ToLowerInvariant();
                    var newMobile = member.MobileNumber?.Trim();
                    // Required field checks
                    if (string.IsNullOrWhiteSpace(newName) ||
                        string.IsNullOrWhiteSpace(newEmail) ||
                        string.IsNullOrWhiteSpace(newMobile) 
                         || member.Zip <= 0||
                        string.IsNullOrWhiteSpace(member.Position))
                    {
                        DisplayAlert("Error", "All required fields must be filled: Name, Email, Mobile, Zip, and Position.", "Ok");
                        return;
                    }
                    // Validate ZIP length
                    if (member.Zip < 5 || member.Zip <= 10)
                    {
                        DisplayAlert("Error", "Please enter a valid Zip Code with at least 5 characters.", "Ok");
                        return;
                    }
                   


                    //var zipPattern = @"^\d{10}$";
                    //if (!System.Text.RegularExpressions.Regex.IsMatch(member.Zip?.ToString() ?? string.Empty, zipPattern))
                    //{
                    //    DisplayAlert("Error", "Zip Code must be  5 digits.", "Ok");
                    //    return;
                    //}
                    // Email format validation
                    var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                    if (!System.Text.RegularExpressions.Regex.IsMatch(newEmail, emailPattern))
                    {
                        DisplayAlert("Error", "Invalid email format.", "Ok");
                        return;
                    }

                    // Mobile number format validation (simple digits-only check, 10–15 digits)
                    var mobilePattern = @"^\d{10,15}$";
                    if (!System.Text.RegularExpressions.Regex.IsMatch(newMobile, mobilePattern))
                    {
                        DisplayAlert("Error", "Invalid mobile number. Only digits allowed (10–15 characters).", "Ok");
                        return;
                    }
                    //Validate DOB is not today or future
                    if (member.Birthday >= DateTime.Today)
                    {
                        DisplayAlert("Error", "Date of Birth cannot be today or a future date.", "Ok");
                        return;
                    }


                    var existingMember = context.ArtistGroupMembers.FirstOrDefault(m => m.FkArtistGroup == artistId && m.Id == member.Id && m.IsDeleted==false); // Check if member exists by Name and ArtistGroup

                    // Check uniqueness of MemberName and Email for this group (excluding self if updating)
                    var duplicate = context.ArtistGroupMembers
                        .Any(m =>
                            m.FkArtistGroup == artistId &&
                            m.IsDeleted == false&&
                            m.Id != member.Id &&
                            (m.MemberName.ToLower() == newName || m.Email.ToLower() == newEmail)
                        );

                    if (duplicate)
                    {
                        DisplayAlert("Error", "Member name or email already exists in this group. Please enter unique values.", "Ok");
                        return;
                    }


                    if (existingMember != null)
                    {
                       
                        var oldmemberImageName = existingMember.MemberPicture;
                        // Update existing member
                        existingMember.Position = member.Position;
                        existingMember.Birthday = member.Birthday;
                        //existingMember.Birthday = DateTime.SpecifyKind(member.Birthday, DateTimeKind.Local);

                        existingMember.Address = member.Address;
                        existingMember.City = member.City;
                        existingMember.State = member.State;
                        existingMember.Zip = member.Zip;
                        existingMember.OfficeNumber = member.OfficeNumber;
                        existingMember.MobileNumber = member.MobileNumber;
                        existingMember.Email = member.Email;
                        existingMember.Facebook = member.Facebook;
                        existingMember.Instagram = member.Instagram;
                        existingMember.AccoplishmentsHistory = member.AccoplishmentsHistory;
                        existingMember.Notes = member.Notes;
                        existingMember.MemberPicture = existingMember.MemberPicture;
                        existingMember.UpdatedAt = DateTime.UtcNow;
                        existingMember.IsDeleted = false;
                        existingMember.MemberName = member.MemberName;

                        if (member.isImageUpdate ?? false)
                        {
                            existingMember.MemberPicture = imageSource1;
                            //No need to set FkArtistGroup as it's already there
                            if (!string.IsNullOrEmpty(imageSource1) && oldmemberImageName != imageSource1)
                            {

                                if (ImageFilesService.DeleteFtpImage("ArtistMemberImages", oldmemberImageName))
                                {
                                    // Handle success case if needed
                                    Console.WriteLine("Image deleted successfully.");
                                }
                                else
                                {
                                    // Handle failure case if needed
                                  //  await Application.Current.MainPage.DisplayAlert("Error", "Failed to delete the image.", "OK");
                                }

                            }
                        }
                       

                    }
                    else
                    {
                        // Create new member
                        var artistMember = new ArtistGroupMember
                        {
                            MemberName = member.MemberName,
                            Position = member.Position,
                            Birthday = member.Birthday,
                            Address = member.Address,
                            City = member.City,
                            State = member.State,
                            Zip = member.Zip,
                            OfficeNumber = member.OfficeNumber,
                            MobileNumber = member.MobileNumber,
                            Email = member.Email,
                            Facebook = member.Facebook,
                            Instagram = member.Instagram,
                            AccoplishmentsHistory = member.AccoplishmentsHistory,
                            Notes = member.Notes,
                            MemberPicture = imageSource1,
                            FkArtistGroup = artistId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt=DateTime.UtcNow,
                            IsDeleted = false,
                           
                        };
                        context.ArtistGroupMembers.Add(artistMember);

                    }
                    context.SaveChanges();
                  //  await Navigation.PopAsync();
                }

                
                await Navigation.PopAsync();
            }
        }
        catch (Exception ex)
        {
            //DisplayAlert("Error", ex.Message, "OK");
            // Consider logging the exception details for debugging.  e.g., Debug.WriteLine(ex.ToString());
        }
    }

    private void OnAddMemberClicked(object sender, EventArgs e)
    {
        MemberLabel.Text= "Member Details";
        //Members.Add(new ArtistGroupMember
        //{
        //    MemberName = "",
        //    Position = "",
        //    Birthday = new DateTime(),
        //    Address = "",
        //    City = "",
        //    State = "",
        //    Zip = 0,
        //    OfficeNumber = "",
        //    MobileNumber = "",
        //    Email = "",
        //    Facebook = "",
        //    Instagram = "",
        //    AccoplishmentsHistory = "",
        //    Notes = "",
        //    MemberPicture=""
        //});
        var newMember = new MemberViewModel
        {
            MemberName = "",
            Position = "",
          //  Birthday = new DateTime(1900, 1, 1), // It's good practice to set a default valid date
          Birthday = null, // Set to current year to avoid invalid date
            Address = "",
            City = "",
            State = "",
            Zip = 0,
            OfficeNumber = "",
            MobileNumber = "",
            Email = "",
            Facebook = "",
            Instagram = "",
            AccoplishmentsHistory = "",
            Notes = "",
            MemberPicture = ""
        };
        Members.Add(newMember);
        // Optionally, set the newly added member as the currently selected one for immediate editing
      //  SelectedMember = newMember;
    }

    private async void SelectImageButton_Clicked(object sender, EventArgs e)
    {
        var member = (sender as Button)?.BindingContext as Member;
        var binddata = new Image();
        await PickImage(true,member, binddata);  // Pass 'true' for Artist
    }

    private  async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void SelectMemberImageButton_Clicked(object sender, EventArgs e)
    {

    }

    private async void OnRemoveMemberClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is MemberViewModel member)
        {
            bool result = await DisplayAlert("Confirm Delete", "Are you sure you want to delete this member?", "Yes", "No");

            if (result)
            {
                try
                {
                    using (var context = new StowTownDbContext())
                    {
                       
                        // 1. Remove from the UI (Members ObservableCollection) FIRST
                        if (Members.Contains(member))
                        {
                            Members.Remove(member);
                        }

                        // 2. Remove from the database
                        var dbMember = await context.ArtistGroupMembers.FindAsync(member.Id); // Use member.Id, not memberToDelete.Id
                        if (dbMember != null)
                        {
                            //context.ArtistGroupMembers.Remove(dbMember);
                            dbMember.IsDeleted = true;
                            var imageName = dbMember.MemberPicture;
                            if (imageName != null)
                            {

                                bool isDelete = ImageFilesService.DeleteFtpImage("ArtistMemberImages", imageName);
                                if (!isDelete)
                                {
                                    await DisplayAlert("Error", "Image File not found", "Ok");
                                }
                            }
                            await context.SaveChangesAsync();
                            await DisplayAlert("Success", "Member Deleted Successfully.", "OK");
                        }
                        else
                        {
                            // 1. Remove from the UI (Members ObservableCollection) FIRST
                            if (Members.Contains(member))
                            {
                                var imageName = member.MemberPicture;
                                if (imageName != null)
                                {

                                    bool isDelete = ImageFilesService.DeleteFtpImage("ArtistMemberImages", imageName);
                                    if (!isDelete)
                                    {
                                        await DisplayAlert("Error", "Image File not found", "Ok");
                                    }
                                }
                                Members.Remove(member);
                            }
                            var totalMember = Members.ToList();
                            if (totalMember.Count == 0)
                            {
                                MemberLabel.Text = " ";
                            }
                            // Handle the case where the member is not found in the database.
                            // await DisplayAlert("Error", "Member not found in database.", "OK");
                        }

                     
                        // Optionally refresh the UI if needed (e.g., if you have a list view)
                        // LoadData(); // If you have a LoadData method.
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", ex.Message, "OK");
                    // Log the exception for debugging:
                    
                }
            }
        }
    }

    private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is ProducerViewModel selectedProducer)
        {
            SelectedProducersText = null; // Deselect the item in the ListView
        }
    }

    // Fix for CS0104: Explicitly specify the namespace for CheckBox to resolve ambiguity.
    private void OnCheckboxChanged(object sender, Microsoft.Maui.Controls.CheckedChangedEventArgs e)
    {
        if (sender is Microsoft.Maui.Controls.CheckBox checkBox && checkBox.BindingContext is ProducerViewModel producer)
        {
            producer.IsSelectedProducer = e.Value;  // Update ViewModel
            UpdateSelectedProducers();
        }
    }

    // Pick Image from Camera or Gallery
    private async Task PickImage(bool isArtist,Member member,object binding)
    {
        
        try
        {


            // Still use a small delay
            await Task.Delay(100);
            Device.BeginInvokeOnMainThread(async () =>
            {
                var result = await DisplayActionSheet("Select Image Source", "Cancel", null as string, "Camera", "Gallery");

            if (result == "Camera")
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    bool hasPermission = await CheckAndRequestPermissions();
                    if (!hasPermission)
                    {
                        await DisplayAlert("Permission Denied", "Camera access is required.", "OK");
                        return;
                    }

                    var photo = await MediaPicker.Default.CapturePhotoAsync();
                    if (photo != null)
                    {
                        await ProcessImage(photo, isArtist,member,binding);
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Camera is not supported on this device", "OK");
                }
            }
            else if (result == "Gallery")
            {
                bool hasPermission = await CheckAndRequestPermissions();
                if (!hasPermission)
                {
                    await DisplayAlert("Permission Denied", "Gallery access is required.", "OK");
                    return;
                }

                var photo = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Select an Image",
                    FileTypes = FilePickerFileType.Images
                });

                if (photo != null)
                {
                    await ProcessImage(photo, isArtist,member,binding);
                }
            }
            
                });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to pick image: {ex.Message}", "OK");
        }

    }
    private async Task<bool> CheckAndRequestPermissions()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
        }

        var storageStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
        if (storageStatus != PermissionStatus.Granted)
        {
            storageStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
        }

        return status == PermissionStatus.Granted && storageStatus == PermissionStatus.Granted;
    }


    // Process & Save Image
    private async Task ProcessImage(FileResult photo, bool isArtist, Member member,object binding)
    {
        try
        {
            string imagesDir = GetDynamicImagePath(isArtist); // Get the correct path
            if (!Directory.Exists(imagesDir))
            {
                Directory.CreateDirectory(imagesDir);
            }

            // Generate a unique filename
            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
            string newFilePath = Path.Combine(imagesDir, fileName);

            // Copy file to the project directory
            using (var stream = await photo.OpenReadAsync())
            using (var newStream = File.OpenWrite(newFilePath))
            {
                await stream.CopyToAsync(newStream);
            }

            // Store image path
            SelectedImagePath = newFilePath;
         //   ImageName = fileName;

            if(isArtist)
            {   //selectedImagePath1 = newFilePath;
                //await UpdateImageDisplay(newFilePath);
                // NoImageLabel.Text = "";
                var imageName = await ImageFilesService.UploadImageToServer(photo, "ArtistGroupImages");
                if (!String.IsNullOrEmpty(imageName))
                {
                    var imagePath = ImageFilesService.GetImageUrl("ArtistGroupImages", imageName);
                    ImageName = imageName;
                    await UpdateImageDisplay(imagePath);
                }
            }
            else
            {

                //imageSource1 = fileName;
                //imageSourcepath = newFilePath;
                // await UpdateImageDisplayForMember(newFilePath,member,binding);
                var imageName = await ImageFilesService.UploadImageToServer(photo, "ArtistMemberImages");
                if (!String.IsNullOrEmpty(imageName))
                {
                    var imagePath = ImageFilesService.GetImageUrl("ArtistMemberImages", imageName);
                    await UpdateImageDisplayForMember(imagePath, member, binding,imageName);
                }
            }
           
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to process image: {ex.Message}", "OK");
        }
    }


    // Get Dynamic Image Path (Inside assets/DJ)
    private string GetDynamicImagePath(bool isArtist)
    {
        string projectRoot = AppDomain.CurrentDomain.BaseDirectory;
        string folderName = isArtist ? "ArtistImages" : "MemberImages"; // Separate folders
        string fullPath = Path.Combine(projectRoot, "assets", folderName);

        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        return fullPath;
    }



    // Update UI with Selected Image
    private async Task UpdateImageDisplay(string imagePath)
    {
        try
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                SelectedImageView.Source = imagePath; // Update Image Source
                NoImageLabel.IsVisible = false; // Hide "NO THUMB" label
            }
            else
            {
                SelectedImageView.Source = "imagepicker.png"; // Default image
                NoImageLabel.IsVisible = true;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                //SelectedImageView.Source = ImageSource.FromFile(imagePath);
                SelectedImageView.Source = imagePath;
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to display image: {ex.Message}", "OK");
        }
    }


    private async Task UpdateImageDisplayForMember(string imagePath, Member member,object binding,string imageName)
    {
        try
        {
            var imageView = this.FindByName<Microsoft.Maui.Controls.Image>("SelectedImageView1");
           var currentImage = this.FindByName<Microsoft.Maui.Controls.Image>("SelectedImageView1");


            if (binding is MemberViewModel currentmember)
            {
                var currentuser =Members.Where(m=>m.Id==currentmember.Id).FirstOrDefault();
                //if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                //{
                //    //Set image path directly on the object
                //    currentuser.MemberPicture = imagePath;




                //}
                if (!string.IsNullOrEmpty(imagePath))
                {
                    currentuser.MemberPicture = imagePath;
                    currentmember.isImageUpdate = true;
                    currentuser.isImageUpdate = true;
                    imageSource1 = imageName;
                }

                else
                {
                    // fallback image
                    currentuser.MemberPicture = "imagepicker.png";
                }

                //  No need to manipulate UI elements like ImageView — let XAML binding handle that
            }
            else
            {
                Console.WriteLine("BindingContext is not of type Member.");
            }

            //if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            //{
            //    if (imageView != null)
            //    {
            //        imageView.Source = ImageSource.FromFile(imagePath);
            //    }
            //    //SelectedImageViewForMember.Source = ImageSource.FromFile(imagePath); // Update Image Source
            //    //memberImageSource.Source = ImageSource.FromFile(imagePath); // Update Image Source
            //    //memberImageSource.Source=ImageSource.FromFile(imagePath); // Update Image Source
            //    // SelectedImageView1.Source = ImageSource.FromFile(imagePath); // Update Image Source
            //    //SelectedImageView1.Source = ImageSource.FromFile(imagePath); // Update Image Source
            //    member.MemberPicture = imagePath;
            //    NoImageLabel.IsVisible = false; // Hide "NO THUMB" label
            //}
            //else
            //{
            //    imageView.Source = "imagepicker.png"; // Default image
            //    NoImageLabel.IsVisible = true;
            //}
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to display image: {ex.Message}", "OK");
        }
    }

    private async void SelectImageButtonForMember_Clicked(object sender, EventArgs e)
    {
        var member = (sender as Button)?.BindingContext as Member;
        //var currentmember = Members.FirstOrDefault(m => m.MemberName == member.Name);
        //await PickImage(false, member);
        if (sender is Image image && image.BindingContext is Member user)
        {
            // Check if the image is already selected
            if (image.Source != null && image.Source.ToString() != "imagepicker.png")
            {
                // If already selected, show a message or handle accordingly
                await DisplayAlert("Info", "Image already selected.", "OK");
                return;
            }
             // Pass 'false' for Member
        }
    }

    private void OnlyNumericTextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = sender as Entry;

        // Only allow numeric input by checking if the new text is a valid number
        if (!string.IsNullOrEmpty(entry.Text) && !entry.Text.All(char.IsDigit))
        {
            // If it's not a valid number, clear the invalid input or give feedback
            entry.Text = e.OldTextValue;
            DisplayAlert("Invalid Input", "Please enter only numeric values ", "OK");
        }
    }

    private void OnEmailTextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = sender as Entry;

        // Regex for validating email format
        string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        var regex = new System.Text.RegularExpressions.Regex(emailPattern);

        // Check if the entered email matches the pattern
        if (!string.IsNullOrEmpty(entry.Text) && !regex.IsMatch(entry.Text))
        {
            // If it doesn't match, revert to the old value
            entry.Text = e.OldTextValue;
            DisplayAlert("Invalid Email", "Please enter a valid email address.", "OK");
        }
    }

    private async void OnMemberImageTapped(object sender, TappedEventArgs e)
    {
      
        if (sender is Image image)
        {
            var binding = image.BindingContext;
            var currentuser = new Member();
            await PickImage(false, currentuser, binding);
            if (binding == null)
            {
                Console.WriteLine(" BindingContext is NULL.");
                return;
            }

            if (binding is Member user)
            {
                // Safe to use user now
                await PickImage(false, user, binding);
            }
        }
    }

    private void ShowPopup(object sender, TappedEventArgs e)
    {

    }
}
