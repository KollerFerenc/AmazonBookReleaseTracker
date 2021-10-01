
namespace AmazonBookReleaseTrackerTray
{
    partial class SettingsForm
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
            this.chboxAutoStart = new System.Windows.Forms.CheckBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnShowFolder = new System.Windows.Forms.Button();
            this.numDays = new System.Windows.Forms.NumericUpDown();
            this.lblNotifiyDays = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numDays)).BeginInit();
            this.SuspendLayout();
            // 
            // chboxAutoStart
            // 
            this.chboxAutoStart.AutoSize = true;
            this.chboxAutoStart.Location = new System.Drawing.Point(12, 15);
            this.chboxAutoStart.Name = "chboxAutoStart";
            this.chboxAutoStart.Size = new System.Drawing.Size(78, 19);
            this.chboxAutoStart.TabIndex = 0;
            this.chboxAutoStart.Text = "Auto start";
            this.chboxAutoStart.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOk.Location = new System.Drawing.Point(8, 201);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(147, 201);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnShowFolder
            // 
            this.btnShowFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnShowFolder.Location = new System.Drawing.Point(135, 12);
            this.btnShowFolder.Name = "btnShowFolder";
            this.btnShowFolder.Size = new System.Drawing.Size(87, 23);
            this.btnShowFolder.TabIndex = 3;
            this.btnShowFolder.Text = "Show Folder";
            this.btnShowFolder.UseVisualStyleBackColor = true;
            this.btnShowFolder.Click += new System.EventHandler(this.btnShowFolder_Click);
            // 
            // numDays
            // 
            this.numDays.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numDays.Location = new System.Drawing.Point(135, 41);
            this.numDays.Name = "numDays";
            this.numDays.Size = new System.Drawing.Size(42, 23);
            this.numDays.TabIndex = 4;
            // 
            // lblNotifiyDays
            // 
            this.lblNotifiyDays.AutoSize = true;
            this.lblNotifiyDays.Location = new System.Drawing.Point(12, 43);
            this.lblNotifiyDays.Name = "lblNotifiyDays";
            this.lblNotifiyDays.Size = new System.Drawing.Size(109, 15);
            this.lblNotifiyDays.TabIndex = 5;
            this.lblNotifiyDays.Text = "Notifiy within days:";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(234, 236);
            this.Controls.Add(this.lblNotifiyDays);
            this.Controls.Add(this.numDays);
            this.Controls.Add(this.btnShowFolder);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.chboxAutoStart);
            this.MinimumSize = new System.Drawing.Size(250, 275);
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            ((System.ComponentModel.ISupportInitialize)(this.numDays)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chboxAutoStart;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnShowFolder;
        private System.Windows.Forms.NumericUpDown numDays;
        private System.Windows.Forms.Label lblNotifiyDays;
    }
}