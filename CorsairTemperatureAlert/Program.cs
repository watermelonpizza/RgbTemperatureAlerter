using RGB.NET.Core;
using RGB.NET.Brushes;

using RGB.NET.Devices.Corsair;
using RGB.NET.Devices.Logitech;
using System;
using System.Threading.Tasks;
using RGB.NET.Groups;
using System.Linq;
using System.IO.Pipes;

namespace CorsairTemperatureAlert
{
    class Program
    {
        private const string PipeName = "CorsairNotificationUpdatePipe";

        static async Task Main(string[] args)
        {
            if (args.Any() && args[0] == "-u")
            {
                var clientPipe = new NamedPipeClientStream(PipeName);
                Console.WriteLine("Initiating pipe, connecting to server");

                try
                {
                    await clientPipe.ConnectAsync(2000);

                    var ss = new StreamString(clientPipe);
                    ss.WriteString(args[1]);
                }
                catch (TimeoutException)
                {
                    Console.Error.WriteLine("Failed to connect to server. It might not be listening or is busy.");
                    return;
                }
            }
            else
            {
                double currentTemperature = 0;
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
                    () => (int)Math.Floor(currentTemperature),
                    75,
                    80,
                    new Color(255, 255, 0),
                    new Color(255, 0, 0));

                group.Brush.AddDecorator(decorator);

                var updateTrigger = new TimerUpdateTrigger();

                surface.RegisterUpdateTrigger(updateTrigger);

                bool hasReset = false;

                while (true)
                {
                    using var serverPipe = new NamedPipeServerStream(PipeName, PipeDirection.InOut);

                    Console.WriteLine("Initiating pipe, waiting for connection");
                    await serverPipe.WaitForConnectionAsync();
                    Console.WriteLine("Connected!");

                    var ss = new StreamString(serverPipe);
                    double.TryParse(ss.ReadString(), out currentTemperature);

                    Console.WriteLine("Read value: " + currentTemperature);

                    if (currentTemperature < 60)
                    {
                        updateTrigger.Stop();
                        CorsairDeviceProvider.Instance.ResetDevices();
                        LogitechDeviceProvider.Instance.ResetDevices();
                        hasReset = true;
                    }
                    else if (hasReset)
                    {
                        CorsairDeviceProvider.Instance.Initialize();
                        LogitechDeviceProvider.Instance.Initialize();
                        updateTrigger.Start();
                        hasReset = false;
                    }
                }
            }
        }
    }
}
