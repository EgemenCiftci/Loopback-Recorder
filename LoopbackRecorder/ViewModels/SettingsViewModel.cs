using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LoopbackRecorder.Enums;
using LoopbackRecorder.Helpers;
using LoopbackRecorder.Properties;
using LoopbackRecorder.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;

namespace LoopbackRecorder.ViewModels;

public class SettingsViewModel : ObservableObject
{
    
    private readonly LogHelper logHelper = App.serviceProvider.GetRequiredService<LogHelper>();

    private bool canConvert = Settings.Default.CanConvert;

    public bool CanConvert
    {
        get => canConvert;
        set
        {
            if (SetProperty(ref canConvert, value))
            {
                Settings.Default.CanConvert = value;
                Settings.Default.Save();
            }
        }
    }

    private Formats convertFormat = Enum.Parse<Formats>(Settings.Default.ConvertFormat);

    public Formats ConvertFormat
    {
        get => convertFormat;
        set
        {
            if (SetProperty(ref convertFormat, value))
            {
                Settings.Default.ConvertFormat = value.ToString();
                Settings.Default.Save();
            }
        }
    }

    private bool canRemoveSilence = Settings.Default.CanRemoveSilence;

    public bool CanRemoveSilence
    {
        get => canRemoveSilence;
        set
        {
            if (SetProperty(ref canRemoveSilence, value))
            {
                Settings.Default.CanRemoveSilence = value;
                Settings.Default.Save();
            }
        }
    }

    private float silenceThreshold = Settings.Default.SilenceThreshold;

    public float SilenceThreshold
    {
        get => silenceThreshold;
        set
        {
            if (SetProperty(ref silenceThreshold, value))
            {
                Settings.Default.SilenceThreshold = value;
                Settings.Default.Save();
            }
        }
    }

    private bool canTranscribe = Settings.Default.CanTranscribe;

    public bool CanTranscribe
    {
        get => canTranscribe;
        set
        {
            if (SetProperty(ref canTranscribe, value))
            {
                Settings.Default.CanTranscribe = value;
                Settings.Default.Save();
            }
        }
    }

    private string transcribeModelName = Settings.Default.TranscribeModelName;

    public string TranscribeModelName
    {
        get => transcribeModelName;
        set
        {
            if (SetProperty(ref transcribeModelName, value))
            {
                Settings.Default.TranscribeModelName = value;
                Settings.Default.Save();
            }
        }
    }

    public ICommand ShowCommand => new RelayCommand(ShowMain);

    public void ShowMain()
    {
        try
        {
            MainView mainView = App.serviceProvider.GetRequiredService<MainView>();
            mainView.DataContext = App.serviceProvider.GetRequiredService<MainViewModel>();
            System.Windows.Application.Current.MainWindow.Content = mainView;
        }
        catch (Exception ex)
        {
            logHelper.AppendException(ex, "Error showing main.");
        }
    }
}
