using System.Globalization;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber, cancellationToken);
    }

    public async Task<(IReadOnlyList<Sale> Sales, int TotalCount)> GetPagedAsync(
        int page,
        int size,
        string? order = null,
        IReadOnlyDictionary<string, string>? filters = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Sales
            .Include(s => s.Items)
            .AsNoTracking();

        query = ApplyFilters(query, filters);

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplyOrdering(query, order);

        var sales = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return (sales, totalCount);
    }

    private static IQueryable<Sale> ApplyFilters(IQueryable<Sale> query, IReadOnlyDictionary<string, string>? filters)
    {
        if (filters == null)
            return query;

        foreach (var (rawKey, value) in filters)
        {
            if (string.IsNullOrWhiteSpace(value))
                continue;

            var key = rawKey.ToLowerInvariant();

            if (key.StartsWith("_min") || key.StartsWith("_max"))
            {
                query = ApplyRangeFilter(query, key, value);
                continue;
            }

            query = key switch
            {
                "salenumber" => ApplyStringFilter(query, value, s => s.SaleNumber),
                "customername" => ApplyStringFilter(query, value, s => s.CustomerName),
                "branchname" => ApplyStringFilter(query, value, s => s.BranchName),
                "customerid" when Guid.TryParse(value, out var cid) => query.Where(s => s.CustomerId == cid),
                "branchid" when Guid.TryParse(value, out var bid) => query.Where(s => s.BranchId == bid),
                "iscancelled" when bool.TryParse(value, out var cancelled) => query.Where(s => s.IsCancelled == cancelled),
                _ => query
            };
        }

        return query;
    }

    private static IQueryable<Sale> ApplyRangeFilter(IQueryable<Sale> query, string key, string value)
    {
        var isMin = key.StartsWith("_min");
        var field = key[4..];

        switch (field)
        {
            case "saledate" when DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed):
                var date = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
                return isMin ? query.Where(s => s.SaleDate >= date) : query.Where(s => s.SaleDate <= date);
            case "totalamount" when decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount):
                return isMin ? query.Where(s => s.TotalAmount >= amount) : query.Where(s => s.TotalAmount <= amount);
            default:
                return query;
        }
    }

    private static IQueryable<Sale> ApplyStringFilter(
        IQueryable<Sale> query,
        string value,
        System.Linq.Expressions.Expression<Func<Sale, string>> selector)
    {
        var startsWildcard = value.StartsWith('*');
        var endsWildcard = value.EndsWith('*');
        var term = value.Trim('*');

        if (!startsWildcard && !endsWildcard)
            return query.Where(BuildEquals(selector, term));

        var pattern = (startsWildcard ? "%" : string.Empty) + term + (endsWildcard ? "%" : string.Empty);
        return query.Where(BuildLike(selector, pattern));
    }

    private static System.Linq.Expressions.Expression<Func<Sale, bool>> BuildEquals(
        System.Linq.Expressions.Expression<Func<Sale, string>> selector, string term)
    {
        var body = System.Linq.Expressions.Expression.Equal(selector.Body, System.Linq.Expressions.Expression.Constant(term));
        return System.Linq.Expressions.Expression.Lambda<Func<Sale, bool>>(body, selector.Parameters);
    }

    private static System.Linq.Expressions.Expression<Func<Sale, bool>> BuildLike(
        System.Linq.Expressions.Expression<Func<Sale, string>> selector, string pattern)
    {
        var efFunctions = System.Linq.Expressions.Expression.Constant(EF.Functions);
        var ilike = typeof(NpgsqlDbFunctionsExtensions).GetMethod(
            nameof(NpgsqlDbFunctionsExtensions.ILike),
            new[] { typeof(DbFunctions), typeof(string), typeof(string) })!;
        var call = System.Linq.Expressions.Expression.Call(ilike, efFunctions, selector.Body,
            System.Linq.Expressions.Expression.Constant(pattern));
        return System.Linq.Expressions.Expression.Lambda<Func<Sale, bool>>(call, selector.Parameters);
    }

    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        _context.Sales.Update(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await GetByIdAsync(id, cancellationToken);
        if (sale == null)
            return false;

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static IQueryable<Sale> ApplyOrdering(IQueryable<Sale> query, string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
            return query.OrderByDescending(s => s.SaleDate);

        IOrderedQueryable<Sale>? ordered = null;

        foreach (var clause in order.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = clause.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var field = parts[0];
            var descending = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            ordered = ApplyOrderClause(query, ordered, field, descending);
        }

        return ordered ?? query.OrderByDescending(s => s.SaleDate);
    }

    private static IOrderedQueryable<Sale> ApplyOrderClause(
        IQueryable<Sale> query,
        IOrderedQueryable<Sale>? ordered,
        string field,
        bool descending)
    {
        System.Linq.Expressions.Expression<Func<Sale, object>> selector = field.ToLowerInvariant() switch
        {
            "salenumber" => s => s.SaleNumber,
            "saledate" => s => s.SaleDate,
            "customername" => s => s.CustomerName,
            "branchname" => s => s.BranchName,
            "totalamount" => s => s.TotalAmount,
            _ => s => s.SaleDate
        };

        if (ordered == null)
            return descending ? query.OrderByDescending(selector) : query.OrderBy(selector);

        return descending ? ordered.ThenByDescending(selector) : ordered.ThenBy(selector);
    }
}
