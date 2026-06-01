using Ambev.DeveloperEvaluation.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Services;

public class DiscountPolicyTests
{
    [Theory]
    [InlineData(1, 0.00)]
    [InlineData(3, 0.00)]
    [InlineData(4, 0.10)]
    [InlineData(9, 0.10)]
    [InlineData(10, 0.20)]
    [InlineData(20, 0.20)]
    public void GetDiscount_ShouldReturnExpectedRate_ForQuantityTier(int quantity, decimal expected)
    {
        var discount = DiscountPolicy.GetDiscount(quantity);

        discount.Should().Be(expected);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void GetDiscount_ShouldThrow_WhenQuantityBelowOne(int quantity)
    {
        var act = () => DiscountPolicy.GetDiscount(quantity);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(21)]
    [InlineData(100)]
    public void GetDiscount_ShouldThrow_WhenQuantityExceedsMaximum(int quantity)
    {
        var act = () => DiscountPolicy.GetDiscount(quantity);

        act.Should().Throw<DomainException>()
            .WithMessage("*more than 20*");
    }
}
