using TextToSpeech.Core;
using Xunit;

namespace TextToSpeech.Core.Tests;

public class SerbianVoicesTests
{
    [Fact]
    public void LanguageCode_is_sr_RS()
    {
        Assert.Equal("sr-RS", SerbianVoices.LanguageCode);
    }

    [Fact]
    public void Default_voice_is_in_All()
    {
        Assert.Contains(SerbianVoices.DefaultVoice, SerbianVoices.All);
    }

    [Fact]
    public void Catalog_contains_both_neural_voices()
    {
        Assert.Contains(SerbianVoices.SophieNeural, SerbianVoices.All);
        Assert.Contains(SerbianVoices.NicholasNeural, SerbianVoices.All);
    }
}
