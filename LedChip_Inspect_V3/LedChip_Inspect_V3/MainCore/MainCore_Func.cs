using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using Emgu.CV.Util;
using System.Windows.Media;
using System.Windows.Controls;
using static EmguCV_Extension.Vision_Tool;
using static EmguCV_Extension.Preprocessing;
using static Util_Tool.UI.Corrdinate;
using EmguCV_Extension;
using System.Drawing;
using AccordBased_processing.Clustering;
using AccordBased_processing.FeatureExtract;
using System.Diagnostics;
using SpeedyCoding;



namespace WaferandChipProcessing
{
    public enum EstChipPosMode { With2Point, With4Point , WithRhombus4Point , WithPatternPoint , With4PointLineEquation}

    public partial class MainCore
    {
        /*Global Function*/
        public Func<double[], double[]> MapCanv2Img;
        public Func<double[], double[]> MapCanv2ImgLTRB;
        public Func<double[], double[]> MapImg2Canv;
        public Func<Image<Gray, byte>, Image<Gray, byte>> CropImgLT;
        public Func<Image<Gray, byte>, Image<Gray, byte>> CropImgLB;
        public Func<Image<Gray, byte>, Image<Gray, byte>> CropImgRT;
        public Func<Image<Gray, byte>, Image<Gray, byte>> CropImgRB;
        public Func<System.Drawing.Rectangle, double> SumInsideBox;
        public Func<int, int, double> SumAreaPoint;
        public Action<Canvas, double, double> SetCornerRect;
        public Func<VectorOfVectorOfPoint, Point[]> CalcCenter;

        /*Image Filtering*/
        /// <summary>
        /// Src , Threshold
        /// </summary>
        public Func<Image<Gray, byte>, int, Image<Gray, byte>> DoThreshold;
        public Func<Bitmap, Dictionary<string, dynamic>> ClusterImg;
        public Func<double[][], Dictionary<string, double[][]>> ClusterData;

        /// <summary>
        /// Src , Size
        /// </summary>
        public Func<Image<Gray, byte>, int, Image<Gray, byte>> ErodeRect;
        public Func<Image<Gray, byte>, int, Image<Gray, byte>> DilateRect;
        public Func<Image<Gray, byte>, int, Image<Gray, byte>> ErodeVerti;
        public Func<Image<Gray, byte>, int, Image<Gray, byte>> DilateVerti;
        public Func<Image<Gray, byte>, int, Image<Gray, byte>> ErodeHori;
        public Func<Image<Gray, byte>, int, Image<Gray, byte>> DilateHori;
        public Func<Image<Gray, byte>, int, Image<Gray, byte>> CloseRect;
        public Func<Image<Gray, byte>, int, Image<Gray, byte>> OpenRect;
        public Func<Image<Gray, byte>, Image<Gray, byte>, Image<Gray, byte>> TempMatch_Sq;
        public Func<Image<Gray, byte>, Image<Gray, byte>, Image<Gray, byte>> TempMatch_Ce;

        /*Local Function*/

        /// <summary>
        /// H Chip Number , W Chip Number
        /// </summary>
        public Func<double, double, double[,,]> EstedChipPos;
        public Func<double, double, PosLineEq > EstedChipPos_Ver2;
        public Func<Image<Gray, byte>, VectorOfVectorOfPoint> FindContour;
        public Func<Image<Gray, byte>, Image<Gray, byte>, VectorOfVectorOfPoint> FindPassContour_template;
        public Func<System.Drawing.PointF, double> InContour;

        /// <summary>
        /// y (row num), x (col num) 
        /// </summary>
        public Func<double, double, bool> InBox;
        public Func<VectorOfVectorOfPoint, VectorOfVectorOfPoint> Sortcontours;
        public Func<VectorOfVectorOfPoint, List<System.Drawing.Rectangle>> ApplyBox;

        /* Analysis */

