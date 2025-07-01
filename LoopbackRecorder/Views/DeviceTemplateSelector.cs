using System.Windows;
using System.Windows.Controls;

namespace LoopbackRecorder.Views;

public class DeviceTemplateSelector : DataTemplateSelector
{
    public DataTemplate? DeviceTemplate { get; set; }

    public DataTemplate? NoneDeviceTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        return item == null ? NoneDeviceTemplate : DeviceTemplate;
    }
}