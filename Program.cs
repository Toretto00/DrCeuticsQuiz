using Avalonia;

namespace DrCeuticsQuiz;

internal static class Program
{
    // Chạy dạng cửa sổ nhỏ để dev:  dotnet run -- --window
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
