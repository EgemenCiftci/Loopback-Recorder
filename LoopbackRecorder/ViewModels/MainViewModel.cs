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
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace LoopbackRecorder.ViewModels;

public class MainViewModel : ObservableObject
{
    private ObservableCollection<Device> renderDevices;

    public ObservableCollection<Device> RenderDevices
    {
        get => renderDevices;
        set => SetProperty(ref renderDevices, value);
    }

    private Device selectedRenderDevice = NoneItem;

    public Device SelectedRenderDevice
    {
        get => selectedRenderDevice;
        set => SetProperty(ref selectedRenderDevice, value);
    }

    private ObservableCollection<Device> captureDevices;

    public ObservableCollection<Device> CaptureDevices
    {
        get => captureDevices;
        set => SetProperty(ref captureDevices, value);
    }

    private Device selectedCaptureDevice = NoneItem;

    public Device SelectedCaptureDevice
    {
        get => selectedCaptureDevice;
        set => SetProperty(ref selectedCaptureDevice, value);
    }

    public ICommand StartStopRecordingCommand => new RelayCommand<bool?>(StartStopRecording);

    public ICommand ShowSettingsCommand => new RelayCommand(ShowSettings);

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

    private LogHelper logHelper = App.serviceProvider.GetRequiredService<LogHelper>();

    public LogHelper LogHelper
    {
        get => logHelper;
        set => SetProperty(ref logHelper, value);
    }

    private WasapiLoopbackCapture? renderCapture;
    private WaveFileWriter? renderWriter;
    private WasapiCapture? captureCapture;
    private WaveFileWriter? captureWriter;
    private static readonly Device NoneItem = new(null);

