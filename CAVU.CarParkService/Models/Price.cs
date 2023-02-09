namespace CAVU.CarParkService.Models;

public class Price
{
    public int Id { get; set; }
    public decimal PricePerDay { get; set; }
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
}