using LoopbackRecorder.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace LoopbackRecorder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ServiceCollection serviceCollection = new();
            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            MainView mainView = _serviceProvider.GetRequiredService<MainView>();
            mainView.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
            mainView.Show();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            //_ = services.AddSingleton<IItemService, ItemService>();
            //_ = services.AddScoped<IItemRepository, ItemRepository>();
            _ = services.AddSingleton<MainViewModel>();
            _ = services.AddSingleton<MainView>();
            //_ = services.AddSingleton<Window>();
        }
    }

}
