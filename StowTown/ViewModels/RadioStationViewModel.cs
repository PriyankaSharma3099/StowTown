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
    public class RadioStationViewModel :INotifyPropertyChanged
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [StringLength(50)]
        [Unicode(false)]
        public string? Name { get; set; }

        [StringLength(50)]
        [Unicode(false)]
        public string? Telephone { get; set; }

        [StringLength(20)]
        public string? Code { get; set; }
        public string? MailingAddress { get; set; }

        [StringLength(50)]
        public string? MailCity { get; set; }

        [StringLength(50)]
        public string? MailState { get; set; }

        [StringLength(10)]
        [Unicode(false)]
        public string? MailZip { get; set; }
        public string? PhysicalAddress { get; set; }


        [StringLength(50)]
        public string? PhyCity { get; set; }

        [StringLength(50)]
        public string? PhyState { get; set; }

        [StringLength(10)]
        [Unicode(false)]
        public string? PhyZip { get; set; }

        [StringLength(50)]
        public string? Email { get; set; }

        public bool? IsActive { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }

        public bool? IsDeleted { get; set; }

        [StringLength(100)]
        [Unicode(false)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? Image { get; set; }

        public string? DjName { get; set; }

        private string _selectedRadioStation;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string SelectedRadioStation
        {
            get => _selectedRadioStation;
            set
            {
                if (_selectedRadioStation != value)
                {
                    _selectedRadioStation = value;
                    OnPropertyChanged(SelectedRadioStation);
                }
            }
        }

        //private bool _isActive;

        //public bool IsActive
        //{
        //    get => _isActive;
        //    set
        //    {
        //        if (_isActive != value)
        //        {
        //            _isActive = value;
        //            OnPropertyChanged(nameof(IsActive));
        //        }
        //    }
        //}
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

    }
}
