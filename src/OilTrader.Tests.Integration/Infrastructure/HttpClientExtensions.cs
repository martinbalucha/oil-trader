using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace OilTrader.Tests.Integration.Infrastructure;

public static class HttpClientExtensions
{
    public const string TickEndpoint = "/api/v1/tick";

    public static Task<HttpResponseMessage> PostTickAsync(
        this HttpClient client,
        string json,
        CancellationToken ct = default) =>
        client.PostAsync(TickEndpoint, TickTestPayloads.JsonContent(json), ct);

    public static Task<HttpResponseMessage> PostTickAsync(
        this HttpClient client,
        StringContent content,
        CancellationToken ct = default) =>
        client.PostAsync(TickEndpoint, content, ct);

    public static async Task<ValidationProblemDetails?> ReadValidationProblemAsync(
        this HttpResponseMessage response,
        CancellationToken ct = default)
    {
        return await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(
            cancellationToken: ct);
    }

    public static async Task<ProblemDetails?> ReadProblemDetailsAsync(
        this HttpResponseMessage response,
        CancellationToken ct = default)
    {
        return await response.Content.ReadFromJsonAsync<ProblemDetails>(
            cancellationToken: ct);
    }

    public static async Task<string?> ReadDetailAsync(
        this HttpResponseMessage response,
        CancellationToken ct = default)
    {
        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(ct),
            cancellationToken: ct);
        return document.RootElement.TryGetProperty("detail", out var detail)
            ? detail.GetString()
            : null;
    }

    public static bool HasValidationErrorFor(
        this ValidationProblemDetails problem,
        string fieldName) =>
        problem.Errors.ContainsKey(fieldName) && problem.Errors[fieldName].Length > 0;
}
