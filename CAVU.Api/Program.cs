using CAVU.CarParkService;
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

app.MapGet("/parkingspot", async (CarParkContext context) =>
    await context.ParkingSpots.ToListAsync());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();