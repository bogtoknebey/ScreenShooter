using System;
using System.Drawing;
using System.Reflection;
using System.Threading;
using WindowsInput;
using WindowsInput.Native;

int left = 384;
int top = 147;
int width = 1440 - left - 30 + 13;
int height = 900 - top - 137;


InputSimulator simulator = new InputSimulator();
while (true)
{
    if (simulator.InputDeviceState.IsKeyDown(VirtualKeyCode.VK_P))
    {
        DoShot(left, top, width, height);
        Thread.Sleep(500);
    }
}


void DoShot(int left, int top, int width, int height, int delay = 0)
{
    Thread.Sleep(delay);
    using (Bitmap screenshot = new Bitmap(width, height))
    {
        using (Graphics graphics = Graphics.FromImage(screenshot))
        {
            graphics.CopyFromScreen(left, top, 0, 0, new Size(width, height));
        }
        string name = $"{DateTime.Now.Ticks}.png";
        screenshot.Save($"C:\\Users\\US999\\Desktop\\Material\\{name}");
    }
    Console.WriteLine("Screenshot captured and saved.");
}

