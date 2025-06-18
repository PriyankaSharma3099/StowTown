using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StowTown.Models;

public partial class User
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? FirstName { get; set; }

    [StringLength(50)]
    public string? LastName { get; set; }

    [StringLength(50)]
    public string? UserName { get; set; }

    [StringLength(100)]
    public string? Password { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [StringLength(100)]
    public string? UserImage { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NotificationDate { get; set; }

    [Column("2StepVerifPassword")]
    [StringLength(50)]
    public string? _2stepVerifPassword { get; set; }
}
