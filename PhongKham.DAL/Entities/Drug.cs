using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PhongKham.DAL.Entities;

public partial class Drug
{
    [Key]
    [Column("DrugID")]
    public int DrugId { get; set; }

    [StringLength(100)]
    public string DrugName { get; set; } = null!;

    [StringLength(50)]
    public string? Unit { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Price { get; set; }

    [InverseProperty("Drug")]
    public virtual ICollection<DrugStock> DrugStocks { get; set; } = new List<DrugStock>();

    [InverseProperty("Drug")]
    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
