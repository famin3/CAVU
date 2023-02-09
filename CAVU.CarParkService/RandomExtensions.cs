namespace CAVU.CarParkService;

public static class RandomExtensions
{
    public static decimal NextDecimal(
        this Random random,
        decimal minValue,
        decimal maxValue)
    {
        return decimal.Round((decimal)random.NextDouble() * (maxValue - minValue) + minValue, 2, MidpointRounding.AwayFromZero);
    }

    public static DateTime NextDate(this Random random,
        DateTime minDate,
        DateTime maxDate)
    {
        TimeSpan timeSpan = maxDate - minDate;
        TimeSpan newSpan = new TimeSpan(0, random.Next(0, (int)timeSpan.TotalMinutes), 0);
        DateTime newDate = minDate + newSpan;
        return newDate;
    }
}