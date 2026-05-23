namespace TextToSpeech.Core;

/// <summary>
/// Azure Speech service connection settings.
/// </summary>
public sealed class AzureSpeechOptions
{
    /// <summary>The subscription key for the Azure Speech resource.</summary>
    public string SubscriptionKey { get; init; } = string.Empty;

    /// <summary>The Azure region (e.g. <c>westeurope</c>) the resource lives in.</summary>
    public string Region { get; init; } = string.Empty;

    /// <summary>Default voice used when a request does not specify one.</summary>
    public string DefaultVoiceName { get; init; } = SerbianVoices.DefaultVoice;

    /// <summary>Default language used when a request does not specify one.</summary>
    public string DefaultLanguage { get; init; } = SerbianVoices.LanguageCode;

    /// <summary>
    /// Validates that required fields are set. Throws <see cref="System.InvalidOperationException"/>
    /// if any required field is missing.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SubscriptionKey))
        {
            throw new System.InvalidOperationException(
                "Azure Speech subscription key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(Region))
        {
            throw new System.InvalidOperationException(
                "Azure Speech region is not configured.");
        }
    }
}
