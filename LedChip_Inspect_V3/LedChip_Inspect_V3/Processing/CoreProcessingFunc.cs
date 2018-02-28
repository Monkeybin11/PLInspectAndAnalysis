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
using System.Windows.Media;
using System.Windows.Controls;
using static EmguCV_Extension.Vision_Tool;
using static EmguCV_Extension.Preprocessing;
using static Util_Tool.UI.Corrdinate;
using EmguCV_Extension;
using System.Drawing;
using AccordBased_processing.Clustering;
using System.Diagnostics;


//namespace WaferandChipProcessing
//{
//    //public enum EstChipPosMode { With2Point, With4Point }

//    public static class CoreProcessingFunc
//    {
//        /*Global Function*/
//        public static Func<double[], double[]> MapCanv2Img;
//        public static Func<double[], double[]> MapCanv2ImgLTRB;
//        public static Func<double[], double[]> MapImg2Canv;
//        public static Func<Image<Gray, byte>, Image<Gray, byte>> CropImgLT;
//        public static Func<Image<Gray, byte>, Image<Gray, byte>> CropImgLB;
//        public static Func<Image<Gray, byte>, Image<Gray, byte>> CropImgRT;
//        public static Func<Image<Gray, byte>, Image<Gray, byte>> CropImgRB;
//        public static Func<System.Drawing.Rectangle, double> SumInsideBox;
//        public static Func<int, int, double> SumAreaPoint;
//        public static Action<Canvas, double, double> SetCornerRect;
//        public static Func<VectorOfVectorOfPoint, Point[]> CalcCenter;

//        /*Image Filtering*/
//        /// <summary>
//        /// Src , Threshold
//        /// </summary>
//        public static Func<Image<Gray, byte>, int, Image<Gray, byte>> DoThreshold;
//        public static Func<Bitmap, Dictionary<string, dynamic>> ClusterImg;
//        public static Func<double[][], Dictionary<string, double[][]>> ClusterData;

//        /// <summary>
//        /// Src , Size
//        /// </summary>
//        public static Func<Image<Gray, byte>, int, Image<Gray, byte>> ErodeRect;
//        public static Func<Image<Gray, byte>, int, Image<Gray, byte>> DilateRect;
//        public static Func<Image<Gray, byte>, int, Image<Gray, byte>> ErodeVerti;
//        public static Func<Image<Gray, byte>, int, Image<Gray, byte>> DilateVerti;
//        public static Func<Image<Gray, byte>, int, Image<Gray, byte>> ErodeHori;
//        public static Func<Image<Gray, byte>, int, Image<Gray, byte>> DilateHori;
//        public static Func<Image<Gray, byte>, int, Image<Gray, byte>> CloseRect;
//        public static Func<Image<Gray, byte>, int, Image<Gray, byte>> OpenRect;
//        public static Func<Image<Gray, byte>, Image<Gray, byte>, Image<Gray, byte>> TempMatch_Sq;
//        public static Func<Image<Gray, byte>, Image<Gray, byte>, Image<Gray, byte>> TempMatch_Ce;

//        /*Local Function*/

//        /// <summary>
//        /// H Chip Number , W Chip Number
//        /// </summary>
//        public static Func<double, double, double[,,]> EstedChipPos;
//        public static Func<Image<Gray, byte>, VectorOfVectorOfPoint> FindContour;
//        public static Func<Image<Gray, byte>, Image<Gray, byte>, VectorOfVectorOfPoint> FindPassContour_template;
//        public static Func<System.Drawing.PointF, double> InContour;

//        /// <summary>
//        /// y (row num), x (col num) 
//        /// </summary>
//        public static Func<double, double, bool> InBox;
//        public static Func<VectorOfVectorOfPoint, VectorOfVectorOfPoint> Sortcontours;
//        public static Func<VectorOfVectorOfPoint, List<System.Drawing.Rectangle>> ApplyBox;

