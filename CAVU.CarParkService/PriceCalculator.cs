using CAVU.CarParkService.Models;

namespace CAVU.CarParkService;

public static class PriceCalculator
{
    public static decimal Calculate(List<Price> prices, DateOnly from, DateOnly to)
    {
        var relevantPrices = prices.Where(x => x.From < to && x.To > from).ToList();

        var days = new List<DateOnly>();
        
        for (var day = from; day <= to; day = day.AddDays(1)) 
        {
            days.Add(day);
        }

        return days.Sum(day => relevantPrices.FirstOrDefault(x => day >= x.From && day <= x.To)?.PricePerDay ?? 0);
    }
}