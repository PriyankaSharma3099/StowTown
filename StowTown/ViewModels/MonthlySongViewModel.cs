using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StowTown.ViewModels
{
    public class MonthlySongViewModel
    {

        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("FK_Song")]
        public int? FkSong { get; set; }

        [Column("FK_Artist")]
        public int? FkArtist { get; set; }

        [Column(TypeName = "datetime")]
        public string? Date { get; set; }

        public bool? IsDeleted { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }
        // Additional property for Radio Station name
        public string ArtistName { get; set; }

        public string SongName { get; set; }

        public string? Duration { get; set; }
        public string Image { get; set; }
        public int SerialNumber { get; set; }
    }
}
