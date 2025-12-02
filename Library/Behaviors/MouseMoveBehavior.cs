using System.Windows;
using System.Windows.Input;

namespace Library.Behaviors;

public static class MouseMoveBehavior
{
    public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
        "Command",
        typeof(ICommand),
        typeof(MouseMoveBehavior),
        new PropertyMetadata(null, OnCommandChanged));

    public static void SetCommand(DependencyObject element, ICommand? value) => element.SetValue(CommandProperty, value);
    public static ICommand? GetCommand(DependencyObject element) => (ICommand?)element.GetValue(CommandProperty);

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement ui)
        {
            // detach old
            if (e.OldValue is ICommand)
            {
                ui.MouseMove -= UiOnMouseMove;
            }

            // attach new
            if (e.NewValue is ICommand)
            {
                ui.MouseMove += UiOnMouseMove;
            }
        }
    }

    private static void UiOnMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is UIElement ui)
        {
            var cmd = GetCommand(ui);
            if (cmd is null) return;
            var p = e.GetPosition(ui);
            if (cmd.CanExecute(p))
                cmd.Execute(p);
        }
    }
}
