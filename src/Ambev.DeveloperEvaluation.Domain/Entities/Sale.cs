using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale : BaseEntity
{
    public string SaleNumber { get; set; } = string.Empty;

    public DateTime SaleDate { get; set; }

    public Guid CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public Guid BranchId { get; set; }

    public string BranchName { get; set; } = string.Empty;

    public decimal TotalAmount { get; private set; }

    public bool IsCancelled { get; private set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<SaleItem> Items { get; set; } = new();

    public Sale()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public SaleItem AddItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        var item = new SaleItem(productId, productName, quantity, unitPrice)
        {
            SaleId = Id
        };

        Items.Add(item);
        RecalculateTotal();
        return item;
    }

    public void RecalculateTotal()
    {
        TotalAmount = Items
            .Where(item => !item.IsCancelled)
            .Sum(item => item.TotalAmount);
    }

    public void Cancel()
    {
        IsCancelled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CancelItem(Guid itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException($"Item {itemId} was not found in sale {SaleNumber}.");

        item.Cancel();
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }
}
