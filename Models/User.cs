using System;
using System.Collections.Generic;

namespace EleonHotel.Models;

public partial class User
{
    public int UserId { get; set; }

    public byte[] PasswordHash { get; set; } = null!;

    public byte[] HashSalt { get; set; } = null!;

    public string Login { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Patronymic { get; set; }

    public string PassportSeries { get; set; } = null!;

    public string PassportNumber { get; set; } = null!;

    public string WhoGavePassport { get; set; } = null!;

    public DateOnly WhenGavePassport { get; set; }

    public string RegistrationAddress { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<Guest> Guests { get; set; } = new List<Guest>();
}
