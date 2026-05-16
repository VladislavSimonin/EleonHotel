using System;
using System.Collections.Generic;

namespace EleonHotel.Models;

public partial class OrderedService
{
    public int GuestId { get; set; }

    public int ServiceId { get; set; }

    public int? OrderedServicesCount { get; set; }

    public bool? IsDone { get; set; }

    public virtual Guest Guest { get; set; } = null!;

    public virtual AdditionalService Service { get; set; } = null!;
}