//        /* Analysis */
//        public static Func<VectorOfVectorOfPoint, double[]> FindAreaUpDwBoundaryt =
//            contours =>
//            {
//                Clustering cluster = new Clustering();
//                var areaArr = contours
//                                .Map(cntrs => FnContours2Areas(cntrs))
//                                .Select(x => new double[1] { x })
//                                .ToArray<double[]>();

//                var result = areaArr
//                                .Map(areas => cluster.DataClustering(ClustMethod.KMean)(areas));

//                var centervalue = result
//                                .Where(x => x.Key == "center")
//                                .Select(y => y.Value)
//                                .ToArray();

//                var centereddata = result
//                                .Where(x => x.Key == "data")
//                                .Select(y => y.Value)
//                                .ToArray();

//                var cl0 = areaArr
//                            .Where((x, i) => centereddata[0][i][0] == centervalue[0][0][0])
//                            .Select(x => x[0])
//                            .ToArray();

//                var cl1 = areaArr
//                            .Where((x, i) => centereddata[0][i][0] == centervalue[0][1][0])
//                            .Select(x => x[0])
//                            .ToArray();

//                var cl2 = areaArr
//                            .Where((x, i) => centereddata[0][i][0] == centervalue[0][2][0])
//                            .Select(x => x[0])
//                            .ToArray();

//                var cl0min = cl0.Min();
//                var cl0max = cl0.Max();
//                var cl1min = cl1.Min();
//                var cl1max = cl1.Max();
//                var cl2min = cl2.Min();
//                var cl2max = cl2.Max();

//                double[] minmaxarray = new double[6]
//                {
//                    cl0min,
//                    cl0max,
//                    cl1min,
//                    cl1max,
//                    cl2min,
//                    cl2max
//                };

//                var output = minmaxarray.OrderBy(x => x).ToArray();
//                return output;

//            };

//        // find optimal point for threshold of intensity
//        public static Func<double[], double[]> FindIntensityUpDw =
//            intenlsit =>
//            {
//                Clustering cluster = new Clustering();

//                var intenArr = intenlsit // Middle Inten Point
//                                .Select(x => new double[1] { x })
//                                .ToArray<double[]>();

//                var result = intenArr
//                                .Map(areas => cluster.DataClustering(ClustMethod.KMean)(areas));

//                var centervalue = result
//                                .Where(x => x.Key == "center")
//                                .Select(y => y.Value)
//                                .ToArray();

//                var centereddata = result
//                                .Where(x => x.Key == "data")
//                                .Select(y => y.Value)
//                                .ToArray();

//                var cl0 = intenArr
//                            .Where((x, i) => centereddata[0][i][0] == centervalue[0][0][0])
//                            .Select(x => x[0])
//                            .ToArray(); // center point 0

//                var cl1 = intenArr
//                            .Where((x, i) => centereddata[0][i][0] == centervalue[0][1][0])
//                            .Select(x => x[0])
//                            .ToArray(); // center point 1

//                var cl2 = intenArr
//                            .Where((x, i) => centereddata[0][i][0] == centervalue[0][2][0])
//                            .Select(x => x[0])
//                            .ToArray(); // center point 2

//                var cl0min = cl0.Min();
//                var cl0max = cl0.Max();
//                var cl1min = cl1.Min();
//                var cl1max = cl1.Max();
//                var cl2min = cl2.Min();
//                var cl2max = cl2.Max();

//                // Array of each cluster min and max value 
//                double[] minmaxarray = new double[6]
//                {
//                    cl0min,
//                    cl0max,
//                    cl1min,
//                    cl1max,
//                    cl2min,
//                    cl2max
//                };

//                var output = minmaxarray.OrderBy(x => x).ToArray();
//                return new double[2] { output[2], output[5] };
//            };