            // need Modify Method Output Type
        public Func<VectorOfVectorOfPoint, double[]> FindAreaUpDwBoundaryt = 
            contours =>
            {
                Clustering cluster = new Clustering();
                var areaArr =  contours
                                .Map( cntrs => FnContours2Areas(cntrs) )
                                .Select( x=> new double[1] {x } )
                                .ToArray<double[]>();

                var result =  areaArr
                                .Map( areas => cluster.DataClustering( ClustMethod.KMean )(areas));

                var centervalue = result
                                .Where( x => x.Key == "center" )
                                .Select( y => y.Value )
                                .ToArray();

                var centereddata = result
                                .Where( x => x.Key == "data" )
                                .Select( y => y.Value )
                                .ToArray();

                var cl0 = areaArr
                            .Where( (x , i )=> centereddata[0][i][0] == centervalue[0][0][0] )
                            .Select( x => x[0])
                            .ToArray() ;

                var cl1 = areaArr
                            .Where( (x , i )=> centereddata[0][i][0]== centervalue[0][1][0] )
                            .Select( x => x[0])
                            .ToArray();

                var cl2 = areaArr
                            .Where( (x , i )=> centereddata[0][i][0] == centervalue[0][2][0] )
                            .Select( x => x[0])
                            .ToArray();

                var cl0min = cl0.Min();
                var cl0max = cl0.Max();
                var cl1min = cl1.Min();
                var cl1max = cl1.Max();
                var cl2min = cl2.Min();
                var cl2max = cl2.Max();
                
                double[] minmaxarray = new double[6]
                {
                    cl0min,
                    cl0max,
                    cl1min,
                    cl1max,
                    cl2min,
                    cl2max
                };
                
                var output = minmaxarray.OrderBy(x => x).ToArray();
                return output;

            };

        // find optimal point for threshold of intensity
        public Func<double[] , double[]> FindIntensityUpDw =
            intenlsit =>
            {
                Clustering cluster = new Clustering();

                var intenArr =  intenlsit // Middle Inten Point
                                .Select( x=> new double[1] {x } )
                                .ToArray<double[]>();

                var result =  intenArr
                                .Map( areas => cluster.DataClustering( ClustMethod.KMean )(areas));

                var centervalue = result
                                .Where( x => x.Key == "center" )
                                .Select( y => y.Value )
                                .ToArray();

                var centereddata = result
                                .Where( x => x.Key == "data" )
                                .Select( y => y.Value )
                                .ToArray(); // segemented

                var cl0 = intenArr
                            .Where( (x , i )=> centereddata[0][i][0] == centervalue[0][0][0] )
                            .Select( x => x[0])
                            .ToArray() ; // center class 0

                var cl1 = intenArr
                            .Where( (x , i )=> centereddata[0][i][0]== centervalue[0][1][0] )
                            .Select( x => x[0])
                            .ToArray() ; // center class 1

				var cl2 = intenArr
                            .Where( (x , i )=> centereddata[0][i][0] == centervalue[0][2][0] )
                            .Select( x => x[0])
                            .ToArray() ; // center class 2

				var cl0min = cl0.Min();
                var cl0max = cl0.Max();
                var cl1min = cl1.Min();
                var cl1max = cl1.Max();
                var cl2min = cl2.Min();
                var cl2max = cl2.Max();

                // Array of each cluster min and max value 
                double[] minmaxarray = new double[6]
                {
                    cl0min,
                    cl0max,
                    cl1min,
                    cl1max,
                    cl2min,
                    cl2max
                };

                var output = minmaxarray.OrderBy(x => x).ToArray();
                return new double[2] { output[2],output[5]  };
            };

        public void InitFunc( Canvas canvas , Canvas corner)
        {
            CropImgLT = FnCropImg( 0 , 0 ,  ( int ) LTRBPixelNumberW ,( int)LTRBPixelNumberH  );
            CropImgLB = FnCropImg( 0 , OriginImg.Height - ( int ) LTRBPixelNumberH , ( int ) LTRBPixelNumberW , OriginImg.Height );
            CropImgRT = FnCropImg( OriginImg.Width - ( int ) LTRBPixelNumberW , 0 , OriginImg.Width , ( int ) LTRBPixelNumberH );
            CropImgRB = FnCropImg( OriginImg.Width - ( int ) LTRBPixelNumberW , OriginImg.Height - ( int ) LTRBPixelNumberH , OriginImg.Width , OriginImg.Height );
            MapCanv2Img = Convt_Window2Real( canvas.Width , canvas.Height , OriginImg.Width , OriginImg.Height );
            MapImg2Canv = Convt_Real2window( canvas.Width , canvas.Height , OriginImg.Width , OriginImg.Height );
            MapCanv2ImgLTRB = Convt_Window2Real( corner.Width , corner.Height , LTRBPixelNumberW , LTRBPixelNumberH );
            SumInsideBox = FnSumInsideBox( OriginImg );
            SetCornerRect = FnSetCornerRect( new EmguCV_Extension.CornerMode[] {
                EmguCV_Extension.CornerMode.LeftTop,
                EmguCV_Extension.CornerMode.LeftBot,
                EmguCV_Extension.CornerMode.RightTop,
                EmguCV_Extension.CornerMode.RightBot,
            } );
            CalcCenter = FnCalcCenter();
        }

