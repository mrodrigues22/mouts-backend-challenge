using Ambev.DeveloperEvaluation.Application.Sales.Common;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesResult
{
    public IReadOnlyList<SaleResult> Sales { get; set; } = new List<SaleResult>();

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int Size { get; set; }
}
