using LoopbackRecorder.Enums;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using System.IO;

namespace LoopbackRecorder.Helpers;

public class ConversionHelper
{
    private readonly LogHelper logHelper = App.serviceProvider.GetRequiredService<LogHelper>();

    public async Task ConvertToAsync(Formats format, string waveFilePath)
    {
        if (!File.Exists(waveFilePath))
        {
            logHelper.AppendLog($"Convert: File does not exist. {waveFilePath}");
            return;
        }

        using var reader = new WaveFileReader(waveFilePath);
        if(reader.Length == 0 || reader.SampleCount == 0)
        {
            logHelper.AppendLog($"Convert: File is empty or has no samples. {waveFilePath}");
            return;
        }

        logHelper.AppendLog($"Converting to {format} format...");
        string convertedFilePath = waveFilePath;

        switch (format)
        {
            case Formats.Aac:
                convertedFilePath = waveFilePath.Replace(".wav", ".mp4", StringComparison.InvariantCultureIgnoreCase);
                await Task.Run(() => MediaFoundationEncoder.EncodeToAac(reader, convertedFilePath));
                break;
            case Formats.Mp3:
                convertedFilePath = waveFilePath.Replace(".wav", ".mp3", StringComparison.InvariantCultureIgnoreCase);
                await Task.Run(() => MediaFoundationEncoder.EncodeToMp3(reader, convertedFilePath));
                break;
            case Formats.Wma:
                convertedFilePath = waveFilePath.Replace(".wav", ".wma", StringComparison.InvariantCultureIgnoreCase);
                await Task.Run(() => MediaFoundationEncoder.EncodeToWma(reader, convertedFilePath));
                break;
        }

        logHelper.AppendLog($"Success.");
    }
}