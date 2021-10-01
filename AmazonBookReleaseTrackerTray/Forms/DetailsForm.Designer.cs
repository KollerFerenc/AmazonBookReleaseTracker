
namespace AmazonBookReleaseTrackerTray
{
    partial class DetailsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.lbl100 = new System.Windows.Forms.Label();
            this.lbl200 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.Location = new System.Drawing.Point(12, 12);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(860, 192);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            this.richTextBox1.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.richTextBox1_LinkClicked);
            // 
            // lbl100
            // 
            this.lbl100.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl100.AutoSize = true;
            this.lbl100.Location = new System.Drawing.Point(796, 207);
            this.lbl100.Name = "lbl100";
            this.lbl100.Size = new System.Drawing.Size(35, 15);
            this.lbl100.TabIndex = 1;
            this.lbl100.Text = "100%";
            this.lbl100.Click += new System.EventHandler(this.lbl100_Click);
            // 
            // lbl200
            // 
            this.lbl200.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl200.AutoSize = true;
            this.lbl200.Location = new System.Drawing.Point(837, 207);
            this.lbl200.Name = "lbl200";
            this.lbl200.Size = new System.Drawing.Size(35, 15);
            this.lbl200.TabIndex = 2;
            this.lbl200.Text = "200%";
            this.lbl200.Click += new System.EventHandler(this.lbl200_Click);
            // 
            // DetailsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 231);
            this.Controls.Add(this.lbl200);
            this.Controls.Add(this.lbl100);
            this.Controls.Add(this.richTextBox1);
            this.MinimumSize = new System.Drawing.Size(625, 200);
            this.Name = "DetailsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Release details";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Label lbl100;
        private System.Windows.Forms.Label lbl200;
    }
}