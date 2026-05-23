using System;
using System.Threading;
using System.Threading.Tasks;

namespace TextToSpeech.Core;

/// <summary>
/// UI-agnostic orchestrator that the WPF app drives. Owns input validation, the chosen voice,
/// and the call to the underlying <see cref="ISpeechSynthesizer"/>. Keeping this in
/// <c>TextToSpeech.Core</c> (instead of in code-behind) is what makes the WPF app testable
/// from cross-platform integration tests using a mocked synthesizer.
/// </summary>
public sealed class TextToSpeechService
{
    private readonly ISpeechSynthesizer _synthesizer;

    public TextToSpeechService(ISpeechSynthesizer synthesizer)
    {
        _synthesizer = synthesizer ?? throw new ArgumentNullException(nameof(synthesizer));
    }

    /// <summary>
    /// Speak <paramref name="text"/> in Serbian using <paramref name="voiceName"/> (or the
    /// default Serbian voice when null/empty).
    /// </summary>
    public Task<SpeechSynthesisResult> SpeakAsync(
        string text,
        string? voiceName = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Task.FromResult(SpeechSynthesisResult.Fail(
                "Please enter some text to speak."));
        }

        var request = new SpeechSynthesisRequest
        {
            Text = text,
            Language = SerbianVoices.LanguageCode,
            VoiceName = string.IsNullOrWhiteSpace(voiceName)
                ? SerbianVoices.DefaultVoice
                : voiceName,
        };

        return _synthesizer.SynthesizeAsync(request, cancellationToken);
    }
}
