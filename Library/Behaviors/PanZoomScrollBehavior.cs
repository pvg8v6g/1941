using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Library.Utilities;
using Microsoft.Xaml.Behaviors;

namespace Library.Behaviors;

/// <summary>
/// Pan/Zoom behavior that integrates with a ScrollViewer so edges are never exposed.
/// - Zoom uses LayoutTransform (ScaleTransform) applied to the Target element so extents match scaled size.
/// - Pan uses ScrollViewer offsets (no render translation), clamped on all sides with configurable hide margins.
/// - Supports Space+Left drag or Middle drag for panning; mouse wheel for zoom around cursor.
/// </summary>
public class PanZoomScrollBehavior : Behavior<ScrollViewer>
{

    #region Dependency Properties

    public static readonly DependencyProperty TargetElementProperty = DependencyProperty.Register(nameof(TargetElement), typeof(FrameworkElement), typeof(PanZoomScrollBehavior),
        new PropertyMetadata(null, OnTargetElementChanged));

    public FrameworkElement? TargetElement
    {
        get => (FrameworkElement?) GetValue(TargetElementProperty);
        set => SetValue(TargetElementProperty, value);
    }

    public static readonly DependencyProperty MinScaleProperty = DependencyProperty.Register(
        nameof(MinScale), typeof(double), typeof(PanZoomScrollBehavior), new PropertyMetadata(0.2));

    public double MinScale
    {
        get => (double) GetValue(MinScaleProperty);
        set => SetValue(MinScaleProperty, value);
    }

    public static readonly DependencyProperty MaxScaleProperty = DependencyProperty.Register(
        nameof(MaxScale), typeof(double), typeof(PanZoomScrollBehavior), new PropertyMetadata(8.0));

    public double MaxScale
    {
        get => (double) GetValue(MaxScaleProperty);
        set => SetValue(MaxScaleProperty, value);
    }

    public static readonly DependencyProperty WheelZoomFactorProperty = DependencyProperty.Register(
        nameof(WheelZoomFactor), typeof(double), typeof(PanZoomScrollBehavior), new PropertyMetadata(1.15));

    public double WheelZoomFactor
    {
        get => (double) GetValue(WheelZoomFactorProperty);
        set => SetValue(WheelZoomFactorProperty, value);
    }

    public static readonly DependencyProperty EdgeHideMarginProperty = DependencyProperty.Register(nameof(EdgeHideMargin), typeof(Thickness), typeof(PanZoomScrollBehavior),
        new PropertyMetadata(new Thickness(16), OnEdgeMarginChanged));

    public Thickness EdgeHideMargin
    {
        get => (Thickness) GetValue(EdgeHideMarginProperty);
        set => SetValue(EdgeHideMarginProperty, value);
    }

