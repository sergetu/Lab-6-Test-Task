using Lab6TestTask.Data;
using Lab6TestTask.Models;
using Lab6TestTask.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Lab6TestTask.Services.Implementations;

/// <summary>
/// ProductService.
/// Implement methods here.
/// </summary>
public class ProductService : IProductService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DateTime _defaultStartDate;

    public ProductService(ApplicationDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _defaultStartDate = configuration.GetValue<DateTime>("ProductSettings:StartDate", new DateTime(2025, 1, 1));
    }

    public async Task<Product> GetProductAsync()
    {
        try
        {
            return await ProductSelect()
                .Where(p => p.Status == Enums.ProductStatus.Reserved)
                .OrderByDescending(p => p.Price)
                .FirstAsync();
        }
        catch (DbException ex)
        {
            throw new InvalidOperationException("Failed to retrieve the reserved product with the highest price.", ex);
        }
    }

    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        try
        {
            return await ProductSelect()
                .Where(p => p.ReceivedDate > _defaultStartDate && p.Quantity > 1000)
                .ToListAsync();
        }
        catch (DbException ex)
        {
            throw new InvalidOperationException("Failed to retrieve products.", ex);
        }
    }
    private IQueryable<Product> ProductSelect()
    {
        return _dbContext.Products
                .AsNoTracking()
                .Select(p => new Product
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Quantity = p.Quantity,
                    Status = p.Status,
                    ReceivedDate = p.ReceivedDate,
                    Price = p.Price,
                    WarehouseId = p.WarehouseId,
                    Warehouse = p.Warehouse,
                });
    }

}
