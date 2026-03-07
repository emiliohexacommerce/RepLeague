using MediatR;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Quotes.DTOs;

namespace RepLeague.Application.Features.Quotes.Queries.GetDailyQuote;

public class GetDailyQuoteQueryHandler(IQuoteService quoteService)
    : IRequestHandler<GetDailyQuoteQuery, QuoteDto>
{
    public Task<QuoteDto> Handle(GetDailyQuoteQuery request, CancellationToken ct)
        => quoteService.GetDailyQuoteAsync(request.Lang, ct);
}
