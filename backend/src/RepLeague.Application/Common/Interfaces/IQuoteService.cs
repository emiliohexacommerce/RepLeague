using RepLeague.Application.Features.Quotes.DTOs;

namespace RepLeague.Application.Common.Interfaces;

public interface IQuoteService
{
    Task<QuoteDto> GetDailyQuoteAsync(string targetLang, CancellationToken ct = default);
}
