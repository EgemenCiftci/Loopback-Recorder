using LoopbackRecorder.Enums;
using NAudio.Wave;
using System.IO;

namespace LoopbackRecorder.Helpers;

public class ConversionHelper
{
    private readonly LogHelper logHelper;

    public ConversionHelper(LogHelper logHelper)
    {
        this.logHelper = logHelper;
    }

    public async Task ConvertToAsync(Formats format, string waveFilePath)
    {
        if (!File.Exists(waveFilePath))
        {
            logHelper.AppendLog($"Convert: File does not exist. {waveFilePath}");
            return;
        }

        using WaveFileReader reader = new(waveFilePath);
        if (reader.Length == 0 || reader.SampleCount == 0)
        {
            logHelper.AppendLog($"Convert: File is empty or has no samples. {waveFilePath}");
            return;
        }

        logHelper.AppendLog($"Converting to {format} format...");
        string convertedFilePath = waveFilePath;

        switch (format)
        {
            case Formats.Aac:
                convertedFilePath = Path.ChangeExtension(waveFilePath, ".mp4");
                await Task.Run(() => MediaFoundationEncoder.EncodeToAac(reader, convertedFilePath));
                break;
            case Formats.Mp3:
                convertedFilePath = Path.ChangeExtension(waveFilePath, ".mp3");
                await Task.Run(() => MediaFoundationEncoder.EncodeToMp3(reader, convertedFilePath));
                break;
            case Formats.Wma:
                convertedFilePath = Path.ChangeExtension(waveFilePath, ".wma");
                await Task.Run(() => MediaFoundationEncoder.EncodeToWma(reader, convertedFilePath));
                break;
            default:
                logHelper.AppendLog($"Unsupported format: {format}. Skipping conversion.");
                return;
        }

        logHelper.AppendLog($"Success.");
    }
}