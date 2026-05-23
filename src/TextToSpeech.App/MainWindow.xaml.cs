using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using TextToSpeech.Core;

namespace TextToSpeech.App;

/// <summary>
/// Interaction logic for MainWindow.xaml. Kept intentionally thin — all orchestration
/// lives in <see cref="TextToSpeechService"/> in TextToSpeech.Core so it can be tested
/// without a UI or an Azure subscription.
/// </summary>
public partial class MainWindow : Window
{
    private readonly TextToSpeechService? _service;
    private readonly string? _startupError;
    private byte[]? _lastAudio;

    public MainWindow()
    {
        InitializeComponent();

        foreach (var voice in SerbianVoices.All)
        {
            VoiceComboBox.Items.Add(voice);
        }
        VoiceComboBox.SelectedItem = SerbianVoices.DefaultVoice;

        try
        {
            var options = AzureSpeechConfigurationLoader.Load();
            _service = new TextToSpeechService(new AzureSpeechSynthesizer(options));
            SetStatus($"Спремно. Регион: {options.Region}.");
        }
        catch (Exception ex)
        {
            _startupError = ex.Message;
            SpeakButton.IsEnabled = false;
            SaveButton.IsEnabled = false;
            SetStatus("Конфигурација није подешена: " + ex.Message);
        }
    }

    private async void SpeakButton_Click(object sender, RoutedEventArgs e)
    {
        if (_service is null)
        {
            SetStatus("Конфигурација није подешена: " + _startupError);
            return;
        }

        await RunSynthesisAsync(playAudio: true);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_service is null)
        {
            SetStatus("Конфигурација није подешена: " + _startupError);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "WAV audio (*.wav)|*.wav",
            FileName = "speech.wav",
        };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        var result = await RunSynthesisAsync(playAudio: false);
        if (result is { Success: true })
        {
            await File.WriteAllBytesAsync(dialog.FileName, result.AudioData);
            SetStatus($"Сачувано: {dialog.FileName} ({result.AudioData.Length} бајтова).");
        }
    }

    private async Task<SpeechSynthesisResult?> RunSynthesisAsync(bool playAudio)
    {
        SpeakButton.IsEnabled = false;
        SaveButton.IsEnabled = false;
        SetStatus("Синтеза у току…");
        try
        {
            var voice = VoiceComboBox.SelectedItem as string;
            var result = await _service!.SpeakAsync(InputTextBox.Text, voice);

            if (!result.Success)
            {
                SetStatus("Грешка: " + result.ErrorMessage);
                return result;
            }

            _lastAudio = result.AudioData;
            if (playAudio)
            {
                PlayAudio(result.AudioData);
                SetStatus($"Готово ({result.AudioData.Length} бајтова аудио података).");
            }

            return result;
        }
        catch (Exception ex)
        {
            SetStatus("Неочекивана грешка: " + ex.Message);
            return null;
        }
        finally
        {
            SpeakButton.IsEnabled = true;
            SaveButton.IsEnabled = true;
        }
    }

    private static void PlayAudio(byte[] audio)
    {
        // The Speech SDK returns a self-contained RIFF/WAV byte stream when no AudioConfig
        // is supplied, so SoundPlayer can play it directly without a temporary file.
        using var stream = new MemoryStream(audio);
        using var player = new SoundPlayer(stream);
        player.Play();
    }

    private void SetStatus(string message) => StatusTextBlock.Text = message;
}
