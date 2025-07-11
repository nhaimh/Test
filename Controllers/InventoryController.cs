using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Test.Data;
using Test.DTO;

namespace Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public InventoryController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<LowStockDto>>> GetLowStockProducts()
        {
            try
            {
                var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3);

                var lowStockProducts = await _context.Products
                    .Select(p => new LowStockDto
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        StockQtty = p.StockQtty,
                        AvgSoldLast3Months = p.OrderDetails
                            .Where(od => od.Order.OrderDate >= threeMonthsAgo)
                            .Average(od => (double?)od.Quantity) ?? 0,
                    })
                    .Where(p => p.StockQtty < 2 * p.AvgSoldLast3Months)
                    .Select(p => new LowStockDto
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        StockQtty = p.StockQtty,
                        AvgSoldLast3Months = p.AvgSoldLast3Months,
                        ExpectedShortage = 2 * p.AvgSoldLast3Months - p.StockQtty
                    })
                    .ToListAsync();

                return Ok(lowStockProducts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while fetching low stock products", message = ex.Message });
            }
        }

        [HttpGet("monthly-report")]
        public async Task<ActionResult<IEnumerable<MonthlyReportDto>>> GetMonthlyReport()
        {
            const string cacheKey = "MonthlyReport";
            if (_cache.TryGetValue(cacheKey, out List<MonthlyReportDto> cachedReport))
            {
                return Ok(cachedReport);
            }

            try
            {
                var reports = await _context.Orders
                    .AsNoTracking()
                    .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalOrders = g.Count(),
                        TotalAmount = g.Sum(o => o.TotalAmount),
                        TopProduct = g.SelectMany(o => o.OrderDetails)
                            .GroupBy(od => od.Product)
                            .OrderByDescending(pg => pg.Sum(od => od.Quantity))
                            .Select(pg => pg.Key.ProductName)
                            .FirstOrDefault()
                    })
                    .OrderByDescending(r => r.Year)
                    .ThenByDescending(r => r.Month)
                    .Take(12)
                    .ToListAsync();

                var monthlyReports = reports.Select((r, i) => new MonthlyReportDto
                {
                    MonthYear = $"{r.Month:D2}/{r.Year}",
                    TotalOrders = r.TotalOrders,
                    TotalAmount = r.TotalAmount,
                    TopSaleProduct = r.TopProduct ?? "N/A",
                    GrowthRate = i < reports.Count - 1 && reports[i + 1].TotalAmount > 0
                        ? (double)((r.TotalAmount - reports[i + 1].TotalAmount) / reports[i + 1].TotalAmount * 100)
                        : 0
                }).ToList();

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                };

                _cache.Set(cacheKey, monthlyReports, cacheOptions);

                return Ok(monthlyReports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while generating monthly report", message = ex.Message });
            }
        }
    }
}
