using Microsoft.EntityFrameworkCore;

namespace IbexScraperApp.Models;

public class MarketPrice
{
    public MarketPrice()
    {
        this.Id = Guid.NewGuid().ToString();
    }
    public string Id { get; set; }
    public string Date { get; set; }
    public int Hour { get; set; }
    [Precision(18,2)]
    public decimal PricePerMWh { get; set; }
}