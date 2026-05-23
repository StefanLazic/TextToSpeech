using System;
using TextToSpeech.Core;
using Xunit;

namespace TextToSpeech.Core.Tests;

public class AzureSpeechOptionsTests
{
    [Fact]
    public void Validate_throws_when_key_missing()
    {
        var options = new AzureSpeechOptions { SubscriptionKey = "", Region = "westeurope" };
        Assert.Throws<InvalidOperationException>(options.Validate);
    }

    [Fact]
    public void Validate_throws_when_region_missing()
    {
        var options = new AzureSpeechOptions { SubscriptionKey = "abc", Region = "  " };
        Assert.Throws<InvalidOperationException>(options.Validate);
    }

    [Fact]
    public void Validate_succeeds_when_key_and_region_present()
    {
        var options = new AzureSpeechOptions { SubscriptionKey = "abc", Region = "westeurope" };
        var ex = Record.Exception(options.Validate);
        Assert.Null(ex);
    }

    [Fact]
    public void Defaults_target_Serbian()
    {
        var options = new AzureSpeechOptions();
        Assert.Equal("sr-RS", options.DefaultLanguage);
        Assert.Equal(SerbianVoices.SophieNeural, options.DefaultVoiceName);
    }
}
