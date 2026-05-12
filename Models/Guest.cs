using System;
using System.Collections.Generic;

namespace EleonHotel.Models;

public partial class Guest
{
    public int GuestId { get; set; }

    public int UserId { get; set; }

    public int? RoomId { get; set; }

    public virtual ICollection<OrderedDish> OrderedDishes { get; set; } = new List<OrderedDish>();

    public virtual ICollection<OrderedService> OrderedServices { get; set; } = new List<OrderedService>();

    public virtual ICollection<PaymentInvoice> PaymentInvoices { get; set; } = new List<PaymentInvoice>();

    public virtual Room? Room { get; set; }

    public virtual User User { get; set; } = null!;
}
