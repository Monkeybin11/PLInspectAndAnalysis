using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows;
using SpeedyCoding;

namespace EmguCV_Extension
{
    public static class EmgucvExtension
    {
        [DllImport( "gdi32" )]
        private static extern int DeleteObject( IntPtr o );
        public static BitmapSource ToBitmapSource( this IImage image )
        {
            try
            {
                using ( System.Drawing.Bitmap source = image.Bitmap )
                {
                    IntPtr ptr = source.GetHbitmap();

                    BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                ptr,
                IntPtr.Zero,
                Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                    DeleteObject( ptr );
                    return bs;
                }
            }
            catch ( Exception )
            {
                return null;
            }
        }


        public static byte [ , , ] ConvertToImgData(
            this byte [ ] [ ] @this )
        {
            var rowNum = @this.GetLength(0);
            var colNum = @this[0].GetLength(0);

            byte[,,] output = new byte[rowNum,colNum,1];

            for ( int j = 0 ; j < @this.GetLength( 0 ) ; j++ )
            {
                for ( int i = 0 ; i < @this [ 0 ].GetLength( 0 ) ; i++ )
                {
                    output [ j , i , 0 ] = @this [ j ] [ i ];
                }
            }
            return output;
        }

        public static byte [ ] [ ] ConvertToJagged(
            this Image<Gray , byte> @this )
        {
            var rowNum = @this.Height;
            var colNum = @this.Width;

            byte[][] output = new byte[rowNum][];

            for ( int j = 0 ; j < rowNum ; j++ )
            {
                for ( int i = 0 ; i < colNum ; i++ )
                {
                    output [ j ] = new byte [ ] { @this.Data [ j , i , 0 ] };
                }
            }
            return output;
        }






        public static DenseHistogram ShowHisto(
            this byte [ ] @this
            , int binsize
            , int min
            , int max )
        {
            int srcLen = @this.GetLength( 0 );
            var histsrc =  new Matrix<float>( 1 , srcLen )
                            .Act( mat =>
                                    mat.Data
                                       .Map( data =>
                                       {
                                           return Enumerable.Range( 0,srcLen )
                                              .Select( i => (float)@this[i] )
                                              .ToArray<float>();
                                       } )
                            );
            return new DenseHistogram( binsize , new RangeF( min , max ) )
                        .Act( densehist =>
                                densehist.Calculate(
                                    new Matrix<float> [ 1 ] { histsrc }
                                    , true
                                    , null
                                    ) );
        }

        public static DenseHistogram ShowHisto(
            this int [ ] @this
            , int binsize
            , int min
            , int max )
        {
            int srcLen = @this.GetLength( 0 );
            var histsrc =  new Matrix<float>( 1 , srcLen )
                            .Act( mat =>
                                    mat.Data
                                       .Map( data =>
                                       {
                                           return Enumerable.Range( 0,srcLen )
                                              .Select( i => (float)@this[i] )
                                              .ToArray<float>();
                                       } )
                            );
            return new DenseHistogram( binsize , new RangeF( min , max ) )
                        .Act( densehist =>
                                densehist.Calculate(
                                    new Matrix<float> [ 1 ] { histsrc }
                                    , true
                                    , null
                                    ) );
        }

        public static DenseHistogram ShowHisto(
            this double [ ] @this
            , int binsize
            , int min
            , int max )
        {
            int srcLen = @this.GetLength( 0 );
            var histsrc =  new Matrix<float>( 1 , srcLen );


            for ( int i = 0 ; i < srcLen ; i++ )
            {
                histsrc [ 0 , i ] = ( float )@this [ i ];
            }

            return new DenseHistogram( binsize , new RangeF( min , max ) )
                        .Act( densehist =>
                                densehist.Calculate(
                                    new Matrix<float> [ 1 ] { histsrc }
                                    , true
                                    , null
                                    ) );
        }

