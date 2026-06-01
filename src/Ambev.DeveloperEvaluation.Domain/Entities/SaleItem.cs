using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Services;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class SaleItem : BaseEntity
{
    public Guid SaleId { get; set; }

    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Discount { get; private set; }

    public decimal TotalAmount { get; private set; }

    public bool IsCancelled { get; private set; }

    public SaleItem()
    {
    }

    public SaleItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Recalculate();
    }

    public void Recalculate()
    {
        Discount = DiscountPolicy.GetDiscount(Quantity);
        TotalAmount = Quantity * UnitPrice * (1 - Discount);
    }

    public void Cancel()
    {
        IsCancelled = true;
    }
}
