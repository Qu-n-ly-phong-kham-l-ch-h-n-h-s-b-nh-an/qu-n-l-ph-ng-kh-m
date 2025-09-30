using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PhongKham.DAL.Entities;

public partial class Appointment
{
    [Key]
    [Column("AppointmentID")]
    public int AppointmentId { get; set; }

    [Column("PatientID")]
    public int PatientId { get; set; }

    [Column("DoctorID")]
    public int DoctorId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime AppointmentDate { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [ForeignKey("DoctorId")]
    [InverseProperty("Appointments")]
    public virtual Doctor Doctor { get; set; } = null!;

    [InverseProperty("Appointment")]
    public virtual ICollection<Encounter> Encounters { get; set; } = new List<Encounter>();

    [ForeignKey("PatientId")]
    [InverseProperty("Appointments")]
    public virtual Patient Patient { get; set; } = null!;
}
