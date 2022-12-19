using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace CompGraph;

public static class Program
{
    private static void Main()
    {
        var nativeWindowSettings = new NativeWindowSettings()
        {
            Size = new Vector2i(800, 800),
            Title = "Some stuff",
        };

        using var window = new Window(GameWindowSettings.Default, nativeWindowSettings);
        window.Run();
    }
}