using System.Text.Json;

namespace PriceBridge.Service;

public class MarketplaceItemService(LocalizationService localizationService)
{
    public async Task<MarketplaceItemDto> GetMarketplaceItems(string url)
    {
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
