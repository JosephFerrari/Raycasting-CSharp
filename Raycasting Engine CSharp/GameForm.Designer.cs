namespace Raycasting_Engine_CSharp
{
    partial class GameForm
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
            this.components = new System.ComponentModel.Container();
            this.GameTicker = new System.Windows.Forms.Timer(this.components);
            this.ViewBox = new System.Windows.Forms.PictureBox();
            this.GUIBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.ViewBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GUIBox)).BeginInit();
            this.SuspendLayout();
            // 
            // GameTicker
            // 
            this.GameTicker.Enabled = true;
            this.GameTicker.Interval = 10;
            this.GameTicker.Tick += new System.EventHandler(this.GameTicker_Tick);
            // 
            // ViewBox
            // 
            this.ViewBox.Location = new System.Drawing.Point(0, 0);
            this.ViewBox.Margin = new System.Windows.Forms.Padding(0);
            this.ViewBox.Name = "ViewBox";
            this.ViewBox.Size = new System.Drawing.Size(960, 480);
            this.ViewBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ViewBox.TabIndex = 0;
            this.ViewBox.TabStop = false;
            this.ViewBox.Click += new System.EventHandler(this.PictureBox1_Click);
            // 
            // GUIBox
            // 
            this.GUIBox.Location = new System.Drawing.Point(0, 0);
            this.GUIBox.Margin = new System.Windows.Forms.Padding(0);
            this.GUIBox.Name = "GUIBox";
            this.GUIBox.Size = new System.Drawing.Size(960, 480);
            this.GUIBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.GUIBox.TabIndex = 1;
            this.GUIBox.TabStop = false;
            this.GUIBox.Visible = false;
            this.GUIBox.Click += new System.EventHandler(this.GUIBox_Click);
            // 
            // GameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(944, 441);
            this.Controls.Add(this.GUIBox);
            this.Controls.Add(this.ViewBox);
            this.DoubleBuffered = true;
            this.Name = "GameForm";
            this.Text = "Raycasting Engine C#";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ViewBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GUIBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer GameTicker;
        private System.Windows.Forms.PictureBox ViewBox;
        private System.Windows.Forms.PictureBox GUIBox;
    }
}

