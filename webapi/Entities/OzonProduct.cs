namespace PriceBridge.Entities;

public class OzonProduct
{
    public int Id { get; set; }
    public required string Sku { get; set; }
    public string? Title { get; set; }
    public decimal? Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string? ImageUrl { get; set; }
    public required string Url { get; set; }

    // When TaobaoProducts were last fetched from the API; used to expire the cache
    public DateTime TaobaoFetchedAt { get; set; }

    public List<TaobaoProduct> TaobaoProducts { get; set; } = [];
}
