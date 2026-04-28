using System;
using System.Collections.Generic;

namespace EleonHotel.Models;

public partial class RoomCategory
{
    public int RoomCategoryId { get; set; }

    public string RoomCategoryName { get; set; } = null!;

    public decimal Cost { get; set; }

    public string? Description { get; set; }

    public int MaxOccupancy { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
