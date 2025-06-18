using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StowTown.Models;

[Table("MonthlySongList")]
public partial class MonthlySongList
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("FK_Song")]
    public int? FkSong { get; set; }

    [Column("FK_Artist")]
    public int? FkArtist { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Date { get; set; }

    public bool? IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("FkArtist")]
    [InverseProperty("MonthlySongLists")]
    public virtual ArtistGroup? FkArtistNavigation { get; set; }

    [ForeignKey("FkSong")]
    [InverseProperty("MonthlySongLists")]
    public virtual Song? FkSongNavigation { get; set; }

    [InverseProperty("FkMonthlySongListNavigation")]
    public virtual ICollection<SongPossition> SongPossitions { get; set; } = new List<SongPossition>();
}
