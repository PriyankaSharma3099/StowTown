using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StowTown.Models;

[Table("DJ")]
public partial class Dj
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
    public string? MobileNumber { get; set; }

    public string? PersonalData { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Birthday { get; set; }

    [StringLength(50)]
    public string? MailingAddress { get; set; }

    [StringLength(50)]
    public string? MailCity { get; set; }

    [StringLength(50)]
    public string? MailState { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? MailZip { get; set; }

    [StringLength(50)]
    public string? PhysicalAddress { get; set; }

    [StringLength(50)]
    public string? PhyCity { get; set; }

    [StringLength(50)]
    public string? PhyState { get; set; }

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

    [InverseProperty("FkDjNavigation")]
    public virtual ICollection<CallRecord> CallRecords { get; set; } = new List<CallRecord>();

    [ForeignKey("FkRadioStation")]
    [InverseProperty("Djs")]
    public virtual RadioStation? FkRadioStationNavigation { get; set; }

    [InverseProperty("FkDjNavigation")]
    public virtual ICollection<WeekRadioTime> WeekRadioTimes { get; set; } = new List<WeekRadioTime>();

    [Column(TypeName = "nvarchar(100)")]
    public string? HomeAddress { get; set; }

    [Column(TypeName = "nvarchar(50)")]
    public string? HomeCity { get; set; }

    [Column(TypeName = "nvarchar(50)")]
    public string? HomeState { get; set; }

    [Column(TypeName = "varchar(10)")]
    public string? HomeZipCode { get; set; }
}
