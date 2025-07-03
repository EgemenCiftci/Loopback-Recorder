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
    private Formats outputFormat = Enum.Parse<Formats>(Settings.Default.OutputFormat);
    private readonly LogHelper logHelper = App.serviceProvider.GetRequiredService<LogHelper>();

    public Formats OutputFormat
    {
        get => outputFormat;
        set
        {
            if (SetProperty(ref outputFormat, value))
            {
                Settings.Default.OutputFormat = value.ToString();
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

    public ICommand ShowMainCommand => new RelayCommand(ShowMain);

    public void ShowMain()
    {
        try
        {
            MainView mainView = App.serviceProvider.GetRequiredService<MainView>();
            mainView.DataContext = App.serviceProvider.GetRequiredService<MainViewModel>();
            Application.Current.MainWindow.Content = mainView;
        }
        catch (Exception ex)
        {
            logHelper.AppendException(ex, "Error showing main.");
        }
    }
}
