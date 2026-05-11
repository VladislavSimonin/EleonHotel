using System;
using System.Collections.Generic;

namespace EleonHotel.Models;

public partial class Guest
{
    public int GuestId { get; set; }

    public int UserId { get; set; }

    public int? RoomId { get; set; }

    public virtual ICollection<OrderedDish> OrderedDishes { get; set; } = new List<OrderedDish>();

    public virtual ICollection<PaymentInvoice> PaymentInvoices { get; set; } = new List<PaymentInvoice>();

    public virtual Room? Room { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<AdditionalService> Services { get; set; } = new List<AdditionalService>();
}
