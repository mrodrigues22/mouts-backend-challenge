using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Fact]
    public void AddItem_ShouldAccumulateTotalFromAllItems()
    {
        var sale = new Sale();

        sale.AddItem(Guid.NewGuid(), "Mouse", 5, 100m);
        sale.AddItem(Guid.NewGuid(), "Keyboard", 2, 50m);

        sale.TotalAmount.Should().Be(550m);
    }

    [Fact]
    public void AddItem_ShouldThrow_WhenQuantityExceedsMaximum()
    {
        var sale = new Sale();

        var act = () => sale.AddItem(Guid.NewGuid(), "Mouse", 21, 100m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CancelItem_ShouldExcludeItemFromTotal()
    {
        var sale = new Sale();
        var item = sale.AddItem(Guid.NewGuid(), "Mouse", 5, 100m);
        sale.AddItem(Guid.NewGuid(), "Keyboard", 2, 50m);

        sale.CancelItem(item.Id);

        item.IsCancelled.Should().BeTrue();
        sale.TotalAmount.Should().Be(100m);
    }

    [Fact]
    public void CancelItem_ShouldThrow_WhenItemNotFound()
    {
        var sale = new Sale();
        sale.AddItem(Guid.NewGuid(), "Mouse", 5, 100m);

        var act = () => sale.CancelItem(Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_ShouldFlagSaleAsCancelled()
    {
        var sale = new Sale();

        sale.Cancel();

        sale.IsCancelled.Should().BeTrue();
        sale.UpdatedAt.Should().NotBeNull();
    }
}
