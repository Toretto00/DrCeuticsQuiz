using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using DrCeuticsQuiz.Controls;

namespace DrCeuticsQuiz;

public partial class MainWindow : Window
{
    // ---------- Màu ----------
    static readonly IBrush BrownB = Hex("#A8613A"), BrownDarkB = Hex("#5B2A17"), OliveB = Hex("#A6B858"),
        OliveDarkB = Hex("#77803A"), OkB = Hex("#7FA43A"), BadB = Hex("#C7553A"), DotOff = Hex("#E6D6C2");
    static IBrush Hex(string hex) => new SolidColorBrush(Color.Parse(hex));

    readonly FontFamily _baloo = new("avares://DrCeuticsQuiz/Assets/Fonts#Mont ExtraBold");
    readonly FontFamily _bvpBold = new("avares://DrCeuticsQuiz/Assets/Fonts#Mont Bold");

    // ---------- Dữ liệu ----------
    readonly QuizData _data = QuizData.Load();
    QuizConfig Cfg => _data.Config;
    readonly Random _rng = new();

    sealed record Opt(string Text, bool Ok);
    sealed record GameQ(Question Src, List<Opt> Opts);

    List<GameQ> _qs = new();
    readonly List<bool> _results = new();
    readonly List<(Border Row, Border Key)> _ansViews = new();
    int _qi, _score, _timeLeft, _resultLeft;
    bool _locked;
    string _mode = "home";

    // ---------- Timer ----------
    readonly DispatcherTimer _secTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    readonly DispatcherTimer _animTimer = new() { Interval = TimeSpan.FromMilliseconds(33) };
    readonly DispatcherTimer _nextTimer = new();
    readonly DispatcherTimer _idleTimer = new();
    readonly DateTime _t0 = DateTime.Now;

    // ---------- Animation ----------
    readonly RotateTransform _homeRot = new();
    readonly TranslateTransform _homeMove = new();
    readonly ScaleTransform _pulse = new();
    readonly List<(Rectangle R, double Vy, double Vr)> _confetti = new();

    public MainWindow()
    {
        InitializeComponent();

        // Chế độ kiosk (toàn màn hình) – chạy với tham số --window để ra cửa sổ nhỏ khi dev
        var windowed = Environment.GetCommandLineArgs().Contains("--window");
        if (!windowed)
        {
            SystemDecorations = SystemDecorations.None;
            WindowState = WindowState.FullScreen;
            Topmost = true;
        }

        foreach (var host in new[] { LogoHome, LogoQuiz, LogoResult })
        {
            host.Child = MakeLogo();
        }

        // Nút
        StartHost.Children.Add(MakePill("BẮT ĐẦU", false, StartGame));
        StartHost.RenderTransform = _pulse;
        NextHost.Children.Add(MakePill("TIẾP TỤC", false, Next, 52));

        HomeMascot.RenderTransform = new TransformGroup { Children = { _homeRot, _homeMove } };

        _secTimer.Tick += (_, _) => OnSecond();
        _animTimer.Tick += (_, _) => OnFrame();
        _nextTimer.Tick += (_, _) => { _nextTimer.Stop(); Next(); };
        _idleTimer.Interval = TimeSpan.FromSeconds(Math.Max(10, Cfg.IdleTimeout));
        _idleTimer.Tick += (_, _) => { _idleTimer.Stop(); if (_mode != "home") GoHome(); };
        _animTimer.Start();
        _secTimer.Start();

        // Mọi lần chạm đều reset bộ đếm "không ai chơi"
        AddHandler(PointerPressedEvent, (_, _) => { _idleTimer.Stop(); _idleTimer.Start(); },
            RoutingStrategies.Tunnel, handledEventsToo: true);

        // Thoát cho nhân viên: Ctrl + Shift + Q
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Q && e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                Close();
        };

