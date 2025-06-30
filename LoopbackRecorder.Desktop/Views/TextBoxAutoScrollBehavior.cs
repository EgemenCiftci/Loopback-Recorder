using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;

namespace LoopbackRecorder.Desktop.Views;

public class TextBoxAutoScrollBehavior : Behavior<TextBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.TextChanged += AssociatedObject_TextChanged;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.TextChanged -= AssociatedObject_TextChanged;
        base.OnDetaching();
    }

    private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
    {
        AssociatedObject.CaretIndex = AssociatedObject.Text.Length;
        AssociatedObject.ScrollToEnd();
    }
}