using Microsoft.EntityFrameworkCore;

namespace DAL;

public class FlightDbContext(DbContextOptions<FlightDbContext> options) : DbContext(options)
{
    public DbSet<Flight> Flights { get; set; }
    public DbSet<Airport> Airports { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Flight>()
            .HasOne(f => f.FromAirport)
            .WithMany()
            .HasForeignKey(f => f.FromAirportId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Flight>()
            .HasOne(f => f.ToAirport)
            .WithMany()
            .HasForeignKey(f => f.ToAirportId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}