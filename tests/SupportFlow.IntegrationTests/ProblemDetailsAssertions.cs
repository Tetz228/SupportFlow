using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace SupportFlow.IntegrationTests;

internal static class ProblemDetailsAssertions
{
    private const string TraceIdPropertyName = "traceId";

    internal static void HasTraceId(ProblemDetails problemDetails)
    {
        Assert.True(problemDetails.Extensions.TryGetValue(TraceIdPropertyName, out var traceIdValue));

        var traceId = traceIdValue switch
        {
            string value => value,
            JsonElement { ValueKind: JsonValueKind.String } element => element.GetString(),
            _ => null
        };

        Assert.False(string.IsNullOrWhiteSpace(traceId));
    }
}
