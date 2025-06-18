using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace StowTown
{
    public partial class CreateArtistManagement : Window, INotifyPropertyChanged
    {
        public CreateArtistManagementViewModel ViewModel { get; }

        public CreateArtistManagement()
        {
            InitializeComponent();
            ViewModel = new CreateArtistManagementViewModel();
            DataContext = ViewModel;
        }
    }

    public class CreateArtistManagementViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ArtistGroupMember> _artistGroupMembers;
        public ObservableCollection<ArtistGroupMember> ArtistGroupMembers 
        { 
            get => _artistGroupMembers;
            set 
            {
                _artistGroupMembers = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddMemberCommand { get; }
        public ICommand RemoveMemberCommand { get; }
        public ICommand SaveMembersCommand { get; }

        public CreateArtistManagementViewModel()
        {
            ArtistGroupMembers = new ObservableCollection<ArtistGroupMember>();
            
            AddMemberCommand = new RelayCommand(AddMember);
            RemoveMemberCommand = new RelayCommand<ArtistGroupMember>(RemoveMember);
            SaveMembersCommand = new RelayCommand(SaveMembers);

            // Add initial member
            AddMember();
        }

        private void AddMember()
        {
            var newMember = new ArtistGroupMember
            {
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };
            ArtistGroupMembers.Add(newMember);
        }

        private void RemoveMember(ArtistGroupMember member)
        {
            if (ArtistGroupMembers.Count > 1)
            {
                ArtistGroupMembers.Remove(member);
            }
            else
            {
                MessageBox.Show("At least one member is required.", 
                    "Cannot Remove", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveMembers()
        {
            // Implement your save logic here
            MessageBox.Show($"Saved {ArtistGroupMembers.Count} members", 
                "Save Successful", MessageBoxButton.OK);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Generic RelayCommand implementation for ICommand
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
        public void Execute(object parameter) => _execute();
    }

    // Generic RelayCommand with parameter
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => 
            _canExecute == null || _canExecute((T)parameter);

        public void Execute(object parameter) => _execute((T)parameter);
    }
}
