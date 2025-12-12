using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Library.Utilities;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Svg.Skia;

namespace UX.ViewModels.LabelsEditorViewModel;

public class LabelsEditorViewModel : UX.ViewModels.BaseViewModel.BaseViewModel
{
    #region Bindable props
    private int InvalidateRequested
    {
        get; set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double MapWidth
    {
        get; private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double MapHeight
    {
        get; private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand<SKPaintSurfaceEventArgs> PaintWaterSurfaceCommand => new(PaintWaterSurface);
    public RelayCommand<SKPaintSurfaceEventArgs> PaintWorldSurfaceCommand => new(PaintWorldSurface);
    public RelayCommand<SKPaintSurfaceEventArgs> PaintBordersSurfaceCommand => new(PaintBordersSurface);
    public RelayCommand<SKPaintSurfaceEventArgs> PaintLabelsSurfaceCommand => new(PaintLabelsSurface);

    #endregion

    #region Fields
    private SKSvg? _waterSvg;
    private SKSvg? _worldSvg;
    private SKSvg? _bordersSvg;
    private SKSvg? _labelsSvg;

    private XDocument? _labelsDoc; // editable DOM
    private XElement? _activeLabel;
    private double _dragOffsetX;
    private double _dragOffsetY;

    private const float DrawScale = 2.5f;
    private string? _basePath;
    private string? _labelsSourcePath; // repository source file path (..\Graphics\Images\labels.svg)

    #endregion

    #region Lifecycle
    protected override void LoadedAction()
    {
        _basePath = AppDomain.CurrentDomain.BaseDirectory;
        // Locate the source labels.svg in the repository so saves persist
        _labelsSourcePath = TryFindRepoLabelsPath(_basePath);
        LoadWater(_basePath);
        LoadWorld(_basePath);
        LoadBorders(_basePath);
        LoadLabels(_basePath);
        InvalidateRequested++;
    }
    #endregion

    #region Painting
    private void PaintWaterSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        if (_waterSvg?.Picture is null) return;
        canvas.Scale(DrawScale);
        canvas.DrawPicture(_waterSvg.Picture);
    }

    private void PaintWorldSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        if (_worldSvg?.Picture is null) return;
        canvas.Scale(DrawScale);
        canvas.DrawPicture(_worldSvg.Picture);
    }

    private void PaintBordersSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        if (_bordersSvg?.Picture is null) return;
        canvas.Scale(DrawScale);
        canvas.DrawPicture(_bordersSvg.Picture);
    }

