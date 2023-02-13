using System.Net;
using System.Net.Http.Json;
using CAVU.CarParkService.DTOs;
using CAVU.CarParkService.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;

namespace CAVU.Tests;

public class ApiTests
{
    private HttpClient client;

    [SetUp]
    public async Task Setup()
    {
        var application = new WebApplicationFactory<Program>();
        client = application.CreateClient();
    }

    [Test]
    public async Task GetBooking_IsCalled_DoesntThrow()
    {
        var response = await client.GetAsync("/booking");

        var ret = JsonConvert.DeserializeObject<List<Booking>>(await response.Content.ReadAsStringAsync());

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.IsNotNull(ret);
    }
    
    [Test]
    public async Task GetBookingAvailableSlotsForDateRange_IsCalled_DoesntThrow()
    {
        using (var requestMessage =
               new HttpRequestMessage(HttpMethod.Get, "/booking/availableslotsfordaterange"))
        {
            requestMessage.Headers.Add("from", "2023-06-01");
            requestMessage.Headers.Add("to", "2023-06-30");

            var response = await client.SendAsync(requestMessage);

        var ret = JsonConvert.DeserializeObject<List<int>>(await response.Content.ReadAsStringAsync());
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
    
    [Test]
    public async Task GetBookingAvailableSlotsCountPerDay_IsCalled_DoesntThrow()
    {
        using (var requestMessage =
               new HttpRequestMessage(HttpMethod.Get, "/booking/availableslotscountperday"))
        {
            requestMessage.Headers.Add("from", "2023-06-01");
            requestMessage.Headers.Add("to", "2023-06-30");

            var response = await client.SendAsync(requestMessage);

            var ret = JsonConvert.DeserializeObject<List<AvailableSlotCountPerDayDto>>(await response.Content.ReadAsStringAsync());
        
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
    
    [Test]
    public async Task GetBookingPrice_IsCalled_DoesntThrow()
    {
        using var requestMessage =
            new HttpRequestMessage(HttpMethod.Get, "/booking/price");
        requestMessage.Headers.Add("from", "2023-06-01");
        requestMessage.Headers.Add("to", "2023-06-30");

        var response = await client.SendAsync(requestMessage);

        var ret = JsonConvert.DeserializeObject<decimal>(await response.Content.ReadAsStringAsync());
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    
    [Test]
    public async Task PostBooking_IsCalled_DoesntThrow()
    {
        //this test is a bit flaky, needs specific data mocked in context for both scenarios (can and cannot book) to cover fully
        var booking = new Booking()
        {
            Id = 666,
            StartDate = new DateTime(2023, 06, 1),
            EndDate = new DateTime(2023, 06, 2),
            Active = true,
            Charged = true,
            ParkingSpotId = 99,
            Price = 0
        };

        var response = await client.PostAsJsonAsync("/booking", booking);
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        
    }
    
    [Test]
    public async Task PutBookingCancel_IsCalled_DoesntThrow()
    {
        using (var requestMessage =
               new HttpRequestMessage(HttpMethod.Put, "/booking/cancel"))
        {
            requestMessage.Headers.Add("id", "1");
            var response = await client.SendAsync(requestMessage);
        
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
    
    [Test]
    public async Task PutBookingCancel_IsCalledWithWrongId_ReturnsNotFound()
    {
        using (var requestMessage =
               new HttpRequestMessage(HttpMethod.Put, "/booking/cancel"))
        {
            requestMessage.Headers.Add("id", "1");
            var response = await client.SendAsync(requestMessage);
        
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }
    
    [Test]
    public async Task PutBooking_IsCalled_DoesntThrow()
    {
        var booking = new Booking()
        {
            Id = 4,
            StartDate = new DateTime(2023, 06, 5),
            EndDate = new DateTime(2023, 06, 10),
            Active = true,
            Charged = true,
            ParkingSpotId = 99,
            Price = 0
        };
        
        var response = await client.PutAsJsonAsync("/booking", booking);
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    
    [Test]
    public async Task PutBooking_IsCalledWithWrongId_ResultNotFound()
    {
        var booking = new Booking()
        {
            Id = 989,
            StartDate = new DateTime(2023, 06, 5),
            EndDate = new DateTime(2023, 06, 10),
            Active = true,
            Charged = true,
            ParkingSpotId = 99,
            Price = 0
        };
        
        var response = await client.PutAsJsonAsync("/booking", booking);
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
    
    [Test]
    public async Task GetParkingSpot_IsCalled_DoesntThrow()
    {
        using var requestMessage =
            new HttpRequestMessage(HttpMethod.Get, "/parkingspot");

        var response = await client.SendAsync(requestMessage);

        var ret = JsonConvert.DeserializeObject<List<ParkingSpotDto>>(await response.Content.ReadAsStringAsync());
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    
    [Test]
    public async Task PostParkingSpot_IsCalled_DoesntThrow()
    {
        var parkingSpot = new ParkingSpot()
        {
            Id = 666,
            ParkingSpotName = "test 123"
        };

        var response = await client.PostAsJsonAsync("/parkingspot", parkingSpot);
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }
    
    [Test]
    public async Task DeleteParkingSpot_IsCalled_DoesntThrow()
    {
        var response = await client.DeleteAsync("/parkingspot/1");
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    
    [Test]
    public async Task GetPrice_IsCalled_DoesntThrow()
    {
        using var requestMessage =
            new HttpRequestMessage(HttpMethod.Get, "/price");

        var response = await client.SendAsync(requestMessage);

        var ret = JsonConvert.DeserializeObject<List<PriceDto>>(await response.Content.ReadAsStringAsync());
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

}