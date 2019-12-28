using RGB.NET.Core;
using RGB.NET.Brushes;

using RGB.NET.Devices.Corsair;
using RGB.NET.Devices.Logitech;
using System;
using System.Threading.Tasks;
using RGB.NET.Groups;
using System.Linq;
using System.IO.Pipes;
using System.IO;
using System.Reflection;
using Polly;

namespace CorsairTemperatureAlert
{
    class Program
    {
        static async Task Main(string[] args)
        {
            double warnAt = 75;
            double dangerAt = 80;
            double temp = double.MinValue;
            double interval = .5;
            double seconds = 2;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-w" || args[i] == "--warn")
                {
                    double.TryParse(args[i + 1], out warnAt);
                }

                if (args[i] == "-d" || args[i] == "--danger")
                {
                    double.TryParse(args[i + 1], out dangerAt);
                }

                if (args[i] == "-i" || args[i] == "--inteval")
                {
                    double.TryParse(args[i + 1], out interval);
                }

                if (args[i] == "-s" || args[i] == "--seconds")
                {
                    double.TryParse(args[i + 1], out seconds);
                }

                if (args[i] == "-t" || args[i] == "--temperature")
                {
                    double.TryParse(args[i + 1], out temp);
                }
            }

            if (temp == double.MinValue)
            {
                Console.Error.WriteLine("Temperature (`-t` or `--temperature`) is required");
                return;
            }

            FileStream fileHandler = null;

            Policy.Handle<IOException>()
                .WaitAndRetry(10, _ => TimeSpan.FromSeconds(1))
                .Execute(
                () =>
                {
                    fileHandler = File.Open(
                        Path.GetFileName(Assembly.GetExecutingAssembly().Location) + "lock",
                        FileMode.OpenOrCreate,
                        FileAccess.ReadWrite,
                        FileShare.None);
                });

            var surface = RGBSurface.Instance;
            surface.Exception += args => Console.WriteLine(args.Exception.Message);
            surface.LoadDevices(CorsairDeviceProvider.Instance);
            surface.LoadDevices(LogitechDeviceProvider.Instance);
            surface.AlignDevices();

            foreach (var led in surface.Leds)
                Console.WriteLine($"{led.Device.DeviceInfo.DeviceName}: {led.Id}");

            var group = new ListLedGroup(surface.Leds)
            {
                Brush = new SolidColorBrush(Color.Transparent)
            };

            var decorator = new HeatWarningFlashDecorator(
                temp,
                warnAt,
                dangerAt,
                new Color(255, 255, 0),
                new Color(255, 0, 0),
                interval);

            group.Brush.AddDecorator(decorator);

            var updateTrigger = new TimerUpdateTrigger();

            surface.RegisterUpdateTrigger(updateTrigger);

            await Task.Delay((int)Math.Floor(seconds * 1000));

            await fileHandler.DisposeAsync();
        }
    }
}
