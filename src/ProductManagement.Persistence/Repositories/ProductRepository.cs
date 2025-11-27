using System.Linq.Expressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Application.Repositories;
using ProductManagement.Application.Shared;
using ProductManagement.Application.UseCases.Products.GetAll;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationContext _appContext;

    public ProductRepository(ApplicationContext appContext)
    {
        _appContext = appContext;
    }

    public void Add(Product product)
    {
        _appContext.Products.Add(product);
    }

    public async Task<Product?> GetAsync(Guid id)
    {
        return await _appContext.Products
            .Include(p => p.MeasurementUnit)
            .Include(p => p.InventoryRecords)
            .Include(p => p.Owner)
            .Where(p => !p.Owner.IsDeleted) //filtering out (no requirements)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Product>> GetAllAsync(GetAllProductsQueryParameters? queryParameters)
    {
        Expression<Func<Product, bool>>? filteringPredicate = ConstructFilteringPredicate(queryParameters);
        Func<IQueryable<Product>, IOrderedQueryable<Product>>? orderingDelegate = ConstructOrderingDelegate(queryParameters);

        var products = _appContext.Products
            .Include(p => p.MeasurementUnit)
            .Include(p => p.InventoryRecords)
            .Include(p => p.Owner)
            .Where(p => !p.Owner.IsDeleted) //same as before
            .AsExpandableEFCore();
        
        if (filteringPredicate is not null) products = products.Where(filteringPredicate);
        if (orderingDelegate is not null) products = orderingDelegate(products);

        //ADD PAGINATION!!!!!!

        return await products.ToListAsync();
    }

    public void Remove(Guid id)
    {
        var fetchedProduct = _appContext.Products.Local.FirstOrDefault(p => p.Id == id);
        if (fetchedProduct is not null)
        {
            _appContext.Entry(fetchedProduct).State = EntityState.Deleted;
            return;
        }

        var product = Activator.CreateInstance<Product>();
        product.Id = id;
        _appContext.Entry(product).State = EntityState.Deleted;
    }

    public void Remove(Product product)
    {
        _appContext.Products.Remove(product);
    }

    private static Func<IQueryable<Product>, IOrderedQueryable<Product>>? ConstructOrderingDelegate(GetAllProductsQueryParameters? queryParameters)
    {
        if (queryParameters is null || queryParameters.OrderBy is null) return null;

        var isAscending = (queryParameters.OrderDirection ?? OrderDirection.Asc) == OrderDirection.Asc;

        Expression<Func<Product, decimal>> minPriceSelector = p => p.InventoryRecords.Min(ir => ir.UnitPrice);
        Expression<Func<Product, decimal>> maxPriceSelector = p => p.InventoryRecords.Max(ir => ir.UnitPrice);
        Expression<Func<Product, decimal>> totalQuantitySelector = p => p.InventoryRecords.Sum(ir => ir.Quantity);

        Func<IQueryable<Product>, IOrderedQueryable<Product>> orderingDelegate = queryParameters.OrderBy switch
        {
            ProductOrderBy.Name => isAscending ? q => q.OrderBy(p => p.Name) : q => q.OrderByDescending(p => p.Name),
            ProductOrderBy.MinPrice => isAscending ? q => q.OrderBy(minPriceSelector) : q => q.OrderByDescending(minPriceSelector),
            ProductOrderBy.MaxPrice => isAscending ? q => q.OrderBy(maxPriceSelector) : q => q.OrderByDescending(maxPriceSelector),
            ProductOrderBy.Quantity => isAscending ? q => q.OrderBy(totalQuantitySelector) : q => q.OrderByDescending(totalQuantitySelector),
            _ => throw new ArgumentException($"Unknown ordering parameter value {queryParameters.OrderBy}")
        };

        return orderingDelegate;
    }

    private static Expression<Func<Product, bool>>? ConstructFilteringPredicate(GetAllProductsQueryParameters? queryParameters)
    {
        if (queryParameters is null) return null;

        var filteringPredicate = PredicateBuilder.New<Product>();

        if (!string.IsNullOrWhiteSpace(queryParameters.NameContains))
        {
            var parameter = queryParameters.NameContains.Trim().ToLower();
            filteringPredicate.And(p => p.Name.ToLower().Contains(parameter));
        }

        if (!string.IsNullOrWhiteSpace(queryParameters.DescriptionContains))
        {
            var parameter = queryParameters.DescriptionContains.Trim().ToLower();
            filteringPredicate.And(p => p.Description.ToLower().Contains(parameter));
        }

        return filteringPredicate.IsStarted ? filteringPredicate : null; 
    }
}