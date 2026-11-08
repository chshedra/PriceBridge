using System.Text.Json;
using AlibabaCloud.SDK.Alimt20181012;
using AlibabaCloud.SDK.Alimt20181012.Models;

namespace PriceBridge.Service;

/// <summary>
/// Translates texts using Alibaba Cloud Machine Translation (e-commerce model).
/// </summary>
/// <param name="client">The Alibaba Cloud Machine Translation client.</param>
public class AlibabaTranslationService(Client client)
{
    // GetBatchTranslate accepts at most 50 texts per request
    private const int BatchSize = 50;

    public async Task<List<string?>> Translate(
        IReadOnlyList<string> texts,
        string sourceLanguage,
        string targetLanguage
    )
    {
        var result = new List<string?>(texts.Count);

        foreach (var chunk in texts.Chunk(BatchSize))
        {
            var sourceText = chunk
                .Select((text, index) => (text, index))
                .ToDictionary(x => x.index.ToString(), x => x.text);

            var response = await client.GetBatchTranslateAsync(
                new GetBatchTranslateRequest
                {
                    ApiType = "translate_ecommerce",
                    Scene = "title",
                    FormatType = "text",
                    SourceLanguage = sourceLanguage,
                    TargetLanguage = targetLanguage,
                    SourceText = JsonSerializer.Serialize(sourceText),
                }
            );

            var translated = new string?[chunk.Length];
            foreach (var entry in response.Body.TranslatedList ?? [])
            {
                if (
                    entry.TryGetValue("index", out var index)
                    && int.TryParse(index?.ToString(), out var i)
                    && i >= 0
                    && i < translated.Length
                    && entry.TryGetValue("translated", out var text)
                )
                {
                    translated[i] = text?.ToString();
                }
            }

            result.AddRange(translated);
        }

        return result;
    }
}
