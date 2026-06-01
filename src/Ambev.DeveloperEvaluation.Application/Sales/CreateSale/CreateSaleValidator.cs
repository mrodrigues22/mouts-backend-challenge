using Ambev.DeveloperEvaluation.Domain.Services;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleValidator()
    {
        RuleFor(sale => sale.SaleNumber).NotEmpty();
        RuleFor(sale => sale.SaleDate).NotEmpty();
        RuleFor(sale => sale.CustomerId).NotEmpty();
        RuleFor(sale => sale.CustomerName).NotEmpty();
        RuleFor(sale => sale.BranchId).NotEmpty();
        RuleFor(sale => sale.BranchName).NotEmpty();
        RuleFor(sale => sale.Items).NotEmpty();

        RuleForEach(sale => sale.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.ProductName).NotEmpty();
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0)
                .LessThanOrEqualTo(DiscountPolicy.MaxQuantity)
                .WithMessage($"Cannot sell more than {DiscountPolicy.MaxQuantity} identical items.");
            item.RuleFor(i => i.UnitPrice).GreaterThan(0);
        });
    }
}
