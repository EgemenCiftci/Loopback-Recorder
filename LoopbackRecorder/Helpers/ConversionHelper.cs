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
        WaveFileReader? normalizedReader = null;
        string? normalizedFilePath = null;

        try
        {
            IWaveProvider source = reader;

            if (TryGetNormalizedWaveFormat(reader.WaveFormat, out WaveFormat? normalizedFormat) &&
                normalizedFormat != null)
            {
                normalizedFilePath = Path.Combine(Path.GetTempPath(), $"LoopbackRecorder-{Guid.NewGuid():N}.wav");
                normalizedReader = CreateNormalizedReader(reader, normalizedFormat, normalizedFilePath);
                source = normalizedReader;
            }

            switch (format)
            {
                case Formats.Aac:
                    convertedFilePath = Path.ChangeExtension(waveFilePath, ".mp4");
                    await Task.Run(() => MediaFoundationEncoder.EncodeToAac(source, convertedFilePath));
                    break;
                case Formats.Mp3:
                    convertedFilePath = Path.ChangeExtension(waveFilePath, ".mp3");
                    await Task.Run(() => MediaFoundationEncoder.EncodeToMp3(source, convertedFilePath));
                    break;
                case Formats.Wma:
                    convertedFilePath = Path.ChangeExtension(waveFilePath, ".wma");
                    await Task.Run(() => MediaFoundationEncoder.EncodeToWma(source, convertedFilePath));
                    break;
                default:
                    logHelper.AppendLog($"Unsupported format: {format}. Skipping conversion.");
                    return;
            }

            logHelper.AppendLog("Success.");
        }
        finally
        {
            if (normalizedReader != null)
            {
                await normalizedReader.DisposeAsync();
            }

            if (normalizedFilePath != null)
            {
                try
                {
                    File.Delete(normalizedFilePath);
                }
                catch (IOException)
                {
                    logHelper.AppendLog($"Failed to delete temporary file: {normalizedFilePath}");
                }
                catch (UnauthorizedAccessException)
                {
                    logHelper.AppendLog($"Failed to delete temporary file due to access restrictions: {normalizedFilePath}");
                }
            }
        }
    }

    private static bool TryGetNormalizedWaveFormat(WaveFormat format, out WaveFormat? normalizedFormat)
    {
        normalizedFormat = null;

        if (format is not WaveFormatExtraData extensible || format.Encoding != WaveFormatEncoding.Extensible)
        {
            return false;
        }

        if (extensible.ExtraData.Length < 22)
        {
            return false;
        }

        byte[] extraData = extensible.ExtraData;

        if (extraData[6] == 1 && extraData[7] == 0 && extraData[8] == 0 && extraData[9] == 0)
        {
            normalizedFormat = new WaveFormat(format.SampleRate, format.BitsPerSample, format.Channels);
            return true;
        }

        if (extraData[6] == 3 && extraData[7] == 0 && extraData[8] == 0 && extraData[9] == 0)
        {
            normalizedFormat = WaveFormat.CreateIeeeFloatWaveFormat(format.SampleRate, format.Channels);
            return true;
        }

        return false;
    }

    private static WaveFileReader CreateNormalizedReader(WaveFileReader source, WaveFormat normalizedFormat, string filePath)
    {
        source.Position = 0;

        using (WaveFileWriter writer = new(filePath, normalizedFormat))
        {
            byte[] buffer = new byte[81920];
            int bytesRead;

            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                writer.Write(buffer, 0, bytesRead);
            }
        }

        source.Position = 0;
        return new WaveFileReader(filePath);
    }
}