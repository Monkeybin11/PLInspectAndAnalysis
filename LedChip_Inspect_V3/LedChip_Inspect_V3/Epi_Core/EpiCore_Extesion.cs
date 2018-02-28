using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using EmguCV_Extension;
using static EmguCV_Extension.Vision_Tool;
using SpeedyCoding;

namespace WaferandChipProcessing
{
    public static class EpiCore_Extesion
    {
        public static Tuple< ImgIdxPos, Image<Gray, byte>>[] Map2ImgZipedPos(
            this string[] @this
            )
        {
            return @this.Select(path => Tuple.Create(path.TrimFileNameOnly().Map2ImgPos()
                                                     , new Image<Gray, byte>(path)))
                        .ToArray();
        }

        //public static EpiProcMethod ImgIdx2EpiMethod(
        //    this ImgIdxPos src)
        //{
        //    if (src == ImgIdxPos.TM || src == ImgIdxPos.BM)
        //        return EpiProcMethod.Mid;
        //    else
        //        return EpiProcMethod.Side;
        //}

		public static EpiProcMethod ImgIdx2EpiMethod( // 20171013 modified for veeco mid top sample
			this ImgIdxPos src )
		{
			if ( src == ImgIdxPos.BM )
				return EpiProcMethod.Mid;
			if ( src == ImgIdxPos.TM  )
				return EpiProcMethod.MTop;
			else
				return EpiProcMethod.Side;
		}



		public static ImgIdxPos Map2ImgPos(
            this string src)
        {
            return src.Match()
                        .With( x => x == "TL" , ImgIdxPos.TL )
                        .With( x => x == "TM" , ImgIdxPos.TM )
                        .With( x => x == "TR" , ImgIdxPos.TR )
                        .With( x => x == "BL" , ImgIdxPos.BL )
                        .With( x => x == "BM" , ImgIdxPos.BM )
                        .With( x => x == "BR" , ImgIdxPos.BR )
                        .Else( null )
                        .Do();
        }

        public static T SetImgPos2GridPos<T>(
            this T src
            , ImgIdxPos pos )
            where T : Control
        {
            var rowcol = pos.Match()
                            .With( x => x == ImgIdxPos.TL , new int[]{ 0 , 0} )
                            .With( x => x == ImgIdxPos.TM , new int[]{ 0 , 1} )
                            .With( x => x == ImgIdxPos.TR , new int[]{ 0 , 2} )
                            .With( x => x == ImgIdxPos.BL , new int[]{ 1 , 0} )
                            .With( x => x == ImgIdxPos.BM , new int[]{ 1 , 1} )
                            .With( x => x == ImgIdxPos.BR , new int[]{ 1 , 2} )
                            .Else( new int[2] )
                            .Do();

            return src.SetGridPos( rowcol [ 0 ] , rowcol [ 1 ] );
        }

	


        public static Dictionary<TEnum, TValue> MapDictionary<TEnum, TValue>(
            this Dictionary<TEnum, TValue> src
            , Func<TValue, TValue> fnc
            )
            where TEnum : struct , IConvertible
        {
            return src.ToDictionary(pair => pair.Key
                                    , pair => fnc(pair.Value));
        }

        public static Image<Bgr,byte> StackSplitted(
            this Dictionary<ImgIdxPos, Image<Bgr, byte>> src)
        {
            return src [ ImgIdxPos.TL ]
                       .HStack( src [ ImgIdxPos.TM ] , 3)
                       .HStack( src [ ImgIdxPos.TR ] , 3)
                       .VStack( src [ ImgIdxPos.BL ]
                                .HStack( src [ ImgIdxPos.BM ] , 3 )
                                .HStack( src [ ImgIdxPos.BR ] , 3 ) );
        }


        #region Processing 
        public static Image<Gray, byte> Threshold(
          this Image<Gray, byte> src
          , int thresValue)
        {
            return FnThreshold(ThresholdMode.Manual)(src, thresValue);
        }


        public static Image<Gray,byte> DilateRect(
            this Image<Gray,byte> src)
        {
            return FnMorp(morpOp.Dilate, kernal.Rect)(src,3);
        }

