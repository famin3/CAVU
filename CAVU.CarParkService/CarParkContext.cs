using CAVU.CarParkService.Models;
using Microsoft.EntityFrameworkCore;

namespace CAVU.CarParkService;

public class CarParkContext : DbContext
{
    public CarParkContext(DbContextOptions<CarParkContext> options) : base(options)
    {
        
    }

    public DbSet<ParkingSpot> ParkingSpots => Set<ParkingSpot>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Price> Prices => Set<Price>();
    
    
}