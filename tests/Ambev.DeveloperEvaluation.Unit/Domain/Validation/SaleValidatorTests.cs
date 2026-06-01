using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Validation;
using FluentValidation.TestHelper;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

public class SaleValidatorTests
{
    private readonly SaleValidator _validator = new();

    private static Sale ValidSale()
    {
        var sale = new Sale
        {
            SaleNumber = "S-0001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "John Doe",
            BranchId = Guid.NewGuid(),
            BranchName = "Downtown"
        };
        sale.AddItem(Guid.NewGuid(), "Mouse", 5, 100m);
        return sale;
    }

    [Fact]
    public void Should_Pass_For_Valid_Sale()
    {
        var result = _validator.TestValidate(ValidSale());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_SaleNumber_Empty()
    {
        var sale = ValidSale();
        sale.SaleNumber = string.Empty;

        var result = _validator.TestValidate(sale);

        result.ShouldHaveValidationErrorFor(s => s.SaleNumber);
    }

    [Fact]
    public void Should_Fail_When_No_Items()
    {
        var sale = ValidSale();
        sale.Items.Clear();

        var result = _validator.TestValidate(sale);

        result.ShouldHaveValidationErrorFor(s => s.Items);
    }
}
