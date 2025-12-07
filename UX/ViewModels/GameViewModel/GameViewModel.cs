using System.IO;
using System.Text;
using System.Windows;
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
            //Console.WriteLine(value);
            if (field == value) return;
            field = value;
            OnPropertyChanged();
            RebuildWorldColors(); // apply hover lightening
        }
    }

    public RelayCommand<SKPaintSurfaceEventArgs> PaintWaterSurfaceCommand => new(PaintWaterSurface);

    public RelayCommand<SKPaintSurfaceEventArgs> PaintWorldSurfaceCommand => new(PaintWorldSurface);

    public RelayCommand<SKPaintSurfaceEventArgs> PaintBordersSurfaceCommand => new(PaintBordersSurface);

    public RelayCommand<SKPaintSurfaceEventArgs> PaintLabelsSurfaceCommand => new(PaintLabelsSurface);

    // Mouse interaction commands (for behaviors)
    public RelayCommand<Point> MouseMoveCommand => new(p => OnMouseMove(p.X, p.Y));

    public RelayCommand<object?> MouseLeaveCommand => new(_ => ClearHover());

    private ISvgClassColorService SvgClassColorService => svgClassColorService;

    #endregion

    #region Fields

    private SKSvg? _waterSvg;
    private SKSvg? _worldSvg;
    private SKSvg? _bordersSvg;
    private SKSvg? _labelsSvg;
    private string? _worldSvgXmlOriginal;
    private string? _worldSvgXmlCurrent;

    // Interaction/hitmap
    private SKBitmap? _hitmapBitmap; // same pixel size as displayed map
    private readonly Dictionary<SKColor, string> _hitColorToClass = new();
    private readonly Dictionary<string, SKColor> _classToHitColor = new(StringComparer.OrdinalIgnoreCase);

    // Ownership/colors
    private readonly Dictionary<string, string> _classOwnership = new(StringComparer.OrdinalIgnoreCase); // class -> power

    private readonly Dictionary<string, SKColor> _powerColors = new(StringComparer.OrdinalIgnoreCase)
    {
        // sensible defaults
        { "russia", new SKColor(68, 114, 196) },
        { "germany", new SKColor(192, 80, 77) },
        { "britain", new SKColor(112, 173, 71) },
        { "usa", new SKColor(91, 155, 213) },
        { "japan", new SKColor(237, 125, 49) },
        { "italy", new SKColor(165, 165, 165) },
        { "neutral", new SKColor(230, 230, 230) }
    };

    private const float DrawScale = 2.5f;

    #endregion

    #region Actions

    // Primary constructor is used; optional parameter keeps design-time support.

    protected override void LoadedAction()
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        LoadWater(basePath);
        LoadWorld(basePath);
        LoadBorders(basePath);
        LoadLabels(basePath);
        InvalidateRequested++;
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

    private void PaintLabelsSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        if (_labelsSvg?.Picture is null) return;
        canvas.Scale(DrawScale);
        canvas.DrawPicture(_labelsSvg.Picture);
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
        if (!ReferenceEquals(updated, _worldSvgXmlCurrent))
        {
            _worldSvgXmlCurrent = updated;
            _worldSvg ??= new SKSvg();
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(_worldSvgXmlCurrent));
            _worldSvg.Load(ms);
            InvalidateRequested++;
        }
    }

    public void SetPowerColor(string power, SKColor color)
    {
        _powerColors[power] = color;
        RebuildWorldColors();
    }

    public void AssignTerritory(string className, string power)
    {
        _classOwnership[className] = power;
        RebuildWorldColors();
    }

    private void OnMouseMove(double x, double y)
    {
        if (_hitmapBitmap is null) return;
        var px = (int) Math.Clamp(x, 0, _hitmapBitmap.Width - 1);
        var py = (int) Math.Clamp(y, 0, _hitmapBitmap.Height - 1);
        var color = _hitmapBitmap.GetPixel(px, py);

        // exact match first
        if (_hitColorToClass.TryGetValue(new SKColor(color.Red, color.Green, color.Blue, 255), out var cls))
        {
            HoveredClass = cls;
            return;
        }

        // small neighborhood scan to beat anti-aliasing at edges
        for (var dy = -1; dy <= 1; dy++)
        {
            var yy = py + dy;
            if (yy < 0 || yy >= _hitmapBitmap.Height) continue;
            for (var dx = -1; dx <= 1; dx++)
            {
                var xx = px + dx;
                if (xx < 0 || xx >= _hitmapBitmap.Width) continue;
                var c2 = _hitmapBitmap.GetPixel(xx, yy);
                if (!_hitColorToClass.TryGetValue(new SKColor(c2.Red, c2.Green, c2.Blue, 255), out var cls2)) continue;
                HoveredClass = cls2;
                return;
            }
        }

        // nearest-color fallback with a reasonable threshold (to cope with blended pixels)
        string? bestClass = null;
        var bestDist = int.MaxValue;
        int r = color.Red, g = color.Green, b = color.Blue;
        foreach (var (hc, value) in _hitColorToClass)
        {
            var dr = r - hc.Red;
            var dg = g - hc.Green;
            var db = b - hc.Blue;
            var d2 = dr * dr + dg * dg + db * db;
            if (d2 >= bestDist) continue;
            bestDist = d2;
            bestClass = value;
            if (bestDist == 0) break;
        }

        // Accept only if within threshold to avoid false positives on transparent/background
        if (bestClass != null && bestDist <= 400) // <= 20 units in RGB space
        {
            HoveredClass = bestClass;
        }
        else
        {
            HoveredClass = null;
        }
    }

    private void ClearHover()
    {
        HoveredClass = null;
    }

    private void RebuildWorldColors()
    {
        if (string.IsNullOrWhiteSpace(_worldSvgXmlOriginal)) return;

        // Build fills dictionary per class ownership; default neutral
        var allClasses = _classToHitColor.Keys.Count > 0 ? _classToHitColor.Keys.AsEnumerable() : SvgClassColorService.ExtractClasses(_worldSvgXmlOriginal!);

        var fills = new Dictionary<string, SKColor>(StringComparer.OrdinalIgnoreCase);
        foreach (var cls in allClasses)
        {
            var power = _classOwnership.GetValueOrDefault(cls, "neutral");
            var color = _powerColors.TryGetValue(power, out var c) ? c : _powerColors["neutral"];
            fills[cls] = color;
        }

        var updated = SvgClassColorService.ApplyClassFills(_worldSvgXmlOriginal!, fills, HoveredClass);
        _worldSvgXmlCurrent = updated;
        _worldSvg ??= new SKSvg();
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(_worldSvgXmlCurrent));
        _worldSvg.Load(ms);
        InvalidateRequested++;
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
            var r = (byte) ((i & 0x0000FF));
            var g = (byte) ((i & 0x00FF00) >> 8);
            var b = (byte) ((i & 0xFF0000) >> 16);
            var col = new SKColor(r, g, b, 255);
            _classToHitColor[cls] = col;
            _hitColorToClass[col] = cls;
            i++;
        }

        // Create svg with these fills
        var hitSvgXml = SvgClassColorService.ApplyClassFills(_worldSvgXmlOriginal!, _classToHitColor);
        var svg = new SKSvg();
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(hitSvgXml)))
        {
            svg.Load(ms);
        }

        if (svg.Picture is null) return;

        var width = (int) Math.Ceiling((svg.Picture.CullRect.Width) * DrawScale);
        var height = (int) Math.Ceiling((svg.Picture.CullRect.Height) * DrawScale);

        _hitmapBitmap?.Dispose();
        _hitmapBitmap = new SKBitmap(width, height, true);
        using var canvas = new SKCanvas(_hitmapBitmap);
        canvas.Clear(SKColors.Transparent);
        canvas.Scale(DrawScale);
        canvas.DrawPicture(svg.Picture);
    }

    #endregion

    #region Private Fields

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

    private void LoadLabels(string basePath)
    {
        _labelsSvg = new SKSvg();
        _labelsSvg.Load(Path.Combine(basePath, "Graphics", "Images", "labels.svg"));
    }

    #endregion

}
