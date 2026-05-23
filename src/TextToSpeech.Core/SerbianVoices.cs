namespace TextToSpeech.Core;

/// <summary>
/// Catalog of the Serbian (sr-RS) neural voices supported by Azure AI Speech.
/// See https://learn.microsoft.com/azure/ai-services/speech-service/language-support
/// </summary>
public static class SerbianVoices
{
    /// <summary>BCP-47 code for Serbian (Serbia).</summary>
    public const string LanguageCode = "sr-RS";

    /// <summary>Female neural voice.</summary>
    public const string SophieNeural = "sr-RS-SophieNeural";

    /// <summary>Male neural voice.</summary>
    public const string NicholasNeural = "sr-RS-NicholasNeural";

    /// <summary>Default voice used by the app when none is selected.</summary>
    public const string DefaultVoice = SophieNeural;

    /// <summary>All supported voices, in display order.</summary>
    public static System.Collections.Generic.IReadOnlyList<string> All { get; } =
        new[] { SophieNeural, NicholasNeural };
}
