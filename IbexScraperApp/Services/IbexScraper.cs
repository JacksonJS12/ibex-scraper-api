using AngleSharp;
using IbexScraperApp.Models;

namespace IbexScraperApp.Services;

public class IbexScraper
{
    public async Task<List<MarketPrice>> ScrapeAsync()
    {
        var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
        var document = await context.OpenAsync("https://ibex.bg/dam-data-chart-2/");

        var prices = document.QuerySelectorAll("td.column-price")
            .Select(x => decimal.Parse(x.TextContent)).ToList();

        var hours = document.QuerySelectorAll("td.column-time_part")
            .Select(x => TimeSpan.Parse(x.TextContent).Hours).ToList();

        var dates = document.QuerySelectorAll("td.column-date_part")
            .Select(x => DateTime.Parse(x.TextContent).Date).ToList();

        var list = new List<MarketPrice>();
        for (int i = 0; i < prices.Count && i < hours.Count && i < dates.Count; i++)
        {
            list.Add(new MarketPrice
            {
                PricePerMWh = prices[i],
                Hour = hours[i] + 1, // to match the graphic in IBEX
                Date = dates[i].ToString("dd/MM/yyyy")
            });
        }

        return list;
    }
}
