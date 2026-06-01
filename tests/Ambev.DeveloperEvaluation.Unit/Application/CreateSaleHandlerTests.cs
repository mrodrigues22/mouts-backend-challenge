using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using AutoMapper;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _publisher = Substitute.For<IPublisher>();
        _handler = new CreateSaleHandler(_saleRepository, _mapper, _publisher);

        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Sale>());
        _mapper.Map<SaleResult>(Arg.Any<Sale>()).Returns(new SaleResult());
    }

    [Fact(DisplayName = "Given valid command When creating sale Then persists and publishes SaleCreatedEvent")]
    public async Task Handle_ValidCommand_PersistsAndPublishesEvent()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();

        await _handler.Handle(command, CancellationToken.None);

        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await _publisher.Received(1).Publish(Arg.Any<SaleCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given item with 5 units When creating sale Then applies 10% discount to the persisted sale")]
    public async Task Handle_ItemInDiscountTier_AppliesDiscount()
    {
        var command = CreateSaleHandlerTestData.GenerateCommandWithItem(quantity: 5, unitPrice: 100m);
        Sale? persisted = null;
        await _saleRepository.CreateAsync(Arg.Do<Sale>(s => persisted = s), Arg.Any<CancellationToken>());

        await _handler.Handle(command, CancellationToken.None);

        persisted.Should().NotBeNull();
        persisted!.TotalAmount.Should().Be(450m);
        persisted.Items.Single().Discount.Should().Be(0.10m);
    }

    [Fact(DisplayName = "Given existing sale number When creating sale Then throws InvalidOperationException")]
    public async Task Handle_DuplicateSaleNumber_Throws()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns(new Sale { SaleNumber = command.SaleNumber });

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact(DisplayName = "Given empty command When creating sale Then throws validation exception")]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        var command = new CreateSaleCommand();

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}
