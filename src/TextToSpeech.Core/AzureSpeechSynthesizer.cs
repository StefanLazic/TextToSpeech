using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;

namespace TextToSpeech.Core;

/// <summary>
/// <see cref="ISpeechSynthesizer"/> implementation backed by the Microsoft Cognitive Services
/// Speech SDK. Returns the synthesized audio as a byte array so the caller (WPF app or tests)
/// can play it, save it to a file, or assert on it without coupling to the SDK.
/// </summary>
/// <remarks>
/// The class is intentionally thin: it only translates a <see cref="SpeechSynthesisRequest"/>
/// into Azure SDK calls and maps the SDK result back into a <see cref="SpeechSynthesisResult"/>.
/// Anything that actually contacts Azure is therefore covered by manual / end-to-end testing
/// (it requires a real subscription key), while the rest of the codebase is tested against
/// the <see cref="ISpeechSynthesizer"/> abstraction using mocks.
/// </remarks>
public sealed class AzureSpeechSynthesizer : ISpeechSynthesizer
{
    private readonly AzureSpeechOptions _options;

    public AzureSpeechSynthesizer(AzureSpeechOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();
    }

    /// <inheritdoc />
    public async Task<SpeechSynthesisResult> SynthesizeAsync(
        SpeechSynthesisRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return SpeechSynthesisResult.Fail("Request must not be null.");
        }

        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return SpeechSynthesisResult.Fail("Text to synthesize must not be empty.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var config = SpeechConfig.FromSubscription(_options.SubscriptionKey, _options.Region);
        config.SpeechSynthesisLanguage = string.IsNullOrWhiteSpace(request.Language)
            ? _options.DefaultLanguage
            : request.Language;
        config.SpeechSynthesisVoiceName = string.IsNullOrWhiteSpace(request.VoiceName)
            ? _options.DefaultVoiceName
            : request.VoiceName;

        try
        {
            // Passing `null` for the AudioConfig keeps the audio in memory instead of
            // streaming to the default speaker device, which is what we want so the WPF
            // app can play it on its own terms (and so tests don't try to open audio devices).
            using var synthesizer = new SpeechSynthesizer(config, audioConfig: null);
            using var result = await synthesizer.SpeakTextAsync(request.Text).ConfigureAwait(false);

            return result.Reason switch
            {
                ResultReason.SynthesizingAudioCompleted => SpeechSynthesisResult.Ok(result.AudioData),
                ResultReason.Canceled => SpeechSynthesisResult.Fail(
                    SpeechSynthesisCancellationDetails.FromResult(result).ErrorDetails
                    ?? "Synthesis was cancelled."),
                _ => SpeechSynthesisResult.Fail($"Unexpected synthesis result: {result.Reason}."),
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return SpeechSynthesisResult.Fail(ex.Message);
        }
    }
}

