using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace LoopbackRecorder.Desktop.ViewModels;

public class MainViewModel : ObservableObject
{
    private ObservableCollection<MMDevice> renderDevices;

    public ObservableCollection<MMDevice> RenderDevices
    {
        get => renderDevices;
        set => SetProperty(ref renderDevices, value);
    }

    private MMDevice? selectedRenderDevice;

    public MMDevice? SelectedRenderDevice
    {
        get => selectedRenderDevice;
        set => SetProperty(ref selectedRenderDevice, value);
    }

    private ObservableCollection<MMDevice> captureDevices;

    public ObservableCollection<MMDevice> CaptureDevices
    {
        get => captureDevices;
        set => SetProperty(ref captureDevices, value);
    }

    private MMDevice? selectedCaptureDevice;

    public MMDevice? SelectedCaptureDevice
    {
        get => selectedCaptureDevice;
        set => SetProperty(ref selectedCaptureDevice, value);
    }

    public ICommand StartStopRecordingCommand { get; }

    private double renderMasterPeakValue;

    public double RenderMasterPeakValue
    {
        get => renderMasterPeakValue;
        set => SetProperty(ref renderMasterPeakValue, value);
    }

    private double captureMasterPeakValue;

    public double CaptureMasterPeakValue
    {
        get => captureMasterPeakValue;
        set => SetProperty(ref captureMasterPeakValue, value);
    }

    private string? log;

    public string? Log
    {
        get => log;
        set => SetProperty(ref log, value);
    }

    private WasapiLoopbackCapture? renderCapture;
    private WaveFileWriter? renderWriter;
    private WasapiCapture? captureCapture;
    private WaveFileWriter? captureWriter;

    public MainViewModel()
    {
        MMDeviceEnumerator deviceEnumerator = new();
        renderDevices = [null, .. deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)];
        captureDevices = [null, .. deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)];
        StartStopRecordingCommand = new RelayCommand<bool?>(StartStopRecording);

        new DispatcherTimer(DispatcherPriority.Background) { IsEnabled = true, Interval = TimeSpan.FromMilliseconds(100) }.Tick += (s, e) =>
        {
            try
            {
                RenderMasterPeakValue = selectedRenderDevice?.AudioMeterInformation.MasterPeakValue ?? 0;
                CaptureMasterPeakValue = selectedCaptureDevice?.AudioMeterInformation.MasterPeakValue ?? 0;
            }
            catch (Exception ex)
            {
                AppendLog($"An error occurred: {ex.Message}");
                AppendLog(ex.StackTrace ?? "No stack trace available.");
            }
        };
    }

    private void StartStopRecording(bool? isChecked)
    {
        try
        {
            string suffix = $"{DateTime.Now:yyyyMMddHHmmss}.wav";

            if (isChecked == true)
            {
                if (selectedRenderDevice != null)
                {
                    string renderFileName = $"render-{suffix}";
                    renderCapture = new(selectedRenderDevice);
                    renderWriter = new(renderFileName, renderCapture.WaveFormat);

                    renderCapture.RecordingStopped += (s, e) =>
                    {
                        renderWriter.Dispose();
                        renderCapture.Dispose();
                        AppendLog("Render recording stopped.");

                        ConvertTo("aac", renderFileName);
                        AppendLog($"Converted to AAC format.");
                    };

                    renderCapture.DataAvailable += (s, e) =>
                    {
                        renderWriter.Write(e.Buffer, 0, e.BytesRecorded);
                    };

                    renderCapture.StartRecording();
                    AppendLog($"Render Wave Format: {renderCapture.WaveFormat}");
                    AppendLog($"Selected Render Device: {selectedRenderDevice}");
                    AppendLog($"Render Filename: {renderFileName}");
                    AppendLog($"Render Recording started.");
                }

                if (selectedCaptureDevice != null)
                {
                    string captureFileName = $"capture-{suffix}";
                    captureCapture = new(selectedCaptureDevice);
                    captureWriter = new(captureFileName, captureCapture.WaveFormat);

                    captureCapture.RecordingStopped += (s, e) =>
                    {
                        captureWriter.Dispose();
                        captureCapture.Dispose();
                        AppendLog("Capture recording stopped.");

                        ConvertTo("aac", captureFileName);
                        AppendLog($"Converted to AAC format.");
                    };

                    captureCapture.DataAvailable += (s, e) =>
                    {
                        captureWriter.Write(e.Buffer, 0, e.BytesRecorded);
                    };

                    captureCapture.StartRecording();
                    AppendLog($"Capture Wave Format: {captureCapture.WaveFormat}");
                    AppendLog($"Selected Capture Device: {selectedCaptureDevice}");
                    AppendLog($"Capture Filename: {captureFileName}");
                    AppendLog($"Capture Recording started.");
                }
            }
            else
            {
                renderCapture?.StopRecording();
                captureCapture?.StopRecording();
            }
        }
        catch (Exception ex)
        {
            AppendLog($"An error occurred: {ex.Message}");
            AppendLog(ex.StackTrace ?? "No stack trace available.");
        }
    }

    private void AppendLog(string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Log += $"{DateTime.Now:HH:mm:ss.fff} - {message}\n";
        });
    }

    private static void ConvertTo(string type, string waveFilePath)
    {
        using WaveFileReader reader = new(waveFilePath);
        switch (type)
        {
            case "aac":
                MediaFoundationEncoder.EncodeToAac(reader, waveFilePath.Replace(".wav", ".mp4", StringComparison.InvariantCultureIgnoreCase));
                break;
            case "mp3":
                MediaFoundationEncoder.EncodeToMp3(reader, waveFilePath.Replace(".wav", ".mp3", StringComparison.InvariantCultureIgnoreCase));
                break;
            case "wma":
                MediaFoundationEncoder.EncodeToWma(reader, waveFilePath.Replace(".wav", ".wma", StringComparison.InvariantCultureIgnoreCase));
                break;
            default:
                throw new ArgumentException($"Unsupported conversion type: {type}");
        }

    }
}
