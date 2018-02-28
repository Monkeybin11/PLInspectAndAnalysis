using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WaferandChipProcessing;

namespace LedChipPassFail_first
{
    public delegate void EvtClosed();
    public partial class ZoomWindow : Form
    {
        public event EvtClosed evtClosed;

        public ZoomWindow()
        {
            InitializeComponent();
            imgboxZoom.SizeMode = PictureBoxSizeMode.Zoom;
        }

        public void ShowImage( Image<Bgr , Byte> img )
        {
            imgboxZoom.Image = img;
        }

        private void ZoomWindow_FormClosing( object sender , FormClosingEventArgs e )
        {
            evtClosed();
        }
    }
}
