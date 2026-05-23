using System.Threading;
using System.Threading.Tasks;

namespace TextToSpeech.Core;

/// <summary>
/// Abstraction over a text-to-speech engine. Implemented by <see cref="AzureSpeechSynthesizer"/>
/// in production, and mocked in tests so the suite never has to call the real Azure service.
/// </summary>
public interface ISpeechSynthesizer
{
    /// <summary>
    /// Synthesize <paramref name="request"/> and return the audio bytes.
    /// Implementations must not throw for empty input — they should return a failed
    /// <see cref="SpeechSynthesisResult"/> instead, so the UI can show a friendly message.
    /// </summary>
    Task<SpeechSynthesisResult> SynthesizeAsync(
        SpeechSynthesisRequest request,
        CancellationToken cancellationToken = default);
}
