using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Raycasting_Engine_CSharp
{
    public partial class CustomPictureBox : PictureBox
    {
        public CustomPictureBox()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
