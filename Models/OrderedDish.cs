using System;
using System.Collections.Generic;

namespace EleonHotel.Models;

public partial class OrderedDish
{
    public int GuestId { get; set; }

    public int DishId { get; set; }

    public int? OrderedDishesCount { get; set; }

    public virtual RestaurantMenu Dish { get; set; } = null!;

    public virtual Guest Guest { get; set; } = null!;
}