        ShowScreen(HomeScreen);
    }

    // =====================================================================
    //  LUỒNG GAME
    // =====================================================================
    void ShowScreen(Panel screen)
    {
        foreach (var s in new[] { HomeScreen, QuizScreen, ResultScreen })
        {
            var on = s == screen;
            s.Opacity = on ? 1 : 0;
            s.IsHitTestVisible = on;
            s.ZIndex = on ? 1 : 0;
        }
    }

    void GoHome()
    {
        _nextTimer.Stop();
        _mode = "home";
        ClearConfetti();
        ShowScreen(HomeScreen);
    }

    void StartGame()
    {
        if (_mode == "quiz") return;
        _qs = Shuffle(_data.Questions).Take(Math.Max(1, Cfg.QuestionsPerGame)).Select(q =>
            new GameQ(q, Shuffle(q.Options.Select((t, i) => new Opt(t, i == q.Answer)).ToList()))).ToList();
        _qi = 0; _score = 0; _results.Clear();
        _mode = "quiz";
        ShowScreen(QuizScreen);
        RenderQuestion();
    }

    void RenderQuestion()
    {
        _nextTimer.Stop();
        _locked = false;
        var q = _qs[_qi];
        QStep.Text = $"CÂU {_qi + 1}/{_qs.Count}";

        QDots.Children.Clear();
        for (var k = 0; k < _qs.Count; k++)
        {
            IBrush b = k < _qi ? (_results[k] ? OkB : BadB) : k == _qi ? OliveB : DotOff;
            QDots.Children.Add(new Border { Width = 42, Height = 18, CornerRadius = new CornerRadius(9), Background = b });
        }

        QText.Text = q.Src.Text;
        Answers.Children.Clear();
        _ansViews.Clear();
        for (var k = 0; k < q.Opts.Count; k++)
        {
            var idx = k;
            var (row, key) = MakeAnswer("ABCDEF"[Math.Min(k, 5)].ToString(), q.Opts[k].Text);
            row.PointerReleased += (_, e) => { e.Handled = true; Answer(idx); };
            Answers.Children.Add(row);
            _ansViews.Add((row, key));
        }

        Feedback.Opacity = 0;
        PeekMascot.Expression = "think";
        NextHost.Opacity = 0;
        NextHost.IsHitTestVisible = false;
        ((TextBlock)((Border)NextHost.Children[0]).Child!).Text = _qi == _qs.Count - 1 ? "XEM KẾT QUẢ" : "TIẾP TỤC";

        _timeLeft = Math.Max(5, Cfg.TimePerQuestion);
        TimerNum.Text = _timeLeft.ToString();
        Ring.Progress = 1;
    }

    void Answer(int k)
    {
        if (_mode != "quiz" || _locked) return;
        _locked = true;
        var q = _qs[_qi];
        var okIdx = q.Opts.FindIndex(o => o.Ok);
        var correct = k == okIdx;

        for (var i = 0; i < _ansViews.Count; i++)
        {
            var (row, key) = _ansViews[i];
            if (i == okIdx) Paint(row, key, "#EEF5D6", OkB, "#7FA43A");
            else if (i == k) Paint(row, key, "#FBE3DA", BadB, "#C7553A");
            else row.Opacity = 0.45;
        }

        _results.Add(correct);
        if (correct) _score++;
        PeekMascot.Expression = correct ? "happy" : "sad";

        FbTitle.Text = correct ? "CHÍNH XÁC!" : k < 0 ? "HẾT GIỜ RỒI!" : "CHƯA ĐÚNG RỒI!";
        FbTitle.Foreground = correct ? OkB : BadB;
        FbText.Text = q.Src.Explain;
        Feedback.Opacity = 1;
        NextHost.Opacity = 1;
        NextHost.IsHitTestVisible = true;

        _nextTimer.Interval = TimeSpan.FromMilliseconds(Math.Max(1000, Cfg.AutoNextMs + 1500));
        _nextTimer.Start();
    }

    static void Paint(Border row, Border key, string bg, IBrush accent, string shadow)
    {
        row.Background = new SolidColorBrush(Color.Parse(bg));
        row.BorderBrush = accent;
        row.BoxShadow = BoxShadows.Parse($"0 8 0 0 {shadow}");
        key.Background = accent;
    }

    void Next()
    {
        _nextTimer.Stop();
        if (_mode != "quiz") return;
        if (!_locked) return;
        _qi++;
        if (_qi < _qs.Count) RenderQuestion();
        else Finish();
    }

    void Finish()
    {
        _mode = "result";
        var n = _qs.Count;
        var win = _score >= Math.Min(Cfg.PassScore, n);

        RTitle.Text = win ? "CHÚC MỪNG!" : "TIẾC QUÁ!";
        RScore.Text = $"{_score}/{n}";
        Gift.IsVisible = win;

        if (win)
        {
            SetMsg(("Bạn đã nhận được ", false), ("1 phần quà", true), (" từ DrCeutics!", false));
            StartConfetti();
        }
        else
        {
            SetMsg(($"Cần đúng ít nhất {Cfg.PassScore}/{n} câu để nhận quà.\nThử lại lần nữa cùng Olive nhé!", false));
        }

        // Nút màn kết thúc: cùng độ rộng, chừa khoảng cách với mép màn hình
        Border ResultBtn(string text, bool brown, Action onTap)
        {
            var b = MakePill(text, brown, onTap, 46);
            b.Width = 380;
            b.Padding = new Thickness(0, 14);
            return b;
        }

        ResultBtns.Children.Clear();
        ResultBtns.Children.Add(ResultBtn("TRANG CHỦ", true, GoHome));
        if (!win) ResultBtns.Children.Add(ResultBtn("CHƠI LẠI", false, () => { _mode = "result-done"; StartGame(); }));

        _resultLeft = Math.Max(10, Cfg.ResultTimeout);
        RCount.Text = $"Tự động về trang chủ sau {_resultLeft}s";
        ShowScreen(ResultScreen);
    }

    void SetMsg(params (string Text, bool Bold)[] parts)
    {
        RMsg.Inlines?.Clear();
        RMsg.Inlines ??= new InlineCollection();
        foreach (var (t, bold) in parts)
        {
            var lines = t.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                if (i > 0) RMsg.Inlines.Add(new LineBreak());
                if (lines[i].Length == 0) continue;
                var run = new Run(lines[i]);
                if (bold) { run.FontFamily = _bvpBold; run.Foreground = BrownB; }
                RMsg.Inlines.Add(run);
            }
        }
    }

    void OnSecond()
    {
        if (_mode == "quiz" && !_locked)
        {
            _timeLeft--;
            TimerNum.Text = Math.Max(_timeLeft, 0).ToString();
            Ring.Progress = Math.Max(0, (double)_timeLeft / Math.Max(5, Cfg.TimePerQuestion));
            if (_timeLeft <= 0) Answer(-1);
        }
        else if (_mode == "result")
        {
            _resultLeft--;
            RCount.Text = $"Tự động về trang chủ sau {Math.Max(_resultLeft, 0)}s";
            if (_resultLeft <= 0) GoHome();
        }
    }

    // =====================================================================
    //  ANIMATION (mascot màn chờ nhún nhảy, nút BẮT ĐẦU nhịp, confetti)
    // =====================================================================
    void OnFrame()
    {
        var t = (DateTime.Now - _t0).TotalSeconds;
        _homeRot.Angle = 3 * Math.Sin(t * 2 * Math.PI / 3);
        _homeMove.Y = -18 * (0.5 - 0.5 * Math.Cos(t * 2 * Math.PI / 3));
        var s = 1 + 0.03 * (1 - Math.Cos(t * 2 * Math.PI / 1.6));
        _pulse.ScaleX = s; _pulse.ScaleY = s;

        for (var i = _confetti.Count - 1; i >= 0; i--)
        {
            var (r, vy, vr) = _confetti[i];
            var top = Canvas.GetTop(r) + vy;
            Canvas.SetTop(r, top);
            if (r.RenderTransform is RotateTransform rt) rt.Angle += vr;
            if (top > 1960) { Confetti.Children.Remove(r); _confetti.RemoveAt(i); }
        }
    }

    void StartConfetti()
    {
        ClearConfetti();
        string[] cols = { "#A6B858", "#A8613A", "#5B2A17", "#D9E3A0", "#F2A7A7", "#5BB8F5" };
        for (var i = 0; i < 70; i++)
        {
            var r = new Rectangle
            {
                Width = 26, Height = 26,
                Fill = Hex(cols[i % cols.Length]),
                RadiusX = _rng.NextDouble() < 0.4 ? 13 : 0,
                RadiusY = _rng.NextDouble() < 0.4 ? 13 : 0,
                RenderTransform = new RotateTransform(_rng.Next(360))
            };
            Canvas.SetLeft(r, _rng.NextDouble() * 1080);
            Canvas.SetTop(r, -40 - _rng.NextDouble() * 900);
            Confetti.Children.Add(r);
            _confetti.Add((r, 12 + _rng.NextDouble() * 14, 4 + _rng.NextDouble() * 8));
        }
    }

    void ClearConfetti()
    {
        Confetti.Children.Clear();
        _confetti.Clear();
    }

    // =====================================================================
    //  THÀNH PHẦN GIAO DIỆN
    // =====================================================================
    Control MakeLogo()
    {
        var file = System.IO.Path.Combine(AppContext.BaseDirectory, "logo.png");
        if (File.Exists(file))
        {
            try { return new Image { Source = new Bitmap(file), Height = 84, VerticalAlignment = VerticalAlignment.Center }; }
            catch { /* dùng chữ */ }
        }
        TextBlock T(string s) => new() { Text = s, FontFamily = _baloo, FontSize = 64, Foreground = BrownDarkB, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 10, 0, 0) };
        return new StackPanel
        {
            Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
            Children = { T("DR"), new LogoC { Width = 52, Height = 52, Margin = new Thickness(4, 0, 2, 0), VerticalAlignment = VerticalAlignment.Center }, T("EUTICS") }
        };
    }

    /// Nút bo tròn kiểu Figma: olive (mặc định) hoặc nâu.
    Border MakePill(string text, bool brown, Action onTap, double fontSize = 60)
    {
        var shadowColor = brown ? "#5B2A17" : "#77803A";
        var up = BoxShadows.Parse($"0 12 0 0 {shadowColor}");
        var down = BoxShadows.Parse($"0 2 0 0 {shadowColor}");
        var move = new TranslateTransform();
        var b = new Border
        {
            Background = brown ? BrownB : OliveB,
            CornerRadius = new CornerRadius(80),
            Padding = new Thickness(fontSize * 1.5, fontSize * 0.28, fontSize * 1.5, fontSize * 0.28),
            BoxShadow = up,
            RenderTransform = move,
            Child = new TextBlock { Text = text, FontFamily = _baloo, FontSize = fontSize, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center },
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        b.PointerPressed += (_, _) => { move.Y = 10; b.BoxShadow = down; };
        b.PointerReleased += (_, e) =>
        {
            move.Y = 0; b.BoxShadow = up;
            e.Handled = true;
            onTap();
        };
        b.PointerCaptureLost += (_, _) => { move.Y = 0; b.BoxShadow = up; };
        return b;
    }

    /// Một dòng đáp án (bản gọn: thấp hơn, chữ 38)
    (Border Row, Border Key) MakeAnswer(string letter, string text)
    {
        var key = new Border
        {
            Width = 76, Height = 76, CornerRadius = new CornerRadius(38), Background = OliveB,
            Child = new TextBlock { Text = letter, FontFamily = _baloo, FontSize = 44, Foreground = Brushes.White,
                                    HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }
        };
        var label = new TextBlock { Text = text, FontFamily = _bvpBold, FontSize = 38, LineHeight = 50, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*") };
        Grid.SetColumn(label, 1);
        label.Margin = new Thickness(28, 0, 0, 0);
        grid.Children.Add(key);
        grid.Children.Add(label);

        var move = new TranslateTransform();
        var row = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#D9FFFFFF")),
            BorderBrush = Hex("#E8DCC6"), BorderThickness = new Thickness(5),
            CornerRadius = new CornerRadius(60), Padding = new Thickness(14, 14, 36, 14),
            BoxShadow = BoxShadows.Parse("0 8 0 0 #DCCBB0"), RenderTransform = move, Child = grid
        };
        row.PointerPressed += (_, _) => { if (!_locked) move.Y = 7; };
        row.PointerReleased += (_, _) => move.Y = 0;
        row.PointerCaptureLost += (_, _) => move.Y = 0;
        return (row, key);
    }

    List<T> Shuffle<T>(IEnumerable<T> src)
    {
        var a = src.ToList();
        for (var i = a.Count - 1; i > 0; i--)
        {
            var j = _rng.Next(i + 1);
            (a[i], a[j]) = (a[j], a[i]);
        }
        return a;
    }
}