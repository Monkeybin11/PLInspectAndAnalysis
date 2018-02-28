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
using WaferandChipProcessing.Data;

namespace WaferandChipProcessing
{
	public enum SampleType { None, _1B6R, _A, _B, _C, _D, _BlueLD, Fullested, Lumence_0620, Lumence_0620_2mm, AOTCW, AOTTB, Epistar, LumenMapFront, LumenMapBack, LumenLineFront, LumenLineBack, Guang2, Guang2Scatter, Guang2Mapping, Guang2MappingSC, SSDisplay1RGBSample, PlaynittideB1, PlaynittideB2, PlaynittideG1 };
	public enum BoxDrawRole { LoutHout, LinHout, LinHin }; // 나중에 

    public enum AdvancedChipPos { None , First , Second , Third }

    public partial class MainCore
    {
        public int Crop_W = 60;
        public int Crop_H = 60;
        public int LineThickness = 3;
        public ImgPData PData;
        public ImgPResult PResult;
        public Image<Gray,byte> OriginImg;
        public Image<Gray,byte> TemplateImg;
        public Image<Bgr,byte> ColorOriImg;
        public Image<Bgr,byte> ProcedImg;
        public Image<Bgr,byte> IndexViewImg;
        public double zoomMax = 20;
        public double zoomMin = 0.2;
        public double zoomSpeed = 0.001;
        public double zoom = 1;
        public double LTRBPixelNumberW;
        public double LTRBPixelNumberH;
        

        public List<System.Drawing.PointF> PassChipList;
        public List<System.Drawing.PointF> FailChipList;

        public readonly float HistogramDwRange = 46;
        public readonly int BinSize = 100;
        public List<System.Drawing.PointF> passChipList;
        public List<System.Drawing.PointF> failChipList;

        public Dictionary<SampleType,Func<Image<Gray,byte>,Image<Gray,byte>>> Proc_Method_List;
        public SampleType SelectedSample;
        public Dictionary<string,SampleType> SampleTypeList;
        public Dictionary<int,SampleType> LinkerSampletoMethod;

        public AdvancedChipPos ChipPosMode;
    }
}
