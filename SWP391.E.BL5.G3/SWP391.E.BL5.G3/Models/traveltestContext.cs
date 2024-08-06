﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace SWP391.E.BL5.G3.Models
{
    public partial class traveltestContext : DbContext
    {
        public traveltestContext()
        {
        }

        public traveltestContext(DbContextOptions<traveltestContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Booking> Bookings { get; set; } = null!;
        public virtual DbSet<Feedback> Feedbacks { get; set; } = null!;
        public virtual DbSet<Hotel> Hotels { get; set; } = null!;
        public virtual DbSet<Restaurant> Restaurants { get; set; } = null!;
        public virtual DbSet<Tour> Tours { get; set; } = null!;
        public virtual DbSet<TourGuide> TourGuides { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Vehicle> Vehicles { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Console.WriteLine(Directory.GetCurrentDirectory());
            IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .Build();
            var strConn = config["ConnectionStrings:MyDatabase"];
            optionsBuilder.UseSqlServer(strConn);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.Message).HasMaxLength(200);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Phone)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.HasOne(d => d.Hotel)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.HotelId)
                    .HasConstraintName("FK__Bookings__HotelI__4AB81AF0");

                entity.HasOne(d => d.Restaurant)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.RestaurantId)
                    .HasConstraintName("FK__Bookings__Restau__4BAC3F29");

                entity.HasOne(d => d.Tour)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.TourId)
                    .HasConstraintName("FK__Bookings__TourId__49C3F6B7");

                entity.HasOne(d => d.Vehicle)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.VehicleId)
                    .HasConstraintName("FK__Bookings__Vehicl__4CA06362");
            });

            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.Property(e => e.Content).HasColumnType("text");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_Feedbacks_Feedbacks");
            });

            modelBuilder.Entity<Hotel>(entity =>
            {
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.HotelName).HasMaxLength(100);

                entity.Property(e => e.Location).HasMaxLength(200);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Restaurant>(entity =>
            {
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Location).HasMaxLength(200);

                entity.Property(e => e.RestaurantName).HasMaxLength(100);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Tour>(entity =>
            {
                entity.Property(e => e.AirPlane).HasMaxLength(100);

                entity.Property(e => e.CreateDate).HasColumnType("date");

                entity.Property(e => e.Description)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Duration).HasMaxLength(50);

                entity.Property(e => e.Image).IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Rating).HasColumnType("decimal(3, 1)");

                entity.HasOne(d => d.Hotel)
                    .WithMany(p => p.Tours)
                    .HasForeignKey(d => d.HotelId)
                    .HasConstraintName("FK__Tours__HotelId__08B54D69");

                entity.HasOne(d => d.Restaurant)
                    .WithMany(p => p.Tours)
                    .HasForeignKey(d => d.RestaurantId)
                    .HasConstraintName("FK__Tours__Restauran__09A971A2");

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.Tours)
                    .HasForeignKey(d => d.StaffId)
                    .HasConstraintName("FK__Tours__StaffId__45F365D3");

                entity.HasOne(d => d.Vehicle)
                    .WithMany(p => p.Tours)
                    .HasForeignKey(d => d.VehicleId)
                    .HasConstraintName("FK__Tours__VehicleId__0A9D95DB");
            });

            modelBuilder.Entity<TourGuide>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(50);

                entity.Property(e => e.Email).HasMaxLength(50);

                entity.Property(e => e.FirstName).HasMaxLength(50);

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.FirstName).HasMaxLength(50);

                entity.Property(e => e.Image).IsUnicode(false);

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.Password)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.RoleId).HasColumnName("RoleID");
            });

            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.VehicleName).HasMaxLength(100);

                entity.Property(e => e.VehicleType).HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
