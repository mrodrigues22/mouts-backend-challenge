using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleItemTests
{
    [Fact]
    public void Constructor_ShouldApplyNoDiscount_WhenQuantityBelowFour()
    {
        var item = new SaleItem(Guid.NewGuid(), "Mouse", 3, 100m);

        item.Discount.Should().Be(0m);
        item.TotalAmount.Should().Be(300m);
    }

    [Fact]
    public void Constructor_ShouldApplyTenPercent_WhenQuantityInLowerTier()
    {
        var item = new SaleItem(Guid.NewGuid(), "Mouse", 5, 100m);

        item.Discount.Should().Be(0.10m);
        item.TotalAmount.Should().Be(450m);
    }

    [Fact]
    public void Constructor_ShouldApplyTwentyPercent_WhenQuantityInUpperTier()
    {
        var item = new SaleItem(Guid.NewGuid(), "Mouse", 10, 100m);

        item.Discount.Should().Be(0.20m);
        item.TotalAmount.Should().Be(800m);
    }

    [Fact]
    public void Cancel_ShouldFlagItemAsCancelled()
    {
        var item = new SaleItem(Guid.NewGuid(), "Mouse", 5, 100m);

        item.Cancel();

        item.IsCancelled.Should().BeTrue();
    }
}
