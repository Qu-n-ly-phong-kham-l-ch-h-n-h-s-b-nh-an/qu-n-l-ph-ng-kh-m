using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PhongKham.DAL.Entities;

[Table("Drug_Stocks")]
public partial class DrugStock
{
    [Key]
    [Column("StockID")]
    public int StockId { get; set; }

    [Column("DrugID")]
    public int DrugId { get; set; }

    public int? QuantityAvailable { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastUpdated { get; set; }

    [ForeignKey("DrugId")]
    [InverseProperty("DrugStocks")]
    public virtual Drug Drug { get; set; } = null!;
}
