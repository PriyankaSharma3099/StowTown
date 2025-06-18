using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StowTown.Models;

[Table("ProjectProducerArtistGroup")]
public partial class ProjectProducerArtistGroup
{
    [Key]
    public int Id { get; set; }

    [Column("Fk_Artist")]
    public int? FkArtist { get; set; }

    [Column("Fk_ProjectProducer")]
    public int? FkProjectProducer { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [ForeignKey("FkArtist")]
    [InverseProperty("ProjectProducerArtistGroups")]
    public virtual ArtistGroup? FkArtistNavigation { get; set; }

    [ForeignKey("FkProjectProducer")]
    [InverseProperty("ProjectProducerArtistGroups")]
    public virtual ProjectProducer? FkProjectProducerNavigation { get; set; }
}
