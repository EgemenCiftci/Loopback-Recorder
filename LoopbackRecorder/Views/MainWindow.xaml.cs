using System.Windows;

namespace LoopbackRecorder.Views;

public partial class MainWindow : Window
{
    private readonly NotifyIcon? _notifyIcon;

    public MainWindow()
    {
        InitializeComponent();

        System.IO.Stream? iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/transcribe.ico"))?.Stream;

        _notifyIcon = new NotifyIcon
        {
            Icon = iconStream != null ? new Icon(iconStream) : SystemIcons.Application,
            Visible = false,
            Text = "Loopback Recorder"
        };

        ContextMenuStrip trayMenu = new();
        _ = trayMenu.Items.Add("Exit", null, (s, e) => System.Windows.Application.Current.Shutdown());

        _notifyIcon.ContextMenuStrip = trayMenu;

        _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
    }

    private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
    {
        Show();
        WindowState = WindowState.Normal;
        _ = Activate();
    }

    private void Window_StateChanged(object sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();

            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = true;
            }
        }
        else if (WindowState == WindowState.Normal && _notifyIcon != null)
        {
            _notifyIcon.Visible = false;
        }
    }
}