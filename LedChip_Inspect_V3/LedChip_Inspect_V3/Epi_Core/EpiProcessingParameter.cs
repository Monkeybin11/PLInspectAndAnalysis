using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeedyCoding;
namespace WaferandChipProcessing
{
    public static class EpiProcessingParameter
    {
        public static int AreaUp = 100000;
        public static int AreaDw = 50;
        public static int Alpha  = 5;
        public static int Beta   = 5;
		public static int OuterExcludeSize = 1800;

        public static void SetParameter(int up, int dw)
        {
            AreaUp = up;
            AreaDw = dw;
        }

    }

    public static class EpiParameterExtension
    {
        public static double GetPosThreshold(
            this ImgIdxPos src)
        {
            return src.Match()
                      .With( s => s == ImgIdxPos.TM , 244 )
                      .With( s => s == ImgIdxPos.BM , 244 )
                      .Else( 170 )
                      .Do();
        }

    }

}