        public static DenseHistogram ShowHisto(
         this List<double> @this
         , int binsize
         , int min
         , int max )
        {
            int srcLen = @this.Count;
            var histsrc = new Matrix<float>(1, srcLen);


            for ( int i = 0 ; i < srcLen ; i++ )
            {
                histsrc [ 0 , i ] = ( float )@this [ i ];
            }

            return new DenseHistogram( binsize , new RangeF( min , max ) )
                        .Act( densehist =>
                                densehist.Calculate(
                                    new Matrix<float> [ 1 ] { histsrc }
                                    , true
                                    , null
                                    ) );
        }


        public static byte [ ] [ ] EmgImgGray2Arr(
            this Image<Gray , byte> @this )
        {
            var data = @this.Data;
            return new byte [ data.Len( 0 ) ] [ ]
                                .Select( ( rows , j ) => Enumerable.Range( 0 , data.Len( 1 ) )
                                                .Select( i => data [ j , i , 0 ] )
                                                .ToArray() )
                                .ToArray();
        }

        public static byte [ ] [ ] [ ] EmgImgRGB2Arr(
            this Image<Rgb , byte> @this )
        {
            var data = @this.Data;
            return new byte [ data.Len( 0 ) ] [ ] [ ]
                         .Select( ( rows , j ) => new byte [ data.Len( 1 ) ] [ ]
                                                 .Select( ( cols , i ) => Enumerable.Range( 0 , 3 )
                                                                        .Select( k => data [ j , i , k ] )
                                                                        .ToArray() )
                                                 .ToArray() )
                          .ToArray();
        }


        #region About Image Processing
        public static Image<Gray , byte> Inverse(
            this Image<Gray , byte> src )
        {
            var output = src.Clone();
            Parallel.For( 0 , output.Height , j =>
            {
                for ( int i = 0 ; i < output.Width ; i++ )
                {
                    output.Data [ j , i , 0 ] = ( byte )( 255 - src.Data [ j , i , 0 ] );
                }
            } );
            return output;
        }


        public static Image<TColor , TDepth> Inverse<TColor, TDepth>(
          this Image<TColor , TDepth> src )
          where TColor : struct, IColor
          where TDepth : new()
        {
            return src.Not();
        }

        public static Image<Gray , TDepth> Brightness<TDepth>(
            this Image<Gray , TDepth> src
            , double a
            , double b
            , double s
            , double E )
            where TDepth : new()
        {
            if ( a > 0 && b > 0 )
            {
                return ( src.Mul( 1 - s )
                            + s * a * ( src.Mul( 1 / a ).Pow( E ) ) )
                        .Pow( Math.Log( b ) / Math.Log( a ) );
            }

            return src;


            //return src.Mul(alpha).Add(new Gray(beta));
        }
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TDepth"></typeparam>
		/// <param name="src"></param>
		/// <param name="alpha for Multifly"></param>
		/// <param name="beta for Add"></param>
		/// <returns></returns>
        public static Image<Gray , TDepth> Brightness<TDepth>(
            this Image<Gray , TDepth> src
            , double alpha
            , double beta )
            where TDepth : new()
        {
            return src.Mul( alpha ).Add( new Gray( beta ) );
        }


        public static Image<Bgr , TDepth> Brightness<TDepth>(
           this Image<Bgr , TDepth> src
           , double alpha
           , int beta )
           where TDepth : new()
        {
            return src.Mul( alpha ).Add( new Bgr( beta , beta , beta ) );
        }

        public static Image<Gray , byte> Normalize(
           this Image<Gray , byte> src
           , byte max )
        {
            //var subimg = src.Resize(5000,5000,Emgu.CV.CvEnum.Inter.Nearest);
            //byte min = subimg.Data.Cast<byte>().Min();
            //byte max = subimg.Data.Cast<byte>().Max();
            byte min = 0;
            return src.Sub( new Gray( min ) ).Mul( 255.0 / ( double )( max - min ) );
        }

