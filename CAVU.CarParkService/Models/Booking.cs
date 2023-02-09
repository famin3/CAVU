namespace CAVU.CarParkService.Models;

public class Booking
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Price { get; set; }
    public bool Charged { get; set; }
    public int ParkingSpotId { get; set; }
}