using NAudio.CoreAudioApi;
using NAudio.Wave;
using Spectre.Console;

WasapiLoopbackCapture? renderCapture = null;
WaveFileWriter? writer = null;

try
{
    MMDeviceEnumerator deviceEnum = new();
    Console.WriteLine("Active Render Devices:");
    MMDevice defaultRenderDevice = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
    foreach (string name in deviceEnum.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).Select(x => x.FriendlyName))
    {
        Console.WriteLine($"{(defaultRenderDevice.FriendlyName == name ? "X" : " ")} {name}");
    }
    Console.WriteLine();
    Console.WriteLine("Active Capture Devices:");
    MMDevice defaultCaptureDevice = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
    foreach (string name in deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).Select(x => x.FriendlyName))
    {
        Console.WriteLine($"{(defaultCaptureDevice.FriendlyName == name ? "X" : " ")} {name}");
    }
    Console.WriteLine();

    string filename = $"{DateTime.Now:yyyyMMddHHmmss}.wav";
    renderCapture = new(defaultRenderDevice);
    writer = new(filename, renderCapture.WaveFormat);
    renderCapture.RecordingStopped += (s, e) =>
    {
        writer.Dispose();
        renderCapture.Dispose();
    };
    renderCapture.DataAvailable += (s, e) =>
    {
        writer.Write(e.Buffer, 0, e.BytesRecorded);
        if (writer.Position > renderCapture.WaveFormat.AverageBytesPerSecond * 20)
        {
            renderCapture.StopRecording();
        }
    };

    renderCapture.StartRecording();
    Console.WriteLine("Recording... Press any key to stop.");
    _ = Console.ReadKey();
    renderCapture.StopRecording();
    Console.WriteLine("Recording stopped.");
    Console.WriteLine($"Filename: {filename}");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}
finally
{
    writer?.Dispose();
    renderCapture?.Dispose();
}