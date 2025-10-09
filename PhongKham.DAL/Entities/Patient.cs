using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PhongKham.DAL.Entities;

public partial class Patient
{
    [Key]
    [Column("PatientID")]
    public int PatientId { get; set; }

    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [Column("DOB")]
    public DateTime? Dob { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string? Phone { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    public string? MedicalHistory { get; set; }

    // 🔗 Liên kết tới tài khoản (nếu có)
    public int? AccountId { get; set; }

    [ForeignKey("AccountId")]
    public virtual Account? Account { get; set; }

    [InverseProperty("Patient")]
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    [InverseProperty("Patient")]
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}