    private static void OnTargetElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PanZoomScrollBehavior b && b.AssociatedObject is not null)
        {
            b._target = e.NewValue as FrameworkElement;
            b.EnsureScale();
            b.ClampOffsets();
        }
    }

    private static void OnEdgeMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PanZoomScrollBehavior b && b.AssociatedObject is not null)
        {
            b.ClampOffsets();
        }
    }

    #endregion

    #region Instance State

    private FrameworkElement? _target;
    private ScaleTransform? _scale;
    private bool _isSpaceDown;
    private bool _isDragging;
    private Point _last;
    private double _scaleValue = 1.0;
    private CacheMode? _originalCacheMode;
    private bool _cacheApplied;

    #endregion

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject is null) return;

        AssociatedObject.Loaded += OnLoaded;
        AssociatedObject.Unloaded += OnUnloaded;
        AssociatedObject.PreviewKeyDown += OnKeyDown;
        AssociatedObject.PreviewKeyUp += OnKeyUp;
        AssociatedObject.PreviewMouseDown += OnMouseDown;
        AssociatedObject.PreviewMouseMove += OnMouseMove;
        AssociatedObject.PreviewMouseUp += OnMouseUp;
        AssociatedObject.LostMouseCapture += OnLostMouseCapture;
        AssociatedObject.PreviewMouseWheel += OnMouseWheel;
        AssociatedObject.ScrollChanged += OnScrollOrSizeChanged;
        AssociatedObject.SizeChanged += OnScrollOrSizeChanged;
        AssociatedObject.Focusable = true;

        EnsureScale();
        ClampOffsets();
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is null) return;

        Mouse.Capture(null);

        // Ensure we restore any temporary cache
        if (_target is not null && _cacheApplied)
        {
            _target.CacheMode = _originalCacheMode;
            _originalCacheMode = null;
            _cacheApplied = false;
        }

        AssociatedObject.Loaded -= OnLoaded;
        AssociatedObject.Unloaded -= OnUnloaded;
        AssociatedObject.PreviewKeyDown -= OnKeyDown;
        AssociatedObject.PreviewKeyUp -= OnKeyUp;
        AssociatedObject.PreviewMouseDown -= OnMouseDown;
        AssociatedObject.PreviewMouseMove -= OnMouseMove;
        AssociatedObject.PreviewMouseUp -= OnMouseUp;
        AssociatedObject.LostMouseCapture -= OnLostMouseCapture;
        AssociatedObject.PreviewMouseWheel -= OnMouseWheel;
        AssociatedObject.ScrollChanged -= OnScrollOrSizeChanged;
        AssociatedObject.SizeChanged -= OnScrollOrSizeChanged;

        base.OnDetaching();
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        EnsureScale();
        ClampOffsets();
        AssociatedObject?.Dispatcher.BeginInvoke(new Action(() => Keyboard.Focus(AssociatedObject)));
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        // no-op
    }

    private void EnsureScale()
    {
        if (AssociatedObject is null) return;
        var target = _target ?? TargetElement;
        if (target is null) return;
        _target = target;

        if (target.LayoutTransform is not ScaleTransform scale)
        {
            scale = new ScaleTransform(1.0, 1.0);
            target.LayoutTransform = scale;
        }

        _scale = scale;
        _scaleValue = scale.ScaleX; // uniform
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (AssociatedObject is null) return;
        if (e.Key is not Key.Space) return;
        _isSpaceDown = true;
        if (!_isDragging) Mouse.OverrideCursor = Cursors.ScrollAll;
        e.Handled = true;
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (AssociatedObject is null) return;
        if (e.Key is not Key.Space) return;
        _isSpaceDown = false;
        if (_isDragging)
        {
            _isDragging = false;
            Mouse.Capture(null);
            // Restore cache when drag ends via Space release
            if (_target is not null && _cacheApplied)
            {
                _target.CacheMode = _originalCacheMode;
                _originalCacheMode = null;
                _cacheApplied = false;
            }
        }

        Mouse.OverrideCursor = Global.GameCursor;
        e.Handled = true;
    }

    private void OnMouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (AssociatedObject is null || _target is null) return;
        if (e.ChangedButton is not (MouseButton.Left or MouseButton.Middle)) return;
        if (e.ChangedButton is MouseButton.Left && !_isSpaceDown) return;

        _isDragging = true;
        _last = e.GetPosition(AssociatedObject);
        Mouse.Capture(AssociatedObject);
        Mouse.OverrideCursor = Cursors.SizeAll;
        // Apply temporary bitmap cache while dragging to improve responsiveness
        if (!_cacheApplied)
        {
            _originalCacheMode = _target.CacheMode;
            _target.CacheMode = new BitmapCache(1.0);
            _cacheApplied = true;
        }

        e.Handled = true;
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        if (AssociatedObject is null || !_isDragging) return;
        var pos = e.GetPosition(AssociatedObject);
        var dx = pos.X - _last.X;
        var dy = pos.Y - _last.Y;
        _last = pos;

        AssociatedObject.ScrollToHorizontalOffset(AssociatedObject.HorizontalOffset - dx);
        AssociatedObject.ScrollToVerticalOffset(AssociatedObject.VerticalOffset - dy);
        ClampOffsets();
        e.Handled = true;
    }

    private void OnMouseUp(object? sender, MouseButtonEventArgs e)
    {
        if (AssociatedObject is null) return;
        if (!_isDragging) return;
        _isDragging = false;
        Mouse.Capture(null);
        Mouse.OverrideCursor = Global.GameCursor;
        // Restore cache when drag ends
        if (_target is not null && _cacheApplied)
        {
            _target.CacheMode = _originalCacheMode;
            _originalCacheMode = null;
            _cacheApplied = false;
        }

        e.Handled = true;
    }

    private void OnLostMouseCapture(object? sender, MouseEventArgs e)
    {
        if (AssociatedObject is null) return;
        if (!_isDragging) return;
        _isDragging = false;
        Mouse.Capture(null);
        // Restore cache when capture is lost
        if (_target is null || !_cacheApplied) return;
        _target.CacheMode = _originalCacheMode;
        _originalCacheMode = null;
        _cacheApplied = false;
    }

    private void OnMouseWheel(object? sender, MouseWheelEventArgs e)
    {
        if (AssociatedObject is null || _target is null) return;
        EnsureScale();
        if (_scale is null) return;

        var factor = e.Delta > 0 ? WheelZoomFactor : 1.0 / WheelZoomFactor;
        var min = ComputeEffectiveMinScale();
        var max = MaxScale;

        var mouse = e.GetPosition(AssociatedObject);
        var oldScale = _scaleValue;
        var newScale = Math.Clamp(oldScale * factor, min, max);
        if (Math.Abs(newScale - oldScale) < 0.0001)
        {
            e.Handled = true;
            return;
        }

        var contentX = AssociatedObject.HorizontalOffset + mouse.X;
        var contentY = AssociatedObject.VerticalOffset + mouse.Y;

        _scaleValue = newScale;
        _scale.ScaleX = newScale;
        _scale.ScaleY = newScale;

        var desiredOffsetX = contentX * (newScale / oldScale) - mouse.X;
        var desiredOffsetY = contentY * (newScale / oldScale) - mouse.Y;
        AssociatedObject.ScrollToHorizontalOffset(desiredOffsetX);
        AssociatedObject.ScrollToVerticalOffset(desiredOffsetY);

        ClampOffsets();
        e.Handled = true;
    }

    private void OnScrollOrSizeChanged(object? sender, EventArgs e)
    {
        if (AssociatedObject is null) return;
        ClampOffsets();
    }

    private double ComputeEffectiveMinScale()
    {
        if (AssociatedObject is null) return MinScale;
        var target = _target;
        if (target is null) return MinScale;
        var vw = Math.Max(0, AssociatedObject.ViewportWidth);
        var vh = Math.Max(0, AssociatedObject.ViewportHeight);
        var cw = Math.Max(0, target.ActualWidth);
        var ch = Math.Max(0, target.ActualHeight);
        if (vw <= 0 || vh <= 0 || cw <= 0 || ch <= 0) return MinScale;
        var needed = Math.Max(vw / cw, vh / ch);
        return Math.Max(MinScale, needed);
    }

    private void ClampOffsets()
    {
        if (AssociatedObject is null || _target is null || _scale is null) return;
        var m = EdgeHideMargin;

        var vw = Math.Max(0, AssociatedObject.ViewportWidth);
        var vh = Math.Max(0, AssociatedObject.ViewportHeight);
        var cw = Math.Max(0, _target.ActualWidth * _scaleValue);
        var ch = Math.Max(0, _target.ActualHeight * _scaleValue);

        var maxX = Math.Max(0, cw - vw - m.Right);
        var maxY = Math.Max(0, ch - vh - m.Bottom);
        var minX = 0 + m.Left;
        var minY = 0 + m.Top;

        if (maxX < minX)
        {
            minX = 0;
            maxX = 0;
        }

        if (maxY < minY)
        {
            minY = 0;
            maxY = 0;
        }

        var newX = Math.Max(minX, Math.Min(maxX, AssociatedObject.HorizontalOffset));
        var newY = Math.Max(minY, Math.Min(maxY, AssociatedObject.VerticalOffset));
        if (!DoubleUtil.AreClose(newX, AssociatedObject.HorizontalOffset)) AssociatedObject.ScrollToHorizontalOffset(newX);
        if (!DoubleUtil.AreClose(newY, AssociatedObject.VerticalOffset)) AssociatedObject.ScrollToVerticalOffset(newY);
    }

    private static class DoubleUtil
    {

        public static bool AreClose(double a, double b) => Math.Abs(a - b) < 0.5;

    }

}
