using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CancelSaleItemHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly CancelSaleItemHandler _handler;

    public CancelSaleItemHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _publisher = Substitute.For<IPublisher>();
        _handler = new CancelSaleItemHandler(_saleRepository, _mapper, _publisher);

        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Sale>());
        _mapper.Map<SaleResult>(Arg.Any<Sale>()).Returns(new SaleResult());
    }

    private static Sale SaleWithTwoItems()
    {
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = "S-0001",
            CustomerId = Guid.NewGuid(),
            CustomerName = "John",
            BranchId = Guid.NewGuid(),
            BranchName = "Downtown"
        };
        sale.AddItem(Guid.NewGuid(), "Mouse", 5, 100m);
        sale.AddItem(Guid.NewGuid(), "Keyboard", 2, 50m);
        return sale;
    }

    [Fact(DisplayName = "Given valid item When cancelling Then recalculates total and publishes ItemCancelledEvent")]
    public async Task Handle_ValidItem_CancelsAndPublishes()
    {
        var sale = SaleWithTwoItems();
        var itemToCancel = sale.Items.First();
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        await _handler.Handle(new CancelSaleItemCommand(sale.Id, itemToCancel.Id), CancellationToken.None);

        itemToCancel.IsCancelled.Should().BeTrue();
        sale.TotalAmount.Should().Be(100m);
        await _publisher.Received(1).Publish(Arg.Any<ItemCancelledEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given missing sale When cancelling item Then throws KeyNotFoundException")]
    public async Task Handle_SaleNotFound_Throws()
    {
        _saleRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => _handler.Handle(new CancelSaleItemCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given unknown item When cancelling Then throws KeyNotFoundException")]
    public async Task Handle_ItemNotFound_Throws()
    {
        var sale = SaleWithTwoItems();
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var act = () => _handler.Handle(new CancelSaleItemCommand(sale.Id, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
