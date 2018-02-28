using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using static EmguCV_Extension.Vision_Tool;
using EmguCV_Extension;

namespace WaferandChipProcessing
{
    public partial class EpiCore
    {
        // Readonly Value for Create Method
        readonly int CntrSizeMin = 200;
        readonly int CntrSizeMax = 200000;


        public Func<Image<Bgr, byte>, Image<Bgr, byte>> DrawWafer;
        public Func<Image<Gray, byte>, VectorOfVectorOfPoint> FindContour;
		

        public Dictionary<EpiProcMethod, 
                          Func< Image<Gray, byte>
                                , Image<Gray, byte>>> EpiProcFnList;


        

        public void Create_GlobalFunc()
        {
            DrawWafer = FnDrawWafer(waferIndexImgSize
                                    , (int)((double)waferIndexImgSize * RatioOfDiameter2Flatzone)
                                    , new Bgr(0,0,0), 8);


            FindContour = FnFindContour(CntrSizeMin, CntrSizeMax);

        }

        public void Create_EpiProcessMethodList()
        {
			PreProcFunc temp = new Veeco6Inch583PL06_02();

			EpiProcFnList = new Dictionary<EpiProcMethod , Func<Image<Gray , byte> , Image<Gray , byte>>>();
            EpiProcFnList.Add( EpiProcMethod.Side , temp .EpiProc_Side);
            EpiProcFnList.Add( EpiProcMethod.Mid  , temp.EpiProc_Mid);
            EpiProcFnList.Add( EpiProcMethod.MTop  , temp.EpiProc_MidTop);
        }

        #region Processing Method
        Func<Image<Gray, byte>, Image<Gray, byte>> EpiProc_Side
        => src =>
        {
            return EpiCommonProcessing(src)
                   //.Threshold(150)
                   .Threshold(170) // 3for un
                   .OpenCross()
                   .Map(img =>
                   {
                       for (int i = 0; i < 2; i++)
                       {
                           img.DilateCross()
                              .DilateRect();
                       }
                       return img;
                   });
        };

        Func<Image<Gray, byte>, Image<Gray, byte>> EpiProc_Mid
        => src =>
        {
			return EpiCommonProcessing( src )
					.Threshold( 235 )
					.OpenCross();
                   //.Map(img =>
                   //      {
                   //          for (int i = 0; i < 2; i++)
                   //          {
                   //              img.DilateCross()
                   //                 .DilateRect();
                   //          }
                   //          return img;
                   //      });
        };

		Func<Image<Gray , byte> , Image<Gray , byte>> EpiProc_MidTop
		=> src =>
		{
			Console.WriteLine( "MTOP Enter" );
			return EpiCommonProcessing( src )
					.Threshold( 240 )
					.OpenCross();
			//.Map(img =>
			//      {
			//          for (int i = 0; i < 2; i++)
			//          {
			//              img.DilateCross()
			//                 .DilateRect();
			//          }
			//          return img;
			//      });
		};



		// 3um veeco
		Func<Image<Gray, byte>, Image<Gray, byte>> EpiCommonProcessing
        => src =>
        {
             return src.Inverse()
                       .HistEqualize()
                       .Median(5)
                       .Median(5);
        };


		// 5um veeco
        //Func<Image<Gray , byte> , Image<Gray , byte>> EpiCommonProcessing
        //=> src =>
        //{
        //    return src.Normalize(255)
        //              .Gamma(1.4)
        //              .Brightness(0.5,0.5)
        //              .Inverse()
        //              .HistEqualize()
        //              .Median( 5 )
        //              .Median( 5 );
        //};

        #endregion
    }
}
