using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Quotes.DTOs;

namespace RepLeague.Infrastructure.Services;

public class QuoteService(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IConfiguration configuration) : IQuoteService
{
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public async Task<QuoteDto> GetDailyQuoteAsync(string targetLang, CancellationToken ct = default)
    {
        var lang = string.IsNullOrWhiteSpace(targetLang)
            ? "es"
            : targetLang.ToLowerInvariant()[..Math.Min(2, targetLang.Length)];

        var dayKey = DateOnly.FromDateTime(DateTime.UtcNow).DayNumber;
        var cacheKey = $"daily-quote-{lang}-{dayKey}";

        if (cache.TryGetValue(cacheKey, out QuoteDto? cached) && cached != null)
            return cached;

        string originalText = "The only bad workout is the one that didn't happen.";
        string author = "Unknown";

        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("RepLeague/1.0");
            var zenStr = await client.GetStringAsync("https://zenquotes.io/api/today", ct);
            var zenData = JsonSerializer.Deserialize<List<ZenQuoteItem>>(zenStr, _json);
            if (zenData is { Count: > 0 } && !string.IsNullOrWhiteSpace(zenData[0].Q))
            {
                originalText = zenData[0].Q;
                author = zenData[0].A ?? "Unknown";
            }
        }
        catch
        {
            // ZenQuotes no disponible → usar frase por defecto
        }

        var translatedText = lang == "en"
            ? originalText
            : await TranslateAsync(originalText, lang, ct);

        var result = new QuoteDto(translatedText, author, lang);
        cache.Set(cacheKey, result, TimeSpan.FromHours(25));
        return result;
    }

    private async Task<string> TranslateAsync(string text, string targetLang, CancellationToken ct)
    {
        var section = configuration.GetSection("AzureTranslator");
        var key = section["Key"];
        var region = section["Region"];
        var endpoint = section["Endpoint"] ?? "https://api.cognitive.microsofttranslator.com";

        if (string.IsNullOrEmpty(key))
            return text; // No configurado → devuelve original

        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
            if (!string.IsNullOrEmpty(region))
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", region);

            var bodyJson = JsonSerializer.Serialize(new[] { new { Text = text } });
            var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                $"{endpoint}/translate?api-version=3.0&from=en&to={targetLang}", content, ct);

            if (!response.IsSuccessStatusCode) return text;

            var responseStr = await response.Content.ReadAsStringAsync(ct);
            var translated = JsonSerializer.Deserialize<List<TranslatorResponse>>(responseStr, _json);
            return translated?[0]?.Translations?[0]?.Text ?? text;
        }
        catch
        {
            return text; // Fallback ante cualquier error de traducción
        }
    }

    private sealed record ZenQuoteItem(
        [property: JsonPropertyName("q")] string Q,
        [property: JsonPropertyName("a")] string A);

    private sealed record TranslatorResponse(
        [property: JsonPropertyName("translations")] List<TranslatorTranslation>? Translations);

    private sealed record TranslatorTranslation(
        [property: JsonPropertyName("text")] string Text,
        [property: JsonPropertyName("to")] string To);
}
