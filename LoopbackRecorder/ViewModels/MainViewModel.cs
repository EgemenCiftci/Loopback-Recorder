using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LoopbackRecorder.Enums;
using LoopbackRecorder.Helpers;
using LoopbackRecorder.Models;
using LoopbackRecorder.Properties;
using LoopbackRecorder.Views;
using Microsoft.Extensions.DependencyInjection;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Threading;

namespace LoopbackRecorder.ViewModels;

public class MainViewModel : ObservableObject, IDisposable
{
    private ObservableCollection<Device> renderDevices;

    public ObservableCollection<Device> RenderDevices
    {
        get => renderDevices;
        set => SetProperty(ref renderDevices, value);
    }

    public Device SelectedRenderDevice
    {
        get;
        set => SetProperty(ref field, value);
    } = NoneItem;

    private ObservableCollection<Device> captureDevices;

    public ObservableCollection<Device> CaptureDevices
    {
        get => captureDevices;
        set => SetProperty(ref captureDevices, value);
    }

    public Device SelectedCaptureDevice
    {
        get;
        set => SetProperty(ref field, value);
    } = NoneItem;

    public bool IsBusy
    {
        get;
        set => SetProperty(ref field, value);
    } = false;

    public ICommand StartStopRecordingCommand { get; }

    public ICommand ShowCommand { get; }

    public double RenderMasterPeakValue
    {
        get;
        set => SetProperty(ref field, value);
    }

    public double CaptureMasterPeakValue
    {
        get;
        set => SetProperty(ref field, value);
    }

    private readonly TranscriptionHelper transcriptionHelper;
    private readonly ConversionHelper conversionHelper;

    public LogHelper LogHelper { get; }

    private WasapiLoopbackCapture? renderCapture;
    private WaveFileWriter? renderWriter;
    private WasapiCapture? captureCapture;
    private WaveFileWriter? captureWriter;
    private static readonly Device NoneItem = new(null);
    private readonly DispatcherTimer? peakValueTimer;

