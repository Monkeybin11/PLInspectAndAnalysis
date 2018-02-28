using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaferandChipProcessing
{
    public enum DefectState { NonDefect , Defect }
    //public enum DefectShape { Circle, Line  };
    public class EpiDataResult
    {
        //statistic 
        public int Size1 = 100  + 100; // 10
        public int Size2 = 900  + 100; // 30
        public int Size3 = 1000 + 100; // 100

        public int Size1Number ;
        public int Size2Number ;
        public int Size3Number ;
        public int Size4Number ;

        public DefectState IsCleanState;

        // Defect Position 
        public List<DefectData> DefectList;

        public EpiDataResult()
        {
            DefectList = new List<DefectData>();
            IsCleanState = DefectState.Defect;
        }

        public EpiDataResult( DefectState state )
        {
            DefectList = new List<DefectData>();
            IsCleanState = state;
        }
    }

    public class DefectData
    {
        private double Resolution;
        public double CenterY , CenterX, Size;
        public double RealY { get { return CenterY * Resolution; } }
        public double RealX { get { return CenterX * Resolution; } }

        private double Radius { get { return Math.Sqrt( Size ); } }

        public double RealSize { get { return Size * Resolution * Resolution; } }
    
        public DefectData( double centery , double centerx , double defectSize , double resol)
        {
            CenterY = centery;
            CenterX = centerx;
            Size = defectSize;
            Resolution = resol;
        }
    }
}
