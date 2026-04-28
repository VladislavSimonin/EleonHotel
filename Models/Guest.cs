using System;
using System.Collections.Generic;

namespace EleonHotel.Models;

public partial class Guest
{
    public int GuestId { get; set; }

    public int UserId { get; set; }

    public int RoomId { get; set; }

    public virtual ICollection<PaymentInvoice> PaymentInvoices { get; set; } = new List<PaymentInvoice>();

    public virtual Room Room { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual ICollection<RestaurantMenu> Dishes { get; set; } = new List<RestaurantMenu>();

    public virtual ICollection<AdditionalService> Services { get; set; } = new List<AdditionalService>();
}
