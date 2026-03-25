using LoopbackRecorder.Properties;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;
using System.Text;
using Whisper.net;
using Whisper.net.Ggml;

namespace LoopbackRecorder.Helpers;

public class TranscriptionHelper(LogHelper logHelper)
{
    private static readonly SemaphoreSlim modelDownloadSemaphore = new(1, 1);

    public async Task TranscribeWithWhisperAsync(string wavFileName)
    {
        if (!File.Exists(wavFileName))
        {
            logHelper.AppendLog($"Transcribe: File does not exist. {wavFileName}");
            return;
        }

        using WaveFileReader reader0 = new(wavFileName);
        if (reader0.Length == 0 || reader0.SampleCount == 0)
        {
            logHelper.AppendLog($"Transcribe: File is empty or has no samples. {wavFileName}");
            return;
        }

        GgmlType ggmlType = GgmlType.Base;
        string modelFileName = Settings.Default.TranscribeModelName;
        string transcriptionFileName = Path.ChangeExtension(wavFileName, ".txt");

        string recordingDir = Path.GetDirectoryName(wavFileName)!;
        string modelsPath = Path.GetFullPath(Path.Combine(recordingDir, "..", "models"));

        if (!Directory.Exists(modelsPath))
        {
            _ = Directory.CreateDirectory(modelsPath);
        }

        string modelFilePath = Path.Combine(modelsPath, modelFileName);

        await modelDownloadSemaphore.WaitAsync();
        try
        {
            if (!File.Exists(modelFilePath))
            {
                logHelper.AppendLog($"Model file '{modelFileName}' not found.");
                await DownloadModel(modelFilePath, ggmlType);
            }
        }
        finally
        {
            _ = modelDownloadSemaphore.Release();
        }

        logHelper.AppendLog($"Transcribing...");

        using WhisperFactory whisperFactory = WhisperFactory.FromPath(modelFilePath);
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

    private async Task DownloadModel(string filePath, GgmlType ggmlType)
    {
        logHelper.AppendLog($"Downloading...");
        using Stream modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(ggmlType);
        using FileStream fileWriter = File.OpenWrite(filePath);
        await modelStream.CopyToAsync(fileWriter);
        logHelper.AppendLog($"Success.");
    }
}