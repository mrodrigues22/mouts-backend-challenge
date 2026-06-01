using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesHandler : IRequestHandler<ListSalesCommand, ListSalesResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public ListSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<ListSalesResult> Handle(ListSalesCommand request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var size = request.Size < 1 ? 10 : request.Size;

        var (sales, totalCount) = await _saleRepository.GetPagedAsync(page, size, request.Order, request.Filters, cancellationToken);

        return new ListSalesResult
        {
            Sales = _mapper.Map<List<SaleResult>>(sales),
            TotalCount = totalCount,
            Page = page,
            Size = size
        };
    }
}
