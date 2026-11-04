using Microsoft.EntityFrameworkCore;
using PriceBridge.Entities;

namespace PriceBridge.Service;

/// <summary>
/// Service for searching Taobao products based on Ozon product information.
/// </summary>
/// <param name="db">The database context.</param>
/// <param name="taobaoClient">The Taobao client.</param>
/// <param name="configuration">The configuration.</param>
public class TaobaoSearchService(
    AppDbContext db,
    TaobaoClient taobaoClient,
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
            return ozonProduct.TaobaoProducts.Select(ToDto).ToList();
        }

        var items = await taobaoClient.SearchByImage(product.Image!);
        Console.WriteLine($"Fetched {items.Count} Taobao items for SKU {product.Sku} from API");

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

    private static TaobaoItemDto ToDto(TaobaoProduct t) =>
        new(
            ItemId: t.ItemId,
            Title: t.Title,
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
            Price = item.Price,
            Currency = item.Currency,
            ImageUrl = item.Image,
            ItemUrl = item.Url,
        };
}
