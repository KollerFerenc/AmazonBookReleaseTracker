using AmazonBookReleaseTracker;
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
    public partial class DetailsForm : Form
    {
        private AmazonContainer AmazonContainer { get; set; } = new AmazonContainer();

        public DetailsForm()
        {
            InitializeComponent();

            Icon = AmazonBookReleaseTrackerTrayContext.icon;

            richTextBox1.ShortcutsEnabled = true;
            richTextBox1.Multiline = true;
            richTextBox1.ReadOnly = true;
            richTextBox1.DetectUrls = true;
        }

        public DetailsForm(AmazonContainer amazonContainer) : this()
        {
            AmazonContainer = amazonContainer;

            FillTextBox();
        }

        public DetailsForm(IEnumerable<string> lines) : this()
        {
            this.Text = "Tracked releases";

            FillTextBox(lines);
        }

        private void FillTextBox()
        {
            if (AmazonContainer.BookCount > 0)
            {
                foreach (var series in AmazonContainer.AmazonSeries)
                {
                    if (series.Books.Count > 0)
                    {
                        richTextBox1.AppendText($"- { series.Title } ({series.Books.Count}), [{ series.AmazonId.Asin }], { series.GetUri() }{ Environment.NewLine }");
                        foreach (var book in series.Books)
                        {
                            richTextBox1.AppendText($"\t- { book.Title } [{ book.AmazonId.Asin }], { book.ReleaseDate.ToShortDateString() }, { book.GetUri() }{ Environment.NewLine }");
                        }
                    }
                }

                foreach (var book in AmazonContainer.AmazonBooks)
                {
                    richTextBox1.AppendText($"- { book.Title } [{ book.AmazonId.Asin }], { book.ReleaseDate.ToShortDateString() }, { book.GetUri() }{ Environment.NewLine }");
                }
            }
            else
            {
                richTextBox1.AppendText($"No release within { Properties.Settings.Default.NotifyWithin } days.{ Environment.NewLine }");
            }
        }

        private void FillTextBox(IEnumerable<string> lines)
        {
            foreach (var item in lines)
            {
                richTextBox1.AppendText(item + Environment.NewLine);
            }
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.LinkText))
            {
                System.Diagnostics.Process.Start("explorer.exe", e.LinkText);
            }
        }

        private void lbl100_Click(object sender, EventArgs e)
        {
            richTextBox1.ZoomFactor = 1f;
        }

        private void lbl200_Click(object sender, EventArgs e)
        {
            richTextBox1.ZoomFactor = 2f;
        }
    }
}
