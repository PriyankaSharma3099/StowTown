using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StowTown.ViewModels
{
    internal class UserViewModel
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [StringLength(50)]
        public string? UserName { get; set; }
        [StringLength(50)]
        public string? Name { get; set; }
        [Column("2StepVerifPassword")]
        [StringLength(50)]
        public string? _2stepVerifPassword { get; set; }

    }
}
