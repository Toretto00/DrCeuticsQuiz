using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace DrCeuticsQuiz.Controls;

internal static class Ink
{
    public static SolidColorBrush B(string hex) => new(Color.Parse(hex));
    public static Pen P(string hex, double w) => new(B(hex), w, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
    public static Geometry G(string path) => StreamGeometry.Parse(path);

    public static LinearGradientBrush Lin(double x1, double y1, double x2, double y2, params (string c, double o)[] stops)
    {
        var b = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(x1, y1, RelativeUnit.Relative),
            EndPoint = new RelativePoint(x2, y2, RelativeUnit.Relative)
        };
        foreach (var (c, o) in stops) b.GradientStops.Add(new GradientStop(Color.Parse(c), o));
        return b;
    }

    /// Scale bản vẽ gốc (w x h) vào giữa Bounds của control.
    public static DrawingContext.PushedState Fit(DrawingContext ctx, Rect bounds, double w, double h)
    {
        var s = Math.Min(bounds.Width / w, bounds.Height / h);
        var ox = (bounds.Width - w * s) / 2;
        var oy = (bounds.Height - h * s) / 2;
        return ctx.PushTransform(Matrix.CreateScale(s, s) * Matrix.CreateTranslation(ox, oy));
    }
}

/// <summary>Mascot quả Olive – vẽ bằng vector. Expression: happy | think | wink | sad</summary>
public class Mascot : Control
{
    public static readonly StyledProperty<string> ExpressionProperty =
        AvaloniaProperty.Register<Mascot, string>(nameof(Expression), "happy");

    public string Expression
    {
        get => GetValue(ExpressionProperty);
        set => SetValue(ExpressionProperty, value);
    }

    static Mascot() => AffectsRender<Mascot>(ExpressionProperty);

    static readonly IBrush Body = Ink.Lin(0.25, 0.1, 0.8, 1, ("#BDCC72", 0), ("#93A548", 0.6), ("#6E7C2E", 1));
    static readonly IBrush Leaf = Ink.Lin(0, 0, 1, 0, ("#C9D88A", 0), ("#8FA247", 1));
    static readonly IBrush Shadow = Ink.B("#1F5B2A17");
    static readonly IBrush Shine = Ink.B("#59FFFFFF");
    static readonly IBrush Cheek = Ink.B("#F2A7A7");
    static readonly IBrush Dark = Ink.B("#3B2A14");
    static readonly IBrush Mouth = Ink.B("#5B2A17");
    static readonly IBrush Tongue = Ink.B("#E98A8A");
    static readonly IBrush White = Brushes.White;
    static readonly IBrush Tear = Ink.B("#7CC4F2");
    static readonly Pen Stem = Ink.P("#7A4A2A", 9);
    static readonly Pen Vein = Ink.P("#7F9440", 2.5);
    static readonly Pen Line7 = Ink.P("#3B2A14", 7);
    static readonly Pen Line6 = Ink.P("#5B2A17", 6);

    static readonly Geometry StemG = Ink.G("M118,52 Q122,26 140,10");
    static readonly Geometry LeafG = Ink.G("M120,40 Q80,4 38,24 Q72,52 120,40 Z");
    static readonly Geometry VeinG = Ink.G("M118,39 Q84,25 48,25");
    static readonly Geometry HappyEyeL = Ink.G("M70,118 Q82,102 94,118");
    static readonly Geometry HappyEyeR = Ink.G("M126,118 Q138,102 150,118");
    static readonly Geometry SadEyeL = Ink.G("M72,112 Q84,122 96,112");
    static readonly Geometry SadEyeR = Ink.G("M126,112 Q138,122 150,112");
    static readonly Geometry WinkEye = Ink.G("M126,118 L138,110 L150,118");
    static readonly Geometry Smile = Ink.G("M96,140 Q111,162 126,140 Z");
    static readonly Geometry TongueG = Ink.G("M103,148 Q111,156 119,148 Z");
    static readonly Geometry Frown = Ink.G("M98,152 Q111,140 124,152");
    static readonly Geometry TearG = Ink.G("M156,128 Q162,142 156,148 Q150,142 156,128 Z");

    static readonly Dictionary<string, Bitmap?> Cache = new();

    static Bitmap? LoadImage(string expr)
    {
        if (Cache.TryGetValue(expr, out var cached)) return cached;
        Bitmap? bmp = null;
        foreach (var name in new[] { $"olive_{expr}.png", "olive.png" })
        {
            var uri = new Uri($"avares://DrCeuticsQuiz/Assets/{name}");
            if (AssetLoader.Exists(uri)) { bmp = new Bitmap(AssetLoader.Open(uri)); break; }
        }
        Cache[expr] = bmp;
        return bmp;
    }

    public override void Render(DrawingContext ctx)
    {
        var bmp = LoadImage(Expression);
        if (bmp == null) { RenderVector(ctx); return; }   // không có ảnh → vẽ mascot cũ

        // Co ảnh vừa khung, giữ đúng tỉ lệ, căn giữa
        var w = bmp.Size.Width;
        var h = bmp.Size.Height;
        var s = Math.Min(Bounds.Width / w, Bounds.Height / h);
        var dest = new Rect((Bounds.Width - w * s) / 2, (Bounds.Height - h * s) / 2, w * s, h * s);
        ctx.DrawImage(bmp, dest);
    }