        public static Image<Gray, byte> DilateCross(
            this Image<Gray, byte> src)
        {
            return FnMorp(morpOp.Dilate, kernal.Cross)(src, 3);
        }

        public static Image<Gray , byte> ErodeRect(
           this Image<Gray , byte> src )
        {
            return FnMorp( morpOp.Erode , kernal.Rect )( src , 3 );
        }

        public static Image<Gray , byte> ErodeCross(
           this Image<Gray , byte> src )
        {
            return FnMorp( morpOp.Erode , kernal.Cross )( src , 3 );
        }

		public static Image<Gray , byte> CloseRect(
		 this Image<Gray , byte> src )
		{
			return FnMorp( morpOp.Close , kernal.Rect )( src , 3 );
		}


		public static Image<Gray, byte> OpenCross(
            this Image<Gray, byte> src)
        {
            return FnMorp(morpOp.Open, kernal.Cross)(src, 5);
        }

		public static Image<Gray , byte> OpenRect(
		   this Image<Gray , byte> src )
		{
			return FnMorp( morpOp.Open , kernal.Rect )( src , 5 );
		}

		public static VectorOfVectorOfPoint FindContour(
            this Image<Gray,byte> src
            , int up
            , int dw)
        {
            return FnFindContour( up, dw )( src ) ;
        }

        public static List<RotatedRect> FindMinimalBox(
            this VectorOfVectorOfPoint src)
        {
            return src?.ToArrayOfArray()
                         .Select( ptarr => 
                                     ptarr.Select( pt => 
                                                     new System.Drawing.PointF( pt.X , pt.Y ) )
                                          .ToArray() )
                         .ToArray()
                         .Select( ptfArr => CvInvoke.MinAreaRect(ptfArr))
                         .ToList();
        }

        public static List<RawDefectInfo> FindDefectInfo(
         this VectorOfVectorOfPoint src )
        {
			//return src?.ToArrayOfArray()
			//             .Select( ptarr => ptarr.PointArr2DefectInfo() )
			//             .ToList();
			var output = new List<RawDefectInfo>();
			for ( int i = 0 ; i < src.Size ; i++ )
			{
				var circle = CvInvoke.MinEnclosingCircle(src[i]);
				output.Add(
					new RawDefectInfo(
					  circle.Center.Y ,
					  circle.Center.X ,
					  circle.Radius ,
					  CvInvoke.ContourArea( src [ i ] ) )
					);
			}

			return output;
		}

        private static System.Drawing.Point PointArrMean(
            this System.Drawing.Point[] src)
        {
            var ymean = src.Select( x => x.Y ).Sum() / src.GetLength( 0 );
            var xmean = src.Select( x => x.X ).Sum() / src.GetLength( 0 );
            return new System.Drawing.Point( xmean , ymean ); // For Get Center Point
        }

		private static RawDefectInfo PointArr2DefectInfo(
            this System.Drawing.Point [] src )
        {
			
            var ptFarr = src.Select( ptf => new System.Drawing.PointF( ptf.X , ptf.Y )).ToArray();
            var pt = src.PointArrMean();
            var size = CvInvoke.MinAreaRect( ptFarr ).Size; 
            var radius = Math.Sqrt( size.Width *size.Height );
            return new RawDefectInfo(
                pt.Y
                , pt.X
                , radius
                , (double)(size.Height * size.Width) );
        }


        #endregion

        #region After Processing

        public static DefectData Cvt2DefectData(
           this RawDefectInfo src 
           , int resolution)
        {
            return  new DefectData(  src.CenterY , src.CenterX , src.Size , resolution );
        }


        private static double GetMinRowPos( 
            this System.Drawing.PointF[] src) // Y
        {
            return src.Select( pos => (double)pos.Y)
                      .Min() ;
        }

        private static double GetMinColPos( 
            this System.Drawing.PointF [] src ) //X
        {
            return src.Select( pos => ( double )pos.X )
                    .Min();
        }

