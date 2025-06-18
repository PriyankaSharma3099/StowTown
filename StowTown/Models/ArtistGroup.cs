using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StowTown.Models;

[Table("ArtistGroup")]
public partial class ArtistGroup
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Name { get; set; }

    public bool? IsActive { get; set; }

    public string? GroupPicture { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? GroupTitle { get; set; }

    [StringLength(100)]
    public string? GroupWebsite { get; set; }

    [StringLength(100)]
    public string? GroupAccomplishment { get; set; }

    public bool? IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("FkArtistGroupNavigation")]
    public virtual ICollection<ArtistGroupMember> ArtistGroupMembers { get; set; } = new List<ArtistGroupMember>();

    [InverseProperty("FkArtistNavigation")]
    public virtual ICollection<MonthlySongList> MonthlySongLists { get; set; } = new List<MonthlySongList>();

    [InverseProperty("FkArtistNavigation")]
    public virtual ICollection<ProjectProducerArtistGroup> ProjectProducerArtistGroups { get; set; } = new List<ProjectProducerArtistGroup>();

    [InverseProperty("FkArtistNavigation")]
    public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
}
