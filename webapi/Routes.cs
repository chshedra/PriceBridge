using Microsoft.AspNetCore.Mvc;
using PriceBridge.Service;

namespace PriceBridge;

public static class Routes
{
    public static void MapApiRoutes(this IEndpointRouteBuilder builder)
    {
        builder.MapGet(
            "/api/items/{url}",
            (
                [FromRoute] string url,
                [FromServices] MarketplaceItemService marketplaceItemService
            ) => marketplaceItemService.GetMarketplaceItems(url)
        );

        builder.MapPost(
            "/api/items",
            async (
                [FromBody] OzonProductDto product,
                [FromServices] TaobaoSearchService taobaoSearchService
            ) =>
            {
                Console.WriteLine(
                    $"Received product: {product.Sku}, {product.Title}, {product.Price}, {product.OriginalPrice}, {string.Join(", ", product.Images ?? [])}, {product.Availability}, {product.Url}"
                );

                if (product.Images is not { Count: > 0 } && product.Category is not { Count: > 0 })
                    return Results.BadRequest("Product image or category is required");

                var items = await taobaoSearchService.FindSimilar(product);
                Console.WriteLine(
                    $"Found {items.Count} Taobao items, top: {items.FirstOrDefault()?.Url}"
                );

                return Results.Ok(items);
            }
        );
    }
}
