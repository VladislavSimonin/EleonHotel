using System;
using System.Collections.Generic;

namespace EleonHotel.Models;

public partial class Penalty
{
    public int PenaltyId { get; set; }

    public int UserId { get; set; }

    public decimal Amount { get; set; }

    public string Reason { get; set; } = null!;

    public DateOnly IssueDate { get; set; }

    public bool IsPaid { get; set; }

    public virtual User User { get; set; } = null!;
}
