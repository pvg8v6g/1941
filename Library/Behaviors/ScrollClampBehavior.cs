using System;
using System.Windows;
using System.Windows.Controls;

namespace Library.Behaviors;

public static class ScrollClampBehavior
{
    // Enables the behavior when set to true on a ScrollViewer
    public static readonly DependencyProperty EnableRightEdgeClampProperty = DependencyProperty.RegisterAttached(
        name: "EnableRightEdgeClamp",
        propertyType: typeof(bool),
        ownerType: typeof(ScrollClampBehavior),
        defaultMetadata: new PropertyMetadata(false, OnEnableRightEdgeClampChanged));

    // The margin (in device-independent pixels) by which to hide the right edge.
    // Example: 8–24 px hides the map boundary so users can't see the sharp edge when scrolled to the end.
    public static readonly DependencyProperty RightEdgeHideMarginProperty = DependencyProperty.RegisterAttached(
        name: "RightEdgeHideMargin",
        propertyType: typeof(double),
        ownerType: typeof(ScrollClampBehavior),
        defaultMetadata: new PropertyMetadata(16.0, OnRightEdgeHideMarginChanged));

    public static void SetEnableRightEdgeClamp(DependencyObject element, bool value) => element.SetValue(EnableRightEdgeClampProperty, value);
    public static bool GetEnableRightEdgeClamp(DependencyObject element) => (bool)element.GetValue(EnableRightEdgeClampProperty);

    public static void SetRightEdgeHideMargin(DependencyObject element, double value) => element.SetValue(RightEdgeHideMarginProperty, value);
    public static double GetRightEdgeHideMargin(DependencyObject element) => (double)element.GetValue(RightEdgeHideMarginProperty);

    private static void OnEnableRightEdgeClampChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScrollViewer sv) return;

        if ((bool)e.NewValue)
        {
            sv.ScrollChanged += SvOnScrollChanged;
            sv.SizeChanged += SvOnSizeChanged;
            // Initial clamp when enabling
            ClampHorizontalOffset(sv);
        }
        else
        {
            sv.ScrollChanged -= SvOnScrollChanged;
            sv.SizeChanged -= SvOnSizeChanged;
        }
    }

    private static void OnRightEdgeHideMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer sv && GetEnableRightEdgeClamp(sv))
        {
            ClampHorizontalOffset(sv);
        }
    }

    private static void SvOnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (sender is ScrollViewer sv)
        {
            // Only care about horizontal changes, but clamping is cheap.
            ClampHorizontalOffset(sv);
        }
    }

    private static void SvOnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is ScrollViewer sv)
        {
            ClampHorizontalOffset(sv);
        }
    }

    private static bool _reentrant;

    private static void ClampHorizontalOffset(ScrollViewer sv)
    {
        if (_reentrant) return;
        try
        {
            _reentrant = true;

            double extent = sv.ExtentWidth;
            double viewport = sv.ViewportWidth;
            double scrollable = extent - viewport;
            if (scrollable <= 0)
                return; // nothing to clamp

            double margin = Math.Max(0, GetRightEdgeHideMargin(sv));
            double maxOffset = Math.Max(0, scrollable - margin);

            if (sv.HorizontalOffset > maxOffset)
            {
                sv.ScrollToHorizontalOffset(maxOffset);
            }
        }
        finally
        {
            _reentrant = false;
        }
    }
}
