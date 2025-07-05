using LoopbackRecorder.Properties;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;
using System.Text;
using Whisper.net;
using Whisper.net.Ggml;

namespace LoopbackRecorder.Helpers;

public class TranscriptionHelper
{
    private readonly LogHelper logHelper = App.serviceProvider.GetRequiredService<LogHelper>();

    public async Task<string> TranscribeWithWhisperAsync(string wavFileName)
    {
        GgmlType ggmlType = GgmlType.Base;
        string modelFileName = Settings.Default.TranscribeModelName;
        string transcriptionFileName = wavFileName.Replace(".wav", ".txt", StringComparison.InvariantCultureIgnoreCase);

        if (!File.Exists(modelFileName))
        {
            logHelper.AppendLog($"Model file '{modelFileName}' not found. Downloading...");
            await DownloadModel(modelFileName, ggmlType);
            logHelper.AppendLog($"Success.");
        }

        using WhisperFactory whisperFactory = WhisperFactory.FromPath(modelFileName);
        using WhisperProcessor processor = whisperFactory.CreateBuilder().WithLanguage("auto").Build();
        using FileStream fileStream = File.OpenRead(wavFileName);
        using MemoryStream wavStream = new();
        using WaveFileReader reader = new(fileStream);
        WdlResamplingSampleProvider resampler = new(reader.ToSampleProvider(), 16000);
        WaveFileWriter.WriteWavFileToStream(wavStream, resampler.ToWaveProvider16());
        _ = wavStream.Seek(0, SeekOrigin.Begin);

        StringBuilder sb = new();

        await foreach (SegmentData result in processor.ProcessAsync(wavStream))
        {
            _ = sb.AppendLine($"{result.Start}->{result.End}: {result.Text}");
        }

        await File.WriteAllTextAsync(transcriptionFileName, sb.ToString(), Encoding.UTF8);

        return transcriptionFileName;
    }

    private static async Task DownloadModel(string fileName, GgmlType ggmlType)
    {
        using Stream modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(ggmlType);
        using FileStream fileWriter = File.OpenWrite(fileName);
        await modelStream.CopyToAsync(fileWriter);
    }
}