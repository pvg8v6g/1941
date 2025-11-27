using System.IO;
using System.Text;
using Library.Services;
using Library.Utilities;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Svg.Skia;

namespace UX.ViewModels.GameViewModel;

public class GameViewModel(ISvgClassColorService svgClassColorService) : BaseViewModel.BaseViewModel
{

    #region Properties

    public int InvalidateRequested
    {
        get;
        private set
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

    public RelayCommand<SKPaintSurfaceEventArgs> PaintWaterSurfaceCommand => new(PaintWaterSurface);

    public RelayCommand<SKPaintSurfaceEventArgs> PaintWorldSurfaceCommand => new(PaintWorldSurface);

    public RelayCommand<SKPaintSurfaceEventArgs> PaintBordersSurfaceCommand => new(PaintBordersSurface);

    public ISvgClassColorService SvgClassColorService => svgClassColorService;

    #endregion

    #region Fields

    private SKSvg? _waterSvg;
    private SKSvg? _worldSvg;
    private SKSvg? _bordersSvg;
    private string? _worldSvgXmlOriginal;
    private string? _worldSvgXmlCurrent;

    #endregion

    #region Actions

    // Primary constructor is used; optional parameter keeps design-time support.

    protected override void LoadedAction()
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        _waterSvg = new SKSvg();
        _waterSvg.Load(Path.Combine(basePath, "Graphics", "Images", "world_water.svg"));
        _worldSvg = new SKSvg();
        // Load world.svg from text so we can mutate styles later
        var worldPath = Path.Combine(basePath, "Graphics", "Images", "world.svg");
        if (File.Exists(worldPath))
        {
            _worldSvgXmlOriginal = File.ReadAllText(worldPath, Encoding.UTF8);
            _worldSvgXmlCurrent = _worldSvgXmlOriginal;
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(_worldSvgXmlCurrent));
            _worldSvg.Load(ms);
        }

        _bordersSvg = new SKSvg();
        _bordersSvg.Load(Path.Combine(basePath, "Graphics", "Images", "world_borders.svg"));

        if (_waterSvg?.Picture is not null)
        {
            var scale = 2.5d;
            MapWidth = _waterSvg.Picture.CullRect.Width * scale;
            MapHeight = _waterSvg.Picture.CullRect.Height * scale;
        }

        InvalidateRequested++;
    }

    private void PaintWaterSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        if (_waterSvg?.Picture is null) return;
        var scale = 2.5f;
        canvas.Scale(scale);
        canvas.DrawPicture(_waterSvg.Picture);
    }

    private void PaintWorldSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;

        canvas.Clear(SKColors.Transparent);
        if (_worldSvg?.Picture is null) return;
        var scale = 2.5f;
        canvas.Scale(scale);
        canvas.DrawPicture(_worldSvg.Picture);
    }

    private void PaintBordersSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        if (_bordersSvg?.Picture is null) return;
        var scale = 2.5f;
        canvas.Scale(scale);
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
        if (!ReferenceEquals(updated, _worldSvgXmlCurrent))
        {
            _worldSvgXmlCurrent = updated;
            if (_worldSvg is null) _worldSvg = new SKSvg();
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(_worldSvgXmlCurrent));
            _worldSvg.Load(ms);
            InvalidateRequested++;
        }
    }

    #endregion

}
