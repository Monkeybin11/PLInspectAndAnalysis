using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Drawing;
using System.Windows.Forms;

namespace Image_Processing_Test
{
    public class Main
    {
        public void main()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if ( ofd.ShowDialog() == DialogResult.OK )
            {
                Image<Bgr, byte> colorimg = new Image<Bgr, byte>(ofd.FileName);
                Processing proc = new Processing();

                var rg = proc.DrawContourAndAreaSize_RG( colorimg );
                var b = proc.DrawContourAndAreaSize_B( colorimg );

                try
                {
                    //if ( rg.Item1 != null ) imageBox1.Image = rg.Item1;
                }
                catch ( Exception )
                {
                }
            }
        }
    }


    public class Processing
    {
        // output : Image with contour , Size of contour 
        public Tuple<Image<Bgr , byte> , double> DrawContourAndAreaSize_RG( Image<Bgr , byte> input )
        {
            var gdata = BGRtoGray( input.Data , 1);
            var rdata = BGRtoGray( input.Data , 2);
            var gimg = new Image<Gray, byte>(gdata);
            var rimg = new Image<Gray, byte>(rdata);
            Image<Gray,byte> workingImg = gimg + rimg / 2;

            #region Processing
            CvInvoke.MedianBlur( workingImg , workingImg , 5 );

            workingImg = workingImg.Convolution( new ConvolutionKernelF( CreateKernel() ) )
                                   .Convert<Gray,byte>();

            workingImg._GammaCorrect( 2.0 );
            workingImg = workingImg.Mul( 255 / 200.0 );
            workingImg._GammaCorrect( 2.0 );
            workingImg = workingImg.Add( new Gray( 50 ) );
            CvInvoke.MedianBlur( workingImg , workingImg , 5 );
            workingImg._GammaCorrect( 2.0 );


            workingImg = workingImg.ThresholdBinary( new Gray( 200 ) , new Gray( 255 ) );
            #endregion

            #region contour
            // Chip Size Range
            int up = 6000;
            int dw = 2000;

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours( workingImg , contours , null , RetrType.External , ChainApproxMethod.ChainApproxNone );
         

            for ( int i = 0 ; i < contours.Size ; i++ )
            {
                double areaSize = CvInvoke.ContourArea(contours[i], false);  //  Find the area of contour
                Console.WriteLine( areaSize );
                if ( areaSize >= dw && areaSize <= up )
                {
                    CvInvoke.DrawContours( input , contours , i , new MCvScalar( 14 , 200 , 40 ) , 2 );
                    return Tuple.Create( input , areaSize );
                }
            }
            return null;
            #endregion
        }


        // output : Image with contour , Size of contour 
        public Tuple<Image<Bgr , byte> , double> DrawContourAndAreaSize_B( Image<Bgr , byte> input )
        {
            var gdata = BGRtoGray( input.Data , 1);
            var rdata = BGRtoGray( input.Data , 2);
            var gimg = new Image<Gray, byte>(gdata);
            var rimg = new Image<Gray, byte>(rdata);
            Image<Gray,byte> workingImg = gimg + rimg / 2;

            #region Processing
            CvInvoke.MedianBlur( workingImg , workingImg , 5 );

            workingImg = workingImg.Convolution( new ConvolutionKernelF( CreateKernel() ) )
                                   .Convert<Gray , byte>();
            workingImg._GammaCorrect( 2.0 );

            CvInvoke.MedianBlur( workingImg , workingImg , 5 );

            workingImg = workingImg.Mul( 255 / 200.0 );
            workingImg._GammaCorrect( 2.0 );
            workingImg = workingImg.Mul( 255 / 200.0 );
            workingImg._GammaCorrect( 2.0 );
            workingImg = TriFilter( workingImg );
            workingImg._GammaCorrect( 2.0 );
            workingImg = workingImg.Add( new Gray( 50 ) );
            workingImg = workingImg.ThresholdToZero( new Gray(100));
            workingImg = workingImg.Add( new Gray( 50 ) );
            workingImg = workingImg.ThresholdBinary( new Gray( 120 ) , new Gray( 255 ) );
          
            #endregion

            #region contour
            // Chip Size Range
            int up = 7500;
            int dw = 4500;

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours( workingImg , contours , null , RetrType.External , ChainApproxMethod.ChainApproxNone );


            for ( int i = 0 ; i < contours.Size ; i++ )
            {
                double areaSize = CvInvoke.ContourArea(contours[i], false);  //  Find the area of contour
                Console.WriteLine( areaSize );
                if ( areaSize >= dw && areaSize <= up )
                {
                    CvInvoke.DrawContours( input , contours , i , new MCvScalar( 14 , 200 , 40 ) , 2 );
                    return Tuple.Create( input , areaSize );
                }
            }
            return null;
            #endregion
        }

        private float [,] CreateKernel()
        {
            float[,] kernel = new float[3,3];
            for ( int i = 0 ; i < 3 ; i++ )
            {
                for ( int j = 0 ; j < 3 ; j++ )
                {
                    kernel [ i , j ] = i == 1 && j == 1 ? 30 : -1;
                }
            }
            return kernel;
        }

        private Image<Gray , byte> TriFilter(Image<Gray,byte> input)
        {
            var data = input.Data;
            for ( int j = 0 ; j < input.Height ; j++ )
            {
                for ( int i = 0 ; i < input.Width ; i++ )
                {
                    data [ j , i , 0 ] = data [ j , i , 0 ] > 127 ? ( byte )( ( 255 - data [ j , i , 0 ] ) * 2 ) : ( byte )( data [ j , i , 0 ] * 2 );
                }
            }
            return new Image<Gray , byte>( data );
        }


        public byte [ , , ] BGRtoGray(byte [ , , ] src , int idx )
        {
            int w = src.GetLength(0);
            int h = src.GetLength(1);
            int c = src.GetLength(2);

            byte[,,] output = new byte[w, h, 1];

            for ( int j = 0 ; j < w ; j++ )
            {
                for ( int i = 0 ; i < h ; i++ )
                {
                    output [ j , i , 0 ] = src [ j , i , idx ];
                }
            }
            return output;
        }

    }
}
