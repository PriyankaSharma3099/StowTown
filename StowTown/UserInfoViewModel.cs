using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel;

namespace StowTown
{
    public class UserInfoViewModel:INotifyPropertyChanged
{
    private string _name;
    private string _email;
    private string _imageUrl;

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(nameof(Name)); }
    }

    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(nameof(Email)); }
    }

    public string ImageUrl
    {
        get => _imageUrl;
        set { _imageUrl = value; OnPropertyChanged(nameof(ImageUrl)); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
}