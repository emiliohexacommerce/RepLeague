using MediatR;
using RepLeague.Application.Features.Quotes.DTOs;

namespace RepLeague.Application.Features.Quotes.Queries.GetDailyQuote;

public record GetDailyQuoteQuery(string Lang = "es") : IRequest<QuoteDto>;
