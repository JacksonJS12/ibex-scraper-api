using Microsoft.EntityFrameworkCore;
using IbexScraperApp.Data;
using IbexScraperApp.Models;

namespace IbexScraperApp.Services;

public class ScheduledScraperService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScheduledScraperService> _logger;
    private Timer? _timer;

    public ScheduledScraperService(IServiceProvider serviceProvider, ILogger<ScheduledScraperService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run immediately on start (check for empty table)
        _ = ScrapeIfNeeded();

        // Then schedule to run every minute
        _timer = new Timer(async _ => await ScheduledScrape(), null, TimeSpan.Zero, TimeSpan.FromMinutes(60));

        return Task.CompletedTask;
    }

    private async Task ScrapeIfNeeded()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var scraper = scope.ServiceProvider.GetRequiredService<IbexScraper>();

        var hasData = await context.MarketPrices.AnyAsync();

        if (!hasData)
        {
            _logger.LogInformation("Table empty. Running scraper...");
            var data = await scraper.ScrapeAsync();

            context.MarketPrices.AddRange(data);
            await context.SaveChangesAsync();
            _logger.LogInformation("Scraped and saved {Count} records (initial).", data.Count);
        }
    }

    private async Task ScheduledScrape()
    {
        var utcNow = DateTime.UtcNow;
        var bulgariaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
        var now = TimeZoneInfo.ConvertTimeFromUtc(utcNow, bulgariaTimeZone);

        if (now.Hour == 14)
        {
            _logger.LogInformation($"Scheduled scrape triggered at {now.Hour}:00.");

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var scraper = scope.ServiceProvider.GetRequiredService<IbexScraper>();

            var data = await scraper.ScrapeAsync();

            foreach (var item in data)
            {
                bool exists = context.MarketPrices.Any(p =>
                    p.Date == item.Date && p.Hour == item.Hour);

                if (!exists)
                    context.MarketPrices.Add(item);
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("Scraped and saved {Count} records.", data.Count);
        }
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}