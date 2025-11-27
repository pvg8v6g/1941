using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Library.Behaviors;

/// <summary>
/// Attached behavior to enable Space+Left-drag panning and mouse wheel zooming
/// on any FrameworkElement. Works with a RenderTransform that contains a
/// ScaleTransform followed by a TranslateTransform (in that order). If missing,
/// it will create them.
/// </summary>
public static class PanZoomBehavior
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled", typeof(bool), typeof(PanZoomBehavior), new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    public static readonly DependencyProperty MinScaleProperty = DependencyProperty.RegisterAttached(
        "MinScale", typeof(double), typeof(PanZoomBehavior), new PropertyMetadata(0.2));
    public static double GetMinScale(DependencyObject obj) => (double)obj.GetValue(MinScaleProperty);
    public static void SetMinScale(DependencyObject obj, double value) => obj.SetValue(MinScaleProperty, value);

    public static readonly DependencyProperty MaxScaleProperty = DependencyProperty.RegisterAttached(
        "MaxScale", typeof(double), typeof(PanZoomBehavior), new PropertyMetadata(8.0));
    public static double GetMaxScale(DependencyObject obj) => (double)obj.GetValue(MaxScaleProperty);
    public static void SetMaxScale(DependencyObject obj, double value) => obj.SetValue(MaxScaleProperty, value);

    public static readonly DependencyProperty WheelZoomFactorProperty = DependencyProperty.RegisterAttached(
        "WheelZoomFactor", typeof(double), typeof(PanZoomBehavior), new PropertyMetadata(1.15));
    public static double GetWheelZoomFactor(DependencyObject obj) => (double)obj.GetValue(WheelZoomFactorProperty);
    public static void SetWheelZoomFactor(DependencyObject obj, double value) => obj.SetValue(WheelZoomFactorProperty, value);

    public static readonly DependencyProperty TargetElementProperty = DependencyProperty.RegisterAttached(
        "TargetElement", typeof(FrameworkElement), typeof(PanZoomBehavior), new PropertyMetadata(null, OnTargetElementChanged));
    public static FrameworkElement? GetTargetElement(DependencyObject obj) => (FrameworkElement?)obj.GetValue(TargetElementProperty);
    public static void SetTargetElement(DependencyObject obj, FrameworkElement? value) => obj.SetValue(TargetElementProperty, value);

    private class State
    {
        public bool IsSpaceDown;
        public bool IsPanning;
        public Point LastMouse;
        public double Scale = 1.0;
        public ScaleTransform? ScaleTransform;
        public TranslateTransform? TranslateTransform;
        public FrameworkElement? Target; // element that actually gets transformed
        public bool SubscribedToTargetSize;
        public Cursor? OriginalCursor; // cursor to restore when exiting interaction
    }

    private static readonly Dictionary<FrameworkElement, State> States = new();

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element) return;

        var enable = (bool)e.NewValue;
        if (enable)
        {
            if (!States.ContainsKey(element)) States[element] = new State();

            element.Loaded += OnElementLoaded;
            element.Unloaded += OnElementUnloaded;

            // Re-apply constraints when attached element (viewport) size changes
            element.SizeChanged += OnAnySizeChanged;

            element.PreviewKeyDown += OnPreviewKeyDown;
            element.PreviewKeyUp += OnPreviewKeyUp;
            // Use tunneling mouse events so we receive input even when children (e.g., SKElement) handle bubbling events
            element.PreviewMouseDown += OnPreviewMouseDown;
            element.PreviewMouseMove += OnPreviewMouseMove;
            element.PreviewMouseUp += OnPreviewMouseUp;
            element.PreviewMouseWheel += OnPreviewMouseWheel;

            EnsureTransformsForAttachedElement(element);

            // Ensure keyboard focus to receive key events
            element.Focusable = true;
        }
        else
        {
            if (States.Remove(element))
            {
                // Ensure we release capture and restore cursor if we were panning
                // when the behavior is disabled.
                Mouse.Capture(null);
                Mouse.OverrideCursor = null;
                element.Loaded -= OnElementLoaded;
                element.Unloaded -= OnElementUnloaded;

                element.SizeChanged -= OnAnySizeChanged;

                element.PreviewKeyDown -= OnPreviewKeyDown;
                element.PreviewKeyUp -= OnPreviewKeyUp;
                element.PreviewMouseDown -= OnPreviewMouseDown;
                element.PreviewMouseMove -= OnPreviewMouseMove;
                element.PreviewMouseUp -= OnPreviewMouseUp;
                element.PreviewMouseWheel -= OnPreviewMouseWheel;
            }
        }
    }

    private static void OnTargetElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element) return;
        if (!States.TryGetValue(element, out var state))
        {
            // If behavior not enabled yet, state will be created on enable; nothing to do now.
            return;
        }

        state.Target = e.NewValue as FrameworkElement ?? element;
        EnsureTransforms(element, state.Target);

        // Subscribe to target size changes to enforce constraints
        if (state.Target != null && !state.SubscribedToTargetSize)
        {
            state.Target.SizeChanged += OnAnySizeChanged;
            state.SubscribedToTargetSize = true;
        }
        ApplyConstraints(element, state);
    }

    private static void OnElementLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            EnsureTransformsForAttachedElement(element);
            element.Dispatcher.BeginInvoke(new Action(() => Keyboard.Focus(element)));
        }
    }

    private static void OnElementUnloaded(object sender, RoutedEventArgs e)
    {
        // Nothing special; cleanup happens on disable.
    }

    private static void EnsureTransformsForAttachedElement(FrameworkElement element)
    {
        if (!States.TryGetValue(element, out var state)) return;
        state.Target = GetTargetElement(element) ?? state.Target ?? element;
        EnsureTransforms(element, state.Target);

        // Subscribe to target size changes once
        if (state.Target != null && !state.SubscribedToTargetSize)
        {
            state.Target.SizeChanged += OnAnySizeChanged;
            state.SubscribedToTargetSize = true;
        }
        ApplyConstraints(element, state);
    }

    private static void EnsureTransforms(FrameworkElement attachedElement, FrameworkElement target)
    {
        if (!States.TryGetValue(attachedElement, out var state)) return;

        TransformGroup? group = target.RenderTransform as TransformGroup;
        if (group == null)
        {
            group = new TransformGroup();
            target.RenderTransform = group;
        }

        // Try to find first ScaleTransform and first TranslateTransform
        ScaleTransform? scale = null;
        TranslateTransform? translate = null;
        foreach (var t in group.Children)
        {
            if (scale == null && t is ScaleTransform s) scale = s;
            if (translate == null && t is TranslateTransform tt) translate = tt;
        }

        if (scale == null)
        {
            scale = new ScaleTransform(1.0, 1.0);
            group.Children.Insert(0, scale);
        }

        if (translate == null)
        {
            translate = new TranslateTransform(0.0, 0.0);
            // Insert after scale to keep order Scale then Translate
            int scaleIndex = group.Children.IndexOf(scale);
            group.Children.Insert(Math.Min(scaleIndex + 1, group.Children.Count), translate);
        }

        state.ScaleTransform = scale;
        state.TranslateTransform = translate;
        state.Scale = state.ScaleTransform.ScaleX; // assume uniform
    }

    private static void OnAnySizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Find the attached element this state belongs to and re-apply constraints
        foreach (var kvp in States)
        {
            var attached = kvp.Key;
            var state = kvp.Value;
            if (sender == attached || sender == state.Target)
            {
                ApplyConstraints(attached, state);
            }
        }
    }

    private static void GetSizes(FrameworkElement attachedElement, FrameworkElement target, out double viewportW, out double viewportH, out double contentW, out double contentH)
    {
        viewportW = Math.Max(0, attachedElement.ActualWidth);
        viewportH = Math.Max(0, attachedElement.ActualHeight);
        contentW = Math.Max(0, target.ActualWidth);
        contentH = Math.Max(0, target.ActualHeight);
    }

    private static double ComputeEffectiveMinScale(FrameworkElement attachedElement, State state)
    {
        var userMin = GetMinScale(attachedElement);
        var target = state.Target ?? attachedElement;
        GetSizes(attachedElement, target, out var vw, out var vh, out var cw, out var ch);
        if (vw <= 0 || vh <= 0 || cw <= 0 || ch <= 0)
        {
            return userMin;
        }
        var needed = Math.Max(vw / cw, vh / ch);
        return Math.Max(userMin, needed);
    }

    private static void ClampTranslation(FrameworkElement attachedElement, State state)
    {
        if (state.TranslateTransform is null) return;
        var target = state.Target ?? attachedElement;
        GetSizes(attachedElement, target, out var vw, out var vh, out var cw, out var ch);
        if (vw <= 0 || vh <= 0 || cw <= 0 || ch <= 0) return;

        var scaledW = cw * state.Scale;
        var scaledH = ch * state.Scale;

        double minX = vw - scaledW;
        double maxX = 0;
        double minY = vh - scaledH;
        double maxY = 0;

        // If content is smaller due to rounding, center it by clamping to range (min..max) which may be positive
        if (minX > maxX)
        {
            // Content narrower than viewport; keep centered horizontally
            double centerX = (vw - scaledW) / 2.0;
            state.TranslateTransform.X = centerX;
        }
        else
        {
            state.TranslateTransform.X = Math.Max(minX, Math.Min(maxX, state.TranslateTransform.X));
        }

        if (minY > maxY)
        {
            double centerY = (vh - scaledH) / 2.0;
            state.TranslateTransform.Y = centerY;
        }
        else
        {
            state.TranslateTransform.Y = Math.Max(minY, Math.Min(maxY, state.TranslateTransform.Y));
        }
    }

    private static void ApplyConstraints(FrameworkElement attachedElement, State state)
    {
        if (state.ScaleTransform is null || state.TranslateTransform is null) return;
        // Enforce minimum scale based on viewport/content
        var effectiveMin = ComputeEffectiveMinScale(attachedElement, state);
        var max = GetMaxScale(attachedElement);
        var clampedScale = Math.Clamp(state.Scale, effectiveMin, max);
        if (Math.Abs(clampedScale - state.Scale) > 0.0001)
        {
            // Re-anchor around current translation center; simplest is to scale about (0,0) and then clamp translation.
            state.Scale = clampedScale;
            state.ScaleTransform.ScaleX = state.Scale;
            state.ScaleTransform.ScaleY = state.Scale;
        }
        ClampTranslation(attachedElement, state);
    }

    private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not FrameworkElement element || !States.TryGetValue(element, out var state)) return;
        if (e.Key == Key.Space)
        {
            state.IsSpaceDown = true;
            // On Space pressed, show OPEN hand cursor if not currently panning
            if (!state.IsPanning)
            {
                state.OriginalCursor ??= Mouse.OverrideCursor; // remember the current override (may be null)
                Mouse.OverrideCursor = Cursors.ScrollAll; // OPEN hand substitute
            }
            e.Handled = true;
        }
    }

    private static void OnPreviewKeyUp(object sender, KeyEventArgs e)
    {
        if (sender is not FrameworkElement element || !States.TryGetValue(element, out var state)) return;
        if (e.Key == Key.Space)
        {
            state.IsSpaceDown = false;
            if (state.IsPanning)
            {
                state.IsPanning = false;
                Mouse.Capture(null);
                // On Space release while panning, end panning and restore original cursor
            }
            // Restore original cursor on Space release
            Mouse.OverrideCursor = state.OriginalCursor;
            state.OriginalCursor = null;
            e.Handled = true;
        }
    }

    private static void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement element || !States.TryGetValue(element, out var state)) return;
        // Consider Space pressed if either our internal flag is set (when we have focus)
        // or Keyboard reports Space down (works even if a child has focus).
        var isSpacePressed = state.IsSpaceDown || Keyboard.IsKeyDown(Key.Space);
        if (isSpacePressed && e.ChangedButton == MouseButton.Left)
        {
            state.IsPanning = true;
            // Measure mouse position in the attached element (viewport) space so
            // panning speed is consistent regardless of current zoom level.
            state.LastMouse = e.GetPosition(element);
            Mouse.Capture(element); // capture on attached element (e.g., Window)
            // Change cursor to grasping hand while panning (substitute)
            state.OriginalCursor ??= Mouse.OverrideCursor; // remember if not already stored
            Mouse.OverrideCursor = Cursors.SizeAll; // GRASPING hand substitute
            e.Handled = true;
        }
    }

    private static void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not FrameworkElement element || !States.TryGetValue(element, out var state)) return;
        if (state.IsPanning && state.TranslateTransform is not null)
        {
            // Use attached element (viewport) coordinates to compute deltas,
            // because TranslateTransform is applied after ScaleTransform and
            // therefore operates in device pixels independent of scale.
            var pos = e.GetPosition(element);
            var dx = pos.X - state.LastMouse.X;
            var dy = pos.Y - state.LastMouse.Y;

            state.TranslateTransform.X += dx;
            state.TranslateTransform.Y += dy;

            state.LastMouse = pos;
            // Constrain after panning
            ClampTranslation(element, state);
            e.Handled = true;
        }
    }

    private static void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement element || !States.TryGetValue(element, out var state)) return;
        if (state.IsPanning && e.ChangedButton == MouseButton.Left)
        {
            state.IsPanning = false;
            Mouse.Capture(null);
            // On mouse release: if Space still held, switch back to OPEN hand, else restore original
            if (state.IsSpaceDown || Keyboard.IsKeyDown(Key.Space))
            {
                Mouse.OverrideCursor = Cursors.ScrollAll; // OPEN hand substitute
            }
            else
            {
                Mouse.OverrideCursor = state.OriginalCursor;
                state.OriginalCursor = null;
            }
            e.Handled = true;
        }
    }

    private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not FrameworkElement element || !States.TryGetValue(element, out var state)) return;
        if (state.ScaleTransform is null || state.TranslateTransform is null) return;

        // Use the attached element (viewport) coordinate space for zoom anchoring
        // so calculations align with the clamping logic that also uses the viewport.
        // Using the target element here can cause inconsistencies and allow the
        // content to drift into positive translation (exposing top/left edges).
        var target = state.Target ?? element;
        var mousePos = e.GetPosition(element);

        var factor = GetWheelZoomFactor(element);
        double zoom = e.Delta > 0 ? factor : 1.0 / factor;
        // Compute effective min scale so edges never show
        var minScale = ComputeEffectiveMinScale(element, state);
        var maxScale = GetMaxScale(element);

        var newScale = Math.Clamp(state.Scale * zoom, minScale, maxScale);
        zoom = newScale / state.Scale;

        // World coords of cursor before zoom (given order Scale then Translate)
        var worldX = (mousePos.X - state.TranslateTransform.X) / state.Scale;
        var worldY = (mousePos.Y - state.TranslateTransform.Y) / state.Scale;

        state.Scale = newScale;
        state.ScaleTransform.ScaleX = state.Scale;
        state.ScaleTransform.ScaleY = state.Scale;

        // Keep cursor anchored
        state.TranslateTransform.X = mousePos.X - worldX * state.Scale;
        state.TranslateTransform.Y = mousePos.Y - worldY * state.Scale;

        // Constrain translation to avoid exposing edges
        ClampTranslation(element, state);

        e.Handled = true;
    }
}
