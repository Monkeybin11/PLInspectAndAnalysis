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
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.IO;
using SpeedyCoding;

namespace Image_Processing_Test
{
	public partial class Resizer : Form
	{

		List<string> pathList = new List<string>();

		public Resizer()
		{
			InitializeComponent();
		}

		private void btnLoad_Click( object sender , EventArgs e )
		{
			OpenFileDialog ofd = new OpenFileDialog();
			if ( ofd.ShowDialog() == DialogResult.OK )
			{
				pathList.Add(ofd.FileName);
			}

			richTextBox1.Text = pathList.Aggregate( ( f , s ) => f + Environment.NewLine + s );
		}

		private void btnREmove_Click( object sender , EventArgs e )
		{
			pathList.RemoveAt( pathList.Count - 1 );
			richTextBox1.Text = pathList.Aggregate( ( f , s ) => f + Environment.NewLine + s );
		}

		private void btnAllClear_Click( object sender , EventArgs e )
		{


			pathList = new List<string>();
			richTextBox1.Text = null;
		}

		private void btnStart_Click( object sender , EventArgs e )
		{
			var basepath = Path.GetDirectoryName(pathList[0]);

			SaveFileDialog ofd  = new SaveFileDialog();
			ofd.InitialDirectory = basepath ;
			if ( ofd.ShowDialog() == DialogResult.OK )
			{
				var imgList = pathList.Select( x => new Image<Gray , byte>( x ).Resize( 0.5 , Inter.Cubic )).ToList();
				imgList.ActLoop( ( x , i ) => x.Save( Path.GetDirectoryName(ofd.FileName).Print("Base") +"\\"+ Path.GetFileName( pathList [ i ] ).Print("Name") ) );
			}

			

		}
	}
}
