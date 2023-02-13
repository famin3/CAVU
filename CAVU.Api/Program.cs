using System.ComponentModel;
using CAVU.Api;
using CAVU.CarParkService;
using CAVU.CarParkService.DTOs;
using CAVU.CarParkService.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CAVU",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 123asd\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<CarParkContext>(opt => opt.UseInMemoryDatabase("CAVU"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Auth0:Domain"];
    options.Audience = builder.Configuration["Auth0:Audience"];
});

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("read:all", p => p.
        RequireAuthenticatedUser().
        RequireClaim("permissions", "read:all"));
});


if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IAuthorizationHandler, AllowAnonymous>();
}




var app = builder.Build();


//Mocked data
MockedData.AddMockedData(app);


app.MapGet("/booking", async (CarParkContext context) =>
    Results.Json(await context.Bookings.ToListAsync())).RequireAuthorization("read:all");

app.MapGet("/booking/availableslotsfordaterange", async ([DefaultValue("2023-06-01")]DateTime from, [DefaultValue("2023-06-30")]DateTime to, CarParkContext context) =>
{
    var parkingSlots = await context.ParkingSpots.Select(x => x.Id).ToListAsync();
    var takenSlots = await context.Bookings.Where(x => x.StartDate.Date <= to && x.EndDate.Date >= from && x.Active)
        .Select(y => y.ParkingSpotId).ToListAsync();
    return Results.Json(parkingSlots.Except(takenSlots));
}).RequireAuthorization("read:all");

app.MapGet("/booking/availableslotscountperday", async ([DefaultValue("2023-06-01")]DateTime from, [DefaultValue("2023-06-30")]DateTime to, CarParkContext context) =>
{
    var parkingSlotsCount = (await context.ParkingSpots.Select(x => x.Id).ToListAsync()).Count;
    var bookings = await context.Bookings.Where(x => x.StartDate.Date <= to && x.EndDate.Date >= from && x.Active).ToListAsync();
    
    var result = new List<AvailableSlotCountPerDayDto>();
    for (var day = from; day <= to; day = day.AddDays(1))
    {
        var takenSlotsPerDay = bookings.Count(x => x.StartDate.Date <= day && x.EndDate.Date >= day);
        var freeSlotPerDay = parkingSlotsCount - takenSlotsPerDay;

        result.Add(new AvailableSlotCountPerDayDto
        {
            Date = day.ToShortDateString(),
            NumberOfSlots = freeSlotPerDay
        });
    }

    return Results.Json(result);
}).RequireAuthorization("read:all");

app.MapGet("/booking/price", async ([DefaultValue("2023-06-01")]DateTime from, [DefaultValue("2023-06-30")]DateTime to, CarParkContext context) =>
{
    var prices = await context.Prices.ToListAsync();
    var result = PriceCalculator.Calculate(prices, DateOnly.FromDateTime(from), DateOnly.FromDateTime(to));
    return Results.Json(result);
}).RequireAuthorization("read:all");

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
}).RequireAuthorization("read:all");

app.MapPut("/booking/cancel", async (int id, CarParkContext context) =>
{
    if (await context.Bookings.FindAsync(id) is Booking booking)
    {
        booking.Cancel();
        await context.SaveChangesAsync();
        return Results.Ok();
    }

    return Results.NotFound();

}).RequireAuthorization("read:all");

app.MapPut("/booking/", async (Booking dto, CarParkContext context) =>
{
    var booking = await context.Bookings.FindAsync(dto.Id);
    if (booking == null) return Results.NotFound();
    booking.Update(dto.StartDate.Date, dto.EndDate.Date);
    await context.SaveChangesAsync();
    return Results.Ok();

}).RequireAuthorization("read:all");


app.MapGet("/parkingspot", async (CarParkContext context) =>
{
    var result = (await context.ParkingSpots.ToListAsync()).Select(ParkingSpotDto.From).ToList();
    return Results.Json(result);
}).RequireAuthorization("read:all");
   

app.MapPost("/parkingspot", async (ParkingSpot parkingSpot, CarParkContext context) =>
{
    context.ParkingSpots.Add(parkingSpot);
    await context.SaveChangesAsync();
    return Results.Ok();
}).RequireAuthorization("read:all");

app.MapDelete("/parkingspot{id}", async (int id, CarParkContext context) =>
{
    if (await context.ParkingSpots.FindAsync(id) is ParkingSpot spot)
    {
        context.ParkingSpots.Remove(spot);
        await context.SaveChangesAsync();
        return Results.Ok(spot);
    }

    return Results.NotFound();
}).RequireAuthorization("read:all");


app.MapGet("/price", async (CarParkContext context) =>
{
    var result = (await context.Prices.ToListAsync()).Select(PriceDto.CreateFrom).ToList();
    return Results.Json(result);
}).RequireAuthorization("read:all");




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.SerializeAsV2 = true;
    });
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();



app.MapControllers();

app.Run();