//        //public static void InitFunc(Canvas canvas, Canvas corner)
//        //{
//        //    CropImgLT = FnCropImg(0, 0, (int)LTRBPixelNumberW, (int)LTRBPixelNumberH);
//        //    CropImgLB = FnCropImg(0, OriginImg.Height - (int)LTRBPixelNumberH, (int)LTRBPixelNumberW, OriginImg.Height);
//        //    CropImgRT = FnCropImg(OriginImg.Width - (int)LTRBPixelNumberW, 0, OriginImg.Width, (int)LTRBPixelNumberH);
//        //    CropImgRB = FnCropImg(OriginImg.Width - (int)LTRBPixelNumberW, OriginImg.Height - (int)LTRBPixelNumberH, OriginImg.Width, OriginImg.Height);
//        //    MapCanv2Img = Convt_Window2Real(canvas.Width, canvas.Height, OriginImg.Width, OriginImg.Height);
//        //    MapImg2Canv = Convt_Real2window(canvas.Width, canvas.Height, OriginImg.Width, OriginImg.Height);
//        //    MapCanv2ImgLTRB = Convt_Window2Real(corner.Width, corner.Height, LTRBPixelNumberW, LTRBPixelNumberH);
//        //    SumInsideBox = FnSumInsideBox(OriginImg);
//        //    SetCornerRect = FnSetCornerRect(new EmguCV_Extension.CornerMode[] {
//        //        EmguCV_Extension.CornerMode.LeftTop,
//        //        EmguCV_Extension.CornerMode.LeftBot,
//        //        EmguCV_Extension.CornerMode.RightTop,
//        //        EmguCV_Extension.CornerMode.RightBot,
//        //    });
//        //    CalcCenter = FnCalcCenter();
//        //}

//        public static void CreateEstedChipFunc(double[][] cornerPos, EstChipPosMode estmode)
//        {
//            switch (estmode)
//            {
//                case EstChipPosMode.With2Point:
//                    EstedChipPos = FnEstChipPos_2Point(cornerPos[0], cornerPos[3]);
//                    break;

//                case EstChipPosMode.With4Point:
//                    EstedChipPos = FnEstChipPos_4Point_Advanced(cornerPos[0], cornerPos[1], cornerPos[2], cornerPos[3]);
//                    break;
//            }
//        }

//        public static void CreateProcFun(ThresholdMode mode)
//        {
//            double thres = PData.ThresholdV;
//            double areaup = PData.UPAreaLimit;
//            double areadw = PData.DWAreaLimit;
//            double cHnum = PData.ChipHNum;
//            double cWnum = PData.ChipWNum;

//            Register_ProcMethod();
//            Clustering cluster = new Clustering();
//            ClusterImg = cluster.Segment(ClustMethod.KMean);
//            ClusterData = cluster.DataClustering(ClustMethod.KMean);
//            Sortcontours = FnSortcontours();

//            DoThreshold = FnThreshold(mode);
//            ErodeRect = FnMorp(morpOp.Erode, kernal.Rect);
//            DilateRect = FnMorp(morpOp.Dilate, kernal.Rect);
//            ErodeVerti = FnMorp(morpOp.Erode, kernal.Vertical);
//            DilateVerti = FnMorp(morpOp.Dilate, kernal.Vertical);
//            ErodeHori = FnMorp(morpOp.Erode, kernal.Horizontal);
//            DilateHori = FnMorp(morpOp.Dilate, kernal.Horizontal);

//            CloseRect = FnMorp(morpOp.Close, kernal.Rect);
//            OpenRect = FnMorp(morpOp.Open, kernal.Rect);
//            TempMatch_Sq = FnTemplateMatch(TempMatchType.Sq);
//            TempMatch_Ce = FnTemplateMatch(TempMatchType.Coeff);

//            FindContour = FnFindContour(areaup, areadw);
//            ApplyBox = FnApplyBox(PData.UPBoxLimit, PData.DWBoxLimit);
//            SumAreaPoint = FnSumAreaPoint((int)PData.ChipHSize, (int)PData.ChipWSize, OriginImg);

//        }