    private void PaintLabelsSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        if (_labelsSvg?.Picture is null) return;
        canvas.Scale(DrawScale);
        canvas.DrawPicture(_labelsSvg.Picture);
    }
    #endregion

    #region Loading
    private void LoadWater(string basePath)
    {
        _waterSvg = new SKSvg();
        _waterSvg.Load(Path.Combine(basePath, "Graphics", "Images", "world_water.svg"));

        if (_waterSvg?.Picture is null) return;
        var scale = 2.5d;
        MapWidth = (_waterSvg.Picture.CullRect.Width) * scale;
        MapHeight = (_waterSvg.Picture.CullRect.Height) * scale;
    }

    private void LoadWorld(string basePath)
    {
        _worldSvg = new SKSvg();
        _worldSvg.Load(Path.Combine(basePath, "Graphics", "Images", "world.svg"));
    }

    private void LoadBorders(string basePath)
    {
        _bordersSvg = new SKSvg();
        _bordersSvg.Load(Path.Combine(basePath, "Graphics", "Images", "world_borders.svg"));
    }

    private void LoadLabels(string basePath)
    {
        // Prefer loading from the repository source file so edits reflect in the project
        var path = _labelsSourcePath ?? Path.Combine(basePath, "Graphics", "Images", "labels.svg");
        _labelsDoc = XDocument.Load(path, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
        ReloadLabelsPicture();
    }
    #endregion

    private void ReloadLabelsPicture()
    {
        if (_labelsDoc is null) return;
        _labelsSvg ??= new SKSvg();
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(_labelsDoc.ToString(SaveOptions.DisableFormatting)));
        _labelsSvg.Load(ms);
        InvalidateRequested++;
    }

    #region Editing API
    public void BeginDragAt(double xDevice, double yDevice)
    {
        // map coords back to SVG space
        var sx = xDevice / DrawScale;
        var sy = yDevice / DrawScale;
        _activeLabel = FindNearestLabel(sx, sy, 20 / DrawScale);
        if (_activeLabel is not null && TryGetXY(_activeLabel, out var lx, out var ly))
        {
            // Keep the pointer-to-anchor offset so dragging feels natural and avoids snapping
            _dragOffsetX = lx - sx;
            _dragOffsetY = ly - sy;
        }
    }

    public void DragTo(double xDevice, double yDevice)
    {
        if (_activeLabel is null || _labelsDoc is null) return;
        var sx = (xDevice / DrawScale) + _dragOffsetX;
        var sy = (yDevice / DrawScale) + _dragOffsetY;

        // Apply to whichever attributes actually control the rendered position
        ApplyPosition(_activeLabel, sx, sy);
        ReloadLabelsPicture();
    }

    public string? EndDragAndSave()
    {
        if (_activeLabel is null) return null;
        _activeLabel = null;
        if (_labelsDoc is null || string.IsNullOrEmpty(_basePath)) return null;

        // Determine save targets
        var sourcePath = _labelsSourcePath;
        // Re-resolve source path if it wasn't found at load time
        sourcePath ??= TryFindRepoLabelsPath(_basePath!) ?? Path.Combine(_basePath!, "Graphics", "Images", "labels.svg");
        _labelsSourcePath = sourcePath; // cache for next time
        var binPath = Path.Combine(_basePath!, "Graphics", "Images", "labels.svg");

        try
        {
            // backup next to source
            var backup = sourcePath + ".bak." + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, backup, overwrite: false);
            }

            // save to source
            Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
            using (var fs = File.Create(sourcePath))
            using (var sw = new StreamWriter(fs, new UTF8Encoding(false)))
            {
                _labelsDoc.Save(sw);
            }

            // Also refresh the running app's bin copy so other views that read from bin see the change immediately
            try
            {
                if (!string.Equals(sourcePath, binPath, StringComparison.OrdinalIgnoreCase))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(binPath)!);
                    File.Copy(sourcePath, binPath, overwrite: true);
                }
            }
            catch
            {
                // best-effort, ignore errors copying to bin
            }

            return sourcePath;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private XElement? FindNearestLabel(double sx, double sy, double maxDist)
    {
        if (_labelsDoc is null) return null;
        var texts = _labelsDoc.Descendants().Where(e => e.Name.LocalName.Equals("text", StringComparison.OrdinalIgnoreCase));
        XElement? best = null;
        double bestD2 = maxDist * maxDist;
        foreach (var t in texts)
        {
            if (!TryGetXY(t, out var tx, out var ty)) continue;
            var dx = sx - tx;
            var dy = sy - ty;
            var d2 = dx * dx + dy * dy;
            if (d2 <= bestD2)
            {
                bestD2 = d2;
                best = t;
            }
        }
        return best;
    }

    private static bool TryGetXY(XElement textNode, out double x, out double y)
    {
        // Effective anchor for a label composed of <text> with optional <tspan> children:
        // - If any child <tspan> has its own x list, use the first such x as effective X.
        // - For Y prefer parent <text y>, otherwise use first child <tspan y> if present; ignore dy for anchor.
        x = y = 0;
        var inv = System.Globalization.CultureInfo.InvariantCulture;

        // Search child tspans with x/y
        var tspanWithX = textNode.Descendants().FirstOrDefault(e => e.Name.LocalName.Equals("tspan", StringComparison.OrdinalIgnoreCase)
                                                                     && e.Attribute("x") != null);
        var tspanWithY = textNode.Descendants().FirstOrDefault(e => e.Name.LocalName.Equals("tspan", StringComparison.OrdinalIgnoreCase)
                                                                     && e.Attribute("y") != null);

        string? xs = null;
        string? ys = null;

        if (tspanWithX is not null)
        {
            xs = (string?)tspanWithX.Attribute("x");
        }
        else
        {
            xs = (string?)textNode.Attribute("x");
        }

        // Y from parent <text> primarily
        ys = (string?)textNode.Attribute("y");
        if (string.IsNullOrWhiteSpace(ys) && tspanWithY is not null)
        {
            ys = (string?)tspanWithY.Attribute("y");
        }

        static string? FirstNumber(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return s!.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        }

        xs = FirstNumber(xs);
        ys = FirstNumber(ys);

        return double.TryParse(xs, System.Globalization.NumberStyles.Float, inv, out x)
               && double.TryParse(ys, System.Globalization.NumberStyles.Float, inv, out y);
    }

    private static void ApplyPosition(XElement textNode, double newX, double newY)
    {
        var inv = System.Globalization.CultureInfo.InvariantCulture;

        // Determine control: if any child <tspan> has x attribute, shift those by dx; else set text x.
        // For Y: prefer setting parent <text y>; if absent but tspans have y, shift those by dy.

        // Current effective position
        if (!TryGetXY(textNode, out var curX, out var curY))
        {
            curX = 0; curY = 0; // fallback
        }

        var dx = newX - curX;
        var dy = newY - curY;

        // Helper to shift number-list attribute by delta
        static void ShiftNumberListAttribute(XAttribute attr, double delta, System.Globalization.CultureInfo inv)
        {
            var raw = attr.Value;
            var parts = raw.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                // treat as single number
                if (double.TryParse(raw, System.Globalization.NumberStyles.Float, inv, out var v))
                {
                    attr.Value = (v + delta).ToString(inv);
                }
                return;
            }

            var nums = new List<string>(parts.Length);
            foreach (var p in parts)
            {
                if (double.TryParse(p, System.Globalization.NumberStyles.Float, inv, out var v))
                {
                    nums.Add((v + delta).ToString(inv));
                }
                else
                {
                    nums.Add(p);
                }
            }
            attr.Value = string.Join(" ", nums);
        }

        // X control
        var tspanXAttrs = textNode.Descendants()
                                  .Where(e => e.Name.LocalName.Equals("tspan", StringComparison.OrdinalIgnoreCase))
                                  .Select(e => e.Attribute("x"))
                                  .Where(a => a is not null)
                                  .Cast<XAttribute>()
                                  .ToList();
        if (tspanXAttrs.Count > 0)
        {
            foreach (var xa in tspanXAttrs)
            {
                ShiftNumberListAttribute(xa, dx, inv);
            }
        }
        else
        {
            textNode.SetAttributeValue("x", newX.ToString(inv));
        }

        // Y control
        var textYAttr = textNode.Attribute("y");
        if (textYAttr is not null)
        {
            textNode.SetAttributeValue("y", newY.ToString(inv));
        }
        else
        {
            var tspanYAttrs = textNode.Descendants()
                                      .Where(e => e.Name.LocalName.Equals("tspan", StringComparison.OrdinalIgnoreCase))
                                      .Select(e => e.Attribute("y"))
                                      .Where(a => a is not null)
                                      .Cast<XAttribute>()
                                      .ToList();
            if (tspanYAttrs.Count > 0)
            {
                foreach (var ya in tspanYAttrs)
                {
                    ShiftNumberListAttribute(ya, dy, inv);
                }
            }
            else
            {
                // As a fallback, set parent <text y>
                textNode.SetAttributeValue("y", newY.ToString(inv));
            }
        }
    }

    private static string? TryFindRepoLabelsPath(string startBaseDir)
    {
        // Walk up a few levels to find the project source Graphics\Images\labels.svg, preferring the topmost occurrence
        // and avoiding any copies inside bin/ or obj/ directories.
        try
        {
            var dir = new DirectoryInfo(startBaseDir);
            string? best = null;
            for (int i = 0; i < 12 && dir is not null; i++, dir = dir.Parent)
            {
                var candidate = Path.Combine(dir.FullName, "Graphics", "Images", "labels.svg");
                if (File.Exists(candidate))
                {
                    var lower = candidate.ToLowerInvariant();
                    if (lower.Contains("\\bin\\") || lower.Contains("/bin/") || lower.Contains("\\obj\\") || lower.Contains("/obj/"))
                    {
                        // skip build output folders
                    }
                    else
                    {
                        best = candidate; // prefer higher-level source file
                    }
                }

                // If we find a solution file, treat that level as the repo root and stop
                var sln = Directory.EnumerateFiles(dir.FullName, "*.sln", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (sln is not null)
                {
                    var fromSln = Path.Combine(dir.FullName, "Graphics", "Images", "labels.svg");
                    if (File.Exists(fromSln)) return fromSln;
                    break;
                }
            }

            return best;
        }
        catch
        {
            // ignore
        }
        return null;
    }
    #endregion
}