        private static double GetAreaSize(
           this System.Drawing.PointF [ ] src ) //AreaSize
        {
            var yPosList = src.Select( pos => ( double )pos.Y );
            var xPosList = src.Select( pos => ( double )pos.X );

            return ( yPosList.Max() - yPosList.Min() ) 
                    * ( xPosList.Max() - xPosList.Min() );
        }

        public static List<DefectData> ShiftDefectData(
            this List<DefectData> src
            , Dictionary<OffsetPos , int> offset
            , ImgIdxPos pos
            , int resol)
        {
            return pos.Match()
                       .With( x => x ==  ImgIdxPos.TM , src.ShiftDefect
                                                        ( 0 , offset[OffsetPos.Col1] , resol ) )
                       .With( x => x ==  ImgIdxPos.TR , src.ShiftDefect
                                                        ( 0 , offset[OffsetPos.Col2] , resol ) )
                       .With( x => x ==  ImgIdxPos.BL , src.ShiftDefect
                                                        ( offset [ OffsetPos.Row1 ] , 0 , resol ) )
                       .With( x => x ==  ImgIdxPos.BM , src.ShiftDefect
                                                        ( offset [ OffsetPos.Row1 ] , offset[ OffsetPos.Col1 ] , resol ) )
                       .With( x => x ==  ImgIdxPos.BR , src.ShiftDefect
                                                        ( offset [ OffsetPos.Row1 ] , offset[ OffsetPos.Col2 ] , resol ) ) 
                       .Else( src )
                       .Do();
        }



        public static List<DefectData> ShiftDefect(
            this List<DefectData> src 
            , int shiftLenY
            , int shiftLenX
            , int resol)
        {
            return src.Select( s => new DefectData( s.CenterY + shiftLenY 
                                                    , s.CenterX + shiftLenX  
                                                    , s.Size
                                                    , resol) )
                       .ToList();
        }


        // output double[3] =  row , col , size 
        public static List<DefectData> Convert2IdxPos(
            this List<DefectData> src
            , int originRowSize
            , int originColSize 
            , int idxImgSize
            , int resol )
        {
            double ratioH =  (double)idxImgSize / (double)originRowSize;
            double ratioW =  (double)idxImgSize / (double)originColSize;

            return src.Select( x => new DefectData( (x.CenterY * ratioH) , (x.CenterX * ratioW) , x.Size , resol ) )
                      .ToList();
        }

   


        public static Image<Bgr,byte> DrawIdxDefect(
            this Image<Bgr,byte> src,
            List<DefectData> defectList)
        {
            var ratio = 254 /defectList.Select( d => d.Size ).Max();

            foreach ( var d in defectList )
            {
				//if ( d.Size > 10000 )
				//{
				//	CvInvoke.Circle( src
				//				, new System.Drawing.Point( ( int )d.CenterX , ( int )d.CenterY )
				//				, 12
				//				, new MCvScalar( 200 , 40 , d.Size * ratio )
				//				, -1 );
				//}
				//else
				//{
				//	CvInvoke.Circle( src
				//				, new System.Drawing.Point( ( int )d.CenterX , ( int )d.CenterY )
				//				, 12
				//				, new MCvScalar( 10 , 240 , d.Size * ratio )
				//				, -1 );
				//
				//}

				if ( d.RealSize > 6400 ) // > 80um 
				{
					CvInvoke.Circle( src
								, new System.Drawing.Point( ( int )d.CenterX , ( int )d.CenterY )
								, 35
								, new MCvScalar( 200 , 51 , 121 )
								, -1 );
				}
				else if( d.RealSize > 2500 ) // > 50 um 
				{
					CvInvoke.Circle( src
								, new System.Drawing.Point( ( int )d.CenterX , ( int )d.CenterY )
								, 25
								, new MCvScalar( 50 , 50 , 200 )
								, -1 );

				}
				else if ( d.RealSize > 900 ) // > 30um
				{
					CvInvoke.Circle( src
								, new System.Drawing.Point( ( int )d.CenterX , ( int )d.CenterY )
								, 15
								, new MCvScalar( 105 , 105 , 120 )
								, -1 );
				
				}


			}
            return src;
        }

        #endregion



    }
}
