using System.Windows;
using System.Windows.Input;

namespace Library.Behaviors;

public class LoadedBehavior
{

    public static DependencyProperty LoadedCommandProperty = DependencyProperty.RegisterAttached(
        "LoadedCommand", typeof(ICommand), typeof(LoadedBehavior), new PropertyMetadata(null, OnLoadedCommandChanged));

    private static void OnLoadedCommandChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
    {
        if (depObj is FrameworkElement frameworkElement && e.NewValue is ICommand)
        {
            frameworkElement.Loaded += (_, _) => { (e.NewValue as ICommand)?.Execute(null); };
        }
    }

    public static ICommand GetLoadedCommand(DependencyObject depObj)
    {
        return (ICommand) depObj.GetValue(LoadedCommandProperty);
    }

    public static void SetLoadedCommand(DependencyObject depObj, ICommand value)
    {
        depObj.SetValue(LoadedCommandProperty, value);
    }

}
