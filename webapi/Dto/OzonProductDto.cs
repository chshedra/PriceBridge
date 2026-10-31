public record OzonProductDto(
    string Sku,
    string? Title,
    decimal? Price,
    decimal? OriginalPrice,
    string? Image,
    string? Availability,
    string Url
);