        public void CreateEstedChipFunc(double[][] cornerPos, EstChipPosMode estmode ) {
            switch ( estmode ) {
                case EstChipPosMode.With2Point:
                    EstedChipPos = FnEstChipPos_2Point( cornerPos[0] , cornerPos[3] );
                    break;

                case EstChipPosMode.With4Point:
                    EstedChipPos = FnEstChipPos_4Point( cornerPos[0] , cornerPos[1] , cornerPos[2] , cornerPos[3] );
                    break;

				case EstChipPosMode.WithRhombus4Point:
					EstedChipPos = FnEstChipPos_4PointP_rhombus ( cornerPos [ 0 ] , cornerPos [ 1 ] , cornerPos [ 2 ] , cornerPos [ 3 ] );
					break;

				case EstChipPosMode.With4PointLineEquation:
					EstedChipPos_Ver2 = FnEstChipPos_4PointAndEQ_Rhombus( cornerPos [ 0 ] , cornerPos [ 1 ] , cornerPos [ 2 ] , cornerPos [ 3 ] );
					break;

			}
        }
        

        public void CreateEstedChipFunc( double [][] patternedposition , double [][] cornerPos  , EstChipPosMode estmode )
        {
            switch ( estmode )
            {
                case EstChipPosMode.WithPatternPoint:
                    EstedChipPos = FnEstChipPos_4Point_Advanced( cornerPos [ 0 ] 
                                                                , cornerPos [ 1 ] 
                                                                , cornerPos [ 2 ] 
                                                                , cornerPos [ 3 ] 
                                                                , new double [ ] 
                                                                {
                                                                    patternedposition[1][0] - patternedposition[0][0]
                                                                    ,patternedposition[2][0] - patternedposition[1][0]
                                                                } );
                    break;
            }
        }



        public void CreateProcFun(ThresholdMode mode) {
            double thres  = PData.ThresholdV ;
            double areaup = PData.UPAreaLimit ;
            double areadw = PData.DWAreaLimit ;
            double cHnum  = PData.ChipHNum   ;
            double cWnum  = PData.ChipWNum   ;

            Register_ProcMethod();
            Clustering cluster = new Clustering();
            ClusterImg  = cluster.Segment( ClustMethod.KMean );
            ClusterData = cluster.DataClustering( ClustMethod.KMean );
            Sortcontours = FnSortcontours();

            DoThreshold   = FnThreshold( mode );
            ErodeRect     = FnMorp( morpOp.Erode  , kernal.Rect);
            DilateRect    = FnMorp( morpOp.Dilate , kernal.Rect);
            ErodeVerti    = FnMorp( morpOp.Erode  , kernal.Vertical);
            DilateVerti   = FnMorp( morpOp.Dilate , kernal.Vertical);
            ErodeHori     = FnMorp( morpOp.Erode , kernal.Horizontal );
            DilateHori    = FnMorp( morpOp.Dilate , kernal.Horizontal );

            CloseRect     = FnMorp( morpOp.Close  , kernal.Rect );
            OpenRect      = FnMorp( morpOp.Open   , kernal.Rect );
            TempMatch_Sq  = FnTemplateMatch( TempMatchType.Sq );
            TempMatch_Ce  = FnTemplateMatch( TempMatchType.Coeff ); 

            FindContour     = FnFindContour( areaup , areadw );
            ApplyBox        = FnApplyBox( PData.UPBoxLimit , PData.DWBoxLimit );
            SumAreaPoint    = FnSumAreaPoint( ( int ) PData.ChipHSize , ( int ) PData.ChipWSize , OriginImg );

        }

        /// <summary>
        /// Set up object box for check
        /// </summary>
        /// <param name="box"> object for check </param>
        /// <param name="margin"> tolerance of check </param>
        public void Create_Inbox( Rectangle box , int margin )
        {
            InBox = FnInBox(box,margin);
        }

        

