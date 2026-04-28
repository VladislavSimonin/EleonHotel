using System;
using System.Collections.Generic;

namespace EleonHotel.Models;

public partial class RestaurantMenu
{
    public int DishId { get; set; }

    public string DishName { get; set; } = null!;

    public string DishComposition { get; set; } = null!;

    public decimal Cost { get; set; }

    public virtual ICollection<Guest> Guests { get; set; } = new List<Guest>();
}
