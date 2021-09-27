using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AmazonBookReleaseTracker;
using Microsoft.Toolkit.Uwp.Notifications;
using System.IO;
using System.Diagnostics;

namespace AmazonBookReleaseTrackerTray
{
    public class AmazonBookReleaseTrackerTrayContext : ApplicationContext
    {
        private NotifyIcon _trayIcon;
        private bool _running = false;

        public AmazonBookReleaseTrackerTrayContext()
        {
            InitializeComponents();
            RunTracker();
        }

        private void InitializeComponents()
        {
            _trayIcon = new NotifyIcon()
            {
                Icon = new System.Drawing.Icon(File.Open(@"Assets\books.ico", FileMode.Open)),
                ContextMenuStrip = new ContextMenuStrip(),
                Visible = true
            };

            _trayIcon.ContextMenuStrip.Items.Add("Run", null, Run);
            _trayIcon.ContextMenuStrip.Items.Add("Settings", null, ShowSettings);
            _trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            _trayIcon.ContextMenuStrip.Items.Add("Exit", null, Exit);
        }

        public void Run(object sender, EventArgs e)
        {
            RunTracker();
        }

        private void RunTracker()
        {
            if (!_running)
            {
                _running = true;

                var tracker = new Process
                {
                    StartInfo = new ProcessStartInfo("AmazonBookReleaseTracker.exe", "run")
                    {
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                    }
                };

                tracker.Start();
                tracker.WaitForExit();

                if ((ExitCode)tracker.ExitCode == ExitCode.Default)
                {
                    var data = new Export().GetData(newOnly: false);
                }

                _running = false;
            }
        }

        private void SendToast()
        {
            new ToastContentBuilder()
                    .AddArgument("action", "viewConversation")
                    .AddText("Andrew sent you a picture")
                    .AddText("Check this out, The Enchantments in Washington!")
                    .Show();
        }

        public void ShowSettings(object sender, EventArgs e)
        {

        }

        public void Exit(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();

            Application.Exit();
        }
    }
}
