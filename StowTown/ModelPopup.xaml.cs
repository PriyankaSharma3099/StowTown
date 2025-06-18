using CommunityToolkit.Maui.Views;

namespace StowTown;

public partial class ModelPopup : Popup
{
    public ModelPopup()
    {
        InitializeComponent();
    }
    //public ModelPopup(View content)
    //{
    //    Content = new Border
    //    {
    //        Padding = 10,
    //        Stroke = Colors.Gray,
    //        Background = Colors.White,
    //        Content = content
    //    };
    //}
    private void OnYesClicked(object sender, EventArgs e)
    {
        Close("yes");
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close("no");
    }

    public ModelPopup(View content)
    {
        Color = Colors.Black.WithAlpha(0.5f); // Dim background
        CanBeDismissedByTappingOutsideOfPopup = true;

        // Create a Close button
        var closeButton = new Button
        {
            Text = "Close",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.End
        };

        // Handle the close button click event
        closeButton.Clicked += (sender, args) =>
        {
            this.Close(); // Close the popup
        };

        // Wrap the content and close button inside a vertical stack layout
        Content = new Frame
        {
            BackgroundColor = Colors.Blue,
            CornerRadius = 10,
            Padding = 20,
            WidthRequest = 300,
            HeightRequest = 400,
            Content = new VerticalStackLayout
            {
                Children =
                {
                    content,    // Your dynamic content
                    closeButton // The Close button
                }
            }
        };
    }



}
