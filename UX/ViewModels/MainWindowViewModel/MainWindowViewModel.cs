using System.IO;
using Library.Utilities;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Svg.Skia;

namespace UX.ViewModels.MainWindowViewModel;

public class MainWindowViewModel : BaseViewModel.BaseViewModel
{

    private SKSvg? _waterSvg;
    private SKSvg? _worldSvg;

    public RelayCommand<SKPaintSurfaceEventArgs> PaintWaterSurfaceCommand => new(PaintWaterSurface);
    public RelayCommand<SKPaintSurfaceEventArgs> PaintWorldSurfaceCommand => new(PaintWorldSurface);

    #region Actions

    protected override void LoadedAction()
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        _waterSvg = new SKSvg();
        _waterSvg.Load(Path.Combine(basePath, "Graphics", "Images", "world_water.svg"));
        _worldSvg = new SKSvg();
        _worldSvg.Load(Path.Combine(basePath, "Graphics", "Images", "world.svg"));
    }

    private void PaintWaterSurface(SKPaintSurfaceEventArgs e)
    {
        Console.WriteLine("here");
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        if (_waterSvg?.Picture == null) return;
        var scale = Math.Min(e.Info.Width / _waterSvg.Picture.CullRect.Width, e.Info.Height / _waterSvg.Picture.CullRect.Height);
        canvas.Scale(scale);
        canvas.DrawPicture(_waterSvg.Picture);
    }

    private void PaintWorldSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        if (_worldSvg?.Picture == null) return;
        var scale = Math.Min(e.Info.Width / _worldSvg.Picture.CullRect.Width, e.Info.Height / _worldSvg.Picture.CullRect.Height);
        canvas.Scale(scale);
        canvas.DrawPicture(_worldSvg.Picture);
    }

    #endregion

}
