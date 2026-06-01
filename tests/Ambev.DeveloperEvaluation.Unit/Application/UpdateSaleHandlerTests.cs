using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly UpdateSaleHandler _handler;

    public UpdateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _publisher = Substitute.For<IPublisher>();
        _handler = new UpdateSaleHandler(_saleRepository, _mapper, _publisher);

        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Sale>());
        _mapper.Map<SaleResult>(Arg.Any<Sale>()).Returns(new SaleResult());
    }

    private static Sale ExistingSale()
    {
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = "S-0001",
            CustomerId = Guid.NewGuid(),
            CustomerName = "Old",
            BranchId = Guid.NewGuid(),
            BranchName = "Old Branch"
        };
        sale.AddItem(Guid.NewGuid(), "Old Product", 2, 50m);
        return sale;
    }

    private static UpdateSaleCommand ValidCommand(Guid id) => new()
    {
        Id = id,
        SaleNumber = "S-0001",
        SaleDate = DateTime.UtcNow,
        CustomerId = Guid.NewGuid(),
        CustomerName = "New",
        BranchId = Guid.NewGuid(),
        BranchName = "New Branch",
        Items = new List<UpdateSaleItemCommand>
        {
            new() { ProductId = Guid.NewGuid(), ProductName = "Mouse", Quantity = 5, UnitPrice = 100m }
        }
    };

    [Fact(DisplayName = "Given valid command When updating Then replaces items, recalculates, and publishes SaleModifiedEvent")]
    public async Task Handle_ValidCommand_UpdatesAndPublishes()
    {
        var sale = ExistingSale();
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        await _handler.Handle(ValidCommand(sale.Id), CancellationToken.None);

        sale.Items.Should().HaveCount(1);
        sale.TotalAmount.Should().Be(450m);
        await _publisher.Received(1).Publish(Arg.Any<SaleModifiedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given missing sale When updating Then throws KeyNotFoundException")]
    public async Task Handle_MissingSale_Throws()
    {
        _saleRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => _handler.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given sale number owned by another sale When updating Then throws InvalidOperationException")]
    public async Task Handle_DuplicateSaleNumber_Throws()
    {
        var sale = ExistingSale();
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.GetBySaleNumberAsync("S-0001", Arg.Any<CancellationToken>())
            .Returns(new Sale { Id = Guid.NewGuid(), SaleNumber = "S-0001" });

        var act = () => _handler.Handle(ValidCommand(sale.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact(DisplayName = "Given empty command When updating Then throws validation exception")]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        var act = () => _handler.Handle(new UpdateSaleCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}