        #region Helper
        byte [ ,,] MatZeros( int channal1 , int channal2 , int channal3 )
        {
            byte[,,] output = new byte[channal1,channal2,channal3];
            for ( int i = 0 ; i < channal1 ; i++ )
            {
                for ( int j = 0 ; j < channal2 ; j++ )
                {
                    for ( int k = 0 ; k < channal3 ; k++ )
                    {
                        output[i , j , k] = 150;
                    }
                }
            }
            return output;
        }

        byte[,,] MatPattern( int channal1 , int channal2 , int channal3 )
        {
            byte[,,] output = new byte[channal1,channal2,channal3];

            Parallel.For( 0 , channal1 , i => {
                for (int j = 0; j < channal2; j++)
                {
                    if (i % 2 == 0)
                    {
                        if (j % 2 == 0)
                        {
                            output[i, j, 0] = 250;
                            output[i, j, 1] = 250;
                            output[i, j, 2] = 250;
                        }
                        else
                        {
                            output[i, j, 0] = 150;
                            output[i, j, 1] = 150;
                            output[i, j, 2] = 150;
                        }
                    }
                    else if (j % 2 == 0)
                    {
                        output[i, j, 0] = 200;
                        output[i, j, 1] = 200;
                        output[i, j, 2] = 200;
                    }
                    else
                    {
                        output[i, j, 0] = 100;
                        output[i, j, 1] = 100;
                        output[i, j, 2] = 100;
                    }
                }
            } );
            return output;
        }

		byte [ , , ] MatWhitePattern( int channal1 , int channal2 , int channal3 )
		{
			byte[,,] output = new byte[channal1,channal2,channal3];

			Parallel.For( 0 , channal1 , i => {
				for ( int j = 0 ; j < channal2 ; j++ )
				{
					if ( i % 2 == 0 )
					{
						if ( j % 2 == 0 )
						{
							output [ i , j , 0 ] = 250;
							output [ i , j , 1 ] = 250;
							output [ i , j , 2 ] = 250;
						}
						else
						{
							output [ i , j , 0 ] = 250;
							output [ i , j , 1 ] = 250;
							output [ i , j , 2 ] = 250;
						}
					}
					else if ( j % 2 == 0 )
					{
						output [ i , j , 0 ] = 250;
						output [ i , j , 1 ] = 250;
						output [ i , j , 2 ] = 250;
					}
					else
					{
						output [ i , j , 0 ] = 250;
						output [ i , j , 1 ] = 250;
						output [ i , j , 2 ] = 250;
					}
				}
			} );
			return output;
		}


		Image<Bgr , byte> DrawContour( Image<Bgr , byte> img , VectorOfVectorOfPoint contr )
        {
            for ( int i = 0 ; i < contr.Size ; i++ )
            {
                CvInvoke.DrawContours( img , contr , i , new MCvScalar( 0 , 255 , 0 ) );
            }
            return img;
        }

        Image<Bgr , byte> DrawCenterPoint (Image<Bgr , byte> img , double[,,] centrPoint)
        {
            for ( int k = 0 ; k < 100 ; k++ )
            {
                for ( int i = 0 ; i < centrPoint.GetLength( 0 ) ; i++ ) // y
                {
                    for ( int j = 0 ; j < centrPoint.GetLength( 1 ) ; j++ ) // x
                    {
                        CircleF cirp = new CircleF();
                        cirp.Center = new PointF( ( float ) (int)centrPoint[i , j , 1] , ( float ) (int)centrPoint[i , j , 0] );
                        img.Draw( cirp , ApCenterPointColor , 1 );
                    }
                }
            }
            return img;
        }

        Image<Bgr , byte> DrawBox( Image<Bgr , byte> img , List<System.Drawing.Rectangle> rclist )
        {
            Parallel.For( 0 , rclist.Count , i =>
            {
                img.Draw( rclist[i] , ApOkChipColor , 1 );
            } );
            return img;
        }

        Image<Bgr , byte> DrawBox_LOW (Image<Bgr , byte> img , List<System.Drawing.Rectangle> rclist)
        {
            Parallel.For( 0 , rclist.Count , i =>
            {
                img.Draw( rclist[i] , ApLowColor , 1 );
            } );
            return img;
        }

        Image<Bgr , byte> DrawBox_OVER (Image<Bgr , byte> img , List<System.Drawing.Rectangle> rclist)
        {
            Parallel.For( 0 , rclist.Count , i =>
            {
                img.Draw( rclist[i] , ApOverColor , 1 );
            } );
            return img;
        }
        #endregion
    }

    
}
