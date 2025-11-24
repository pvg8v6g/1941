using System.ComponentModel;
using System.Windows;
using SkiaSharp.Views.WPF;
using System.Linq;

namespace Library.Behaviors;

public static class InvalidateBehavior
{
    public static readonly DependencyProperty TriggerProperty =
        DependencyProperty.RegisterAttached("Trigger", typeof(INotifyPropertyChanged), typeof(InvalidateBehavior), new PropertyMetadata(OnTriggerChanged));

    public static readonly DependencyProperty PropertyNameProperty =
        DependencyProperty.RegisterAttached("PropertyName", typeof(string), typeof(InvalidateBehavior));

    public static INotifyPropertyChanged? GetTrigger(DependencyObject obj) => (INotifyPropertyChanged?)obj.GetValue(TriggerProperty);
    public static void SetTrigger(DependencyObject obj, INotifyPropertyChanged? value) => obj.SetValue(TriggerProperty, value);

    public static string? GetPropertyName(DependencyObject obj) => (string?)obj.GetValue(PropertyNameProperty);
    public static void SetPropertyName(DependencyObject obj, string? value) => obj.SetValue(PropertyNameProperty, value);

    private static void OnTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SKElement element) return;

        if (e.NewValue is INotifyPropertyChanged newNotify)
            newNotify.PropertyChanged += OnPropertyChanged;
    }

    private static void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not INotifyPropertyChanged notify) return;
        
        foreach (var element in Application.Current.Windows.OfType<Window>().SelectMany(w => FindVisualChildren<SKElement>(w)))
        {
            if (GetTrigger(element) == notify)
            {
                var propertyName = GetPropertyName(element);
                if (e.PropertyName == propertyName)
                    element.InvalidateVisual();
            }
        }
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T t)
                yield return t;
            foreach (var descendant in FindVisualChildren<T>(child))
                yield return descendant;
        }
    }
}
