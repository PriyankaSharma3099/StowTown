using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StowTown.Models;

[Table("SongPossition")]
public partial class SongPossition
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("FK_Song")]
    public int? FkSong { get; set; }

    [Column("FK_RadioStation")]
    public int? FkRadioStation { get; set; }

    [Column("FK_MonthlySongList")]
    public int? FkMonthlySongList { get; set; }

    public int? Spins { get; set; }

    public int? Possition { get; set; }

    public int? Feild1 { get; set; }

    public int? Feild2 { get; set; }

    [Column("Rotation/Notes")]
    public string? RotationNotes { get; set; }

    [ForeignKey("FkMonthlySongList")]
    [InverseProperty("SongPossitions")]
    public virtual MonthlySongList? FkMonthlySongListNavigation { get; set; }

    [ForeignKey("FkRadioStation")]
    [InverseProperty("SongPossitions")]
    public virtual RadioStation? FkRadioStationNavigation { get; set; }

    [ForeignKey("FkSong")]
    [InverseProperty("SongPossitions")]
    public virtual Song? FkSongNavigation { get; set; }
}
