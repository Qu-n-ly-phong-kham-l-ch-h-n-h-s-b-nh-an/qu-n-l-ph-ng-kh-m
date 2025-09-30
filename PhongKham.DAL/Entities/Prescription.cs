using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PhongKham.DAL.Entities;

public partial class Prescription
{
    [Key]
    [Column("PrescriptionID")]
    public int PrescriptionId { get; set; }

    [Column("EncounterID")]
    public int EncounterId { get; set; }

    [Column("DrugID")]
    public int DrugId { get; set; }

    public int? Quantity { get; set; }

    [StringLength(200)]
    public string? Usage { get; set; }

    [ForeignKey("DrugId")]
    [InverseProperty("Prescriptions")]
    public virtual Drug Drug { get; set; } = null!;

    [ForeignKey("EncounterId")]
    [InverseProperty("Prescriptions")]
    public virtual Encounter Encounter { get; set; } = null!;
}
