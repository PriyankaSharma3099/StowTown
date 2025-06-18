using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StowTown.ViewModels
{
    class DjViewModel
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [StringLength(50)]
        [Unicode(false)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        [Unicode(false)]
        public string? LastName { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsDeleted { get; set; }

        [Column("FK_RadioStation")]
        public int? FkRadioStation { get; set; }

        [StringLength(50)]
        public string? Email { get; set; }

        [StringLength(50)]
        [Unicode(false)]
        public string? MobileNumber { get; set; }

        public string? PersonalData { get; set; }

        [StringLength(20)]
        [Unicode(false)]
        public string? Birthday { get; set; }
        [StringLength(50)]
        public string? MailingAddress { get; set; }

        [Column("Mail_City")]
        [StringLength(50)]
        public string? MailCity { get; set; }

        [Column("Mail_State")]
        [StringLength(50)]
        public string? MailState { get; set; }

        [Column("Mail_Zip")]
        [StringLength(10)]
        [Unicode(false)]
        public string? MailZip { get; set; }
        [StringLength(50)]
        public string? PhysicalAddress { get; set; }

        [Column("Phy_City")]
        [StringLength(50)]
        public string? PhyCity { get; set; }

        [Column("Phy_State")]
        [StringLength(50)]
        public string? PhyState { get; set; }

        [Column("Phy_ZipCode")]
        [StringLength(10)]
        [Unicode(false)]
        public string? PhyZipCode { get; set; }

        [StringLength(20)]
        [Unicode(false)]
        public string? Spouse { get; set; }

        [StringLength(20)]
        [Unicode(false)]
        public string? Children { get; set; }

        [Column("TShirtSize")]
        [StringLength(10)]
        [Unicode(false)]
        public string? TshirtSize { get; set; }

        [StringLength(50)]
        [Unicode(false)]
        public string? PrayerRequest { get; set; }

        [StringLength(50)]
        [Unicode(false)]
        public string? NotesOfCall { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }

        [StringLength(100)]
        public string? Image { get; set; }

        // Additional property for Radio Station name
        public string RadioStationName { get; set; }

        public int SerialNumber { get; set; }
    }
}
