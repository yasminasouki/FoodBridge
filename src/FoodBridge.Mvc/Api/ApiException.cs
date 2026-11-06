using System.Text.Json;

namespace FoodBridge.Mvc.Api;

public sealed class ApiException : Exception
{
    public ApiException(int statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }

    public static async Task<ApiException> FromAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return new ApiException((int)response.StatusCode, ReadMessage(body));
    }

    private static string ReadMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return "The request failed.";
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (root.TryGetProperty("errors", out var errors))
            {
                foreach (var property in errors.EnumerateObject())
                {
                    foreach (var item in property.Value.EnumerateArray())
                    {
                        var text = item.GetString();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            return text;
                        }
                    }
                }
            }

            if (root.TryGetProperty("detail", out var detail) && detail.GetString() is string detailText)
            {
                return detailText;
            }

            if (root.TryGetProperty("title", out var title) && title.GetString() is string titleText)
            {
                return titleText;
            }
        }
        catch (JsonException)
        {
            return body;
        }

        return "The request failed.";
    }
}
