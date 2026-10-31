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
            ([FromBody] OzonProductDto product) =>
                Console.WriteLine(
                    $"Received product: {product.Sku}, {product.Title}, {product.Price}, {product.OriginalPrice}, {product.Image}, {product.Availability}, {product.Url}"
                )
        );
    }
}
