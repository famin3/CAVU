using CAVU.CarParkService.Models;

namespace CAVU.CarParkService.DTOs;

public class ParkingSpotDto
{
    public int Id { get; set; }
    public string ParkingSpotName { get; set; }

    public static ParkingSpotDto From(ParkingSpot parkingSpot)
    {
        return new ParkingSpotDto
        {
            Id = parkingSpot.Id,
            ParkingSpotName = parkingSpot.ParkingSpotName
        };
    }
}