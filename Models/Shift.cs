using System;
using System.Collections.Generic;

namespace EleonHotel.Models;

public partial class Shift
{
    public int ShiftId { get; set; }

    public TimeOnly ShiftTimeStart { get; set; }

    public TimeOnly? ShiftTimeEnd { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
