using System;
using System.Threading.Tasks;
using TextToSpeech.Core;
using Xunit;

namespace TextToSpeech.Core.Tests;

public class AzureSpeechSynthesizerTests
{
    private static AzureSpeechOptions ValidOptions() => new()
    {
        SubscriptionKey = "fake-key-for-tests",
        Region = "westeurope",
    };

    [Fact]
    public void Ctor_throws_for_null_options()
    {
        Assert.Throws<ArgumentNullException>(() => new AzureSpeechSynthesizer(null!));
    }

    [Fact]
    public void Ctor_validates_options()
    {
        var bad = new AzureSpeechOptions { SubscriptionKey = "", Region = "" };
        Assert.Throws<InvalidOperationException>(() => new AzureSpeechSynthesizer(bad));
    }

    [Fact]
    public async Task SynthesizeAsync_returns_failure_for_null_request()
    {
        var sut = new AzureSpeechSynthesizer(ValidOptions());
        var result = await sut.SynthesizeAsync(null!);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        Assert.Empty(result.AudioData);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public async Task SynthesizeAsync_returns_failure_for_empty_text(string text)
    {
        var sut = new AzureSpeechSynthesizer(ValidOptions());
        var result = await sut.SynthesizeAsync(new SpeechSynthesisRequest { Text = text });

        Assert.False(result.Success);
        Assert.Contains("empty", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }
}
