using System.Windows;
using System.Windows.Input;

namespace Library.Behaviors;

public static class MouseLeaveBehavior
{
    public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
        "Command",
        typeof(ICommand),
        typeof(MouseLeaveBehavior),
        new PropertyMetadata(null, OnCommandChanged));

    public static void SetCommand(DependencyObject element, ICommand? value) => element.SetValue(CommandProperty, value);
    public static ICommand? GetCommand(DependencyObject element) => (ICommand?)element.GetValue(CommandProperty);

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement ui)
        {
            if (e.OldValue is ICommand)
            {
                ui.MouseLeave -= UiOnMouseLeave;
            }

            if (e.NewValue is ICommand)
            {
                ui.MouseLeave += UiOnMouseLeave;
            }
        }
    }

    private static void UiOnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is UIElement ui)
        {
            var cmd = GetCommand(ui);
            if (cmd is null) return;
            if (cmd.CanExecute(null))
                cmd.Execute(null);
        }
    }
}
