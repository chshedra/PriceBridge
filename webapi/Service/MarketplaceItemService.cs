using System.Text.Json;

namespace PriceBridge.Service;

public class MarketplaceItemService(
    LinkProcessor linkProcessor,
    LocalizationService localizationService
)
{
    public async Task<MarketplaceItemDto> GetMarketplaceItems(string url)
    {
        OzonItemDto ozonItem = await linkProcessor.ProcessLink(url);

        Console.WriteLine(
            $"Extracted OzonItemDto: Name = {ozonItem.Name}, Prices = {ozonItem.Prices}"
        );

        return new MarketplaceItemDto(
            Id: "1",
            Name: "Sample Item",
            Description: "This is a sample item.",
            Price: 9.99m,
            Currency: "USD",
            ImageUrl: "https://example.com/sample-item.jpg",
            Category: "Sample Category"
        );
    }
}
