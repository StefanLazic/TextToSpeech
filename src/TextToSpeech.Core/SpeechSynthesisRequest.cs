namespace TextToSpeech.Core;

/// <summary>
/// Input for a single text-to-speech synthesis call.
/// </summary>
public sealed class SpeechSynthesisRequest
{
    /// <summary>The text to synthesize. Must be non-empty.</summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>BCP-47 voice language. Defaults to Serbian (Serbia).</summary>
    public string Language { get; init; } = SerbianVoices.LanguageCode;

    /// <summary>
    /// Short voice name (e.g. <c>sr-RS-SophieNeural</c>). When null, the synthesizer
    /// uses the service default for the chosen <see cref="Language"/>.
    /// </summary>
    public string? VoiceName { get; init; }
}
