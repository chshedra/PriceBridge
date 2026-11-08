using Microsoft.EntityFrameworkCore;
using PriceBridge.Entities;

namespace PriceBridge.Service;

/// <summary>
/// Service for searching Taobao products based on Ozon product information.
/// </summary>
/// <param name="db">The database context.</param>
/// <param name="taobaoClient">The Taobao client.</param>
/// <param name="localizationService">The localization service.</param>
/// <param name="configuration">The configuration.</param>
public class TaobaoSearchService(
    AppDbContext db,
    TaobaoClient taobaoClient,
    LocalizationService localizationService,
    IConfiguration configuration
)
{
    private readonly TimeSpan _cacheDuration =
        configuration.GetValue<TimeSpan?>("Taobao:CacheDuration") ?? TimeSpan.FromHours(24);

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

        var items = await taobaoClient.SearchByImage(product.Image!);
        Console.WriteLine($"Fetched {items.Count} Taobao items for SKU {product.Sku} from API");

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
        ozonProduct.Price = product.Price;
        ozonProduct.OriginalPrice = product.OriginalPrice;
        ozonProduct.ImageUrl = product.Image;
        ozonProduct.Url = product.Url;
        ozonProduct.TaobaoFetchedAt = DateTime.UtcNow;

        db.TaobaoProducts.RemoveRange(ozonProduct.TaobaoProducts);
        ozonProduct.TaobaoProducts = items.Select(ToEntity).ToList();

        await db.SaveChangesAsync();

        return items;
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
            Url: t.ItemUrl
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
        };
}
