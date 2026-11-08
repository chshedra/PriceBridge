namespace PriceBridge.Service;

/// <summary>
/// Localizes texts coming from marketplaces into English.
/// </summary>
/// <param name="translationService">The translation service.</param>
public class LocalizationService(AlibabaTranslationService translationService)
{
    /// <summary>
    /// Translates Chinese texts into English. Returns null for texts that could not be translated.
    /// </summary>
    public async Task<List<string?>> ToEnglish(IReadOnlyList<string?> texts)
    {
        var result = new string?[texts.Count];
        var toTranslate = texts
            .Select((text, index) => (text, index))
            .Where(x => !string.IsNullOrWhiteSpace(x.text))
            .ToList();

        if (toTranslate.Count == 0)
            return [.. result];

        try
        {
            var translated = await translationService.Translate(
                toTranslate.Select(x => x.text!).ToList(),
                sourceLanguage: "zh",
                targetLanguage: "en"
            );

            for (var i = 0; i < toTranslate.Count; i++)
                result[toTranslate[i].index] = translated[i];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to translate {toTranslate.Count} texts: {ex.Message}");
        }

        return [.. result];
    }
}
