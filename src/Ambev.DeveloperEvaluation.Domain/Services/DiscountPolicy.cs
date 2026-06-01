namespace Ambev.DeveloperEvaluation.Domain.Services;

public static class DiscountPolicy
{
    public const int MaxQuantity = 20;

    public static decimal GetDiscount(int quantity)
    {
        if (quantity < 1)
            throw new DomainException("Quantity must be at least 1.");

        if (quantity > MaxQuantity)
            throw new DomainException($"Cannot sell more than {MaxQuantity} identical items.");

        return quantity switch
        {
            >= 10 => 0.20m,
            >= 4 => 0.10m,
            _ => 0m
        };
    }
}
