using Microsoft.EntityFrameworkCore;
using IbexScraperApp.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<MarketPrice> MarketPrices { get; set; }
}