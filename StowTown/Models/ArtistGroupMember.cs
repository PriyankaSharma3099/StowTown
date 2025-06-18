using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StowTown.Models;

[Table("ArtistGroupMember")]
public partial class ArtistGroupMember
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? MemberName { get; set; }

    [Column("FK_ArtistGroup")]
    public int? FkArtistGroup { get; set; }

    [StringLength(100)]
    public string? MemberPicture { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Position { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Birthday { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Address { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? City { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? State { get; set; }

    public int? Zip { get; set; }

    [StringLength(50)]
    public string? OfficeNumber { get; set; }

    [StringLength(50)]
    public string? MobileNumber { get; set; }

    [StringLength(50)]
    public string? Email { get; set; }

    public string? Facebook { get; set; }

    public string? Instagram { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? AccoplishmentsHistory { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Notes { get; set; }

    public bool? IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("FkArtistGroup")]
    [InverseProperty("ArtistGroupMembers")]
    public virtual ArtistGroup? FkArtistGroupNavigation { get; set; }

}
