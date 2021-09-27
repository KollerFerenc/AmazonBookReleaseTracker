using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AmazonBookReleaseTrackerTray
{
    static class Program
    {
        public static Mutex mutex = new Mutex(true, "AmazonBookReleaseTracker - Tray");

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new AmazonBookReleaseTrackerTrayContext());
                mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("Only one instance at a time.",
                    "One instance!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }
}
