using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PhongKham.DAL.Entities;

public partial class Encounter
{
    [Key]
    [Column("EncounterID")]
    public int EncounterId { get; set; }

    [Column("AppointmentID")]
    public int AppointmentId { get; set; }

    [Column("DoctorID")]
    public int DoctorId { get; set; }

    public string? Notes { get; set; }

    [ForeignKey("AppointmentId")]
    [InverseProperty("Encounters")]
    public virtual Appointment Appointment { get; set; } = null!;

    [InverseProperty("Encounter")]
    public virtual ICollection<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();

    [ForeignKey("DoctorId")]
    [InverseProperty("Encounters")]
    public virtual Doctor Doctor { get; set; } = null!;

    [InverseProperty("Encounter")]
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    [InverseProperty("Encounter")]
    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
