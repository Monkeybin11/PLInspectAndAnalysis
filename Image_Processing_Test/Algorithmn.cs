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
    public enum kernal { Horizontal, Vertical, Cross, Rect }
    public enum morpOp { Erode, Dilate, Open, Close }
    public class Algorithmn
    {
        public static Image<Gray, byte> Threshold(Image<Gray, byte> rootimg, int thres)
        {
            return rootimg.ThresholdBinary(new Gray(thres), new Gray(255));
        }

        public static Image<Gray, byte> Contour(Image<Gray, byte> rootimg, int thres)
        {
            return rootimg.ThresholdBinary(new Gray(thres), new Gray(255));
        }


        public static Image<Gray, byte> Morp(Image<Gray, byte> rootimg, morpOp op, int size, kernal kernal)
        {
            if (size == 0) size = 1;
            var kern = CreateKernal(kernal, new System.Drawing.Size(size, size));
            return rootimg.MorphologyEx(CreateMorpOp(op), kern, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
        }

        private static MorphOp CreateMorpOp(morpOp op)
        {
            switch (op)
            {
                case morpOp.Erode:
                    return MorphOp.Erode;
                case morpOp.Dilate:
                    return MorphOp.Dilate;
                case morpOp.Open:
                    return MorphOp.Open;
                case morpOp.Close:
                    return MorphOp.Close;
                default:
                    return MorphOp.Erode;
            }
        }

        private static Mat CreateKernal(kernal kernal, System.Drawing.Size size)
        {
            switch (kernal)
            {
                case kernal.Vertical:
                    var verisize = new System.Drawing.Size(1, size.Height);
                    return CvInvoke.GetStructuringElement(ElementShape.Rectangle, verisize, new System.Drawing.Point(-1, -1));

                case kernal.Horizontal:
                    var horisize = new System.Drawing.Size(size.Width, 1);
                    return CvInvoke.GetStructuringElement(ElementShape.Rectangle, horisize, new System.Drawing.Point(-1, -1));

                case kernal.Cross:
                    return CvInvoke.GetStructuringElement(ElementShape.Cross, size, new System.Drawing.Point(-1, -1));

                case kernal.Rect:
                    return CvInvoke.GetStructuringElement(ElementShape.Rectangle, size, new System.Drawing.Point(-1, -1));

                default:
                    return CvInvoke.GetStructuringElement(ElementShape.Cross, size, new System.Drawing.Point(-1, -1));
            }
        }

    }
}
