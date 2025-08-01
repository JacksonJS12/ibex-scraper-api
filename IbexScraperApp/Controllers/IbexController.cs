using Microsoft.AspNetCore.Mvc;
using IbexScraperApp.Data;
using IbexScraperApp.Models;
using IbexScraperApp.Services;

namespace IbexScraperApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IbexController : ControllerBase
{
    private readonly IbexScraper _scraper;
    private readonly ApplicationDbContext _context;

    public IbexController(IbexScraper scraper, ApplicationDbContext context)
    {
        _scraper = scraper;
        _context = context;
    }

    [HttpPost("scrape")]
    public async Task<IActionResult> ScrapeAndSave()
    {
        var data = await _scraper.ScrapeAsync();

        foreach (var item in data)
        {
            bool exists = _context.MarketPrices.Any(p =>
                p.Date == item.Date && p.Hour == item.Hour);

            if (!exists)
                _context.MarketPrices.Add(item);
        }

        await _context.SaveChangesAsync();

        return Ok(new { Saved = data.Count });
    }
}