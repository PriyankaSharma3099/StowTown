using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StowTown.Models;

[Table("RadioStation")]
public partial class RadioStation
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Name { get; set; }

    [StringLength(50)]
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

    [InverseProperty("FkRadioStationNavigation")]
    public virtual ICollection<CallRecord> CallRecords { get; set; } = new List<CallRecord>();

    [InverseProperty("FkRadioStationNavigation")]
    public virtual ICollection<Dj> Djs { get; set; } = new List<Dj>();

    [InverseProperty("FkRadioStationNavigation")]
    public virtual ICollection<SongPossition> SongPossitions { get; set; } = new List<SongPossition>();

    [InverseProperty("FkRadioNavigation")]
    public virtual ICollection<WeekRadioTime> WeekRadioTimes { get; set; } = new List<WeekRadioTime>();
}
