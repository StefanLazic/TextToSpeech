using System.Threading;
using System.Threading.Tasks;
using Moq;
using TextToSpeech.Core;
using Xunit;

namespace TextToSpeech.IntegrationTests;

/// <summary>
/// End-to-end tests for <see cref="TextToSpeechService"/>. The Azure SDK is replaced with
/// a Moq-based <see cref="ISpeechSynthesizer"/>, so the suite runs offline and does not
/// require an Azure subscription key.
/// </summary>
public class TextToSpeechServiceIntegrationTests
{
    [Fact]
    public async Task SpeakAsync_passes_text_and_default_Serbian_voice_to_synthesizer()
    {
        SpeechSynthesisRequest? captured = null;
        var mock = new Mock<ISpeechSynthesizer>();
        mock.Setup(s => s.SynthesizeAsync(It.IsAny<SpeechSynthesisRequest>(), It.IsAny<CancellationToken>()))
            .Callback<SpeechSynthesisRequest, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(SpeechSynthesisResult.Ok(new byte[] { 1, 2, 3, 4 }));

        var service = new TextToSpeechService(mock.Object);

        var result = await service.SpeakAsync("Здраво, свете!");

        Assert.True(result.Success);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, result.AudioData);
        Assert.NotNull(captured);
        Assert.Equal("Здраво, свете!", captured!.Text);
        Assert.Equal(SerbianVoices.LanguageCode, captured.Language);
        Assert.Equal(SerbianVoices.DefaultVoice, captured.VoiceName);
        mock.Verify(s => s.SynthesizeAsync(It.IsAny<SpeechSynthesisRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SpeakAsync_uses_explicitly_selected_voice()
    {
        SpeechSynthesisRequest? captured = null;
        var mock = new Mock<ISpeechSynthesizer>();
        mock.Setup(s => s.SynthesizeAsync(It.IsAny<SpeechSynthesisRequest>(), It.IsAny<CancellationToken>()))
            .Callback<SpeechSynthesisRequest, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(SpeechSynthesisResult.Ok(new byte[] { 9 }));

        var service = new TextToSpeechService(mock.Object);

        await service.SpeakAsync("Добар дан", SerbianVoices.NicholasNeural);

        Assert.NotNull(captured);
        Assert.Equal(SerbianVoices.NicholasNeural, captured!.VoiceName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SpeakAsync_short_circuits_on_empty_text_without_calling_synthesizer(string? text)
    {
        var mock = new Mock<ISpeechSynthesizer>(MockBehavior.Strict);
        var service = new TextToSpeechService(mock.Object);

        var result = await service.SpeakAsync(text!);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        mock.Verify(
            s => s.SynthesizeAsync(It.IsAny<SpeechSynthesisRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SpeakAsync_propagates_synthesizer_failure_to_caller()
    {
        var mock = new Mock<ISpeechSynthesizer>();
        mock.Setup(s => s.SynthesizeAsync(It.IsAny<SpeechSynthesisRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SpeechSynthesisResult.Fail("Azure said no."));

        var service = new TextToSpeechService(mock.Object);

        var result = await service.SpeakAsync("Здраво");

        Assert.False(result.Success);
        Assert.Equal("Azure said no.", result.ErrorMessage);
    }

    [Fact]
    public async Task SpeakAsync_forwards_cancellation_token()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        CancellationToken received = default;
        var mock = new Mock<ISpeechSynthesizer>();
        mock.Setup(s => s.SynthesizeAsync(It.IsAny<SpeechSynthesisRequest>(), It.IsAny<CancellationToken>()))
            .Callback<SpeechSynthesisRequest, CancellationToken>((_, ct) => received = ct)
            .ReturnsAsync(SpeechSynthesisResult.Ok(System.Array.Empty<byte>()));

        var service = new TextToSpeechService(mock.Object);

        await service.SpeakAsync("Здраво", cancellationToken: cts.Token);

        Assert.True(received.IsCancellationRequested);
    }
}
