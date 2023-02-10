using CAVU.CarParkService.Models;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace CAVU.CarParkService.DTOs;

public class PriceDto
{
    public int Id { get; set; }
    public decimal PricePerDay { get; set; }
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }

    
    public static PriceDto CreateFrom(Price price)
    {
        return new PriceDto
        {
            Id = price.Id,
            PricePerDay = price.PricePerDay,
            From = price.From,
            To = price.To
        };
    }
}