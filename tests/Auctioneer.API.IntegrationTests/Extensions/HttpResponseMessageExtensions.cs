using System.Net;
using System.Text.Json;


namespace Auctioneer.API.IntegrationTests.Extensions;

public static class HttpResponseMessageExtensions
{
    public static async Task<HttpResponse<T>> DeserializeResponseAsync<T>(this Task<HttpResponseMessage> message)
    {
        var messageValue = await message;
        return await messageValue.DeserializeResponseAsync<T>();
    }

    private static async Task<HttpResponse<T>> DeserializeResponseAsync<T>(this HttpResponseMessage message)
    {
        var stringResult = await message.Content.ReadAsStringAsync();
        var value = JsonSerializer.Deserialize<T>(stringResult);
        return new HttpResponse<T>
        {
            IsSuccess = message.IsSuccessStatusCode,
            StatusCode = message.StatusCode,
            Value = value
        };
    }
}

public class HttpResponse<T>
{
    public bool IsSuccess { get; init; }
    public HttpStatusCode StatusCode { get; init; }
    public T? Value { get; init; }
}