public record TaobaoItemDto(
    string? ItemId,
    string? Title,
    string? TranslatedTitle,
    decimal? Price,
    string Currency,
    string? Image,
    string Url,
    string? MatchSource = null
);
