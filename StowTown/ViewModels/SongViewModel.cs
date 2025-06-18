using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StowTown.ViewModels
{
    internal class SongViewModel
    {

        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [StringLength(50)]
        [Unicode(false)]
        public string? Name { get; set; }

        [Column("FK_Artist")]
        public int? FkArtist { get; set; }

        [StringLength(10)]
        [Unicode(false)]
        public string? Duration { get; set; }

        [Column(TypeName = "datetime")]
        public string? ReleaseDate { get; set; }

        [StringLength(100)]
        public string? Image { get; set; }

        public bool? IsDeleted { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }
        // Additional property for Artist name
        public string ArtistName { get; set; }

        public int SerialNumber { get; set; }

    }
}