    void RenderVector(DrawingContext ctx)
    {
        using var _ = Ink.Fit(ctx, Bounds, 222, 240);

        ctx.DrawEllipse(Shadow, null, new Point(111, 226), 70, 10);
        ctx.DrawGeometry(null, Stem, StemG);
        ctx.DrawGeometry(Leaf, null, LeafG);
        ctx.DrawGeometry(null, Vein, VeinG);
        ctx.DrawEllipse(Body, null, new Point(111, 132), 96, 88);
        ctx.DrawEllipse(Shine, null, new Point(74, 88), 20, 11);

        switch (Expression)
        {
            case "think":
                Eye(ctx, 84); Eye(ctx, 138);
                ctx.DrawEllipse(Mouth, null, new Point(111, 146), 9, 7);
                break;
            case "wink":
                Eye(ctx, 84);
                ctx.DrawGeometry(null, Line7, WinkEye);
                ctx.DrawGeometry(Mouth, null, Smile);
                ctx.DrawGeometry(Tongue, null, TongueG);
                break;
            case "sad":
                ctx.DrawGeometry(null, Line7, SadEyeL);
                ctx.DrawGeometry(null, Line7, SadEyeR);
                ctx.DrawGeometry(Tear, null, TearG);
                ctx.DrawGeometry(null, Line6, Frown);
                break;
            default: // happy
                ctx.DrawGeometry(null, Line7, HappyEyeL);
                ctx.DrawGeometry(null, Line7, HappyEyeR);
                ctx.DrawGeometry(Mouth, null, Smile);
                ctx.DrawGeometry(Tongue, null, TongueG);
                break;
        }
        ctx.DrawEllipse(Cheek, null, new Point(62, 148), 15, 10);
        ctx.DrawEllipse(Cheek, null, new Point(160, 148), 15, 10);
    }

    static void Eye(DrawingContext ctx, double x)
    {
        ctx.DrawEllipse(Dark, null, new Point(x, 116), 9, 9);
        ctx.DrawEllipse(White, null, new Point(x + 3, 112), 3, 3);
    }
}

/// <summary>Hộp quà màu nâu – nơ olive.</summary>
public class GiftBox : Control
{
    static readonly IBrush Brown = Ink.B("#A8613A"), BrownDark = Ink.B("#8A4A28"), Lid = Ink.B("#B9744C"),
        Olive = Ink.B("#A6B858"), OliveDark = Ink.B("#77803A"), Peach = Ink.B("#F5E1D0"), Shadow = Ink.B("#265B2A17");
    static readonly Pen Bow = new(OliveDark, 5);
    static readonly Geometry BowL = Ink.G("M110,62 Q60,2 44,44 Q38,66 110,62 Z");
    static readonly Geometry BowR = Ink.G("M110,62 Q160,2 176,44 Q182,66 110,62 Z");

    public override void Render(DrawingContext ctx)
    {
        using var _ = Ink.Fit(ctx, Bounds, 220, 210);
        ctx.DrawEllipse(Shadow, null, new Point(110, 200), 90, 9);
        ctx.DrawRectangle(Brown, null, new RoundedRect(new Rect(22, 88, 176, 108), 14));
        ctx.DrawRectangle(BrownDark, null, new Rect(22, 88, 176, 22));
        ctx.DrawRectangle(Lid, null, new RoundedRect(new Rect(10, 62, 200, 40), 12));
        ctx.DrawRectangle(Olive, null, new Rect(96, 62, 28, 134));
        ctx.DrawGeometry(Olive, Bow, BowL);
        ctx.DrawGeometry(Olive, Bow, BowR);
        ctx.DrawEllipse(OliveDark, null, new Point(110, 62), 14, 14);
        ctx.DrawRectangle(Peach, null, new Rect(40, 130, 16, 16));
        ctx.DrawRectangle(Peach, null, new Rect(160, 150, 12, 12));
    }
}

/// <summary>Vòng đếm ngược. Progress: 1 → 0.</summary>
public class TimerRing : Control
{
    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<TimerRing, double>(nameof(Progress), 1.0);

    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    static TimerRing() => AffectsRender<TimerRing>(ProgressProperty);

    static readonly IBrush Fill = Ink.B("#A6B858");
    static readonly Pen Track = new(Ink.B("#E8DCC6"), 7);
    static readonly Pen Bar = Ink.P("#77803A", 7);

    public override void Render(DrawingContext ctx)
    {
        using var _ = Ink.Fit(ctx, Bounds, 100, 100);
        var c = new Point(50, 50);
        ctx.DrawEllipse(Fill, Track, c, 46, 46);
        var p = Math.Clamp(Progress, 0, 1);
        if (p >= 0.999) { ctx.DrawEllipse(null, Bar, c, 46, 46); return; }
        if (p <= 0.001) return;
        ctx.DrawGeometry(null, Bar, Arc(c, 46, -90, p * 360));
    }

    internal static StreamGeometry Arc(Point c, double r, double startDeg, double sweepDeg)
    {
        Point At(double deg) => new(c.X + r * Math.Cos(deg * Math.PI / 180), c.Y + r * Math.Sin(deg * Math.PI / 180));
        var g = new StreamGeometry();
        using (var sg = g.Open())
        {
            sg.BeginFigure(At(startDeg), false);
            sg.ArcTo(At(startDeg + sweepDeg), new Size(r, r), 0, sweepDeg > 180, SweepDirection.Clockwise);
            sg.EndFigure(false);
        }
        return g;
    }
}

/// <summary>Chữ "C" hình vòng olive trong logo DR·C·EUTICS.</summary>
public class LogoC : Control
{
    static readonly Pen Ring = Ink.P("#A6B858", 9);
    public override void Render(DrawingContext ctx)
    {
        using var _ = Ink.Fit(ctx, Bounds, 52, 52);
        ctx.DrawGeometry(null, Ring, TimerRing.Arc(new Point(26, 26), 21.5, 40, 280));
    }
}
