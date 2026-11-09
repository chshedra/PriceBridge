using System.Text.RegularExpressions;

namespace PriceBridge.Service;

/// <summary>
/// Chinese keyword queries for Taobao. Primary is the most specific one; Fallback is a
/// shorter query (brand + category) to use when the primary one finds nothing.
/// </summary>
public record SearchQuery(string? Primary, string? Fallback);

/// <summary>
/// Builds Taobao keyword queries from Ozon product information.
/// </summary>
/// <param name="localizationService">The localization service.</param>
public partial class SearchQueryBuilder(LocalizationService localizationService)
{
    private const int MaxAttributes = 2;

    // Values that carry no meaning without their key ("Наличие микрофона: Да")
    private static readonly HashSet<string> MeaninglessValues = new(
        ["да", "нет", "есть"],
        StringComparer.OrdinalIgnoreCase
    );

    public async Task<SearchQuery> Build(OzonProductDto product)
    {
        var categoryLeaf = product.Category?.LastOrDefault();
        var attributes = GetAttributes(product.Characteristics, categoryLeaf);

        // Machine translation handles a natural phrase much better than single words:
        // "наушники полноразмерные, активное шумоподавление" => "全尺寸耳机,主动降噪"
        var phrase = string.Join(", ", new[] { categoryLeaf?.ToLower() }.Concat(attributes))
            .Trim(' ', ',');

        var translated = await localizationService.ToChinese([product.Brand, categoryLeaf, phrase]);
        var (brand, category, description) = (
            // Brand is translated too: "Xiaomi" => "小米", which is what Taobao titles use
            Clean(translated[0]) ?? product.Brand,
            Clean(translated[1]),
            Clean(translated[2])
        );

        var primary = JoinQuery(brand, description);
        var fallback = JoinQuery(brand, category);

        return new SearchQuery(primary, fallback == primary ? null : fallback);
    }

    private static List<string> GetAttributes(
        Dictionary<string, string>? characteristics,
        string? categoryLeaf
    ) =>
        (characteristics ?? [])
            .Where(c =>
                !MeaninglessValues.Contains(c.Value)
                && !string.Equals(c.Value, categoryLeaf, StringComparison.OrdinalIgnoreCase)
            )
            // A single-word key names the feature itself ("Шумоподавление"), so it's needed
            // for context: "активное шумоподавление". A multi-word key is just a label
            // ("Конструкция наушников"), and the value alone reads better: "полноразмерные"
            .Select(c =>
                c.Key.Contains(' ') ? c.Value.ToLower() : $"{c.Value.ToLower()} {c.Key.ToLower()}"
            )
            .Take(MaxAttributes)
            .ToList();

    // Taobao keyword search is strict, so punctuation the translation adds only hurts
    private static string? Clean(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var cleaned = Punctuation().Replace(text, " ");
        return Whitespace().Replace(cleaned, " ").Trim() is { Length: > 0 } t ? t : null;
    }

    private static string? JoinQuery(params string?[] parts) =>
        string.Join(' ', parts.Where(p => !string.IsNullOrWhiteSpace(p))) is { Length: > 0 } q
            ? q
            : null;

    [GeneratedRegex(@"[\p{P}，。：；、！？]")]
    private static partial Regex Punctuation();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
