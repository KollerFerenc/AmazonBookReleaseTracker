using AmazonBookReleaseTracker;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AmazonBookReleaseTrackerTray
{
    public partial class IgnoreForm : Form
    {
        private readonly List<Tuple<AmazonLink, bool>> _links = new(4);

        public IgnoreForm()
        {
            InitializeComponent();

            Icon = AmazonBookReleaseTrackerTrayContext.icon;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (_links.Count > 0)
            {
                var tracker = new AmazonBookReleaseTracker.AmazonBookReleaseTracker();
                foreach (var link in _links)
                {
                    tracker.IgnoreLink(link.Item1, link.Item2);
                }
            }

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Ignore(string linkText, bool remove)
        {
            var link = new AmazonLink(linkText);

            var result = new AmazonLinkValidator().Validate(link);
            if (result.IsValid)
            {
                _links.Add(new Tuple<AmazonLink, bool>(link, remove));
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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (txtLink.Text != string.Empty)
            {
                Ignore(txtLink.Text, remove: false);
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (txtLink.Text != string.Empty)
            {
                Ignore(txtLink.Text, remove: true);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _links.Clear();
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
