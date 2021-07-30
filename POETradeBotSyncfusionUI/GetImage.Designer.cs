
namespace POETradeBotSyncfusionUI
{
    partial class GetImage
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
            this.AssetDisplay = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.AssetDisplay)).BeginInit();
            this.SuspendLayout();
            // 
            // AssetDisplay
            // 
            this.AssetDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AssetDisplay.Location = new System.Drawing.Point(0, 0);
            this.AssetDisplay.Name = "AssetDisplay";
            this.AssetDisplay.Size = new System.Drawing.Size(800, 450);
            this.AssetDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.AssetDisplay.TabIndex = 0;
            this.AssetDisplay.TabStop = false;
            this.AssetDisplay.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AssetDisplay_MouseDown);
            this.AssetDisplay.MouseUp += new System.Windows.Forms.MouseEventHandler(this.AssetDisplay_MouseUp);
            // 
            // GetImage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.AssetDisplay);
            this.Name = "GetImage";
            this.Text = "GetImage";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GetImage_FormClosing);
            this.Load += new System.EventHandler(this.GetImage_Load);
            ((System.ComponentModel.ISupportInitialize)(this.AssetDisplay)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox AssetDisplay;
    }
}