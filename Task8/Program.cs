using System;

namespace CompGraph;

static class Program
{
    static void Main()
    {
        using MainWindow mainWindow = new MainWindow();
        mainWindow.Run(Math.Min(OpenTK.DisplayDevice.Default.RefreshRate, 144));
    }
}