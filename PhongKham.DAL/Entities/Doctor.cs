using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PhongKham.DAL.Entities;

public partial class Doctor
{
    [Key]
    [Column("DoctorID")]
    public int DoctorId { get; set; }

    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [Column("SpecialtyID")]
    public int? SpecialtyId { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string? Phone { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    [InverseProperty("Doctor")]
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    [InverseProperty("Doctor")]
    public virtual ICollection<Encounter> Encounters { get; set; } = new List<Encounter>();

    [ForeignKey("SpecialtyId")]
    [InverseProperty("Doctors")]
    public virtual Specialty? Specialty { get; set; }
}
