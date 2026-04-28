using System;
using System.Collections.Generic;

namespace EleonHotel.Models;

public partial class PaymentInvoice
{
    public int PaymentId { get; set; }

    public int GuestId { get; set; }

    public decimal Total { get; set; }

    public bool IsPaid { get; set; }

    public virtual Guest Guest { get; set; } = null!;
}
