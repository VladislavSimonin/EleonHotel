using System;
using System.Collections.Generic;

namespace EleonHotel.Models;

public partial class FacilitiesList
{
    public int RoomFacilityId { get; set; }

    public string RoomFacilityName { get; set; } = null!;

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
