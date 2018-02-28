using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;

namespace EmguCV_Extension
{
    public enum CornerMode { LeftTop, LeftBot, RightTop, RightBot }
    public class Preprocessing
    {
        public static Func<Image<Gray , byte> , Image<Gray , byte>> FnCropImg( int xS , int yS , int width , int height )
        {
            return new Func<Image<Gray , byte> , Image<Gray , byte>>( ( img ) => {
                img.ROI = new System.Drawing.Rectangle( xS , yS , width , height );
                var tempimg = img.Copy();
                img.ROI = System.Drawing.Rectangle.Empty;
                return tempimg;
            } );
        }

        public static Action<Canvas , double , double> FnSetCornerRect( params CornerMode [ ] corner )
        {
            return new Action<Canvas , double , double>( ( canvas , rectH , rectW ) => {
                RenderOptions.SetBitmapScalingMode( canvas , BitmapScalingMode.NearestNeighbor );
                for ( int i = 0 ; i < corner.Length ; i++ )
                {
                    Rectangle rect = new Rectangle();
                    rect.Height = rectH;
                    rect.Width = rectW;
                    rect.StrokeThickness = 2;
                    rect.Stroke = new SolidColorBrush( Colors.BlueViolet );
                    switch ( corner [ i ] )
                    {
                        case CornerMode.LeftTop:
                            Canvas.SetLeft( rect , 0 );
                            Canvas.SetTop( rect , 0 );
                            break;
                        case CornerMode.LeftBot:
                            Canvas.SetLeft( rect , 0 );
                            Canvas.SetBottom( rect , 0 );
                            break;
                        case CornerMode.RightTop:
                            Canvas.SetRight( rect , 0 );
                            Canvas.SetTop( rect , 0 );
                            break;
                        case CornerMode.RightBot:
                            Canvas.SetRight( rect , 0 );
                            Canvas.SetBottom( rect , 0 );
                            break;
                    }
                    canvas.Children.Add( rect );
                }
            } );
        }


        #region Event Func
        /* if u need to change some value in other project, just copy this function and modify some code with external variables*/
        public static Action<object , MouseButtonEventArgs> FnGetClickPosInImg(
            Canvas canvas , double [ ] imgPosSvBox , double [ ] canvasPosSvBox , dynamic posConvertFunc )
        {
            return new Action<object , MouseButtonEventArgs>( ( ob , ev ) => {
                double px = ev.GetPosition( canvas ).X - 4   ;
                double py = ev.GetPosition( canvas ).Y - 4   ;

                imgPosSvBox = posConvertFunc( new double [ 2 ] { px , py } );
                canvasPosSvBox = new double [ 2 ] { px , py };
            } );
        }


        #endregion  


    }
}
