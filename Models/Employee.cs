using System;
using System.Collections.Generic;

namespace EleonHotel.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public int UserId { get; set; }

    public int? ShiftId { get; set; }

    public decimal? Salary { get; set; }

    public int? PositionId { get; set; }

    public virtual ICollection<Penalty> Penalties { get; set; } = new List<Penalty>();

    public virtual Position? Position { get; set; }

    public virtual Shift? Shift { get; set; }

    public virtual User User { get; set; } = null!;
}
