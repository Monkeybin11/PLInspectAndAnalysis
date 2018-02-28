using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace WaferandChipProcessing
{
    public partial class MainCore
    {
        public int APBoxTolerance = 8;
        Bgr ApCenterPointColor  = new Bgr(46 , 5 , 244);
        Bgr ApCenteBoxColor     = new Bgr(241,50,237);
        Bgr ApOkChipColor       = new Bgr(35,222,101);
        Bgr ApNgChipColor       = new Bgr(116,35,222);
        Bgr ApLowColor          = new Bgr(233,140,35);
        Bgr ApOverColor         = new Bgr(21,163,216);


        Bgr ApEpiDefectColor    = new Bgr(35,222,101);
        Bgr ApEpiDefectC1Color  = new Bgr(35,222,101);
        Bgr ApEpiDefectC2Color  = new Bgr(35,222,101);
        Bgr ApEpiDefectC3Color  = new Bgr(35,222,101);



    }
}
