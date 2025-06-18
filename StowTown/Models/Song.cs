using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StowTown.Models;

public partial class Song
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Name { get; set; }

    [Column("FK_Artist")]
    public int? FkArtist { get; set; }

    public int? Minutes { get; set; }

    public int? Seconds { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ReleaseDate { get; set; }

    [StringLength(100)]
    public string? Image { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [ForeignKey("FkArtist")]
    [InverseProperty("Songs")]
    public virtual ArtistGroup? FkArtistNavigation { get; set; }

    [InverseProperty("FkSongNavigation")]
    public virtual ICollection<MonthlySongList> MonthlySongLists { get; set; } = new List<MonthlySongList>();

    [InverseProperty("FkSongNavigation")]
    public virtual ICollection<SongPossition> SongPossitions { get; set; } = new List<SongPossition>();
}
