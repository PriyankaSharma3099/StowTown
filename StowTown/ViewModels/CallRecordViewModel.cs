using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace StowTown.ViewModels
{
    public class CallRecordViewModel  : INotifyPropertyChanged
    {
        public CallRecordViewModel()
        {
            // Initialize default values if needed  
           // IsEnabled = true;
            //_checked = false;
        }

        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("FK_DJ")]
        public int? FkDj { get; set; }

        [Column("FK_RadioStation")]
        public int? FkRadioStation { get; set; }

        [Column("Start_Time", TypeName = "datetime")]
        public DateTime? StartTime { get; set; }

        [Column("End_Time", TypeName = "datetime")]
        public DateTime? EndTime { get; set; }

        [StringLength(100)]
        [Unicode(false)]
        public string? Notes { get; set; }

        [StringLength(100)]
        [Unicode(false)]
        public string? Label { get; set; }

        public bool? IsDeleted { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }
         //public bool? IsChecked { get; set; }
        // Additional property for Radio Station name
        public string RadioStationName { get; set; }

       // public string DjName { get; set; }
        public string Timing { get; set; }
        public string Image { get; set; }
        //public bool Checked { get; set; }
      // public bool IsEnabled { get; set; }
        public int SerialNumber { get; set; }
        public string DjName { get; internal set; }

        public bool _suppressCheckedEvents { get; set; } = false;
        //private bool _checked;
        //public bool Checked
        //{
        //    get => _checked;
        //    set
        //    {
        //        if (_checked != value)
        //        {
        //            _checked = value;
        //            OnPropertyChanged(nameof(Checked));
        //        }
        //    }
        //}

        private bool _checked;
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    OnPropertyChanged(nameof(Checked));
                    // Auto-update IsEnabled based on logic
                    IsEnabled = !_checked;
                }
            }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        //protected void OnPropertyChanged(string name) =>
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
       


        public void OnPropertyChanged(string propertyName)
        {
            if (Application.Current?.Dispatcher != null)
            {
                Application.Current.Dispatcher.Dispatch(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                });
            }
            else
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        //bool _suppressCheckedEvents = false;
    }
}
