using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

public class SaleCreatedEventHandler : INotificationHandler<SaleCreatedEvent>
{
    private readonly ILogger<SaleCreatedEventHandler> _logger;

    public SaleCreatedEventHandler(ILogger<SaleCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[Event] SaleCreated - Sale {SaleNumber} ({SaleId}) with total {TotalAmount}",
            notification.Sale.SaleNumber, notification.Sale.Id, notification.Sale.TotalAmount);
        return Task.CompletedTask;
    }
}

public class SaleModifiedEventHandler : INotificationHandler<SaleModifiedEvent>
{
    private readonly ILogger<SaleModifiedEventHandler> _logger;

    public SaleModifiedEventHandler(ILogger<SaleModifiedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleModifiedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[Event] SaleModified - Sale {SaleNumber} ({SaleId}) with total {TotalAmount}",
            notification.Sale.SaleNumber, notification.Sale.Id, notification.Sale.TotalAmount);
        return Task.CompletedTask;
    }
}

public class SaleCancelledEventHandler : INotificationHandler<SaleCancelledEvent>
{
    private readonly ILogger<SaleCancelledEventHandler> _logger;

    public SaleCancelledEventHandler(ILogger<SaleCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[Event] SaleCancelled - Sale {SaleNumber} ({SaleId})",
            notification.Sale.SaleNumber, notification.Sale.Id);
        return Task.CompletedTask;
    }
}

public class ItemCancelledEventHandler : INotificationHandler<ItemCancelledEvent>
{
    private readonly ILogger<ItemCancelledEventHandler> _logger;

    public ItemCancelledEventHandler(ILogger<ItemCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ItemCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[Event] ItemCancelled - Item {ProductName} ({ItemId}) on sale {SaleNumber} ({SaleId})",
            notification.Item.ProductName, notification.Item.Id,
            notification.Sale.SaleNumber, notification.Sale.Id);
        return Task.CompletedTask;
    }
}
