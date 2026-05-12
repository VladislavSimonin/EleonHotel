using System;
using System.Collections.Generic;

namespace EleonHotel.Models;

public partial class AdditionalService
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = null!;

    public decimal Cost { get; set; }

    public virtual ICollection<OrderedService> OrderedServices { get; set; } = new List<OrderedService>();
}
