using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepLeague.Application.Features.Quotes.Queries.GetDailyQuote;

namespace RepLeague.API.Controllers;

[Authorize]
[ApiController]
[Route("api/quotes")]
public class QuotesController(IMediator mediator) : BaseApiController(mediator)
{
    /// <summary>Devuelve la frase motivacional del día traducida al idioma solicitado.</summary>
    [HttpGet("daily")]
    public async Task<IActionResult> GetDailyQuote(
        [FromQuery] string lang = "es",
        CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetDailyQuoteQuery(lang), ct));
}