        public static Image<Gray , byte> Normalize(
           this Image<Gray , byte> src )
        {
            var subimg = src.Resize(5000,5000,Emgu.CV.CvEnum.Inter.Nearest);
            byte min = subimg.Data.Cast<byte>().Min();
            byte max = subimg.Data.Cast<byte>().Max();
            return src.Sub( new Gray( min ) ).Mul( 255.0 / ( double )( max - min ) );
        }


        public static Image<Bgr , byte> Normalize(
           this Image<Bgr , byte> src
           , byte max )
        {
            byte min = 0;
            return src.Sub( new Bgr( min , min , min ) ).Mul( 255.0 / ( double )( max - min ) );
        }



        public static Image<Bgr , byte> Inverse(
          this Image<Bgr , byte> src )
        {
            return src.Not();
        }

        public static Image<Gray , byte> InvThres(
          this Image<Gray , byte> src
          , int thres )
        {

            for ( int j = 0 ; j < src.Data.GetLength( 0 ) ; j++ )
            {
                for ( int i = 0 ; i < src.Data.GetLength( 1 ) ; i++ )
                {
                    src.Data [ j , i , 0 ] = src.Data [ j , i , 0 ] > ( byte )thres ? ( byte )0 : src.Data [ j , i , 0 ];
                }
            }
            return src;
        }


        public static Image<TColor , TDepth> HistEqualize<TColor, TDepth>(
            this Image<TColor , TDepth> src )
            where TColor : struct, IColor
            where TDepth : new()
        {
            src._EqualizeHist();
            return src;
        }

        public static Image<TColor , TDepth> Gamma<TColor, TDepth>(
            this Image<TColor , TDepth> src
            , double gamma )
            where TColor : struct, IColor
            where TDepth : new()
        {
            src._GammaCorrect( gamma );
            return src;
        }


        public static Image<TColor , TDepth> Median<TColor, TDepth>(
           this Image<TColor , TDepth> src
           , int size )
           where TColor : struct, IColor
           where TDepth : new()
        {
            CvInvoke.MedianBlur( src , src , size );
            return src;
        }


        public static Image<TColor , TDepth> HStack<TColor, TDepth>(
            this Image<TColor , TDepth> src ,
            Image<TColor , TDepth> rightSrc )
            where TColor : struct, IColor
            where TDepth : new()
        {
            return new Image<TColor , TDepth>( src.Data.Concate_H( rightSrc.Data ) );
        }

        public static Image<TColor , TDepth> HStack<TColor, TDepth>(
            this Image<TColor , TDepth> src ,
            Image<TColor , TDepth> rightSrc ,
            int clipping )
            where TColor : struct, IColor
            where TDepth : new()
        {
            return new Image<TColor , TDepth>( src.Data.Concate_H( rightSrc.Data , clipping ) );
        }



        public static Image<TColor , TDepth> VStack<TColor, TDepth>(
             this Image<TColor , TDepth> src ,
             Image<TColor , TDepth> bottomSrc )
             where TColor : struct, IColor
             where TDepth : new()
        {
            return new Image<TColor , TDepth>( src.Data.Concate_V( bottomSrc.Data ) );
        }


        #endregion

        #region For Debug

        // Original save method retrun void . 
        public static Image<TColor , TDepth> SaveImg<TColor, TDepth>(
            this Image<TColor , TDepth> src
            , string savepath )
            where TColor : struct, IColor
            where TDepth : new()
        {
            src.Save( savepath );
            return src;
        }
        #endregion


        #region Helper


        #endregion



        #region Draw

        public static Image<Bgr , TDepth> DrawRotatedRect<TDepth>(
            this Image<Bgr , TDepth> src
            , RotatedRect rect
            , Bgr color )
            where TDepth : new()
        {
            var output = src.Clone();
            var points = CvInvoke.BoxPoints(rect);
            var lines = Enumerable.Range(0 , points.GetLength(0))
                            .Select( i =>
                                        new LineSegment2DF( points[i] , points [ ( i + 1 ) % 4 ] ) )
                            .ToArray()
                            .ActLoop( x => output.Draw(x,color,2));
            return output;
        }

        #endregion  



    }
}
