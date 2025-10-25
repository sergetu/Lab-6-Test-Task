using Lab6TestTask.Data;
using Lab6TestTask.Models;
using Lab6TestTask.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Lab6TestTask.Services.Implementations;

/// <summary>
/// WarehouseService.
/// Implement methods here.
/// </summary>
public class WarehouseService : IWarehouseService
{
    private readonly ApplicationDbContext _dbContext;

    private readonly DateTime _defaultQ2StartDate;
    private readonly DateTime _defaultQ2EndDate;

    public WarehouseService(ApplicationDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _defaultQ2StartDate = configuration.GetValue<DateTime>("WarehauseSettings:Q2StartDate", new DateTime(2025, 4, 1));
        _defaultQ2EndDate = configuration.GetValue<DateTime>("WarehauseSettings:Q2EndDate", new DateTime(2025, 6, 30));
    }

    public async Task<Warehouse> GetWarehouseAsync()
    {
        try
        {
            var warehouseValues = await WarehouseSelect()
            .Select(w => new
            {
                Warehouse = w,
                TotalValue = w.Products
                    .Where(p => p.Status == Enums.ProductStatus.ReadyForDistribution)
                    .Sum(p => (double?)p.Price * p.Quantity)
            })
            .OrderByDescending(w => w.TotalValue)
            .FirstOrDefaultAsync();

            return warehouseValues.Warehouse;
        }
        catch (DbException ex)
        {
            throw new InvalidOperationException(
                "Failed to retrieve the warehouse with the highest value of ready products.", ex);
        }
    }
    public async Task<IEnumerable<Warehouse>> GetWarehousesAsync()
    {
        try
        {
            return await WarehouseSelect()
                .Where(w => w.Products.Any(p =>
                    p.ReceivedDate >= _defaultQ2StartDate &&
                    p.ReceivedDate <= _defaultQ2EndDate))
                .ToListAsync();
        }
        catch (DbException ex)
        {
            throw new InvalidOperationException(
                "Failed to retrieve warehouses with products received in Q2 2025.", ex);
        }
    }
    private IQueryable<Warehouse> WarehouseSelect()
    {
        return _dbContext.Warehouses
            .AsNoTracking()
            .Select(w => new Warehouse
            {
                WarehouseId = w.WarehouseId,
                Name = w.Name,
                Location = w.Location,
                Products = w.Products
            });
    }
}

