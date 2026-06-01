using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Validation;
using FluentValidation.TestHelper;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

public class SaleItemValidatorTests
{
    private readonly SaleItemValidator _validator = new();

    [Fact]
    public void Should_Pass_For_Valid_Item()
    {
        var item = new SaleItem(Guid.NewGuid(), "Mouse", 5, 100m);

        var result = _validator.TestValidate(item);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_ProductName_Empty()
    {
        var item = new SaleItem(Guid.NewGuid(), string.Empty, 5, 100m);

        var result = _validator.TestValidate(item);

        result.ShouldHaveValidationErrorFor(i => i.ProductName);
    }

    [Fact]
    public void Should_Fail_When_UnitPrice_Not_Positive()
    {
        var item = new SaleItem(Guid.NewGuid(), "Mouse", 5, 0m);

        var result = _validator.TestValidate(item);

        result.ShouldHaveValidationErrorFor(i => i.UnitPrice);
    }
}
