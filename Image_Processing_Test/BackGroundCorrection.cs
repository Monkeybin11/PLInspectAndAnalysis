using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.IO;

namespace Image_Processing_Test
{
	public partial class BackGroundCorrection : Form
	{
		Image<Gray,byte> Img;
		Image<Gray,byte> ResImg;
		string basepath;

		public BackGroundCorrection()
		{
			InitializeComponent();
			pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
			pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
		}

		private void btnLoad_Click( object sender , EventArgs e )
		{
			OpenFileDialog ofd = new OpenFileDialog();
			if ( ofd.ShowDialog() == DialogResult.OK )
			{
				Img = new Image<Gray , byte>(ofd.FileName);
				basepath = Path.GetDirectoryName( ofd.FileName );
			}
		}

		private void btnStart_Click( object sender , EventArgs e )
		{
			int medSize = (int)nudSize.Value;
			var back = Img
							.SmoothMedian(medSize)
							.SmoothMedian(medSize)
							.SmoothMedian(medSize)
							.SmoothMedian(medSize)
							.SmoothMedian(medSize);

			var newimg = Img*0.5 + back.Not()*0.5;
			ResImg = newimg;
			pictureBox1.Image = newimg.ToBitmap();
			pictureBox2.Image = back.ToBitmap();

		}

		private void btnSave_Click( object sender , EventArgs e )
		{
			SaveFileDialog ofd = new SaveFileDialog();
			ofd.InitialDirectory = basepath;
			if ( ofd.ShowDialog() == DialogResult.OK)
			{
				ResImg.Save( ofd.FileName + ".png" );
			}
			
		}

		private void btnStartInv_Click( object sender , EventArgs e )
		{
			int medSize = (int)nudSize.Value;
			var back = Img.Not();

			var newimg = Img*0.5 + back*0.5;
			ResImg = newimg;
			pictureBox1.Image = newimg.ToBitmap();
			pictureBox2.Image = back.ToBitmap();
		}
	}
}
