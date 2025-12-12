using System.Windows;
using System.Windows.Input;
using UX.ViewModels.LabelsEditorViewModel;

namespace UX.Views.LabelsEditor;

public partial class LabelsEditorWindow
{
    private bool _isDragging;

    public LabelsEditorWindow()
    {
        // Ensure DataContext is available before InitializeComponent so behaviors can bind LoadedCommand
        DataContext ??= new LabelsEditorViewModel();
        InitializeComponent();
    }

    private LabelsEditorViewModel? VM => DataContext as LabelsEditorViewModel;

    private void OnMapMouseDown(object sender, MouseButtonEventArgs e)
    {
        var pos = e.GetPosition(MapGrid);
        VM?.BeginDragAt(pos.X, pos.Y);
        _isDragging = true;
        Mouse.Capture(MapGrid);
    }

    private void OnMapMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        var pos = e.GetPosition(MapGrid);
        VM?.DragTo(pos.X, pos.Y);
    }

    private void OnMapMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging) return;
        _isDragging = false;
        Mouse.Capture(null);
        VM?.EndDragAndSave();
    }
}
