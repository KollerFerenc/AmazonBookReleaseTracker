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
using Microsoft.Win32;
using System.Drawing;

namespace AmazonBookReleaseTrackerTray
{
    public class AmazonBookReleaseTrackerTrayContext : ApplicationContext
    {
        private NotifyIcon _trayIcon;
        private bool _running = false;

        internal int Days { get; private set; } = 7;
        internal AmazonContainer AmazonContainer { get; private set; } = new AmazonContainer();
        internal static readonly Icon icon = new System.Drawing.Icon(File.Open(Path.Combine(Program.baseDirectory, "Assets\\books.ico"), FileMode.Open));

        public AmazonBookReleaseTrackerTrayContext()
        {
            InitializeComponents();

            CheckAutoStart();

            RunTracker();
        }

        private void InitializeComponents()
        {
            _trayIcon = new NotifyIcon()
            {
                Icon = icon,
                ContextMenuStrip = new ContextMenuStrip(),
                Visible = true
            };

            _trayIcon.ContextMenuStrip.Items.Add("Run", null, Run);
            _trayIcon.ContextMenuStrip.Items.Add("Details", null, Details);
            _trayIcon.ContextMenuStrip.Items.Add("Settings", null, Settings);
            _trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            _trayIcon.ContextMenuStrip.Items.Add("Exit", null, Exit);

            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                // Obtain the arguments from the notification
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

                if (args["action"] == "viewDetails")
                {
                    ShowDetails();
                }
            };
        }

        private void CheckAutoStart()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            string appName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;

            if (Properties.Settings.Default.AutoStart)
            {
                registryKey.SetValue(appName, Application.ExecutablePath);
            }
            else
            {
                registryKey.DeleteValue(appName);
            }
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
                    StartInfo = new ProcessStartInfo(Path.Combine(Program.baseDirectory, "AmazonBookReleaseTracker.exe"), "run")
                    {
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                    }
                };

                try
                {
                    tracker.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    throw;
                }
                tracker.WaitForExit();

                if ((ExitCode)tracker.ExitCode == ExitCode.Default)
                {
                    AmazonContainer = new Export().GetData(newOnly: false).GetWithin(Days);
                    SendToast($"{ AmazonContainer.BookCount } book(s) to be released within { Days } day(s).");
                }
                else
                {
                    MessageBox.Show(((ExitCode)tracker.ExitCode).ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }


                _running = false;
            }
        }

        private void SendToast(string message)
        {
            new ToastContentBuilder()
                    .AddArgument("action", "showReleaseSummary")
                    .AddText("Book release!")
                    .AddText(message)
                    .AddButton(new ToastButton()
                        .SetContent("See more details")
                        .AddArgument("action", "viewDetails"))
                    .Show();
        }

        public void Settings(object sender, EventArgs e)
        {
            ShowSettings();
        }

        private void ShowSettings()
        {
            SettingsForm settingsForm= new();
            var reslut = settingsForm.ShowDialog();

            if (reslut == DialogResult.OK)
            {
                CheckAutoStart();
            }

            settingsForm.Dispose();
        }

        public void Details(object sender, EventArgs e)
        {
            ShowDetails();
        }

        private void ShowDetails()
        {
            DetailsForm detailsForm = new(AmazonContainer);
            detailsForm.ShowDialog();
            detailsForm.Dispose();
        }

        public void Exit(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();

            Application.Exit();
        }
    }
}
