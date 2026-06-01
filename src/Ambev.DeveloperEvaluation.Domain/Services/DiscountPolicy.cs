namespace Ambev.DeveloperEvaluation.Domain.Services;

/// <summary>
/// Encapsulates the quantity-based discount rules for a single sale item.
/// This is the single source of truth for the discounting business rules so the
/// behaviour can be reasoned about and unit-tested in isolation.
/// </summary>
/// <remarks>
/// Rules:
/// <list type="bullet">
/// <item>1-3 identical items: no discount.</item>
/// <item>4-9 identical items: 10% discount.</item>
/// <item>10-20 identical items: 20% discount.</item>
/// <item>More than 20 identical items: not allowed.</item>
/// </list>
/// </remarks>
public static class DiscountPolicy
{
    /// <summary>The maximum number of identical items allowed in a single sale item.</summary>
    public const int MaxQuantity = 20;

    /// <summary>
    /// Returns the discount rate (as a fraction, e.g. 0.10 for 10%) for the given quantity.
    /// </summary>
    /// <param name="quantity">The number of identical items in the line.</param>
    /// <returns>The discount rate to apply: 0, 0.10 or 0.20.</returns>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="quantity"/> is less than 1 or greater than <see cref="MaxQuantity"/>.
    /// </exception>
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
