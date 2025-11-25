using System.IO;
using Library.Utilities;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Svg.Skia;

namespace UX.ViewModels.MainWindowViewModel;

public class MainWindowViewModel : BaseViewModel.BaseViewModel
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

    #endregion

    #region Fields

    private SKSvg? _waterSvg;
    private SKSvg? _worldSvg;
    private SKSvg? _bordersSvg;

    #endregion

    #region Actions

    protected override void LoadedAction()
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        _waterSvg = new SKSvg();
        _waterSvg.Load(Path.Combine(basePath, "Graphics", "Images", "world_water.svg"));
        _worldSvg = new SKSvg();
        _worldSvg.Load(Path.Combine(basePath, "Graphics", "Images", "world.svg"));
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

    #endregion

}
