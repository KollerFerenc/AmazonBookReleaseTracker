using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AmazonBookReleaseTracker;

namespace AmazonBookReleaseTrackerTray
{
    public partial class AddForm : Form
    {
        private readonly List<AmazonLink> _links = new(4);

        public AddForm()
        {
            InitializeComponent();

            Icon = AmazonBookReleaseTrackerTrayContext.icon;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (txtLink.Text != string.Empty)
            {
                var link = new AmazonLink(txtLink.Text);

                var result = new AmazonLinkValidator().Validate(link);
                if (result.IsValid)
                {
                    _links.Add(link);
                    txtLink.Text = string.Empty;
                }
                else
                {
                    MessageBox.Show($"{ txtLink.Text } is not valid.",
                                    "Not valid link.",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                }
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (_links.Count > 0)
            {
                var tracker = new AmazonBookReleaseTracker.AmazonBookReleaseTracker();
                foreach (var link in _links)
                {
                    tracker.AddLink(link);
                }
            }

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _links.Clear();
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
