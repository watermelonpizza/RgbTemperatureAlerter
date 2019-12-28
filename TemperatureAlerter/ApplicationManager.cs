using RGB.NET.Brushes;
using RGB.NET.Core;
using RGB.NET.Devices.Corsair;
using RGB.NET.Devices.Logitech;
using RGB.NET.Groups;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TemperatureAlerter
{
    public class ApplicationManager
    {
        private readonly CancellationTokenSource _updateTokenSource;
        private readonly CancellationToken _updateToken;

        public static ApplicationManager Instance { get; } = new ApplicationManager();

        private ApplicationManager()
        {
            _updateTokenSource = new CancellationTokenSource();
            _updateToken = _updateTokenSource.Token;
        }

        public Command ExitCommand { get; } =
            new Command(() =>
            {
                try { RGBSurface.Instance?.Dispose(); } catch { }
                Application.Current.Shutdown();
            });

        internal void Initialize()
        {
            Task.Factory.StartNew(
                (object token) =>
                {
                    double currentTemperature = double.MinValue;
                    double lastTemperature = double.MinValue;
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

                    var warningTemp = 75;

                    var decorator = new HeatWarningFlashDecorator(
                        () => (int)Math.Floor(currentTemperature),
                        warningTemp,
                        80,
                        new Color(255, 255, 0),
                        new Color(255, 0, 0));

                    group.Brush.AddDecorator(decorator);

                    var updateTrigger = new TimerUpdateTrigger();

                    surface.RegisterUpdateTrigger(updateTrigger);

                    while (!((CancellationToken)token).IsCancellationRequested)
                    {
                        using var serverPipe = new NamedPipeServerStream(App.PipeName, PipeDirection.InOut);

                        Console.WriteLine("Initiating pipe, waiting for connection");
                        serverPipe.WaitForConnection();

                        Console.WriteLine("Connected!");

                        var ss = new StreamString(serverPipe);
                        double.TryParse(ss.ReadString(), out currentTemperature);

                        Console.WriteLine("Read value: " + currentTemperature);

                        if (lastTemperature != double.MinValue)
                        {
                            if (currentTemperature < warningTemp && lastTemperature >= warningTemp)
                            {
                                updateTrigger.Stop();
                                CorsairDeviceProvider.Instance.ResetDevices();
                                LogitechDeviceProvider.Instance.ResetDevices();
                                Thread.Sleep(2000);
                            }
                            else if (currentTemperature >= warningTemp && lastTemperature < warningTemp)
                            {
                                CorsairDeviceProvider.Instance.Initialize();
                                LogitechDeviceProvider.Instance.Initialize();
                                updateTrigger.Start();
                                Thread.Sleep(2000);
                            }
                        }

                        lastTemperature = currentTemperature;
                    }
                },
                _updateToken);
        }
    }
}
