using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PhongKham.DAL.Entities;

[Index("Username", Name = "UQ__Accounts__536C85E439975CEA", IsUnique = true)]
public partial class Account
{
    [Key]
    [Column("AccountID")]
    public int AccountId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Username { get; set; } = null!;

    [StringLength(256)]
    [Unicode(false)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(50)]
    public string? Role { get; set; }

    [InverseProperty("Account")]
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
