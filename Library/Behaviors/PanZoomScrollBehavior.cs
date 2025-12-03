using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Library.Behaviors;

/// <summary>
/// Pan/Zoom behavior that integrates with a ScrollViewer so edges are never exposed.
/// - Zoom uses LayoutTransform (ScaleTransform) applied to the Target element so extents match scaled size.
/// - Pan uses ScrollViewer offsets (no render translation), clamped on all sides with configurable hide margins.
/// - Supports Space+Left drag or Middle drag for panning; mouse wheel for zoom around cursor.
/// </summary>
public static class PanZoomScrollBehavior
{
    // Attach to ScrollViewer
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled", typeof(bool), typeof(PanZoomScrollBehavior), new PropertyMetadata(false, OnIsEnabledChanged));
    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    public static readonly DependencyProperty TargetElementProperty = DependencyProperty.RegisterAttached(
        "TargetElement", typeof(FrameworkElement), typeof(PanZoomScrollBehavior), new PropertyMetadata(null, OnTargetElementChanged));
    public static FrameworkElement? GetTargetElement(DependencyObject obj) => (FrameworkElement?)obj.GetValue(TargetElementProperty);
    public static void SetTargetElement(DependencyObject obj, FrameworkElement? value) => obj.SetValue(TargetElementProperty, value);

    public static readonly DependencyProperty MinScaleProperty = DependencyProperty.RegisterAttached(
        "MinScale", typeof(double), typeof(PanZoomScrollBehavior), new PropertyMetadata(0.2));
    public static double GetMinScale(DependencyObject obj) => (double)obj.GetValue(MinScaleProperty);
    public static void SetMinScale(DependencyObject obj, double value) => obj.SetValue(MinScaleProperty, value);

    public static readonly DependencyProperty MaxScaleProperty = DependencyProperty.RegisterAttached(
        "MaxScale", typeof(double), typeof(PanZoomScrollBehavior), new PropertyMetadata(8.0));
    public static double GetMaxScale(DependencyObject obj) => (double)obj.GetValue(MaxScaleProperty);
    public static void SetMaxScale(DependencyObject obj, double value) => obj.SetValue(MaxScaleProperty, value);

    public static readonly DependencyProperty WheelZoomFactorProperty = DependencyProperty.RegisterAttached(
        "WheelZoomFactor", typeof(double), typeof(PanZoomScrollBehavior), new PropertyMetadata(1.15));
    public static double GetWheelZoomFactor(DependencyObject obj) => (double)obj.GetValue(WheelZoomFactorProperty);
    public static void SetWheelZoomFactor(DependencyObject obj, double value) => obj.SetValue(WheelZoomFactorProperty, value);

    // Edge hide margins (DIPs)
    public static readonly DependencyProperty EdgeHideMarginProperty = DependencyProperty.RegisterAttached(
        "EdgeHideMargin", typeof(Thickness), typeof(PanZoomScrollBehavior), new PropertyMetadata(new Thickness(16), OnEdgeMarginChanged));
    public static Thickness GetEdgeHideMargin(DependencyObject obj) => (Thickness)obj.GetValue(EdgeHideMarginProperty);
    public static void SetEdgeHideMargin(DependencyObject obj, Thickness value) => obj.SetValue(EdgeHideMarginProperty, value);

    private class State
    {
        public FrameworkElement? Target;
        public ScaleTransform? Scale;
        public bool IsSpaceDown;
        public bool IsDragging;
        public Point Last;
        public double ScaleValue = 1.0;
        public Cursor? OriginalCursor;
    }

