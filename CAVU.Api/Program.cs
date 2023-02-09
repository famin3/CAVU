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
    var takenSlots = await context.Bookings.Where(x => x.StartDate < to && x.EndDate > from)
        .Select(y => y.ParkingSpotId).ToListAsync();
    return "Available slots: " + string.Join(',',parkingSlots.Except(takenSlots));
});

app.MapGet("/booking/checkprices", async ([DefaultValue("2023-06-01")]DateTime from, [DefaultValue("2023-06-30")]DateTime to, CarParkContext context) =>
{
    var prices = await context.Prices.ToListAsync();
    var result = PriceCalculator.Calculate(prices, DateOnly.FromDateTime(from), DateOnly.FromDateTime(to));
    return result;
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