using CommunityToolkit.Maui.Views;
using StowTown.Models;
using StowTown.Pages.DJ;
using StowTown.Services.SaveImageService;
using StowTown.ViewModels;
using System.Collections.ObjectModel;

namespace StowTown;

public partial class PojectProducerManagement : ContentPage
{
    private ObservableCollection<ProducerViewModel> _allProducers;
    private ObservableCollection<ProducerViewModel> _currentPageProducers;
    private const int PageSize = 10;
    private int _currentPage = 1;
    private int _totalPages = 1;

    private ContentView _mainContent;

  
    public PojectProducerManagement()
    {
        InitializeComponent();
        _currentPageProducers = new ObservableCollection<ProducerViewModel>();
        ProducerList.ItemsSource = _currentPageProducers;
        LoadData();

   
    // Subscribe to the refresh message
    MessagingCenter.Subscribe<CreateProjectProducer>(this, "RefreshProducerList", (sender) =>
        {
            LoadData();
        });
    }
    public PojectProducerManagement(ContentView mainContent)
    {

        InitializeComponent();
        _currentPageProducers = new ObservableCollection<ProducerViewModel>();
        ProducerList.ItemsSource = _currentPageProducers;
        LoadData();


        // Subscribe to the refresh message
        MessagingCenter.Subscribe<CreateProjectProducer>(this, "RefreshProducerList", (sender) =>
        {
            LoadData();
        });
    
    _mainContent = mainContent;

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
        LoadData(); // Refresh data every time the page appears
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Unsubscribe from the message when the page disappears
        MessagingCenter.Unsubscribe<CreateProjectProducer>(this, "RefreshProducerList");
    }

