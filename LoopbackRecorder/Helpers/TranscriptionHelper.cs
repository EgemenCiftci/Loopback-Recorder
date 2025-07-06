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

    public async Task TranscribeWithWhisperAsync(string wavFileName)
    {
        GgmlType ggmlType = GgmlType.Base;
        string modelFileName = Settings.Default.TranscribeModelName;
        string transcriptionFileName = wavFileName.Replace(".wav", ".txt", StringComparison.InvariantCultureIgnoreCase);

        string currentDirectory = Directory.GetCurrentDirectory();
        string modelsPath = Path.GetFullPath(Path.Combine(currentDirectory, "..", "models"));

        if (!Directory.Exists(modelsPath))
        {
            _ = Directory.CreateDirectory(modelsPath);
        }

        Directory.SetCurrentDirectory(modelsPath);

        if (!File.Exists(modelFileName))
        {
            logHelper.AppendLog($"Model file '{modelFileName}' not found.");
            await DownloadModel(modelFileName, ggmlType);
        }

        logHelper.AppendLog($"Transcribing...");

        using MemoryStream modelStream = new();
        using FileStream modelFileStream = File.OpenRead(modelFileName);
        await modelFileStream.CopyToAsync(modelStream);

        Directory.SetCurrentDirectory(currentDirectory);

        using WhisperFactory whisperFactory = WhisperFactory.FromBuffer(modelStream.ToArray());
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

        logHelper.AppendLog($"Success.");
    }

    private async Task DownloadModel(string fileName, GgmlType ggmlType)
    {
        logHelper.AppendLog($"Downloading...");
        using Stream modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(ggmlType);
        using FileStream fileWriter = File.OpenWrite(fileName);
        await modelStream.CopyToAsync(fileWriter);
        logHelper.AppendLog($"Success.");
    }
}