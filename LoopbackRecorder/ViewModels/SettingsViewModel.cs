using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LoopbackRecorder.Enums;
using LoopbackRecorder.Helpers;
using LoopbackRecorder.Properties;
using LoopbackRecorder.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;

namespace LoopbackRecorder.ViewModels;

public class SettingsViewModel : ObservableObject
{

    private readonly LogHelper? logHelper = App.ServiceProvider?.GetRequiredService<LogHelper>();

    public bool CanConvert
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                Settings.Default.CanConvert = value;
                Settings.Default.Save();
            }
        }
    } = Settings.Default.CanConvert;

    public Formats ConvertFormat
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                Settings.Default.ConvertFormat = value.ToString();
                Settings.Default.Save();
            }
        }
    } = Enum.Parse<Formats>(Settings.Default.ConvertFormat);

    public bool CanRemoveSilence
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                Settings.Default.CanRemoveSilence = value;
                Settings.Default.Save();
            }
        }
    } = Settings.Default.CanRemoveSilence;

    public float SilenceThreshold
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                Settings.Default.SilenceThreshold = value;
                Settings.Default.Save();
            }
        }
    } = Settings.Default.SilenceThreshold;

    public bool CanTranscribe
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                Settings.Default.CanTranscribe = value;
                Settings.Default.Save();
            }
        }
    } = Settings.Default.CanTranscribe;

    public string TranscribeModelName
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                Settings.Default.TranscribeModelName = value;
                Settings.Default.Save();
            }
        }
    } = Settings.Default.TranscribeModelName;

    public ICommand ShowCommand => new RelayCommand(ShowMain);

    public void ShowMain()
    {
        try
        {
            MainView? mainView = App.ServiceProvider?.GetRequiredService<MainView>();

            if (mainView == null)
            {
                logHelper?.AppendLog("Main view is null.");
                return;
            }

            mainView.DataContext = App.ServiceProvider?.GetRequiredService<MainViewModel>();
            System.Windows.Application.Current.MainWindow.Content = mainView;
        }
        catch (Exception ex)
        {
            logHelper?.AppendException(ex, "Error showing main.");
        }
    }
}
