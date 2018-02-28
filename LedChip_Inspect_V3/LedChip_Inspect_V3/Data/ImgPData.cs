using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.Util;

namespace WaferandChipProcessing.Data
{
    public class ImgPData
    {
        /*Display Window Data*/
        public double CanvasH;
        public double CanvasW;
        public int ImgRealH;
        public int ImgRealW;

        public double[][] CornerPos_Img { get { return GetcornerPos(); } }

        public double[] LTPos_Img;
        public double[] LBPos_Img;
        public double[] RTPos_Img;
        public double[] RBPos_Img;

        public double[][]AdvHChipPos = new double[3][];

        public int ChipHNum;
        public int ChipWNum;

        // Size Unit = um //
        public double ChipHSize;
        public double ChipWSize;

        /* Img Processing Parameter */
        public int ThresholdV;
        
        public int UPBoxLimit { get { return (int)(ChipHSize*ChipWSize); } }
        public int DWBoxLimit = 1;
        public int UPAreaLimit;
        public int DWAreaLimit;
        public int IntenSumUPLimit;
        public int IntenSumDWLimit;

        double[][] GetcornerPos() {
            return new double[4][] {
                this.LTPos_Img,
                this.LBPos_Img,
                this.RTPos_Img,
                this.RBPos_Img
            };
        }

        public void SetFrame( double hc , double wc ,int hi,int wi )
        {
            CanvasH  = hc;
            CanvasW  = wc;
            ImgRealH = hi;
            ImgRealW = wi;
        }
    }
}
