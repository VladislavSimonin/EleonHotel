using System;
using System.Collections.Generic;

namespace EleonHotel.Models;

public partial class RoomStatus
{
    public int RoomStatusId { get; set; }

    public string RoomStatus1 { get; set; } = null!;

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
