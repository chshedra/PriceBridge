namespace PriceBridge.Entities;

public class TaobaoProduct
{
    public int Id { get; set; }
    public string? ItemId { get; set; }
    public string? Title { get; set; }
    public string? TranslatedTitle { get; set; }
    public decimal? Price { get; set; }
    public required string Currency { get; set; }
    public string? ImageUrl { get; set; }
    public required string ItemUrl { get; set; }
    public string? MatchSource { get; set; }

    public int OzonProductId { get; set; }
    public OzonProduct OzonProduct { get; set; } = null!;
}
