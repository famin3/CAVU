using System.ComponentModel;
using CAVU.CarParkService;
using CAVU.CarParkService.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CarParkContext>(opt => opt.UseInMemoryDatabase("CAVU"));

var app = builder.Build();

MockedData.AddMockedData(app);

app.MapGet("/booking", async (CarParkContext context) =>
    await context.Bookings.ToListAsync());

app.MapGet("/booking/checkdates", async ([DefaultValue("2023-06-01")]DateTime from, [DefaultValue("2023-06-30")]DateTime to, CarParkContext context) =>
{
    var parkingSlots = await context.ParkingSpots.Select(x => x.Id).ToListAsync();
    var takenSlots = await context.Bookings.Where(x => x.StartDate.Date <= to && x.EndDate.Date >= from && x.Active)
        .Select(y => y.ParkingSpotId).ToListAsync();
    return "Available slots: " + string.Join(',',parkingSlots.Except(takenSlots));
});

app.MapGet("/booking/checkdatesdetailed", async ([DefaultValue("2023-06-01")]DateTime from, [DefaultValue("2023-06-30")]DateTime to, CarParkContext context) =>
{
    var parkingSlotsCount = (await context.ParkingSpots.Select(x => x.Id).ToListAsync()).Count;
    var bookings = await context.Bookings.Where(x => x.StartDate.Date <= to && x.EndDate.Date >= from && x.Active).ToListAsync();
    
    var result = "";
    for (var day = from; day <= to; day = day.AddDays(1))
    {
        var takenSlotsPerDay = bookings.Count(x => x.StartDate.Date <= day && x.EndDate.Date >= day);
        var freeSlotPerDay = parkingSlotsCount - takenSlotsPerDay;

        result += $"Available slots for {day.ToShortDateString()}: " + freeSlotPerDay + Environment.NewLine;
    }


    return result;
});

app.MapGet("/booking/checkprices", async ([DefaultValue("2023-06-01")]DateTime from, [DefaultValue("2023-06-30")]DateTime to, CarParkContext context) =>
{
    var prices = await context.Prices.ToListAsync();
    var result = PriceCalculator.Calculate(prices, DateOnly.FromDateTime(from), DateOnly.FromDateTime(to));
    return result;
});

app.MapPost("/booking/", async (Booking booking, CarParkContext context) =>
{
    //check if you can book first
    var parkingSlots = await context.ParkingSpots.Select(x => x.Id).ToListAsync();
    var takenSlots = await context.Bookings.Where(x => x.StartDate.Date <= booking.EndDate.Date && x.EndDate.Date >= booking.StartDate.Date && x.Active)
        .Select(y => y.ParkingSpotId).ToListAsync();
    var availableSlotsCount = parkingSlots.Except(takenSlots).Count();

    if (availableSlotsCount == 0) return Results.Problem("Can't book for these dates");
    
    context.Bookings.Add(booking);
    await context.SaveChangesAsync();
    return Results.Ok();
});

app.MapPut("/booking/cancel", async (int id, CarParkContext context) =>
{
    if (await context.Bookings.FindAsync(id) is Booking booking)
    {
        booking.Cancel();
        await context.SaveChangesAsync();
        return Results.Ok();
    }

    return Results.NotFound();

});

app.MapPut("/booking/", async (Booking dto, CarParkContext context) =>
{
    var booking = await context.Bookings.FindAsync(dto.Id);
    if (booking == null) return Results.NotFound();
    booking.Update(dto.StartDate.Date, dto.EndDate.Date);
    await context.SaveChangesAsync();
    return Results.Ok();

});


app.MapGet("/parkingspot", async (CarParkContext context) =>
    await context.ParkingSpots.ToListAsync());

app.MapPost("/parkingspot/", async (ParkingSpot parkingSpot, CarParkContext context) =>
{
    context.ParkingSpots.Add(parkingSpot);
    await context.SaveChangesAsync();
});

app.MapDelete("/parkingspot/{id}", async (int id, CarParkContext context) =>
{
    if (await context.ParkingSpots.FindAsync(id) is ParkingSpot spot)
    {
        context.ParkingSpots.Remove(spot);
        await context.SaveChangesAsync();
        return Results.Ok(spot);
    }

    return Results.NotFound();
});


app.MapGet("/price", async (CarParkContext context) =>
    await context.Prices.ToListAsync());




   



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.SerializeAsV2 = true;
    });
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();