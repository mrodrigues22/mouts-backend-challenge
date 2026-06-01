using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class ListSalesHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ListSalesHandler _handler;

    public ListSalesHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new ListSalesHandler(_saleRepository, _mapper);
        _mapper.Map<List<SaleResult>>(Arg.Any<IReadOnlyList<Sale>>()).Returns(new List<SaleResult>());
    }

    [Fact(DisplayName = "Given a page request When listing Then returns paginated result")]
    public async Task Handle_ReturnsPaginatedResult()
    {
        IReadOnlyList<Sale> sales = new List<Sale> { new() { Id = Guid.NewGuid() } };
        _saleRepository.GetPagedAsync(2, 5, "saleDate desc", Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .Returns((sales, 11));

        var result = await _handler.Handle(new ListSalesCommand { Page = 2, Size = 5, Order = "saleDate desc" }, CancellationToken.None);

        result.TotalCount.Should().Be(11);
        result.Page.Should().Be(2);
        result.Size.Should().Be(5);
    }

    [Fact(DisplayName = "Given invalid page and size When listing Then normalizes to defaults")]
    public async Task Handle_NormalizesInvalidPaging()
    {
        IReadOnlyList<Sale> sales = new List<Sale>();
        _saleRepository.GetPagedAsync(1, 10, Arg.Any<string?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .Returns((sales, 0));

        await _handler.Handle(new ListSalesCommand { Page = 0, Size = -3 }, CancellationToken.None);

        await _saleRepository.Received(1).GetPagedAsync(1, 10, Arg.Any<string?>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>());
    }
}
