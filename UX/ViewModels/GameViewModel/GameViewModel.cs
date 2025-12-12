using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Library.Attributes;
using Library.Enumerations;
using Library.Extensions;
using Library.Services;
using Library.Utilities;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Svg.Skia;

namespace UX.ViewModels.GameViewModel;

public class GameViewModel(ISvgClassColorService svgClassColorService) : BaseViewModel.BaseViewModel
{
    #region Properties

    private int InvalidateRequested
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double MapWidth
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double MapHeight
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    private string? HoveredClass
    {
        get;
        set
        {
            Console.WriteLine(value);
            if (field == value) return;
            field = value;
            OnPropertyChanged();

            // Tooltip logic: restart delay when country changes; hide immediately if changed
            OnHoveredClassChanged(value);
        }
    }

    // Tooltip surface-bound coordinates (relative to MapGrid)
    public double TooltipX
    {
        get;
        private set
        {
            if (Math.Abs(field - value) < 0.1) return;
            field = value;
            OnPropertyChanged();
        }
    }

    public double TooltipY
    {
        get;
        private set
        {
            if (Math.Abs(field - value) < 0.1) return;
            field = value;
            OnPropertyChanged();
        }
    }

    public string? TooltipText
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    }

    public bool TooltipIsOpen
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand<SKPaintSurfaceEventArgs> PaintWaterSurfaceCommand => new(PaintWaterSurface);

    public RelayCommand<SKPaintSurfaceEventArgs> PaintWorldSurfaceCommand => new(PaintWorldSurface);

    public RelayCommand<SKPaintSurfaceEventArgs> PaintBordersSurfaceCommand => new(PaintBordersSurface);

    // Mouse interaction commands (for behaviors)
    public RelayCommand<Point> MouseMoveCommand => new(p => OnMouseMove(p.X, p.Y));

    public RelayCommand<object?> MouseLeaveCommand => new(_ => ClearHover());

    private ISvgClassColorService SvgClassColorService => svgClassColorService;

    #endregion

    #region Fields

    private SKSvg? _waterSvg;
    private SKSvg? _worldSvg;

    private SKSvg? _bordersSvg;

    private string? _worldSvgXmlOriginal;
    private string? _worldSvgXmlCurrent;

    // Interaction/hitmap
    private SKBitmap? _hitmapBitmap; // same pixel size as displayed map
    private readonly Dictionary<SKColor, string> _hitColorToClass = new();
    private readonly Dictionary<string, SKColor> _classToHitColor = new(StringComparer.OrdinalIgnoreCase);

    private const float DrawScale = 2.5f;

    // Tooltip state
    private readonly DispatcherTimer _hoverDelayTimer = new() { Interval = TimeSpan.FromMilliseconds(500) };
    private readonly DispatcherTimer _tooltipCloseTimer = new() { Interval = TimeSpan.FromSeconds(5) };
    private double _lastMouseX;
    private double _lastMouseY;
    private const double TooltipOffsetX = 12; // shift tooltip right to avoid cursor overlap
    private const double TooltipOffsetY = 16; // shift tooltip down to avoid cursor overlap

    #endregion

    #region Actions

    // Primary constructor is used; optional parameter keeps design-time support.

    protected override void LoadedAction()
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        LoadWater(basePath);
        LoadWorld(basePath);
        LoadBorders(basePath);
        InvalidateRequested++;
        FactionColors();

        // Wire timers
        _hoverDelayTimer.Tick += (_, _) =>
        {
            _hoverDelayTimer.Stop();
            if (string.IsNullOrWhiteSpace(HoveredClass)) return;
            // Show tooltip where the pointer is at show time
            // Find the matching territory by its SVG class key and display its attribute Name
            var displayName = GetTerritoryNameFromClassKey(HoveredClass!);
            TooltipText = displayName ?? HoveredClass?.Replace('_', ' ');
            TooltipX = _lastMouseX + TooltipOffsetX;
            TooltipY = _lastMouseY + TooltipOffsetY;
            TooltipIsOpen = true;
            _tooltipCloseTimer.Stop();
            _tooltipCloseTimer.Start();
        };

        _tooltipCloseTimer.Tick += (_, _) =>
        {
            _tooltipCloseTimer.Stop();
            TooltipIsOpen = false;
        };
    }

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

    /// <summary>
    /// Updates the fill color of all shapes that have the given class in world.svg by injecting/updating
    /// a CSS rule in the SVG and reloading the picture. Triggers invalidate on success.
    /// </summary>
    /// <param name="className">Class name without the dot.</param>
    /// <param name="color">Fill color.</param>
    public void SetCountryFillByClass(string className, SKColor color)
    {
        if (string.IsNullOrWhiteSpace(_worldSvgXmlCurrent)) return;
        var updated = SvgClassColorService.UpsertClassFill(_worldSvgXmlCurrent!, className, color);
        if (ReferenceEquals(updated, _worldSvgXmlCurrent)) return;
        _worldSvgXmlCurrent = updated;
        _worldSvg ??= new SKSvg();
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(_worldSvgXmlCurrent));
        _worldSvg.Load(ms);
        InvalidateRequested++;
    }

    private void OnMouseMove(double x, double y)
    {
        if (_hitmapBitmap is null) return;
        _lastMouseX = x;
        _lastMouseY = y;
        var px = (int) Math.Clamp(x, 0, _hitmapBitmap.Width - 1);
        var py = (int) Math.Clamp(y, 0, _hitmapBitmap.Height - 1);
        var color = _hitmapBitmap.GetPixel(px, py);
        // exact match only; hitmap is rendered crisp/no-stroke, so borders are exact
        HoveredClass = _hitColorToClass.GetValueOrDefault(new SKColor(color.Red, color.Green, color.Blue, 255));
    }

    private void ClearHover()
    {
        HoveredClass = null;
    }

    private void OnHoveredClassChanged(string? newClass)
    {
        // If country changed, hide tooltip immediately and reset timers
        _tooltipCloseTimer.Stop();
        TooltipIsOpen = false;

        _hoverDelayTimer.Stop();
        if (!string.IsNullOrWhiteSpace(newClass))
        {
            // Start waiting to show tooltip for this country
            _hoverDelayTimer.Start();
        }
        else
        {
            TooltipText = null;
        }
    }

    private void BuildHitmap()
    {
        if (string.IsNullOrWhiteSpace(_worldSvgXmlOriginal)) return;
        _hitColorToClass.Clear();
        _classToHitColor.Clear();

        var classes = SvgClassColorService.ExtractClasses(_worldSvgXmlOriginal!);
        // assign unique visible colors (avoid black background)
        var i = 1;
        foreach (var cls in classes.OrderBy(s => s, StringComparer.OrdinalIgnoreCase))
        {
            // pack id into RGB
            var r = (byte) (i & 0x0000FF);
            var g = (byte) ((i & 0x00FF00) >> 8);
            var b = (byte) ((i & 0xFF0000) >> 16);
            var col = new SKColor(r, g, b, 255);
            _classToHitColor[cls] = col;
            _hitColorToClass[col] = cls;
            i++;
        }

        // Create svg with these fills
        var hitSvgXml = SvgClassColorService.ApplyClassFills(_worldSvgXmlOriginal!, _classToHitColor);
        // Inject global crisp, no-stroke rules specifically for the hitmap to avoid anti-aliasing at edges
        hitSvgXml = InjectHitmapCrispStyles(hitSvgXml);
        var svg = new SKSvg();
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(hitSvgXml)))
        {
            svg.Load(ms);
        }

        if (svg.Picture is null) return;

        var width = (int) Math.Ceiling(svg.Picture.CullRect.Width * DrawScale);
        var height = (int) Math.Ceiling(svg.Picture.CullRect.Height * DrawScale);

        _hitmapBitmap?.Dispose();
        _hitmapBitmap = new SKBitmap(width, height, true);
        using var canvas = new SKCanvas(_hitmapBitmap);
        canvas.Clear(SKColors.Transparent);
        canvas.Scale(DrawScale);
        // Draw with AA disabled to keep pick colors exact
        using var paint = new SKPaint();
        paint.IsAntialias = false;
        paint.IsDither = false;
        canvas.DrawPicture(svg.Picture, paint);
    }

    #endregion

    #region Private Fields

    private void LoadWater(string basePath)
    {
        _waterSvg = new SKSvg();
        _waterSvg.Load(Path.Combine(basePath, "Graphics", "Images", "world_water.svg"));

        if (_waterSvg?.Picture is null) return;
        const double scale = 2.5d;
        MapWidth = _waterSvg.Picture.CullRect.Width * scale;
        MapHeight = _waterSvg.Picture.CullRect.Height * scale;
    }

    private void LoadWorld(string basePath)
    {
        _worldSvg = new SKSvg();
        // Load world.svg from text so we can mutate styles later
        var worldPath = Path.Combine(basePath, "Graphics", "Images", "world.svg");
        if (!File.Exists(worldPath)) return;
        _worldSvgXmlOriginal = File.ReadAllText(worldPath, Encoding.UTF8);
        _worldSvgXmlCurrent = _worldSvgXmlOriginal;
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(_worldSvgXmlCurrent));
        _worldSvg.Load(ms);
        BuildHitmap();
    }

    private void LoadBorders(string basePath)
    {
        _bordersSvg = new SKSvg();
        _bordersSvg.Load(Path.Combine(basePath, "Graphics", "Images", "world_borders.svg"));
    }

    private void FactionColors()
    {
        if (string.IsNullOrWhiteSpace(_worldSvgXmlOriginal)) return;

        // Build map of normalized SVG class key -> SKColor based on each territory's starting faction color
        var fills = new Dictionary<string, SKColor>(StringComparer.OrdinalIgnoreCase);
        foreach (var territory in Enum.GetValues<Territories>())
        {
            var info = territory.GetAttribute<TerritoryInfo>();
            var factionColorHex = info.StartingFaction.GetAttribute<FactionColor>()
                .Color;
            if (string.IsNullOrWhiteSpace(info.Key)) continue;
            if (string.IsNullOrWhiteSpace(factionColorHex)) continue;

            if (!TryParseHexColor(factionColorHex, out var color)) continue;
            // Keys in TerritoryInfo are already normalized to underscore-separated which matches SvgClassColorService expectations
            fills[info.Key] = color;
        }

        if (fills.Count == 0) return;

        // Apply fills to original world SVG and reload
        var updated = SvgClassColorService.ApplyClassFills(_worldSvgXmlOriginal!, fills);
        _worldSvgXmlCurrent = updated;
        _worldSvg ??= new SKSvg();
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(_worldSvgXmlCurrent)))
        {
            _worldSvg.Load(ms);
        }

        InvalidateRequested++;
    }

    private static bool TryParseHexColor(string hex, out SKColor color)
    {
        color = SKColors.Transparent;
        if (hex.IsNullOrWhiteSpace()) return false;
        var s = hex.Trim();
        if (s.StartsWith('#')) s = s[1..];
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) s = s[2..];

        switch (s.Length)
        {
            // Support RRGGBB and AARRGGBB
            case 6:
            {
                if (!byte.TryParse(s[..2], System.Globalization.NumberStyles.HexNumber, null, out var r) ||
                    !byte.TryParse(s.AsSpan(2, 2), System.Globalization.NumberStyles.HexNumber, null, out var g) ||
                    !byte.TryParse(s.AsSpan(4, 2), System.Globalization.NumberStyles.HexNumber, null, out var b)) return false;
                color = new SKColor(r, g, b, 255);
                return true;
            }
            case 8:
            {
                if (!byte.TryParse(s[..2], System.Globalization.NumberStyles.HexNumber, null, out var a) ||
                    !byte.TryParse(s.AsSpan(2, 2), System.Globalization.NumberStyles.HexNumber, null, out var r) ||
                    !byte.TryParse(s.AsSpan(4, 2), System.Globalization.NumberStyles.HexNumber, null, out var g) ||
                    !byte.TryParse(s.AsSpan(6, 2), System.Globalization.NumberStyles.HexNumber, null, out var b)) return false;
                color = new SKColor(r, g, b, a);
                return true;
            }
            default:
                return false;
        }
    }

    private static string? GetTerritoryNameFromClassKey(string classKey)
    {
        if (string.IsNullOrWhiteSpace(classKey)) return null;
        return Enum.GetValues<Territories>()
            .Select(territory => territory.GetAttribute<TerritoryInfo>())
            .Where(info => !info.Key.IsNullOrWhiteSpace())
            .Where(info => string.Equals(info.Key, classKey, StringComparison.OrdinalIgnoreCase))
            .Select(info => info.Name.IsNullOrWhiteSpace() ? info.Key.Replace('_', ' ') : info.Name)
            .FirstOrDefault();
    }

    private static string InjectHitmapCrispStyles(string svgXml)
    {
        if (string.IsNullOrWhiteSpace(svgXml)) return svgXml;

        // Ensure the root <svg> has shape-rendering="crispEdges"
        var idxSvg = svgXml.IndexOf("<svg", StringComparison.OrdinalIgnoreCase);
        if (idxSvg >= 0)
        {
            var endTag = svgXml.IndexOf('>', idxSvg);
            if (endTag > idxSvg)
            {
                var openTag = svgXml.Substring(idxSvg, endTag - idxSvg + 1);
                if (!openTag.Contains("shape-rendering", StringComparison.OrdinalIgnoreCase))
                {
                    var withoutGt = openTag.TrimEnd('>');
                    var selfClosing = withoutGt.EndsWith("/");
                    if (selfClosing)
                    {
                        withoutGt = withoutGt.Substring(0, withoutGt.Length - 1);
                    }

                    if (!withoutGt.EndsWith(" "))
                        withoutGt += " ";

                    withoutGt += "shape-rendering=\"crispEdges\"";

                    var rebuilt = withoutGt + (selfClosing ? " />" : ">");

                    svgXml = svgXml.Remove(idxSvg, endTag - idxSvg + 1)
                        .Insert(idxSvg, rebuilt);
                }
            }
        }

        // Inject a dedicated style block after the opening <svg ...>
        var svgOpenEnd = svgXml.IndexOf('>', idxSvg >= 0 ? idxSvg : 0);
        if (svgOpenEnd >= 0)
        {
            // Keep fills opaque and remove strokes; avoid adding shape-rendering here to keep CSS valid across parsers
            const string styleBlock =
                "\n<style type=\"text/css\"><![CDATA[\n/* hitmap rules */\n*{ stroke:none !important; fill-opacity:1 !important; }\n]]></style>\n";
            svgXml = svgXml.Insert(svgOpenEnd + 1, styleBlock);
        }

        return svgXml;
    }

    #endregion
}