    public MainViewModel(LogHelper logHelper, TranscriptionHelper transcriptionHelper, ConversionHelper conversionHelper)
    {
        LogHelper = logHelper;
        this.transcriptionHelper = transcriptionHelper;
        this.conversionHelper = conversionHelper;

        StartStopRecordingCommand = new RelayCommand<bool?>(StartStopRecording);
        ShowCommand = new RelayCommand(ShowSettings);

        MMDeviceEnumerator deviceEnumerator = new();
        renderDevices = [NoneItem, .. deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).Select(x => new Device(x))];
        captureDevices = [NoneItem, .. deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).Select(x => new Device(x))];

        peakValueTimer = new DispatcherTimer(DispatcherPriority.Background) { IsEnabled = true, Interval = TimeSpan.FromMilliseconds(100) };
        peakValueTimer.Tick += (s, e) =>
        {
            try
            {
                RenderMasterPeakValue = SelectedRenderDevice?.AudioMeterInformation?.MasterPeakValue ?? 0;
                CaptureMasterPeakValue = SelectedCaptureDevice?.AudioMeterInformation?.MasterPeakValue ?? 0;
            }
            catch (Exception ex)
            {
                LogHelper.AppendException(ex, "Error updating peak values");
            }
        };
    }

    private void StartStopRecording(bool? isChecked)
    {
        try
        {
            if (isChecked == true)
            {
                if (IsBusy)
                {
                    LogHelper.AppendLog("Recording already in progress.");
                    return;
                }

                IsBusy = true;
                StartRecording();
            }
            else
            {
                StopRecording();
                IsBusy = false;
            }
        }
        catch (Exception ex)
        {
            IsBusy = false;
            LogHelper.AppendException(ex, "Error starting/stopping recording");
        }
    }

    private void StartRecording()
    {
        string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DateTime.Now.ToString("yyyyMMddHHmmss"));
        _ = Directory.CreateDirectory(folderPath);

        if (SelectedRenderDevice.MMDevice != null)
        {
            string renderFileName = Path.Combine(folderPath, "render.wav");
            renderCapture = new(SelectedRenderDevice.MMDevice);
            renderWriter = new(renderFileName, renderCapture.WaveFormat);

            renderCapture.RecordingStopped += async (s, e) =>
            {
                try
                {
                    renderWriter?.Dispose();
                    renderCapture?.Dispose();
                    renderWriter = null;
                    renderCapture = null;

                    LogHelper.AppendLog("Render recording stopped.");

                    if (Settings.Default.CanConvert)
                    {
                        bool result = Enum.TryParse(Settings.Default.ConvertFormat, true, out Formats format);

                        if (result)
                        {
                            await conversionHelper.ConvertToAsync(format, renderFileName);
                        }
                        else
                        {
                            LogHelper.AppendLog($"Invalid convert format: {Settings.Default.ConvertFormat}. Skipping...");
                        }
                    }

                    if (Settings.Default.CanTranscribe)
                    {
                        await transcriptionHelper.TranscribeWithWhisperAsync(renderFileName);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.AppendException(ex, "Error during render recording stop");
                }
            };

            renderCapture.DataAvailable += (s, e) =>
            {
                try
                {
                    if (!Settings.Default.CanRemoveSilence || !IsSilent(e.Buffer, e.BytesRecorded, renderCapture.WaveFormat))
                    {
                        renderWriter.Write(e.Buffer, 0, e.BytesRecorded);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.AppendException(ex, "Error writing render data");
                }
            };

            renderCapture.StartRecording();
            LogHelper.AppendLog($"Render Wave Format: {renderCapture.WaveFormat}");
            LogHelper.AppendLog($"Selected Render Device: {SelectedRenderDevice.FriendlyName}");
            LogHelper.AppendLog($"Directory: {folderPath}");
            LogHelper.AppendLog("Render Recording started.");
        }

        if (SelectedCaptureDevice.MMDevice != null)
        {
            string captureFileName = Path.Combine(folderPath, "capture.wav");
            captureCapture = new(SelectedCaptureDevice.MMDevice);
            captureWriter = new(captureFileName, captureCapture.WaveFormat);

            captureCapture.RecordingStopped += async (s, e) =>
            {
                try
                {
                    captureWriter?.Dispose();
                    captureCapture?.Dispose();
                    captureWriter = null;
                    captureCapture = null;

                    LogHelper.AppendLog("Capture recording stopped.");

                    if (Settings.Default.CanConvert)
                    {
                        bool result = Enum.TryParse(Settings.Default.ConvertFormat, true, out Formats format);

                        if (result)
                        {
                            await conversionHelper.ConvertToAsync(format, captureFileName);
                        }
                        else
                        {
                            LogHelper.AppendLog($"Invalid convert format: {Settings.Default.ConvertFormat}. Skipping...");
                        }
                    }

                    if (Settings.Default.CanTranscribe)
                    {
                        await transcriptionHelper.TranscribeWithWhisperAsync(captureFileName);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.AppendException(ex, "Error during capture recording stop");
                }
            };

            captureCapture.DataAvailable += (s, e) =>
            {
                try
                {
                    if (!Settings.Default.CanRemoveSilence || !IsSilent(e.Buffer, e.BytesRecorded, captureCapture.WaveFormat))
                    {
                        captureWriter.Write(e.Buffer, 0, e.BytesRecorded);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.AppendException(ex, "Error writing capture data");
                }
            };

            captureCapture.StartRecording();
            LogHelper.AppendLog($"Capture Wave Format: {captureCapture.WaveFormat}");
            LogHelper.AppendLog($"Selected Capture Device: {SelectedCaptureDevice.FriendlyName}");
            LogHelper.AppendLog($"Directory: {folderPath}");
            LogHelper.AppendLog("Capture Recording started.");
        }
    }

    private void StopRecording()
    {
        renderCapture?.StopRecording();
        captureCapture?.StopRecording();
    }

    private static bool IsSilent(byte[] buffer, int bytesRecorded, WaveFormat format)
    {
        int bytesPerSample = format.BitsPerSample / 8;
        int sampleCount = bytesRecorded / bytesPerSample;

        if (sampleCount == 0)
        {
            return true;
        }

        double sumSquares = 0;

        for (int i = 0; i < bytesRecorded; i += bytesPerSample)
        {
            int bufferIndex = i;
            float sample = 0;

            if (format.Encoding == WaveFormatEncoding.IeeeFloat && format.BitsPerSample == 32)
            {
                sample = BitConverter.ToSingle(buffer, bufferIndex);
            }
            else if (format.BitsPerSample == 16)
            {
                sample = BitConverter.ToInt16(buffer, bufferIndex) / 32768f;
            }
            else if (format.BitsPerSample == 32)
            {
                sample = BitConverter.ToInt32(buffer, bufferIndex) / 2147483648f;
            }
            else if (format.BitsPerSample == 8)
            {
                sample = (buffer[bufferIndex] - 128) / 128f;
            }

            sumSquares += sample * sample;
        }

        double rms = Math.Sqrt(sumSquares / sampleCount);

        return rms < Settings.Default.SilenceThreshold;
    }

    public void ShowSettings()
    {
        try
        {
            SettingsView? settingsView = App.ServiceProvider?.GetRequiredService<SettingsView>();

            if (settingsView == null)
            {
                LogHelper.AppendLog("SettingsView service not found.");
                return;
            }

            settingsView.DataContext = App.ServiceProvider?.GetRequiredService<SettingsViewModel>();
            System.Windows.Application.Current.MainWindow.Content = settingsView;
        }
        catch (Exception ex)
        {
            LogHelper.AppendException(ex, "Error showing settings.");
        }
    }

    private bool disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                peakValueTimer?.Stop();
                renderCapture?.StopRecording();
                captureCapture?.StopRecording();

                renderWriter?.Dispose();
                captureWriter?.Dispose();
                renderCapture?.Dispose();
                captureCapture?.Dispose();
            }
            disposed = true;
        }
    }

    ~MainViewModel()
    {
        Dispose(false);
    }
}
