using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AmazonBookReleaseTrackerTray
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();

            Icon = AmazonBookReleaseTrackerTrayContext.icon;
            LoadSettings();
        }

        private void LoadSettings()
        {
            chboxAutoStart.Checked = Properties.Settings.Default.AutoStart;
            numDays.Value = Properties.Settings.Default.NotifyWithin;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutoStart = chboxAutoStart.Checked;
            Properties.Settings.Default.NotifyWithin = (int)numDays.Value;

            Properties.Settings.Default.Save();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnShowFolder_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", Program.baseDirectory);
        }
    }
}
