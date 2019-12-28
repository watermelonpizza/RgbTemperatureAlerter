using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TemperatureAlerter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string PipeName = "CorsairNotificationUpdatePipe";

        private TaskbarIcon _taskbarIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Any() && e.Args[0] == "-u")
            {
                var clientPipe = new NamedPipeClientStream(PipeName);
                Console.WriteLine("Initiating pipe, connecting to server");

                try
                {
                    clientPipe.Connect(2000);

                    var ss = new StreamString(clientPipe);
                    ss.WriteString(e.Args[1]);

                }
                catch (TimeoutException)
                {
                    Console.Error.WriteLine("Failed to connect to server. It might not be listening or is busy.");
                }

                ApplicationManager.Instance.ExitCommand.Execute(null);
            }
            else
            {
                base.OnStartup(e);

                _taskbarIcon = (TaskbarIcon)FindResource("TaskbarIcon");
                _taskbarIcon.ShowBalloonTip("Temperature Alerts is running", "Listening for temp data.", BalloonIcon.Info);

                ApplicationManager.Instance.Initialize();
            }
        }
    }
}
