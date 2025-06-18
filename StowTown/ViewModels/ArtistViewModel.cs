using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StowTown.Models;

namespace StowTown.ViewModels
{
    public class ArtistViewModel
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

        public int SerialNumber { get; set; }

        public int? NoOfMembers { get; set; }
        public int? NoOfSongs { get; set; }


       

    }
}
