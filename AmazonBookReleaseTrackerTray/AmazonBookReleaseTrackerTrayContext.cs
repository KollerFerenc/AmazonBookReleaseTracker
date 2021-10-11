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
using System.ComponentModel;
using System.Threading;

namespace AmazonBookReleaseTrackerTray
{
    public class AmazonBookReleaseTrackerTrayContext : ApplicationContext
    {
        private NotifyIcon _trayIcon;

        private readonly BackgroundWorker _backgroundWorker = new();
        private CancellationTokenSource _cancellationTokenSource = new();

        internal AmazonContainer AmazonContainer { get; private set; } = new AmazonContainer();
        internal static readonly Icon icon = new System.Drawing.Icon(File.Open(Path.Combine(Program.baseDirectory, "Assets\\books.ico"), FileMode.Open));

        public AmazonBookReleaseTrackerTrayContext()
        {
            InitializeComponents();
            ConfigureBackgroundWorker();

#if RELEASE
            CheckAutoStart();
            RunTracker();
#endif
        }

        private void InitializeComponents()
        {
            _trayIcon = new NotifyIcon()
            {
                Icon = icon,
                ContextMenuStrip = new ContextMenuStrip(),
                Visible = true
            };

            var tracker = new ToolStripMenuItem("Tracker");
            tracker.DropDownItems.Add("Add", null, Add);
            tracker.DropDownItems.Add("Remove", null, Remove);
            tracker.DropDownItems.Add("Ignore", null, Ignore);
            tracker.DropDownItems.Add(new ToolStripSeparator());
            tracker.DropDownItems.Add("Show tracked", null, ShowTracked);

            _trayIcon.ContextMenuStrip.Items.Add("Run", null, Run);
            _trayIcon.ContextMenuStrip.Items.Add(tracker);
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

        private void ConfigureBackgroundWorker()
        {
            _backgroundWorker.WorkerReportsProgress = false;
            _backgroundWorker.WorkerSupportsCancellation = true;

            _backgroundWorker.DoWork += BackgroundWorker_DoWork;
            _backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
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
            if (!_backgroundWorker.IsBusy)
            {
                _cancellationTokenSource = new();
                var ct = _cancellationTokenSource.Token;
                ct.Register(_backgroundWorker.CancelAsync);

                _backgroundWorker.RunWorkerAsync();
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

        public void Add(object sender, EventArgs e)
        {
            AddForm addForm = new();
            addForm.ShowDialog();
            addForm.Dispose();
        }

        public void Remove(object sender, EventArgs e)
        {
            RemoveForm removeForm = new();
            removeForm.ShowDialog();
            removeForm.Dispose();
        }

        public void Ignore(object sender, EventArgs e)
        {
            IgnoreForm ignoreForm = new();
            ignoreForm.ShowDialog();
            ignoreForm.Dispose();
        }

        public void ShowTracked(object sender, EventArgs e)
        {
            DetailsForm detailsForm = new(new Export().GetExportConsoleLines(newOnly: false));
            detailsForm.ShowDialog();
            detailsForm.Dispose();
        }

        public void Exit(object sender, EventArgs e)
        {
            if (_backgroundWorker.WorkerSupportsCancellation && _backgroundWorker.IsBusy)
            {
                _backgroundWorker.CancelAsync();
                _cancellationTokenSource.Cancel();

                while (_backgroundWorker.IsBusy)
                {
                    Thread.Sleep(100);
                }
            }

            _trayIcon.Visible = false;
            _trayIcon.Dispose();

            _backgroundWorker.Dispose();
            _cancellationTokenSource.Dispose();

            Application.Exit();
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            var tracker = new AmazonBookReleaseTracker.AmazonBookReleaseTracker();
            try
            {
                int? result = Task.Run(() => tracker.RunTrackingAsync(_cancellationTokenSource.Token)).GetAwaiter().GetResult();
                e.Result = result;
            }
            catch (TaskCanceledException)
            {
                e.Cancel = true;
            }
            catch (OperationCanceledException)
            {
                e.Cancel = true;
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                MessageBox.Show("Cancelled!", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                int? result = e.Result as int?;
                if (result.HasValue && (ExitCode)result.Value == ExitCode.Default)
                {
                    AmazonContainer = new Export().GetData(newOnly: false).GetWithin(Properties.Settings.Default.NotifyWithin);
                    if (AmazonContainer.BookCount > 0)
                    {
                        SendToast($"{ AmazonContainer.BookCount } book(s) to be released within { Properties.Settings.Default.NotifyWithin } day(s).");
                    }
                }
                else if (result.HasValue)
                {
                    MessageBox.Show(((ExitCode)result.Value).ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
