using System.Windows;
using System.Windows.Input;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace Library.Behaviors;

public static class PaintSurfaceBehavior
{
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(PaintSurfaceBehavior), new PropertyMetadata(OnCommandChanged));

    public static ICommand? GetCommand(DependencyObject obj) => (ICommand?)obj.GetValue(CommandProperty);
    public static void SetCommand(DependencyObject obj, ICommand? value) => obj.SetValue(CommandProperty, value);

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SKElement element) return;
        
        element.PaintSurface -= OnPaintSurface;
        if (e.NewValue is ICommand)
            element.PaintSurface += OnPaintSurface;
    }

    private static void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        if (sender is SKElement element)
        {
            var command = GetCommand(element);
            command?.Execute(e);
        }
    }
}