    public MainViewModel()
    {
        MMDeviceEnumerator deviceEnumerator = new();
        renderDevices = [NoneItem, .. deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).Select(x => new Device(x))];
        captureDevices = [NoneItem, .. deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).Select(x => new Device(x))];

        new DispatcherTimer(DispatcherPriority.Background) { IsEnabled = true, Interval = TimeSpan.FromMilliseconds(100) }.Tick += (s, e) =>
        {
            try
            {
                RenderMasterPeakValue = selectedRenderDevice?.AudioMeterInformation?.MasterPeakValue ?? 0;
                CaptureMasterPeakValue = selectedCaptureDevice?.AudioMeterInformation?.MasterPeakValue ?? 0;
            }
            catch (Exception ex)
            {
                logHelper.AppendException(ex, "Error updating peak values");
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
                if (selectedRenderDevice.MMDevice != null)
                {
                    string renderFileName = $"render-{suffix}";
                    renderCapture = new(selectedRenderDevice.MMDevice);
                    renderWriter = new(renderFileName, renderCapture.WaveFormat);

                    renderCapture.RecordingStopped += (s, e) =>
                    {
                        try
                        {
                            renderWriter.Dispose();
                            renderCapture.Dispose();

                            logHelper.AppendLog("Render recording stopped.");

                            bool result = Enum.TryParse(Settings.Default.OutputFormat, true, out Formats format);

                            if (!result)
                            {
                                logHelper.AppendLog($"Invalid output format: {Settings.Default.OutputFormat}. Defaulting to Wav.");
                                format = Formats.Wav;
                            }

                            ConvertTo(format, renderFileName);
                            logHelper.AppendLog($"Converted to {format} format.");
                        }
                        catch (Exception ex)
                        {
                            logHelper.AppendException(ex, "Error during render recording stop");
                        }
                    };

                    renderCapture.DataAvailable += (s, e) =>
                    {
                        try
                        {
                            if (!IsSilent(e.Buffer, e.BytesRecorded, renderCapture.WaveFormat))
                            {
                                renderWriter.Write(e.Buffer, 0, e.BytesRecorded);
                            }
                        }
                        catch (Exception ex)
                        {
                            logHelper.AppendException(ex, "Error writing render data");
                        }
                    };

                    renderCapture.StartRecording();
                    logHelper.AppendLog($"Render Wave Format: {renderCapture.WaveFormat}");
                    logHelper.AppendLog($"Selected Render Device: {selectedRenderDevice}");
                    logHelper.AppendLog($"Render Filename: {renderFileName}");
                    logHelper.AppendLog($"Render Recording started.");
                }

                if (selectedCaptureDevice.MMDevice != null)
                {
                    string captureFileName = $"capture-{suffix}";
                    captureCapture = new(selectedCaptureDevice.MMDevice);
                    captureWriter = new(captureFileName, captureCapture.WaveFormat);

                    captureCapture.RecordingStopped += (s, e) =>
                    {
                        try
                        {
                            captureWriter.Dispose();
                            captureCapture.Dispose();

                            logHelper.AppendLog("Capture recording stopped.");

                            bool result = Enum.TryParse(Settings.Default.OutputFormat, true, out Formats format);

                            if (!result)
                            {
                                logHelper.AppendLog($"Invalid output format: {Settings.Default.OutputFormat}. Defaulting to Wav.");
                                format = Formats.Wav;
                            }

                            ConvertTo(format, captureFileName);
                            logHelper.AppendLog($"Converted to {format} format.");
                        }
                        catch (Exception ex)
                        {
                            logHelper.AppendException(ex, "Error during capture recording stop");
                        }
                    };

                    captureCapture.DataAvailable += (s, e) =>
                    {
                        try
                        {
                            if (!IsSilent(e.Buffer, e.BytesRecorded, captureCapture.WaveFormat))
                            {
                                captureWriter.Write(e.Buffer, 0, e.BytesRecorded);
                            }
                        }
                        catch (Exception ex)
                        {
                            logHelper.AppendException(ex, "Error writing capture data");
                        }
                    };

                    captureCapture.StartRecording();
                    logHelper.AppendLog($"Capture Wave Format: {captureCapture.WaveFormat}");
                    logHelper.AppendLog($"Selected Capture Device: {selectedCaptureDevice}");
                    logHelper.AppendLog($"Capture Filename: {captureFileName}");
                    logHelper.AppendLog($"Capture Recording started.");
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
            logHelper.AppendException(ex, "Error starting/stopping recording");
        }
    }

    private static void ConvertTo(Formats format, string waveFilePath)
    {
        using WaveFileReader reader = new(waveFilePath);

        switch (format)
        {
            case Formats.Aac:
                MediaFoundationEncoder.EncodeToAac(reader, waveFilePath.Replace(".wav", ".mp4", StringComparison.InvariantCultureIgnoreCase));
                break;
            case Formats.Mp3:
                MediaFoundationEncoder.EncodeToMp3(reader, waveFilePath.Replace(".wav", ".mp3", StringComparison.InvariantCultureIgnoreCase));
                break;
            case Formats.Wma:
                MediaFoundationEncoder.EncodeToWma(reader, waveFilePath.Replace(".wav", ".wma", StringComparison.InvariantCultureIgnoreCase));
                break;
        }
    }

    private static bool IsSilent(byte[] buffer, int bytesRecorded, WaveFormat format)
    {
        int bytesPerSample = format.BitsPerSample / 8;
        int samples = bytesRecorded / bytesPerSample;

        if (samples == 0)
        {
            return true;
        }

        double sumSquares = 0;

        for (int i = 0; i < bytesRecorded; i += bytesPerSample)
        {
            float sample = 0;

            if (format.BitsPerSample == 16)
            {
                sample = BitConverter.ToInt16(buffer, i) / 32768f;
            }
            else if (format.BitsPerSample == 32)
            {
                sample = BitConverter.ToInt32(buffer, i) / 2147483648f;
            }
            else if (format.BitsPerSample == 8)
            {
                sample = (buffer[i] - 128) / 128f;
            }

            sumSquares += sample * sample;
        }

        double rms = Math.Sqrt(sumSquares / samples);

        return rms < Settings.Default.SilenceThreshold;
    }

    public void ShowSettings()
    {
        try
        {
            SettingsView settingsView = App.serviceProvider.GetRequiredService<SettingsView>();
            settingsView.DataContext = App.serviceProvider.GetRequiredService<SettingsViewModel>();
            Application.Current.MainWindow.Content = settingsView;
        }
        catch (Exception ex)
        {
            logHelper.AppendException(ex, "Error showing settings.");
        }
    }
}
