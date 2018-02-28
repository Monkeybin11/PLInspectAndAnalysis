namespace LedChipPassFail_first
{
    partial class ZoomWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.imgboxZoom = new Emgu.CV.UI.ImageBox();
            ((System.ComponentModel.ISupportInitialize)(this.imgboxZoom)).BeginInit();
            this.SuspendLayout();
            // 
            // imgboxZoom
            // 
            this.imgboxZoom.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.imgboxZoom.FunctionalMode = Emgu.CV.UI.ImageBox.FunctionalModeOption.PanAndZoom;
            this.imgboxZoom.Location = new System.Drawing.Point(0, 0);
            this.imgboxZoom.Name = "imgboxZoom";
            this.imgboxZoom.Size = new System.Drawing.Size(648, 958);
            this.imgboxZoom.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imgboxZoom.TabIndex = 2;
            this.imgboxZoom.TabStop = false;
            // 
            // ZoomWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(650, 961);
            this.Controls.Add(this.imgboxZoom);
            this.Name = "ZoomWindow";
            this.Text = "ZoomWindow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ZoomWindow_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.imgboxZoom)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Emgu.CV.UI.ImageBox imgboxZoom;
    }
}