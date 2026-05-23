namespace TextToSpeech.Core;

/// <summary>
/// Outcome of a synthesis call. <see cref="AudioData"/> contains the rendered audio (typically
/// 16 kHz / 16-bit mono PCM) when <see cref="Success"/> is <c>true</c>; otherwise
/// <see cref="ErrorMessage"/> describes the failure.
/// </summary>
public sealed class SpeechSynthesisResult
{
    public bool Success { get; init; }
    public byte[] AudioData { get; init; } = System.Array.Empty<byte>();
    public string? ErrorMessage { get; init; }

    public static SpeechSynthesisResult Ok(byte[] audio) =>
        new() { Success = true, AudioData = audio };

    public static SpeechSynthesisResult Fail(string message) =>
        new() { Success = false, ErrorMessage = message };
}
