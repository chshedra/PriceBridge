public record OzonProductDto(
    string Sku,
    string? Title,
    string? Brand,
    List<string>? Category,
    Dictionary<string, string>? Characteristics,
    decimal? Price,
    decimal? OriginalPrice,
    List<string>? Images,
    string? Availability,
    string Url
);
