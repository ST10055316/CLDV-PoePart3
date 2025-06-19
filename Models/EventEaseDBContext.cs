using Microsoft.EntityFrameworkCore;
using EventEase.Models;

namespace EventEase.Data
{
    public class EventEaseDBContext : DbContext
    {
        public EventEaseDBContext(DbContextOptions<EventEaseDBContext> options) : base(options)
        {
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<EventType> EventType { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.EventId, b.VenueId, b.BookingDate })
                .IsUnique();

            /// Inside OnModelCreating method in EventEaseDBContext.cs
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Venue)
                .WithMany(v => v.Events) // <--- CHANGE THIS LINE!
                .HasForeignKey(e => e.VenueId)
                // .HasPrincipalKey(v => v.Id) // <--- REMOVE THIS LINE if you added it
                .OnDelete(DeleteBehavior.Restrict);

            // If you added EventType, configure its relationship as well
            modelBuilder.Entity<Event>()
                .HasOne(e => e.EventType)
                .WithMany() // Or .WithMany(et => et.Events)
                .HasForeignKey(e => e.EventTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Optional: Seed data
            modelBuilder.Entity<EventType>().HasData(
                new EventType { EventTypeId = 1, EventTypeName = "Concert", Description = "Live music performances" },
                new EventType { EventTypeId = 2, EventTypeName = "Festival", Description = "Large-scale events with multiple acts" },
                new EventType { EventTypeId = 3, EventTypeName = "Conference", Description = "Meetings for discussions and information exchange" },
                new EventType { EventTypeId = 4, EventTypeName = "Workshop", Description = "Interactive educational sessions" },
                new EventType { EventTypeId = 5, EventTypeName = "Exhibition", Description = "Display of art, products, or information" },
                new EventType { EventTypeId = 6, EventTypeName = "Sporting Event", Description = "Competitions and games" },
                new EventType { EventTypeId = 7, EventTypeName = "Wedding", Description = "Marriage ceremonies and receptions" },
                new EventType { EventTypeId = 8, EventTypeName = "Birthday Party", Description = "Celebrations of birthdays" }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}