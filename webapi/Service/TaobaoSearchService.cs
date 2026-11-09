using Microsoft.EntityFrameworkCore;
using PriceBridge.Entities;

namespace PriceBridge.Service;

/// <summary>
/// Service for searching Taobao products based on Ozon product information.
/// </summary>
/// <param name="db">The database context.</param>
/// <param name="taobaoClient">The Taobao client.</param>
/// <param name="localizationService">The localization service.</param>
/// <param name="searchQueryBuilder">The keyword query builder.</param>
/// <param name="configuration">The configuration.</param>
public class TaobaoSearchService(
    AppDbContext db,
    TaobaoClient taobaoClient,
    LocalizationService localizationService,
    SearchQueryBuilder searchQueryBuilder,
    IConfiguration configuration
)
{
    // Keyword results outside this share of the Ozon price are mostly accessories or other products
    private const decimal MinPriceShare = 0.1m;
    private const decimal MaxPriceShare = 1.2m;

    private readonly TimeSpan _cacheDuration =
        configuration.GetValue<TimeSpan?>("Taobao:CacheDuration") ?? TimeSpan.FromHours(24);

    // Approximate rate, only used to narrow the keyword search price range; no filter when unset
    private readonly decimal? _rubToCnyRate = configuration.GetValue<decimal?>(
        "Taobao:RubToCnyRate"
    );

    public async Task<List<TaobaoItemDto>> FindSimilar(OzonProductDto product)
    {
        var ozonProduct = await db
            .OzonProducts.Include(p => p.TaobaoProducts)
            .FirstOrDefaultAsync(p => p.Sku == product.Sku);

        if (ozonProduct != null && DateTime.UtcNow - ozonProduct.TaobaoFetchedAt < _cacheDuration)
        {
            Console.WriteLine(
                $"Serving {ozonProduct.TaobaoProducts.Count} Taobao items for SKU {product.Sku} from DB"
            );
            await TranslateMissingTitles(ozonProduct.TaobaoProducts);
            return ozonProduct.TaobaoProducts.Select(ToDto).ToList();
        }

        var searchQuery = await searchQueryBuilder.Build(product);
        var image = product.Images?.FirstOrDefault();

        var imageSearch =
            image != null
                ? taobaoClient.SearchByImage(image)
                : Task.FromResult(new List<TaobaoItemDto>());
        var keywordSearch = SearchByKeyword(searchQuery, product.Price);
        await Task.WhenAll(imageSearch, keywordSearch);

        var (keywordItems, usedQuery) = keywordSearch.Result;
        var items = Merge(imageSearch.Result, keywordItems);
        Console.WriteLine(
            $"Fetched {items.Count} Taobao items for SKU {product.Sku} from API "
                + $"(image: {imageSearch.Result.Count}, keyword: {keywordItems.Count}, query: '{usedQuery}')"
        );

        var translatedTitles = await localizationService.ToEnglish(
            items.Select(i => i.Title).ToList()
        );
        items = items
            .Select((item, i) => item with { TranslatedTitle = translatedTitles[i] })
            .ToList();

        if (ozonProduct == null)
        {
            ozonProduct = new OzonProduct { Sku = product.Sku, Url = product.Url };
            db.OzonProducts.Add(ozonProduct);
        }

        ozonProduct.Title = product.Title;
        ozonProduct.Brand = product.Brand;
        ozonProduct.Category = product.Category;
        ozonProduct.Characteristics = product.Characteristics;
        ozonProduct.Price = product.Price;
        ozonProduct.OriginalPrice = product.OriginalPrice;
        ozonProduct.ImageUrl = image;
        ozonProduct.Url = product.Url;
        ozonProduct.SearchQuery = usedQuery;
        ozonProduct.TaobaoFetchedAt = DateTime.UtcNow;

        db.TaobaoProducts.RemoveRange(ozonProduct.TaobaoProducts);
        ozonProduct.TaobaoProducts = items.Select(ToEntity).ToList();

        await db.SaveChangesAsync();

        return items;
    }

    private async Task<(List<TaobaoItemDto> Items, string? Query)> SearchByKeyword(
        SearchQuery query,
        decimal? ozonPrice
    )
    {
        decimal? startPrice = null,
            endPrice = null;
        if (ozonPrice != null && _rubToCnyRate != null)
        {
            var cnyPrice = ozonPrice.Value * _rubToCnyRate.Value;
            (startPrice, endPrice) = (cnyPrice * MinPriceShare, cnyPrice * MaxPriceShare);
        }

        // Taobao keyword search is strict, so a specific query often finds nothing
        foreach (var q in new[] { query.Primary, query.Fallback })
        {
            if (string.IsNullOrWhiteSpace(q))
                continue;

            var items = await taobaoClient.SearchByKeyword(q, startPrice, endPrice);
            if (items.Count > 0)
                return (items, q);
        }

        return ([], query.Primary ?? query.Fallback);
    }

    // Items found by both searches go first, then image and keyword results alternate
    private static List<TaobaoItemDto> Merge(
        List<TaobaoItemDto> imageItems,
        List<TaobaoItemDto> keywordItems
    )
    {
        static string Key(TaobaoItemDto item) => item.ItemId ?? item.Url;

        var keywordKeys = keywordItems.Select(Key).ToHashSet();
        var both = imageItems
            .Where(i => keywordKeys.Contains(Key(i)))
            .Select(i => i with { MatchSource = "both" });
        var imageOnly = imageItems
            .Where(i => !keywordKeys.Contains(Key(i)))
            .Select(i => i with { MatchSource = "image" })
            .ToList();
        var imageKeys = imageItems.Select(Key).ToHashSet();
        var keywordOnly = keywordItems
            .Where(i => !imageKeys.Contains(Key(i)))
            .Select(i => i with { MatchSource = "keyword" })
            .ToList();

        var interleaved = Enumerable
            .Range(0, Math.Max(imageOnly.Count, keywordOnly.Count))
            .SelectMany(i =>
                new[] { imageOnly.ElementAtOrDefault(i), keywordOnly.ElementAtOrDefault(i) }
            )
            .OfType<TaobaoItemDto>();

        return both.Concat(interleaved).DistinctBy(Key).ToList();
    }

    // Products cached before translation was introduced (or when it failed) have no translated title
    private async Task TranslateMissingTitles(ICollection<TaobaoProduct> products)
    {
        var untranslated = products
            .Where(p => p.TranslatedTitle == null && !string.IsNullOrWhiteSpace(p.Title))
            .ToList();

        if (untranslated.Count == 0)
            return;

        var translatedTitles = await localizationService.ToEnglish(
            untranslated.Select(p => p.Title).ToList()
        );

        for (var i = 0; i < untranslated.Count; i++)
            untranslated[i].TranslatedTitle = translatedTitles[i];

        await db.SaveChangesAsync();
    }

    private static TaobaoItemDto ToDto(TaobaoProduct t) =>
        new(
            ItemId: t.ItemId,
            Title: t.Title,
            TranslatedTitle: t.TranslatedTitle,
            Price: t.Price,
            Currency: t.Currency,
            Image: t.ImageUrl,
            Url: t.ItemUrl,
            MatchSource: t.MatchSource
        );

    private static TaobaoProduct ToEntity(TaobaoItemDto item) =>
        new()
        {
            ItemId = item.ItemId,
            Title = item.Title,
            TranslatedTitle = item.TranslatedTitle,
            Price = item.Price,
            Currency = item.Currency,
            ImageUrl = item.Image,
            ItemUrl = item.Url,
            MatchSource = item.MatchSource,
        };
}
