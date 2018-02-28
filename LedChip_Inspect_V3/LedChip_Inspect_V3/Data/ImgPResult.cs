using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.Util;
using Emgu.CV.Util;
using System.Drawing;

namespace WaferandChipProcessing.Data
{
    public class ImgPResult
    {
        public int ChipTotalCount { get { return ChipPassCount + ChipLowCount + ChipOverCount + ChipNOPLCount; } }
        public int ChipPassCount = 0;
        public int ChipLowCount = 0;
        public int ChipOverCount = 0;
        public int ChipNOPLCount = 0;
        public int ChipTotalNgCount { get { return ChipLowCount + ChipOverCount + ChipNOPLCount; } }

        public int AreaUpLimit;
        public int AreaDwLimit;
        public int IntenUpLimit;
        public int IntenDwLimit;

        public List<ExResult> OutData = new System.Collections.Generic.List<ExResult>();
        public List<int> SizeHist = new System.Collections.Generic.List<int>();
        public List<int> ChipIntensityHist = new System.Collections.Generic.List<int>();

        public ImgPResult (
            int areaup,
            int areadw,
            int intenup,
            int intendw) {
            AreaUpLimit  = areaup;
            AreaDwLimit  = areadw;
            IntenUpLimit = intenup;
            IntenDwLimit = intendw;

        }
    }

    public class ContourData
    {
        public VectorOfPoint Coordinate;
        public double Area;
        public double[] Center;
    }

    public class ExResult
    {
        public int Hindex;
        public int Windex;
        public int HindexError;
        public int WindexError;
        public string OKNG;
        public double Intensity;
        public double ContourSize;
        public System.Drawing.Rectangle BoxData;
        public System.Drawing.Point PositionError;

		public ExResult( int hindex , int windex )
		{
			Hindex = hindex;
			Windex = windex;
			HindexError = 0;
			WindexError = 0;
			OKNG		= "NOPL";
			Intensity	= 0;
			ContourSize = 0;
			BoxData = new Rectangle();
		}

        public ExResult(
            int hindex
            , int windex
            , int hindexError
            , int windexError
            , string passfail 
            , double inten 
            , double contsize 
            , Rectangle boxData = new Rectangle())
        {
            Hindex = hindex;
            Windex = windex;
            HindexError = hindexError;
            WindexError = windexError;
            OKNG = passfail;
            Intensity    = inten;
            ContourSize = contsize;
            BoxData = boxData;
        }
    }

}
