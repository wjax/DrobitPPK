namespace AutoUpdaterDotNET
{
    partial class DownloadUpdateDialog
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
            this.progressBar = new MaterialSkin.Controls.MaterialProgressBar();
            this.labelInformation = new System.Windows.Forms.Label();
            this.labelSize = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Depth = 0;
            this.progressBar.Location = new System.Drawing.Point(32, 143);
            this.progressBar.MouseState = MaterialSkin.MouseState.HOVER;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(809, 5);
            this.progressBar.TabIndex = 4;
            // 
            // labelInformation
            // 
            this.labelInformation.AutoSize = true;
            this.labelInformation.Location = new System.Drawing.Point(32, 92);
            this.labelInformation.Name = "labelInformation";
            this.labelInformation.Size = new System.Drawing.Size(179, 32);
            this.labelInformation.TabIndex = 7;
            this.labelInformation.Text = "Downloading ...";
            // 
            // labelSize
            // 
            this.labelSize.AutoSize = true;
            this.labelSize.Location = new System.Drawing.Point(573, 92);
            this.labelSize.Name = "labelSize";
            this.labelSize.Size = new System.Drawing.Size(43, 32);
            this.labelSize.TabIndex = 8;
            this.labelSize.Text = "    ";
            // 
            // DownloadUpdateDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(861, 194);
            this.Controls.Add(this.labelSize);
            this.Controls.Add(this.labelInformation);
            this.Controls.Add(this.progressBar);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Margin = new System.Windows.Forms.Padding(7, 4, 7, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DownloadUpdateDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Software Update";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DownloadUpdateDialog_FormClosing);
            this.Load += new System.EventHandler(this.DownloadUpdateDialogLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private MaterialSkin.Controls.MaterialProgressBar progressBar;
        private System.Windows.Forms.Label labelInformation;
        private System.Windows.Forms.Label labelSize;
    }
}