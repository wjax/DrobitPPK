using System.Threading;

namespace AutoUpdaterDotNET
{
    partial class UpdateForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateForm));
            this.webBrowser = new System.Windows.Forms.WebBrowser();
            this.labelUpdate = new System.Windows.Forms.Label();
            this.labelDescription = new System.Windows.Forms.Label();
            this.labelReleaseNotes = new System.Windows.Forms.Label();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.buttonRemindLater = new System.Windows.Forms.Button();
            this.pictureBoxIcon = new System.Windows.Forms.PictureBox();
            this.buttonSkip = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // webBrowser
            // 
            this.webBrowser.Location = new System.Drawing.Point(175, 256);
            this.webBrowser.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.webBrowser.MinimumSize = new System.Drawing.Size(43, 49);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.Size = new System.Drawing.Size(999, 922);
            this.webBrowser.TabIndex = 0;
            // 
            // labelUpdate
            // 
            this.labelUpdate.AutoSize = true;
            this.labelUpdate.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.labelUpdate.Location = new System.Drawing.Point(169, 30);
            this.labelUpdate.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.labelUpdate.MaximumSize = new System.Drawing.Size(1040, 0);
            this.labelUpdate.Name = "labelUpdate";
            this.labelUpdate.Size = new System.Drawing.Size(443, 38);
            this.labelUpdate.TabIndex = 5;
            this.labelUpdate.Text = "A new version of {0} is available!";
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelDescription.Location = new System.Drawing.Point(169, 107);
            this.labelDescription.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.labelDescription.MaximumSize = new System.Drawing.Size(1021, 0);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(971, 32);
            this.labelDescription.TabIndex = 6;
            this.labelDescription.Text = "{0} {1} is now available. You have version {2} installed. Would you like to downl" +
    "oad it now?";
            // 
            // labelReleaseNotes
            // 
            this.labelReleaseNotes.AutoSize = true;
            this.labelReleaseNotes.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.labelReleaseNotes.Location = new System.Drawing.Point(169, 192);
            this.labelReleaseNotes.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.labelReleaseNotes.Name = "labelReleaseNotes";
            this.labelReleaseNotes.Size = new System.Drawing.Size(191, 36);
            this.labelReleaseNotes.TabIndex = 7;
            this.labelReleaseNotes.Text = "Release Notes:";
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.buttonUpdate.Image = global::AutoUpdaterDotNET.Properties.Resources.download;
            this.buttonUpdate.Location = new System.Drawing.Point(890, 1222);
            this.buttonUpdate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(284, 60);
            this.buttonUpdate.TabIndex = 2;
            this.buttonUpdate.Text = "Update";
            this.buttonUpdate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonUpdate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonUpdate.UseVisualStyleBackColor = true;
            this.buttonUpdate.Click += new System.EventHandler(this.ButtonUpdateClick);
            // 
            // buttonRemindLater
            // 
            this.buttonRemindLater.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.buttonRemindLater.Image = global::AutoUpdaterDotNET.Properties.Resources.clock_go;
            this.buttonRemindLater.Location = new System.Drawing.Point(598, 1222);
            this.buttonRemindLater.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonRemindLater.Name = "buttonRemindLater";
            this.buttonRemindLater.Size = new System.Drawing.Size(284, 60);
            this.buttonRemindLater.TabIndex = 3;
            this.buttonRemindLater.Text = "Remind me later";
            this.buttonRemindLater.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonRemindLater.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonRemindLater.UseVisualStyleBackColor = true;
            this.buttonRemindLater.Click += new System.EventHandler(this.ButtonRemindLaterClick);
            // 
            // pictureBoxIcon
            // 
            this.pictureBoxIcon.Image = global::AutoUpdaterDotNET.Properties.Resources.update;
            this.pictureBoxIcon.Location = new System.Drawing.Point(22, 30);
            this.pictureBoxIcon.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.pictureBoxIcon.Name = "pictureBoxIcon";
            this.pictureBoxIcon.Size = new System.Drawing.Size(130, 141);
            this.pictureBoxIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBoxIcon.TabIndex = 8;
            this.pictureBoxIcon.TabStop = false;
            // 
            // buttonSkip
            // 
            this.buttonSkip.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.buttonSkip.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.buttonSkip.Image = global::AutoUpdaterDotNET.Properties.Resources.hand_point;
            this.buttonSkip.Location = new System.Drawing.Point(176, 1222);
            this.buttonSkip.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonSkip.Name = "buttonSkip";
            this.buttonSkip.Size = new System.Drawing.Size(284, 60);
            this.buttonSkip.TabIndex = 1;
            this.buttonSkip.Text = "Skip this version";
            this.buttonSkip.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonSkip.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonSkip.UseVisualStyleBackColor = true;
            this.buttonSkip.Click += new System.EventHandler(this.ButtonSkipClick);
            // 
            // UpdateForm
            // 
            this.AcceptButton = this.buttonUpdate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1194, 1306);
            this.Controls.Add(this.pictureBoxIcon);
            this.Controls.Add(this.labelReleaseNotes);
            this.Controls.Add(this.labelDescription);
            this.Controls.Add(this.labelUpdate);
            this.Controls.Add(this.webBrowser);
            this.Controls.Add(this.buttonUpdate);
            this.Controls.Add(this.buttonSkip);
            this.Controls.Add(this.buttonRemindLater);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "{0} {1} is available!";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.UpdateForm_FormClosed);
            this.Load += new System.EventHandler(this.UpdateFormLoad);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonRemindLater;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.Button buttonSkip;
        private System.Windows.Forms.WebBrowser webBrowser;
        private System.Windows.Forms.Label labelUpdate;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.Label labelReleaseNotes;
        private System.Windows.Forms.PictureBox pictureBoxIcon;

    }
}