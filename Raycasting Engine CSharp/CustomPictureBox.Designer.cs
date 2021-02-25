namespace Raycasting_Engine_CSharp
{
    partial class CustomPictureBox
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TransparentBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.TransparentBox)).BeginInit();
            this.SuspendLayout();
            // 
            // TransparentBox
            // 
            this.TransparentBox.Location = new System.Drawing.Point(0, 0);
            this.TransparentBox.Name = "TransparentBox";
            this.TransparentBox.Size = new System.Drawing.Size(100, 50);
            this.TransparentBox.TabIndex = 0;
            this.TransparentBox.TabStop = false;
            this.TransparentBox.Click += new System.EventHandler(this.pictureBox1_Click);
            ((System.ComponentModel.ISupportInitialize)(this.TransparentBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox TransparentBox;
    }
}
