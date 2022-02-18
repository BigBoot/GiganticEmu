using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;

namespace GiganticEmu.Launcher;

public class BlurBehind : Control
{
    public static readonly StyledProperty<Brush> BackgroundProperty =
        AvaloniaProperty.Register<Border, Brush>(nameof(Background));

    public static readonly StyledProperty<double> CornerRadiusProperty =
        AvaloniaProperty.Register<Border, double>(nameof(CornerRadius));

    public static readonly StyledProperty<double> StrengthProperty =
        AvaloniaProperty.Register<Border, double>(nameof(Strength), defaultValue: 3.0);

    static BlurBehind()
    {
        AffectsRender<Border>(
            BackgroundProperty,
            CornerRadiusProperty,
            StrengthProperty
        );
    }

    public BlurBehind()
    {
    }

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public double CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public double Strength
    {
        get => GetValue(StrengthProperty);
        set => SetValue(StrengthProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        if (Background is IBrush background)
        {
            context.DrawRectangle(background, null, new Rect(Bounds.Size), CornerRadius, CornerRadius);
        }

        context.Custom(new RenderOperation(new RenderConfig(
            Bounds: new RoundedRect(new Rect(default, Bounds.Size), CornerRadius),
            Strength: Strength
        )));

        Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
    }

    private record RenderConfig(RoundedRect Bounds, double Strength);

    private class RenderOperation : ICustomDrawOperation
    {
        private readonly RenderConfig _config;

        public RenderOperation(RenderConfig config)
        {
            _config = config;
        }

        public void Dispose()
        {
        }

        public bool HitTest(Point p) => _config.Bounds.Rect.Contains(p);

        public void Render(IDrawingContextImpl context)
        {
            if (context is not ISkiaDrawingContextImpl skia)
                return;

            if (!skia.SkCanvas.TotalMatrix.TryInvert(out var currentInvertedTransform))
                return;

            using var snapshot = skia.SkSurface.Snapshot();

            using var backdropShader = SKShader.CreateImage(snapshot, SKShaderTileMode.Clamp,
                SKShaderTileMode.Clamp, currentInvertedTransform);

            var width = (int)Math.Max(Math.Ceiling(_config.Bounds.Rect.Width), 1);
            var height = (int)Math.Max(Math.Ceiling(_config.Bounds.Rect.Height), 1);
            var imageInfo = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            var sigma = (float)_config.Strength;

            if (skia.GrContext != null)
            {
                skia.SkCanvas.Save();
                skia.SkCanvas.ClipRoundRect(_config.Bounds.ToSKRoundRect());

                using var filter = SKImageFilter.CreateBlur(sigma, sigma, SKShaderTileMode.Clamp, null);
                using var tmp = new SKPaint()
                {
                    Shader = backdropShader,
                    ImageFilter = filter,
                };

                skia.SkCanvas.DrawRoundRect(_config.Bounds.ToSKRoundRect(), tmp);
                skia.SkCanvas.Restore();
            }
        }

        public Rect Bounds => _config.Bounds.Rect.Inflate(4);

        public bool Equals(ICustomDrawOperation? other)
        {
            return other is RenderOperation op && op._config == _config;
        }
    }
}