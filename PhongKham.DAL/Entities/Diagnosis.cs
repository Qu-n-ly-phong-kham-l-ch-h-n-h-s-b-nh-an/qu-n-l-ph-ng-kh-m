using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PhongKham.DAL.Entities;

public partial class Diagnosis
{
    [Key]
    [Column("DiagnosisID")]
    public int DiagnosisId { get; set; }

    [Column("EncounterID")]
    public int EncounterId { get; set; }

    public string? Description { get; set; }

    public byte[]? ResultFile { get; set; }

    [ForeignKey("EncounterId")]
    [InverseProperty("Diagnoses")]
    public virtual Encounter Encounter { get; set; } = null!;
}
