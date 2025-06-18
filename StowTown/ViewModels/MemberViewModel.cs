using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StowTown.ViewModels
{
    public class MemberViewModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string? MemberName { get; set; }

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

        public string? Position { get; set; }
        // public DateTime? Birthday { get; set; }

        private DateTime? _birthday = null;
        public DateTime? Birthday
        {
            get => _birthday;
            set
            {
                if (value.HasValue)
                    _birthday = DateTime.SpecifyKind(value.Value, DateTimeKind.Local);
                else
                    _birthday = null;

                OnPropertyChanged(nameof(Birthday));
            }
        }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public int? Zip { get; set; }
        public string? OfficeNumber { get; set; }
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? AccoplishmentsHistory { get; set; }
        public string? Notes { get; set; }

        public int? FkArtistGroup { get; set; }
        public string? ArtistGroupName { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public bool? isImageUpdate { get; set; } = false;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
