using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StowTown.Models;

[Table("WeekRadioTime")]
public partial class WeekRadioTime
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    public int? WeekDay { get; set; }

    [Column("FK_Radio")]
    public int? FkRadio { get; set; }

    [Column("FK_Timing")]
    public int? FkTiming { get; set; }

    [Column("FK_DJ")]
    public int? FkDj { get; set; }

    [ForeignKey("FkDj")]
    [InverseProperty("WeekRadioTimes")]
    public virtual Dj? FkDjNavigation { get; set; }

    [ForeignKey("FkRadio")]
    [InverseProperty("WeekRadioTimes")]
    public virtual RadioStation? FkRadioNavigation { get; set; }

    [ForeignKey("FkTiming")]
    [InverseProperty("WeekRadioTimes")]
    public virtual Timing? FkTimingNavigation { get; set; }
}