    private static readonly Dictionary<ScrollViewer, State> States = new();

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScrollViewer sv) return;

        if ((bool)e.NewValue)
        {
            if (!States.ContainsKey(sv)) States[sv] = new State();
            sv.Loaded += OnLoaded;
            sv.Unloaded += OnUnloaded;
            sv.PreviewKeyDown += OnKeyDown;
            sv.PreviewKeyUp += OnKeyUp;
            sv.PreviewMouseDown += OnMouseDown;
            sv.PreviewMouseMove += OnMouseMove;
            sv.PreviewMouseUp += OnMouseUp;
            sv.PreviewMouseWheel += OnMouseWheel;
            sv.ScrollChanged += OnScrollOrSizeChanged;
            sv.SizeChanged += OnScrollOrSizeChanged;
            EnsureScale(sv);
            ClampOffsets(sv);
        }
        else
        {
            if (States.Remove(sv))
            {
                sv.Loaded -= OnLoaded;
                sv.Unloaded -= OnUnloaded;
                sv.PreviewKeyDown -= OnKeyDown;
                sv.PreviewKeyUp -= OnKeyUp;
                sv.PreviewMouseDown -= OnMouseDown;
                sv.PreviewMouseMove -= OnMouseMove;
                sv.PreviewMouseUp -= OnMouseUp;
                sv.PreviewMouseWheel -= OnMouseWheel;
                sv.ScrollChanged -= OnScrollOrSizeChanged;
                sv.SizeChanged -= OnScrollOrSizeChanged;
            }
        }
    }

    private static void OnTargetElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer sv && States.TryGetValue(sv, out var st))
        {
            st.Target = e.NewValue as FrameworkElement;
            EnsureScale(sv);
            ClampOffsets(sv);
        }
    }

    private static void OnEdgeMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer sv)
        {
            ClampOffsets(sv);
        }
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is ScrollViewer sv) { EnsureScale(sv); ClampOffsets(sv); }
    }

    private static void OnUnloaded(object sender, RoutedEventArgs e) { /* cleanup on disable */ }

    private static void EnsureScale(ScrollViewer sv)
    {
        if (!States.TryGetValue(sv, out var st)) return;
        var target = st.Target ?? GetTargetElement(sv) as FrameworkElement;
        if (target == null) return;
        st.Target = target;

        // Ensure LayoutTransform has a ScaleTransform
        if (target.LayoutTransform is not ScaleTransform scale)
        {
            scale = new ScaleTransform(1.0, 1.0);
            target.LayoutTransform = scale;
        }
        st.Scale = scale;
        st.ScaleValue = scale.ScaleX; // uniform
    }

    private static void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not ScrollViewer sv || !States.TryGetValue(sv, out var st)) return;
        if (e.Key == Key.Space)
        {
            st.IsSpaceDown = true;
            if (!st.IsDragging)
            {
                st.OriginalCursor ??= Mouse.OverrideCursor;
                Mouse.OverrideCursor = Cursors.ScrollAll;
            }
            e.Handled = true;
        }
    }

    private static void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (sender is not ScrollViewer sv || !States.TryGetValue(sv, out var st)) return;
        if (e.Key == Key.Space)
        {
            st.IsSpaceDown = false;
            if (st.IsDragging)
            {
                st.IsDragging = false;
                Mouse.Capture(null);
            }
            Mouse.OverrideCursor = st.OriginalCursor;
            st.OriginalCursor = null;
            e.Handled = true;
        }
    }

    private static void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ScrollViewer sv || !States.TryGetValue(sv, out var st) || st.Target is null) return;
        if (e.ChangedButton == MouseButton.Middle || (e.ChangedButton == MouseButton.Left && st.IsSpaceDown))
        {
            st.IsDragging = true;
            st.Last = e.GetPosition(sv);
            Mouse.Capture(sv);
            e.Handled = true;
        }
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not ScrollViewer sv || !States.TryGetValue(sv, out var st) || !st.IsDragging) return;
        var pos = e.GetPosition(sv);
        var dx = pos.X - st.Last.X;
        var dy = pos.Y - st.Last.Y;
        st.Last = pos;

        // Scroll offsets move opposite to drag direction
        sv.ScrollToHorizontalOffset(sv.HorizontalOffset - dx);
        sv.ScrollToVerticalOffset(sv.VerticalOffset - dy);
        ClampOffsets(sv);
        e.Handled = true;
    }

    private static void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ScrollViewer sv || !States.TryGetValue(sv, out var st)) return;
        if (st.IsDragging)
        {
            st.IsDragging = false;
            Mouse.Capture(null);
            e.Handled = true;
        }
    }

    private static void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not ScrollViewer sv || !States.TryGetValue(sv, out var st) || st.Target is null) return;
        EnsureScale(sv);
        if (st.Scale is null) return;

        double factor = e.Delta > 0 ? GetWheelZoomFactor(sv) : 1.0 / GetWheelZoomFactor(sv);
        double min = ComputeEffectiveMinScale(sv, st);
        double max = GetMaxScale(sv);

        // Anchor zoom around cursor position within the ScrollViewer
        var mouse = e.GetPosition(sv);
        double oldScale = st.ScaleValue;
        double newScale = Math.Clamp(oldScale * factor, min, max);
        if (Math.Abs(newScale - oldScale) < 0.0001) { e.Handled = true; return; }

        // Compute content point under cursor in scaled space
        double contentX = sv.HorizontalOffset + mouse.X;
        double contentY = sv.VerticalOffset + mouse.Y;

        st.ScaleValue = newScale;
        st.Scale.ScaleX = newScale;
        st.Scale.ScaleY = newScale;

        // Keep the same content point under the cursor after scaling
        double desiredOffsetX = contentX * (newScale / oldScale) - mouse.X;
        double desiredOffsetY = contentY * (newScale / oldScale) - mouse.Y;
        sv.ScrollToHorizontalOffset(desiredOffsetX);
        sv.ScrollToVerticalOffset(desiredOffsetY);

        ClampOffsets(sv);
        e.Handled = true;
    }

    private static void OnScrollOrSizeChanged(object sender, EventArgs e)
    {
        if (sender is ScrollViewer sv)
        {
            ClampOffsets(sv);
        }
    }

    private static double ComputeEffectiveMinScale(ScrollViewer sv, State st)
    {
        var target = st.Target;
        if (target == null) return GetMinScale(sv);
        double vw = Math.Max(0, sv.ViewportWidth);
        double vh = Math.Max(0, sv.ViewportHeight);
        double cw = Math.Max(0, target.ActualWidth);
        double ch = Math.Max(0, target.ActualHeight);
        if (vw <= 0 || vh <= 0 || cw <= 0 || ch <= 0) return GetMinScale(sv);
        double needed = Math.Max(vw / cw, vh / ch); // fit-to-viewport min
        return Math.Max(GetMinScale(sv), needed);
    }

    private static void ClampOffsets(ScrollViewer sv)
    {
        if (!States.TryGetValue(sv, out var st) || st.Target is null || st.Scale is null) return;
        var m = GetEdgeHideMargin(sv);

        double vw = Math.Max(0, sv.ViewportWidth);
        double vh = Math.Max(0, sv.ViewportHeight);
        double cw = Math.Max(0, st.Target.ActualWidth * st.ScaleValue);
        double ch = Math.Max(0, st.Target.ActualHeight * st.ScaleValue);

        // When content smaller than viewport, center it and set offsets to 0 (ScrollViewer centers content automatically if alignment is centered; we clamp to 0 to avoid negative offsets)
        double maxX = Math.Max(0, cw - vw - m.Right);
        double maxY = Math.Max(0, ch - vh - m.Bottom);
        double minX = 0 + m.Left;
        double minY = 0 + m.Top;

        // If margins exceed available scroll, clamp to 0 range
        if (maxX < minX) { minX = 0; maxX = 0; }
        if (maxY < minY) { minY = 0; maxY = 0; }

        double newX = Math.Max(minX, Math.Min(maxX, sv.HorizontalOffset));
        double newY = Math.Max(minY, Math.Min(maxY, sv.VerticalOffset));
        if (!DoubleUtil.AreClose(newX, sv.HorizontalOffset)) sv.ScrollToHorizontalOffset(newX);
        if (!DoubleUtil.AreClose(newY, sv.VerticalOffset)) sv.ScrollToVerticalOffset(newY);
    }

    private static class DoubleUtil
    {
        public static bool AreClose(double a, double b) => Math.Abs(a - b) < 0.5; // within half a DIP is fine
    }
}
