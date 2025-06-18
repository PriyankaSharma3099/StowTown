using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StowTown.Models;

[Table("ProjectProducer")]
public partial class ProjectProducer
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? ProducerName { get; set; }

    [StringLength(100)]
    public string? ProducerImage { get; set; }

    [Column("DOB", TypeName = "datetime")]
    public DateTime? Dob { get; set; }

    public string? Address { get; set; }

    [StringLength(50)]
    public string? City { get; set; }

    [StringLength(50)]
    public string? State { get; set; }

    public int? ZipCode { get; set; }

    [StringLength(50)]
    public string? MobileNo { get; set; }

    [StringLength(50)]
    public string? Email { get; set; }

    public bool? IsDeleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("FkProjectProducerNavigation")]
    public virtual ICollection<ProjectProducerArtistGroup> ProjectProducerArtistGroups { get; set; } = new List<ProjectProducerArtistGroup>();
}
