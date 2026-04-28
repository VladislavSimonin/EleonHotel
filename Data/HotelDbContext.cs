using Microsoft.EntityFrameworkCore;

namespace EleonHotel.Data
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options)
        {
        }



        // Добавьте ваши DbSet здесь
        // Пример:
        // public DbSet<Hotel> Hotels { get; set; } 
        // public DbSet<Room> Rooms { get; set; }
        // public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Конфигурация моделей здесь
        }
    }
}
