using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.Json;
using WindowsInput;
using WindowsInput.Native;

NativeMethods.SetProcessDPIAware();

string appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ScreenShooter");
Directory.CreateDirectory(appFolder);
string configPath = Path.Combine(appFolder, "region.json");

int left, top, width, height;

if (File.Exists(configPath) && LoadConfig(configPath, out left, out top, out width, out height))
{
    Console.WriteLine($"Loaded region: ({left}, {top}) {width}x{height}");
    Console.WriteLine("Press C to recalibrate, any other key to start.");
    if (Console.ReadKey(true).Key == ConsoleKey.C)
        Calibrate(configPath, out left, out top, out width, out height);
}
else
{
    Console.WriteLine("No saved region found. Starting calibration...");
    Calibrate(configPath, out left, out top, out width, out height);
}

Console.WriteLine("Ready. Hold P to take a screenshot. Ctrl+C to exit.");

InputSimulator simulator = new InputSimulator();
while (true)
{
    if (simulator.InputDeviceState.IsKeyDown(VirtualKeyCode.VK_P))
    {
        DoShot(left, top, width, height);
        Thread.Sleep(500);
    }
    Thread.Sleep(10);
}

void DoShot(int left, int top, int width, int height)
{
    using Bitmap screenshot = new Bitmap(width, height);
    using (Graphics graphics = Graphics.FromImage(screenshot))
    {
        graphics.CopyFromScreen(left, top, 0, 0, new Size(width, height));
    }
    string name = $"{DateTime.Now.Ticks}.png";
    screenshot.Save(Path.Combine(appFolder, name));
    Console.WriteLine("Screenshot captured and saved.");
}

void Calibrate(string path, out int l, out int t, out int w, out int h)
{
    Console.WriteLine("Move mouse to the TOP-LEFT corner of the board, then press Enter.");
    Console.ReadLine();
    NativeMethods.GetCursorPos(out POINT topLeft);
    Console.WriteLine($"  Top-left: ({topLeft.X}, {topLeft.Y})");

    Console.WriteLine("Move mouse to the BOTTOM-RIGHT corner of the board, then press Enter.");
    Console.ReadLine();
    NativeMethods.GetCursorPos(out POINT bottomRight);
    Console.WriteLine($"  Bottom-right: ({bottomRight.X}, {bottomRight.Y})");

    l = topLeft.X;
    t = topLeft.Y;
    w = bottomRight.X - topLeft.X;
    h = bottomRight.Y - topLeft.Y;

    var json = JsonSerializer.Serialize(new { Left = l, Top = t, Width = w, Height = h });
    File.WriteAllText(path, json);
    Console.WriteLine($"Region saved: ({l}, {t}) {w}x{h}");
}

bool LoadConfig(string path, out int l, out int t, out int w, out int h)
{
    try
    {
        var doc = JsonDocument.Parse(File.ReadAllText(path));
        l = doc.RootElement.GetProperty("Left").GetInt32();
        t = doc.RootElement.GetProperty("Top").GetInt32();
        w = doc.RootElement.GetProperty("Width").GetInt32();
        h = doc.RootElement.GetProperty("Height").GetInt32();
        return true;
    }
    catch
    {
        l = t = w = h = 0;
        return false;
    }
}

[StructLayout(LayoutKind.Sequential)]
struct POINT { public int X; public int Y; }

static class NativeMethods
{
    [DllImport("user32.dll")]
    public static extern bool SetProcessDPIAware();

    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out POINT lpPoint);
}

