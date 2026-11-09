using System.Globalization;
using System.Text.Json;

namespace PriceBridge.Service;

public class TaobaoClient(HttpClient httpClient)
{
    // item_search_image_x returns only encrypted ids, so use the endpoint that includes itemUrl
    public Task<List<TaobaoItemDto>> SearchByImage(string imageUrl, int pageSize = 20) =>
        Search($"item_search_image_2?imgUrl={Uri.EscapeDataString(imageUrl)}&pageSize={pageSize}");

    public Task<List<TaobaoItemDto>> SearchByKeyword(
        string query,
        decimal? startPrice = null,
        decimal? endPrice = null
    )
    {
        var url = $"item_search?q={Uri.EscapeDataString(query)}&page=1";
        if (startPrice != null)
            url += $"&startPrice={startPrice.Value.ToString("0", CultureInfo.InvariantCulture)}";
        if (endPrice != null)
            url += $"&endPrice={endPrice.Value.ToString("0", CultureInfo.InvariantCulture)}";

        return Search(url);
    }

    private async Task<List<TaobaoItemDto>> Search(string query)
    {
        using var response = await httpClient.GetAsync(query);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Console.WriteLine($"Taobao search response: {json}");

        var result = json.GetProperty("result");

        // Errors such as "no results" or "unsupported image format" still come back as HTTP 200
        if (result.TryGetProperty("status", out var status) && GetString(status, "data") == "error")
        {
            Console.WriteLine(
                $"Taobao {query.Split('?')[0]} returned error: {(status.TryGetProperty("msg", out var msg) ? msg : status)}"
            );
            return [];
        }

        var currency = result.TryGetProperty("settings", out var settings)
            ? GetString(settings, "currency") ?? "CNY"
            : "CNY";

        if (!result.TryGetProperty("resultList", out var resultList))
            return [];

        return resultList
            .EnumerateArray()
            .Select(entry => entry.GetProperty("item"))
            .Select(item => new TaobaoItemDto(
                ItemId: GetString(item, "itemId"),
                Title: GetString(item, "title"),
                TranslatedTitle: null,
                Price: GetPrice(item),
                Currency: currency,
                Image: ToAbsoluteUrl(GetString(item, "image")),
                Url: ToAbsoluteUrl(GetString(item, "itemUrl"))
                    ?? $"https://item.taobao.com/item.htm?id={GetString(item, "itemId")}"
            ))
            .ToList();
    }

    private static string? GetString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.ToString()
            : null;

    private static decimal? GetPrice(JsonElement item)
    {
        if (!item.TryGetProperty("sku", out var sku) || !sku.TryGetProperty("def", out var def))
            return null;

        var price = GetString(def, "promotionPrice") ?? GetString(def, "price");
        return decimal.TryParse(price, NumberStyles.Number, CultureInfo.InvariantCulture, out var p)
            ? p
            : null;
    }

    private static string? ToAbsoluteUrl(string? url) =>
        url?.StartsWith("//") == true ? $"https:{url}" : url;
}
