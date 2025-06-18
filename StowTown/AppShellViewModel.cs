using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StowTown
{

    public class AppShellViewModel : INotifyPropertyChanged
    {
        private string _flyoutTitle;
        public string FlyoutTitle
        {
            get => _flyoutTitle;
            set
            {
                _flyoutTitle = value;
                OnPropertyChanged(nameof(FlyoutTitle));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