    public void LoadData()
    {
        try
        {
            using (var context = new StowTownDbContext())
            {
                var producerList = context.ProjectProducers
                    .Where(d => d.IsDeleted != true)
                    .OrderByDescending(a => a.Id)
                    .ToList();

                var producerViewList = new List<ProducerViewModel>();
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imagesDirectory = Path.Combine(exeDirectory, "Images", "Producer");

                for (int i = 0; i < producerList.Count; i++)
                {
                    var producer = producerList[i];
                    producerViewList.Add(new ProducerViewModel
                    {
                        Id = producer.Id,
                        ProducerName = producer.ProducerName,
                        MobileNo = producer.MobileNo,
                        Email = producer.Email,
                        Address = producer.Address,
                        City = producer.City,
                        State = producer.State,
                        ZipCode = producer.ZipCode,
                        Dob = producer.Dob,
                        CreatedAt = producer.CreatedAt,
                        UpdatedAt = producer.UpdatedAt,
                        //ProducerImage = producer.ProducerImage != null ? Path.Combine(imagesDirectory, producer.ProducerImage) : null,
                        ProducerImage=ImageFilesService.GetImageUrl("ProducerImages", producer.ProducerImage),
                        SerialNumber = i + 1
                    });
                }

                _allProducers = new ObservableCollection<ProducerViewModel>(producerViewList);
                _totalPages = (int)Math.Ceiling(_allProducers.Count / (double)PageSize);
                LoadCurrentPage();
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void LoadCurrentPage()
    {
        _currentPageProducers.Clear();
        var pageItems = _allProducers
            .Skip((_currentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        foreach (var item in pageItems)
        {
            _currentPageProducers.Add(item);
        }

        UpdatePaginationControls();
    }

    private void UpdatePaginationControls()
    {
        PreviousButton.IsEnabled = _currentPage > 1;
        NextButton.IsEnabled = _currentPage < _totalPages;
        PageInfo.Text = $"Page {_currentPage} of {_totalPages}";
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

    private async void CreateProducer_Click(object sender, EventArgs e)
    {

        if (sender is ImageButton button && button.CommandParameter is int selectedId)
        {
            // NavigationData.SelectedId = selectedId;
            // NavigationData.Type = "View";
            // await Shell.Current.GoToAsync(nameof(CreateProjectProducer));
            await Shell.Current.GoToAsync($"{nameof(CreateProjectProducer)}?SelectedId={selectedId}&Type=");
        }
    }

   


    public async void Delete_Click(object sender, EventArgs e)
    {
        //var producer = (ProducerViewModel)((Button)sender).BindingContext;
        //bool result = await DisplayAlert("Confirm Delete", "Are you sure you want to delete this item?", "Yes", "No");

        //if (result)
        //{
        //    try
        //    {
        //        using (var context = new StowTownDbContext())
        //        {
        //            var button = sender as Button;
        //            var producerRow = button?.BindingContext as ProducerViewModel;
        //            if (producerRow != null)
        //            {
        //                var getRowToDelete = await context.ProjectProducers.FindAsync(producerRow.Id);
        //                if (getRowToDelete != null)
        //                {
        //                    getRowToDelete.IsDeleted = true;
        //                    await context.SaveChangesAsync();
        //                    await DisplayAlert("Success", "Project Producer Deleted Successfully.", "OK");
        //                    LoadData();
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await DisplayAlert("Error", ex.Message, "OK");
        //    }
        //}

        try
        {
            if (sender is ImageButton button)
            {
                var producerId = button.CommandParameter as int?;

                if (!producerId.HasValue)
                {
                    await DisplayAlert("Error", "Producer ID not found.", "OK");
                    return;
                }

                bool result = await DisplayAlert("Confirm Delete", "Are you sure you want to delete this item?", "Yes", "No");

                if (result)
                {
                    using (var context = new StowTownDbContext())
                    {
                        var producerRow = await context.ProjectProducers.FindAsync(producerId.Value);

                        if (producerRow != null)
                        {
                            var imageFileName = producerRow.ProducerImage;
                            if (imageFileName != null) {
                                // Delete the image file from the server
                                var isDeleted =  ImageFilesService.DeleteFtpImage("ProducerImages", imageFileName);
                                if (!isDeleted)
                                {
                                    //await DisplayAlert("Error", "Failed to delete the image file.", "OK");
                                    return;
                                }
                            }
                            producerRow.IsDeleted = true;
                            await context.SaveChangesAsync();

                            await DisplayAlert("Success", "Producer deleted successfully.", "OK");

                            // Reload data after deletion
                            LoadData();
                        }
                        else
                        {
                            await DisplayAlert("Error", "Producer not found in the database.", "OK");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }



    }

    private void nameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        //string filter = nameTextBox.Text.Trim().ToLower();

        //if (string.IsNullOrEmpty(filter))
        //{
        //    ProducerList.ItemsSource = _producerCollection; // Reset to original data
        //}
        //else
        //{
        //    _filteredCollection = new ObservableCollection<ProducerViewModel>(
        //        _producerCollection?.Where(a =>
        //            (a.ProducerName?.ToLower().Contains(filter) ?? false) ||
        //            (a.Email?.ToLower().Contains(filter) ?? false) ||
        //            (a.MobileNo?.ToLower().Contains(filter) ?? false)) ??
        //        new List<ProducerViewModel>()
        //    );

        //    ProducerList.ItemsSource = _filteredCollection; // Display filtered data
        //}
    }

 

    private async void DeleteProducer_Click(object sender, EventArgs e)
    {
        bool result = await DisplayAlert("Confirm Delete", "Are you sure you want to delete this item?", "Yes", "No");

        if (result)
        {
            try
            {
                using (var context = new StowTownDbContext())
                {
                    var button = sender as ImageButton;
                    var producerRow = button?.BindingContext as ProducerViewModel;
                    if (producerRow != null)
                    {
                        var getRowToDelete = await context.ProjectProducers.FindAsync(producerRow.Id);
                        if (getRowToDelete != null)
                        {
                            getRowToDelete.IsDeleted = true;
                            await context.SaveChangesAsync();
                            await DisplayAlert("Success", "Project Producer Deleted Successfully.", "OK");
                            LoadData();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.ToLower() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(searchText))
        {
            // Reset to first page and show all records
            _currentPage = 1;
            LoadCurrentPage();
            PreviousButton.IsVisible = true;
            NextButton.IsVisible = true;
            PageInfo.IsVisible = true;
            IsEmptyPageInfo.Text = "No Data Availabel";
            IsEmptyPageInfo.IsVisible = false;
        }
        else
        {
            // Filter all records and update pagination
            var filteredResults = _allProducers.Where(producer =>
                producer.ProducerName.ToLower().Contains(searchText) ||
                producer.MobileNo.ToLower().Contains(searchText)
            ).ToList();

            // Update serial numbers
            for (int i = 0; i < filteredResults.Count; i++)
            {
                filteredResults[i].SerialNumber = i + 1;
            }

            _currentPageProducers.Clear();
            var pageItems = filteredResults
                .Skip((_currentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            foreach (var item in pageItems)
            {

                _currentPageProducers.Add(item);
            }

           if (pageItems.Count==0)
            {
                PreviousButton.IsVisible = false;
                NextButton.IsVisible = false;
                PageInfo.IsVisible = false;
                IsEmptyPageInfo.Text = "No Data Availabel";
                IsEmptyPageInfo.IsVisible = true;
                await  DisplayAlert("No Results", "No Producer found matching your search criteria", "Ok");    
            }
            _totalPages = (int)Math.Ceiling(filteredResults.Count / (double)PageSize);
            UpdatePaginationControls();
        }
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        // Instead of assigning static properties in the object initializer, set them explicitly after instantiation.  
        var createProducerPage = new CreateProjectProducer();
        CreateProjectProducer.ProducerId = 0;
        CreateProjectProducer.Type = "";

        await Shell.Current.Navigation.PushAsync(createProducerPage);
    }

    async void OnViewButtonClicked(object sender, EventArgs e)
    {
        //if (sender is ImageButton button && button.CommandParameter is int selectedId)
        //{
        //    // NavigationData.SelectedId = selectedId;
        //    // NavigationData.Type = "View";
        //    // await Shell.Current.GoToAsync(nameof(CreateProjectProducer));
        //    await Shell.Current.GoToAsync($"{nameof(CreateProjectProducer)}?SelectedId={selectedId}&Type=View");
        //}
        try
        {
            var button = sender as ImageButton;
            var producerRow = button?.BindingContext as ProducerViewModel;
            if (producerRow != null)
            {
                CreateProjectProducer.ProducerId = producerRow.Id;
                CreateProjectProducer.Type = "View";
                // await Shell.Current.GoToAsync(nameof(CreateProjectProducer));
                await Shell.Current.GoToAsync($"{nameof(CreateProjectProducer)}?SelectedId={producerRow.Id}&Type=View");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }

    }

    private async void OnEditButtonClicked(object sender, EventArgs e)
    {
        //if (sender is ImageButton button && button.CommandParameter is int selectedId)
        //{
        //    NavigationData.SelectedId = selectedId;
        //    NavigationData.Type = "Edit";
        //    //    await Shell.Current.GoToAsync(nameof(CreateProjectProducer));
        //    await Shell.Current.GoToAsync($"{nameof(CreateProjectProducer)}?SelectedId={selectedId}&Type=Edit");
        //}
        try
        {
            var button = sender as ImageButton;
            var producerRow = button?.BindingContext as ProducerViewModel;
            if (producerRow != null)
            {
                CreateProjectProducer.ProducerId = producerRow.Id;
                CreateProjectProducer.Type = "Edit";
               // await Shell.Current.GoToAsync(nameof(CreateProjectProducer));
                await Shell.Current.GoToAsync($"{nameof(CreateProjectProducer)}?SelectedId={producerRow.Id}&Type=Edit");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StowTownLogs");
            Directory.CreateDirectory(folder);

            string filePath = Path.Combine(folder, $"startup-error-{DateTime.Now:yyyyMMdd-HHmmss}.log");

            // Check if file exists
            if (File.Exists(filePath))
            {
                File.AppendAllText(filePath, "Crash Error" + Environment.NewLine + ex);
            }
            else
            {
                File.WriteAllText(filePath, "Crash Error" + Environment.NewLine + ex);
            }
        }


    }

    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        bool result = await DisplayAlert("Confirm Delete", "Are you sure you want to delete this item?", "Yes", "No");

        if (result)
        {
            try
            {
                if (sender is ImageButton button && button.CommandParameter is int selectedId)
                {
                    using (var context = new StowTownDbContext())
                    {
                        var producer = await context.ProjectProducers.FindAsync(selectedId);
                        if (producer != null)
                        {
                            var imageFileName = producer.ProducerImage;
                            if (imageFileName != null)
                            {
                                // Delete the image file from the server
                                var isDeleted = ImageFilesService.DeleteFtpImage("ProducerImages", imageFileName);
                                if (!isDeleted)
                                {
                                    //await DisplayAlert("Error", "Failed to delete the image file.", "OK");
                                    return;
                                }
                            }
                            producer.IsDeleted = true;
                            await context.SaveChangesAsync();
                            await App.Current.MainPage.DisplayAlert("Success", "Project Producer Deleted Successfully.", "OK");
                            LoadData();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
        //try
        //{
        //    // Get the button that was clicked
        //    var button = (ImageButton)sender;
        //    var producerId = (int)button.CommandParameter;  // CommandParameter should be the Id

        //    // Confirm before deleting
        //    bool result = await Application.Current.MainPage.DisplayAlert("Confirm Delete", "Are you sure you want to delete this item?", "Yes", "No");

        //    if (result)
        //    {
        //        using (var context = new StowTownDbContext())
        //        {
        //            // Find the producer to delete
        //            var producer = await context.ProjectProducers.FindAsync(producerId);
        //            if (producer != null)
        //            {
        //                producer.IsDeleted = true;  // Mark as deleted
        //                await context.SaveChangesAsync();
        //                await Application.Current.MainPage.DisplayAlert("Success", "Project Producer Deleted Successfully.", "OK");

        //                // Refresh data (assuming you have a LoadData method)
        //                LoadData();
        //            }
        //            else
        //            {
        //                await Application.Current.MainPage.DisplayAlert("Error", "Producer not found.", "OK");
        //            }
        //        }
        //    }
        //}
        //catch (Exception ex)
        //{
        //    await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        //}
    }

    private async void AddButton_Clicked(object sender, EventArgs e)
    {
        var navigationParameter = new Dictionary<string, object>
        {
            { "SelectedId", 0 },
            { "Type", "Create" }
        };
        await Shell.Current.GoToAsync($"///CreateProjectProducer", navigationParameter);
    }

    
private async void OnOpenPopupClicked(object sender, EventArgs e)
    {
        try
        {
            //var dshboardPage = new Dashboard();
            //var page = new CreateProjectProducer(0, "Create", dshboardPage);

            //// Extract only the content from the ContentPage
            //var viewContent = page.Content;

            //// Show the popup using extracted content
            //var popup = new ModelPopup(viewContent);
            //await this.ShowPopupAsync(popup);
            var dashboardPage = new Dashboard(); // optional, based on your logic
            var producerView = new CreateDJ();

            var popup = new ModelPopup(producerView.Content);
            await Application.Current.MainPage.ShowPopupAsync(popup); // 'this' must be a Page or VisualElement
            // Show the popup as a modal ContentPage  
            ;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
  

}