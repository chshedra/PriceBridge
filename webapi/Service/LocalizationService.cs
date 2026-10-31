namespace PriceBridge.Service;

public class LocalizationService
{
    public async Task<string> GetLocalizedString(string key, string language)
    {
        // Simulate fetching a localized string from an external service
        await Task.Delay(100); // Simulate network delay

        // For demonstration purposes, return a hardcoded localized string
        return $"Localized string for key '{key}' in language '{language}'";
    }
}
