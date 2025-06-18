using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StowTown.Custom_model
{
    public class Member : INotifyPropertyChanged
    {
    
        public string Name { get; set; }
        public string Position { get; set; }
        public string DOB { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string OfficeNumber { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Facebook { get; set; }
        public string Instagram { get; set; }
        public string History { get; set; }
        public string Notes { get; set; }

        public bool IsEnabled { get; set; } = true; // Default enabled

        private string _memberPicture;
        public string MemberPicture
        {
            get => _memberPicture;
            set
            {
                if (_memberPicture != value)
                {
                    _memberPicture = value;
                    OnPropertyChanged(nameof(MemberPicture));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
