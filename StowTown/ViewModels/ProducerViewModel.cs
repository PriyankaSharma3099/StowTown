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
   public class ProducerViewModel:INotifyPropertyChanged
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [StringLength(100)]
        [Unicode(false)]
        public string? ProducerName { get; set; }

        [StringLength(100)]
        public string? ProducerImage { get; set; }

        [Column("DOB", TypeName = "datetime")]
        public DateTime? Dob { get; set; }

        public string? Address { get; set; }

        [StringLength(50)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? State { get; set; }

        public int? ZipCode { get; set; }

        [StringLength(50)]
        [Unicode(false)]
        public string? MobileNo { get; set; }

        [StringLength(50)]
        public string? Email { get; set; }

        public bool? IsDeleted { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }
        // Additional property for Radio Station name
       
        public int SerialNumber { get; set; }
        private bool _isSelected;
        public bool IsSelectedProducer
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelectedProducer));
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