//        /// <summary>
//        /// Set up object box for check
//        /// </summary>
//        /// <param name="box"> object for check </param>
//        /// <param name="margin"> tolerance of check </param>
//        public static void Create_Inbox(Rectangle box, int margin)
//        {
//            InBox = FnInBox(box, margin);
//        }

//        #region Create Main Processing Method
//        void Register_ProcMethod()
//        {
//            Proc_Method_List = new Dictionary<SampleType, Func<Image<Gray, byte>, Image<Gray, byte>>>();
//            Proc_Method_List.Add(SampleType.None, CreateMethod_None());
//            Proc_Method_List.Add(SampleType._1B6R, CreateMethod_1B6R());
//            Proc_Method_List.Add(SampleType._A, CreateMethod_A());
//            Proc_Method_List.Add(SampleType._B, CreateMethod_BC());
//            Proc_Method_List.Add(SampleType._C, CreateMethod_BC());
//            Proc_Method_List.Add(SampleType._D, CreateMethod_D());
//            Proc_Method_List.Add(SampleType._BlueLD, CreateMethod_BlueLD());
//            Proc_Method_List.Add(SampleType.Fullested, CreateMethod_FullEst());
//        }

//        Func<Image<Gray, byte>, Image<Gray, byte>> CreateMethod_None()
//        {
//            var method = new Func<Image<Gray, byte>, Image<Gray, byte>>((img) =>
//            {
//                var backInten = BackgroundInten(img);
//                var thresImg = DoThreshold(img, (int)backInten + 5);
//                return thresImg;
//            });
//            return method;
//        }

//        Func<Image<Gray, byte>, Image<Gray, byte>> CreateMethod_1B6R()
//        {
//            var method = new Func<Image<Gray, byte>, Image<Gray, byte>>((img) =>
//            {
//                var thresImg = DoThreshold(img, (int)BackgroundInten(img) + 5);
//                return thresImg;
//            });
//            return method;
//        }
//        Func<Image<Gray, byte>, Image<Gray, byte>> CreateMethod_A()
//        {
//            var method = new Func<Image<Gray, byte>, Image<Gray, byte>>((img) =>
//            {
//                // Not Work
//                return null;
//            });
//            return method;
//        }
//        Func<Image<Gray, byte>, Image<Gray, byte>> CreateMethod_BC()
//        {
//            var method = new Func<Image<Gray, byte>, Image<Gray, byte>>((img) =>
//            {
//                //var morped =  OpenRect(CloseRect( DoThreshold( img , 23) , 3) ,3);
//                var morped = DoThreshold(TempMatch_Ce(img, TemplateImg), 120);

//                for (int i = 0; i < 8; i++)
//                {
//                    morped = ErodeHori(morped, 3);
//                }


//                for (int i = 0; i < 8; i++)
//                {
//                    morped = DilateRect(morped, 3);
//                }
//                //
//                //for (int i = 0; i < 6; i++)
//                //{
//                //    morped = DilateVerti(morped , 3);
//                //}
//                return morped;
//            });
//            return method;
//        }
//        Func<Image<Gray, byte>, Image<Gray, byte>> CreateMethod_D()
//        {
//            var method = new Func<Image<Gray, byte>, Image<Gray, byte>>((img) =>
//            {
//                var imgg = DilateRect(DilateRect(CloseRect(DoThreshold(img, BackgroundInten(img) + 1), 5), 3), 3);
//                return DilateRect(DilateRect(CloseRect(DoThreshold(img, BackgroundInten(img) + 1), 5), 3), 3);
//            });
//            return method;
//        }
//        Func<Image<Gray, byte>, Image<Gray, byte>> CreateMethod_BlueLD()
//        {
//            var method = new Func<Image<Gray, byte>, Image<Gray, byte>>((img) =>
//            {
//                var backInten = BackgroundInten(img);
//                var thresImg = DoThreshold(img, (int)backInten + 5);
//                return thresImg;
//            });
//            return method;
//        }

