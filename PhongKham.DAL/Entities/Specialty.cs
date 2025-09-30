using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PhongKham.DAL.Entities;

[Index("SpecialtyName", Name = "UQ__Specialt__7DCA57487E174C8B", IsUnique = true)]
public partial class Specialty
{
    [Key]
    [Column("SpecialtyID")]
    public int SpecialtyId { get; set; }

    [StringLength(100)]
    public string? SpecialtyName { get; set; }

    [InverseProperty("Specialty")]
    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
