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
using System.Drawing;
using EmguCV_Extension;
using SpeedyCoding;

namespace WaferandChipProcessing
{
    public delegate void ImgForDisplay( ImgIdxPos pos, Image<Gray, byte> img );
    public delegate void TrsFullImage( Tuple< ImgIdxPos, Image<Gray, byte> >[] zippedlist );
    public delegate void TrsProcedImage( Image<Bgr, byte> processedImg );
    public delegate void TrsProgress( int percentage );
    //public delegate void TrsProgress( int percentage );
    public delegate void TrsStatistic( int[] sizeNumber );

    public partial class EpiCore
    {
        public event TrsFullImage evtTrsFullImg;
        public event TrsProcedImage evtTrsResizedProcedImg;
        public event TrsProcedImage evtTrsIdxImg;
        //public event TrsProgress evtProgressTime;
        public event TrsProgress evtProcTime;
        public event TrsStatistic evtStatistic;


        public readonly double RatioOfDiameter2Flatzone  = 10859f / 49889f; // veeco 513 sample
        public int waferIndexImgSize;

        // double[3] => row , col , size 

        //public Image<Gray, byte>[][] OriginImg = new Image<Gray, byte>[2][];
        //public Image<Bgr, byte>[][] ColorOriImg ;
        //public Image<Bgr, byte>[][] ProcedImg;
        public Image<Bgr, byte> IndexViewImg;


        Dictionary<ImgIdxPos, Image<Gray, byte>> EpiGrayImgDic;
        Dictionary<ImgIdxPos, Image<Bgr, byte>> EpiColorImgDic;
        Dictionary<ImgIdxPos, Image<Bgr, byte>> EpiProcedImgDic;

        Dictionary<Tuple<int?, int?>, ImgIdxPos> Pos2EnumTable;

        public EpiSeperatedImgEvent[] EpiSeperatedImgTrsEvt;
      
        public Tuple<ImgIdxPos, int, int>[] ImgPosPair;
        public Dictionary<OffsetPos, int> Offset;

        public Dictionary<ImgIdxPos , EpiDataResult> EpiProcResultDict;
        public EpiDataResult EpiProcResult_FullScale;
        public EpiDataResult EpiProcResul_IdxScale;

        public int SampleResolution;


        void ResetData()
        {
            EpiProcResult_FullScale = new EpiDataResult();
            EpiProcResul_IdxScale = new EpiDataResult();

            EpiProcedImgDic = new Dictionary<ImgIdxPos , Image<Bgr , byte>>();
            EpiProcResultDict = new Dictionary<ImgIdxPos , EpiDataResult>();

            var enumlist = Enum.GetValues(typeof(ImgIdxPos))
                                       .Cast<ImgIdxPos>()
                                       .ToArray()
                                       .ActLoop(x =>
                                                   EpiProcResultDict.Add(
                                                       x,
                                                       new EpiDataResult()));

            IndexViewImg = DrawWafer( new Image<Bgr , byte>( waferIndexImgSize , waferIndexImgSize )
                                       .Inverse() );
        }

        public Image<Bgr,byte> GetProcedImg(ImgIdxPos pos )
        {
			//return new Image<Bgr , byte>( @"C:\Veeco_Result\TestSample.png" );
			try
			{
				return EpiProcedImgDic [ pos ];
			}
			catch ( Exception )
			{

				return null;
			}
           
        }
    }

    public class RawDefectInfo
    {
        public double CenterY;
        public double CenterX;
        public double Radius;
        public double Size;

        public RawDefectInfo( double centery , double centerx , double radius , double size )
        {
            CenterY = centery;
            CenterX = centerx;
            Radius = radius;
            Size = size;
        }

    }

}
