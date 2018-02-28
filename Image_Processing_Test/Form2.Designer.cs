namespace Image_Processing_Test
{
    partial class Form2
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
            this.boxhisto = new Emgu.CV.UI.HistogramBox();
            this.SuspendLayout();
            // 
            // boxhisto
            // 
            this.boxhisto.Location = new System.Drawing.Point(20, 6);
            this.boxhisto.Name = "boxhisto";
            this.boxhisto.Size = new System.Drawing.Size(907, 492);
            this.boxhisto.TabIndex = 0;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(939, 513);
            this.Controls.Add(this.boxhisto);
            this.Name = "Form2";
            this.Text = "Form2";
            this.ResumeLayout(false);

        }

        #endregion

        public Emgu.CV.UI.HistogramBox boxhisto;
    }
}