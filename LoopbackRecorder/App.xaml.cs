using LoopbackRecorder.Helpers;
using LoopbackRecorder.ViewModels;
using LoopbackRecorder.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace LoopbackRecorder;

public partial class App : Application
{
    public static IServiceProvider serviceProvider;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        ServiceCollection serviceCollection = new();
        ConfigureServices(serviceCollection);

        serviceProvider = serviceCollection.BuildServiceProvider();

        MainWindow mainWindow = serviceProvider.GetRequiredService<MainWindow>();
        MainView mainView = serviceProvider.GetRequiredService<MainView>();
        mainView.DataContext = serviceProvider.GetRequiredService<MainViewModel>();
        mainWindow.Content = mainView;
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        _ = services.AddSingleton<LogHelper>();
        _ = services.AddSingleton<TranscriptionHelper>();

        _ = services.AddSingleton<MainViewModel>();
        _ = services.AddSingleton<MainView>();

        _ = services.AddSingleton<SettingsViewModel>();
        _ = services.AddSingleton<SettingsView>();

        _ = services.AddSingleton<MainWindow>();
    }
}
