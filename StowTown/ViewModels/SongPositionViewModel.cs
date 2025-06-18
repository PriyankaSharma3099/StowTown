using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StowTown.ViewModels
{
    public class SongPositionViewModel
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("FK_Song")]
        public int? FkSong { get; set; }
        
        [Column("Fk_RadioStation")]
        public int? FkRadioStation { get; set; }
        [Column("FK_MonthlySongList")]
        public int? FkMonthlySongList { get; set; }
        [Column("Rotation/Notes")]
        public string? RotationNotes { get; set; }


        [Column(TypeName = "datetime")]
        public string? Date { get; set; }

        public bool? IsDeleted { get; set; }
        // Additional property for Radio Station name
        public string ArtistName { get; set; }

        public string ArtistImage { get; set; }

        public string SongName { get; set; }

        public string SongImage { get; set; }

        public string? Duration { get; set; }

        public int? Spins { get; set; }
        public int? Possition { get; set; }


        public int SerialNumber { get; set; }
        public string? Image { get; internal set; }
    }
}
