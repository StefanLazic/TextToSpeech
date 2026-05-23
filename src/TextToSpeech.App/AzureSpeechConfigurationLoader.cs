using System;
using System.IO;
using TextToSpeech.Core;

namespace TextToSpeech.App;

/// <summary>
/// Loads <see cref="AzureSpeechOptions"/> from (in order of precedence):
/// <list type="number">
///   <item>environment variables <c>AZURE_SPEECH_KEY</c> and <c>AZURE_SPEECH_REGION</c>;</item>
///   <item>a JSON file <c>appsettings.json</c> next to the executable containing
///         <c>{ "AzureSpeech": { "SubscriptionKey": "...", "Region": "..." } }</c>.</item>
/// </list>
/// The key is intentionally not committed to source control — see README for setup steps.
/// </summary>
internal static class AzureSpeechConfigurationLoader
{
    public static AzureSpeechOptions Load()
    {
        var key = Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY");
        var region = Environment.GetEnvironmentVariable("AZURE_SPEECH_REGION");

        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(region))
        {
            var fileSettings = TryLoadFromFile();
            key ??= fileSettings.Key;
            region ??= fileSettings.Region;
        }

        var options = new AzureSpeechOptions
        {
            SubscriptionKey = key ?? string.Empty,
            Region = region ?? string.Empty,
        };
        options.Validate();
        return options;
    }

    private static (string? Key, string? Region) TryLoadFromFile()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
        {
            return (null, null);
        }

        try
        {
            using var stream = File.OpenRead(path);
            using var doc = System.Text.Json.JsonDocument.Parse(stream);
            if (!doc.RootElement.TryGetProperty("AzureSpeech", out var section))
            {
                return (null, null);
            }

            var key = section.TryGetProperty("SubscriptionKey", out var k) ? k.GetString() : null;
            var region = section.TryGetProperty("Region", out var r) ? r.GetString() : null;
            return (key, region);
        }
        catch
        {
            // A malformed file should not crash the UI; the loader will surface a missing-key
            // error to the user instead.
            return (null, null);
        }
    }
}
