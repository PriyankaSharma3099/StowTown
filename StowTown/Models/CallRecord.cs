using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StowTown.Models;

[Table("CallRecord")]
public partial class CallRecord
{
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

    public bool? IsChecked { get; set; }

    [ForeignKey("FkDj")]
    [InverseProperty("CallRecords")]
    public virtual Dj? FkDjNavigation { get; set; }

    [ForeignKey("FkRadioStation")]
    [InverseProperty("CallRecords")]
    public virtual RadioStation? FkRadioStationNavigation { get; set; }
}
