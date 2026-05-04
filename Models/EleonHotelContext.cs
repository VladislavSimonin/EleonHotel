using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EleonHotel.Models;

public partial class EleonHotelContext : DbContext
{
    public EleonHotelContext()
    {
    }

    public EleonHotelContext(DbContextOptions<EleonHotelContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdditionalService> AdditionalServices { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<FacilitiesList> FacilitiesLists { get; set; }

    public virtual DbSet<Guest> Guests { get; set; }

    public virtual DbSet<PaymentInvoice> PaymentInvoices { get; set; }

    public virtual DbSet<Penalty> Penalties { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<RestaurantMenu> RestaurantMenus { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomCategory> RoomCategories { get; set; }

    public virtual DbSet<RoomStatus> RoomStatuses { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-SGSC2AR\\SQLEXPRESS;Database=EleonHotel;User Id=Vladislav;Password=lolihanter1000-7;TrustServerCertificate=true;", x => x.UseNetTopologySuite());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdditionalService>(entity =>
        {
            entity.HasKey(e => e.ServiceId);

            entity.ToTable("Additional_services");

            entity.Property(e => e.ServiceId)
                .ValueGeneratedNever()
                .HasColumnName("service_id");
            entity.Property(e => e.Cost)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("cost");
            entity.Property(e => e.ServiceName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("service_name");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(e => e.EmployeeId)
                .ValueGeneratedNever()
                .HasColumnName("employee_id");
            entity.Property(e => e.PositionId).HasColumnName("position_id");
            entity.Property(e => e.Salary)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("salary");
            entity.Property(e => e.ShiftId).HasColumnName("shift_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Position).WithMany(p => p.Employees)
                .HasForeignKey(d => d.PositionId)
                .HasConstraintName("fk26");

            entity.HasOne(d => d.Shift).WithMany(p => p.Employees)
                .HasForeignKey(d => d.ShiftId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk7");

            entity.HasOne(d => d.User).WithMany(p => p.Employees)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk6");
        });

        modelBuilder.Entity<FacilitiesList>(entity =>
        {
            entity.HasKey(e => e.RoomFacilityId);

            entity.ToTable("Facilities_list");

            entity.Property(e => e.RoomFacilityId)
                .ValueGeneratedNever()
                .HasColumnName("room_facility_id");
            entity.Property(e => e.RoomFacilityName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("room_facility_name");
        });

        modelBuilder.Entity<Guest>(entity =>
        {
            entity.HasKey(e => e.GuestId).HasName("PK_Guests1");

            entity.Property(e => e.GuestId)
                .ValueGeneratedNever()
                .HasColumnName("guest_id");
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Room).WithMany(p => p.Guests)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk24");

            entity.HasOne(d => d.User).WithMany(p => p.Guests)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk25");

            entity.HasMany(d => d.Dishes).WithMany(p => p.Guests)
                .UsingEntity<Dictionary<string, object>>(
                    "OrderedDish",
                    r => r.HasOne<RestaurantMenu>().WithMany()
                        .HasForeignKey("DishId")
                        .HasConstraintName("fk17"),
                    l => l.HasOne<Guest>().WithMany()
                        .HasForeignKey("GuestId")
                        .HasConstraintName("fk18"),
                    j =>
                    {
                        j.HasKey("GuestId", "DishId").HasName("pk7");
                        j.ToTable("Ordered_dishes");
                        j.IndexerProperty<int>("GuestId").HasColumnName("guest_id");
                        j.IndexerProperty<int>("DishId").HasColumnName("dish_id");
                    });

            entity.HasMany(d => d.Services).WithMany(p => p.Guests)
                .UsingEntity<Dictionary<string, object>>(
                    "OrderedService",
                    r => r.HasOne<AdditionalService>().WithMany()
                        .HasForeignKey("ServiceId")
                        .HasConstraintName("fk15"),
                    l => l.HasOne<Guest>().WithMany()
                        .HasForeignKey("GuestId")
                        .HasConstraintName("fk16"),
                    j =>
                    {
                        j.HasKey("GuestId", "ServiceId").HasName("pk6");
                        j.ToTable("Ordered_services");
                        j.IndexerProperty<int>("GuestId").HasColumnName("guest_id");
                        j.IndexerProperty<int>("ServiceId").HasColumnName("service_id");
                    });
        });

        modelBuilder.Entity<PaymentInvoice>(entity =>
        {
            entity.HasKey(e => e.PaymentId);

            entity.ToTable("Payment_invoices");

            entity.Property(e => e.PaymentId)
                .ValueGeneratedNever()
                .HasColumnName("payment_id");
            entity.Property(e => e.GuestId).HasColumnName("guest_id");
            entity.Property(e => e.IsPaid).HasColumnName("is_paid");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.Guest).WithMany(p => p.PaymentInvoices)
                .HasForeignKey(d => d.GuestId)
                .HasConstraintName("fk19");
        });

        modelBuilder.Entity<Penalty>(entity =>
        {
            entity.Property(e => e.PenaltyId)
                .ValueGeneratedNever()
                .HasColumnName("penalty_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.IsPaid).HasColumnName("is_paid");
            entity.Property(e => e.IssueDate).HasColumnName("issue_date");
            entity.Property(e => e.Reason)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasColumnName("reason");

            entity.HasOne(d => d.Employee).WithMany(p => p.Penalties)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("fk8");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.Property(e => e.PositionId)
                .ValueGeneratedNever()
                .HasColumnName("position_id");
            entity.Property(e => e.PositionName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("position_name");
        });

        modelBuilder.Entity<RestaurantMenu>(entity =>
        {
            entity.HasKey(e => e.DishId);

            entity.ToTable("Restaurant_menu");

            entity.Property(e => e.DishId)
                .ValueGeneratedNever()
                .HasColumnName("dish_id");
            entity.Property(e => e.Cost)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("cost");
            entity.Property(e => e.DishComposition)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("dish_composition");
            entity.Property(e => e.DishName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("dish_name");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.Property(e => e.RoomId)
                .ValueGeneratedNever()
                .HasColumnName("room_id");
            entity.Property(e => e.RoomCategoryId).HasColumnName("room_category_id");
            entity.Property(e => e.RoomStatusId).HasColumnName("room_status_id");

            entity.HasOne(d => d.RoomCategory).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.RoomCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk20");

            entity.HasOne(d => d.RoomStatus).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.RoomStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk21");

            entity.HasMany(d => d.RoomFacilities).WithMany(p => p.Rooms)
                .UsingEntity<Dictionary<string, object>>(
                    "RoomFacility",
                    r => r.HasOne<FacilitiesList>().WithMany()
                        .HasForeignKey("RoomFacilityId")
                        .HasConstraintName("FK__Room_faci__room___2FCF1A8A"),
                    l => l.HasOne<Room>().WithMany()
                        .HasForeignKey("RoomId")
                        .HasConstraintName("FK__Room_faci__room___2EDAF651"),
                    j =>
                    {
                        j.HasKey("RoomId", "RoomFacilityId").HasName("PK__Room_fac__38DCB6D7655197DE");
                        j.ToTable("Room_facilities");
                        j.IndexerProperty<int>("RoomId").HasColumnName("room_id");
                        j.IndexerProperty<int>("RoomFacilityId").HasColumnName("room_facility_id");
                    });
        });

        modelBuilder.Entity<RoomCategory>(entity =>
        {
            entity.ToTable("Room_categories");

            entity.Property(e => e.RoomCategoryId)
                .ValueGeneratedNever()
                .HasColumnName("room_category_id");
            entity.Property(e => e.Cost)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("cost");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.MaxOccupancy).HasColumnName("max_occupancy");
            entity.Property(e => e.RoomCategoryName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("room_category_name");
        });

        modelBuilder.Entity<RoomStatus>(entity =>
        {
            entity.ToTable("Room_statuses");

            entity.Property(e => e.RoomStatusId)
                .ValueGeneratedNever()
                .HasColumnName("room_status_id");
            entity.Property(e => e.RoomStatus1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("room_status");
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.Property(e => e.ShiftId)
                .ValueGeneratedNever()
                .HasColumnName("shift_id");
            entity.Property(e => e.ShiftTime).HasColumnName("shift_time");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.Birthsday).HasColumnName("birthsday");
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.HashSalt).HasColumnName("hash_salt");
            entity.Property(e => e.Login)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("login");
            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.PassportNumber)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("passport_number");
            entity.Property(e => e.PassportSeries)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasColumnName("passport_series");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.Patronymic)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("patronymic");
            entity.Property(e => e.Phone)
                .HasMaxLength(19)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.RegistrationAddress)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("registration_address");
            entity.Property(e => e.Surname)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("surname");
            entity.Property(e => e.WhenGavePassport).HasColumnName("when_gave_passport");
            entity.Property(e => e.WhoGavePassport)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("who_gave_passport");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
