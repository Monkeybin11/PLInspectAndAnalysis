
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
namespace Image_Processing_Test
{
    public static class Data
    {
        public static Image<Gray, byte> RootImg;
        public static Image<Bgr, byte> RootImgColor;
        public static Image<Gray, byte> WorkingImg;
        public static Image<Gray, byte> TemplateImg;
        public static List<Image<Gray, byte>> HistoryImg;
        public static kernal currentMask;


    }
}

