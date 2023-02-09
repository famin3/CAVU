using CAVU.CarParkService.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CAVU.CarParkService;

public static class MockedData
{
    public static void AddMockedData(WebApplication app)
    {
        var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetService<CarParkContext>();

        for (var i = 1; i <= 10; i++)
        {
            db?.ParkingSpots.Add(new ParkingSpot()
            {
                Id = i,
                ParkingSpotName = "Spot " + i
            });
        }

        var random = new Random();
        for (var i = 1; i <= 20; i++)
        {
            var startDate = random.NextDate(new DateTime(2023, 6, 1), new DateTime(2023, 6, 30));
            var endDate = startDate.AddDays(random.NextInt64(0, 7));
            db?.Bookings.Add(new Booking()
            {
                Id = i,
                Charged = random.NextDouble() > 0.5,
                Price = random.NextDecimal(1, 100),
                StartDate = startDate,
                EndDate = endDate,
                ParkingSpotId = (int)random.NextInt64(1,10)
            });
        }

        db?.SaveChangesAsync();


    }
}