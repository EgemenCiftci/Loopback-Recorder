using CommunityToolkit.Mvvm.ComponentModel;

namespace LoopbackRecorder.Helpers;

public class LogHelper : ObservableObject
{
    public string? Log
    {
        get;
        set => SetProperty(ref field, value);
    }

    public void AppendLog(string message)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            Log += $"{DateTime.Now:HH:mm:ss.fff} - {message}\n";
        });
    }

    public void AppendException(Exception ex, string message = "An error occurred")
    {
        AppendLog($"{message}: {ex.Message}");
        AppendLog(ex.StackTrace ?? "No stack trace available.");
    }
}