//        Func<Image<Gray, byte>, Image<Gray, byte>> CreateMethod_FullEst()
//        {
//            var method = new Func<Image<Gray, byte>, Image<Gray, byte>>((img) =>
//            {
//                return img;
//            });
//            return method;
//        }

//        #endregion

//        #region Helper
//        byte[,,] MatZeros(int channal1, int channal2, int channal3)
//        {
//            byte[,,] output = new byte[channal1, channal2, channal3];
//            for (int i = 0; i < channal1; i++)
//            {
//                for (int j = 0; j < channal2; j++)
//                {
//                    for (int k = 0; k < channal3; k++)
//                    {
//                        output[i, j, k] = 150;
//                    }
//                }
//            }
//            return output;
//        }

//        byte[,,] MatPattern(int channal1, int channal2, int channal3)
//        {
//            byte[,,] output = new byte[channal1, channal2, channal3];

//            Parallel.For(0, channal1, i => {
//                for (int j = 0; j < channal2; j++)
//                {
//                    if (i % 2 == 0)
//                    {
//                        if (j % 2 == 0)
//                        {
//                            output[i, j, 0] = 250;
//                            output[i, j, 1] = 250;
//                            output[i, j, 2] = 250;
//                        }
//                        else
//                        {
//                            output[i, j, 0] = 150;
//                            output[i, j, 1] = 150;
//                            output[i, j, 2] = 150;
//                        }
//                    }
//                    else if (j % 2 == 0)
//                    {
//                        output[i, j, 0] = 200;
//                        output[i, j, 1] = 200;
//                        output[i, j, 2] = 200;
//                    }
//                    else
//                    {
//                        output[i, j, 0] = 100;
//                        output[i, j, 1] = 100;
//                        output[i, j, 2] = 100;
//                    }
//                }
//            });
//            return output;
//        }

//        Image<Bgr, byte> DrawContour(Image<Bgr, byte> img, VectorOfVectorOfPoint contr)
//        {
//            for (int i = 0; i < contr.Size; i++)
//            {
//                CvInvoke.DrawContours(img, contr, i, new MCvScalar(0, 255, 0));
//            }
//            return img;
//        }

//        Image<Bgr, byte> DrawCenterPoint(Image<Bgr, byte> img, double[,,] centrPoint)
//        {
//            for (int k = 0; k < 100; k++)
//            {
//                for (int i = 0; i < centrPoint.GetLength(0); i++)
//                {
//                    for (int j = 0; j < centrPoint.GetLength(1); j++)
//                    {
//                        CircleF cirp = new CircleF();
//                        cirp.Center = new PointF((float)(int)centrPoint[i, j, 1], (float)(int)centrPoint[i, j, 0]);
//                        img.Draw(cirp, new Bgr(0, 0, 250), 1);
//                    }
//                }
//            }
//            return img;
//        }

//        Image<Bgr, byte> DrawBox(Image<Bgr, byte> img, List<System.Drawing.Rectangle> rclist)
//        {
//            Parallel.For(0, rclist.Count, i =>
//            {
//                img.Draw(rclist[i], new Bgr(40, 165, 5), 1);
//            });
//            return img;
//        }

//        Image<Bgr, byte> DrawBox_LOW(Image<Bgr, byte> img, List<System.Drawing.Rectangle> rclist)
//        {
//            Parallel.For(0, rclist.Count, i =>
//            {
//                img.Draw(rclist[i], new Bgr(165, 40, 5), 1);
//            });
//            return img;
//        }

//        Image<Bgr, byte> DrawBox_OVER(Image<Bgr, byte> img, List<System.Drawing.Rectangle> rclist)
//        {
//            Parallel.For(0, rclist.Count, i =>
//            {
//                img.Draw(rclist[i], new Bgr(40, 165, 165), 1);
//            });
//            return img;
//        }
//        #endregion

//    }
//}
