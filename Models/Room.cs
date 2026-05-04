using System;
using System.Collections.Generic;

namespace EleonHotel.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public int RoomCategoryId { get; set; }

    public int RoomStatusId { get; set; }

    public int Number { get; set; }

    public virtual ICollection<Guest> Guests { get; set; } = new List<Guest>();

    public virtual RoomCategory RoomCategory { get; set; } = null!;

    public virtual RoomStatus RoomStatus { get; set; } = null!;

    public virtual ICollection<FacilitiesList> RoomFacilities { get; set; } = new List<FacilitiesList>();
}
