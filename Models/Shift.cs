using System;
using System.Collections.Generic;

namespace EleonHotel.Models;

public partial class Shift
{
    public int ShiftId { get; set; }

    public TimeOnly ShiftTime { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
