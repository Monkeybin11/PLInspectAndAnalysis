using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.Features2D;
using Emgu.CV.XFeatures2D;
using AccordBased_processing.Clustering;
using static Image_Processing_Test.Data;
using static Image_Processing_Test.Algorithmn;
//using Accord.Imaging.Converters;
//using Accord.MachineLearning;
using Accord.Math;
using static EmguCV_Extension.Vision_Tool;
using EmguCV_Extension;
using static EmguCV_Extension.EmgucvExtension;
using System.Diagnostics;
using AccordBased_processing.FeatureExtract;
using SpeedyCoding;

namespace Image_Processing_Test
{
    public partial class Form1 : Form
    {
        bool ColorMode = false;
        string basepath;
        Feature_Extract<byte, byte> Fe = new Feature_Extract<byte, byte>();

        public Form1()
        {
            InitializeComponent();
            InitName();
            InitData();
            currentMask = kernal.Cross;
        }

        void InitName()
        {
            COntour.Text = "Contour";
            Thres.Text = "Thres";
            ThresToZero.Text = "Thers 2Zero";
            Erode.Text = "Erode";
            Dilate.Text = "Dilate";
            Open.Text = "Open";
            Close.Text = "Close";
            squ.Text = "sq";
            Coeff.Text = "Coeff";
            Corr.Text = "COrr";
            Hori.Text = "Hori";
            Verti.Text = "verti";
            Cross.Text = "Cross";
            Rect.Text = "Rect";

        }

        void InitData()
        {
            HistoryImg = new List<Image<Gray, byte>>();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {

            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ColorMode = false;
                Reset();

                RootImg = new Image<Gray, byte>(ofd.FileName);
                WorkingImg = new Image<Gray, byte>(ofd.FileName);
                var templatepath = System.IO.Path.GetDirectoryName(ofd.FileName);
                
                rtxLog.AppendText("Load" + Environment.NewLine);

                imageBox1.Image = WorkingImg;
                History();
                basepath = System.IO.Path.GetFullPath(ofd.FileName);
                Console.WriteLine(basepath);
                try
                {
                    TemplateImg = new Image<Gray, byte>(templatepath + "\\template.bmp");
                 
                }
                catch (Exception)
                {
                }
            }


        }

        void Reset() {
            rtxLog.Clear();
            RootImg = null;
            WorkingImg = null;
            HistoryImg.Clear();
        }

        #region tab1
        private void Thres_Click(object sender, EventArgs e)
        {
            var thres = (int)nudThres.Value;
            WorkingImg = WorkingImg.ThresholdBinary(new Gray(thres), new Gray(255));
            rtxLog.AppendText("Thres_Click  " + thres.ToString() + Environment.NewLine);
            RegistHisroty(WorkingImg);
        }

        private void ThresToZero_Click(object sender, EventArgs e)
        {
            var thres = (int)nudThres.Value;
            WorkingImg = WorkingImg.ThresholdToZero(new Gray(thres));
            rtxLog.AppendText("Thres_Click  " + thres.ToString() + Environment.NewLine);
            RegistHisroty(WorkingImg);
        }

        private void COntour_Click(object sender, EventArgs e)
        {
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            var tempimg1 = WorkingImg.Clone();
            var tempimg2 = WorkingImg.Clone();
            CvInvoke.FindContours(tempimg1, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            Image<Bgr, byte> colorimg = tempimg2.Convert<Bgr, byte>();
            for (int i = 0; i < contours.Size; i++)
            {
                CvInvoke.DrawContours(colorimg, contours, i, new MCvScalar(14, 200, 40));
            }
            rtxLog.AppendText("Contour" + Environment.NewLine);
            RegistHisroty(WorkingImg, false);
            imageBox1.Image = colorimg;
        }

        private void btnBox_Click(object sender, EventArgs e)
        {
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            var tempimg1 = WorkingImg.Clone();
            var tempimg2 = WorkingImg.Clone();
            CvInvoke.FindContours(tempimg1, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            Image<Bgr, byte> colorimg = tempimg2.Convert<Bgr, byte>();
            for (int i = 0; i < contours.Size; i++)
            {
                CvInvoke.DrawContours(colorimg, contours, i, new MCvScalar(14, 200, 40));
            }
            List<System.Drawing.Rectangle> PassBoxArr = new List<System.Drawing.Rectangle>();
            for (int i = 0; i < contours.Size; i++)
            {
                System.Drawing.Rectangle rc = CvInvoke.BoundingRectangle(contours[i]);
                PassBoxArr.Add(rc);
            }

            Parallel.For(0, PassBoxArr.Count, i =>
            {
                colorimg.Draw(PassBoxArr[i], new Bgr(20, 5, 165), 1);
            });

            rtxLog.AppendText("Box" + Environment.NewLine);
            RegistHisroty(WorkingImg);
        }

        private void btnBoxOnOri_Click(object sender, EventArgs e)
        {
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            var temproot = RootImg.Clone();
            var tempimg1 = WorkingImg.Clone();
            var tempimg2 = WorkingImg.Clone();
            CvInvoke.FindContours(tempimg1, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            Image<Bgr, byte> colorimg = tempimg2.Convert<Bgr, byte>();
            Image<Bgr, byte> tempOriImg = temproot.Convert<Bgr, byte>();
            //for ( int i = 0 ; i < contours.Size ; i++ )
            //{
            //    CvInvoke.DrawContours( tempOriImg , contours , i , new MCvScalar( 14 , 200 , 40 ) );
            //}
            List<System.Drawing.Rectangle> PassBoxArr = new List<System.Drawing.Rectangle>();

            List<double> arealist = new List<double>();

            for (int i = 0; i < contours.Size; i++)
            {
                System.Drawing.Rectangle rc = CvInvoke.BoundingRectangle(contours[i]);
                double areasize = rc.Width * rc.Height;
                //.Act( @ths => arealist.Add(@ths));
                arealist.Add(areasize);

                if (areasize > (double)nudAreaDw.Value || areasize < (double)nudAreaUp.Value)
                {
                    PassBoxArr.Add(rc); // Check Display 
                }
            }

            var areaarr = arealist.ToArray<double>();
            var histodata = areaarr.Map(@ths =>
                                           @ths.ShowHisto(30
                                           , (int)@ths.Min()
                                           , (int)@ths.Max()));

            float minv = (float)areaarr.Min();
            float maxv = (float)areaarr.Max();

            Form2 f2 = new Form2();
            f2.boxhisto.ClearHistogram();
            f2.boxhisto.AddHistogram(null, System.Drawing.Color.Black, histodata, 300, new float[] { minv, maxv });
            f2.boxhisto.Refresh();
            f2.Show();

            rtxLog.AppendText("Box on Origin" + Environment.NewLine);
            RegistHisroty(tempOriImg);
        }
        private void btnUnion_Click(object sender, EventArgs e)
        {
            WorkingImg = FnOp_UnionTrans(WorkingImg);
            rtxLog.AppendText("TbtnUnion_Click  " + Environment.NewLine);
            RegistHisroty(WorkingImg);
        }

        #endregion



        #region morp

        private void Erode_Click(object sender, EventArgs e)
        {

            WorkingImg = Morp(WorkingImg, morpOp.Erode, (int)nudMaskSize.Value, Selected_Kernal());
            rtxLog.AppendText("Erode_Click" + Selected_Kernal().ToString() + Environment.NewLine);
            RegistHisroty(WorkingImg);
        }

        private void Dilate_Click(object sender, EventArgs e)
        {
            WorkingImg = Morp(WorkingImg, morpOp.Dilate, (int)nudMaskSize.Value, Selected_Kernal());
            rtxLog.AppendText("Dilate_Click" + Selected_Kernal().ToString() + Environment.NewLine);
            RegistHisroty(WorkingImg);
        }

        private void Open_Click(object sender, EventArgs e)
        {
            WorkingImg = Morp(WorkingImg, morpOp.Open, (int)nudMaskSize.Value, Selected_Kernal());
            rtxLog.AppendText("Open_Click" + Selected_Kernal().ToString() + Environment.NewLine);
            RegistHisroty(WorkingImg);
        }

        private void Close_Click(object sender, EventArgs e)
        {
            WorkingImg = Morp(WorkingImg, morpOp.Close, (int)nudMaskSize.Value, Selected_Kernal());
            rtxLog.AppendText("Close_Click" + Environment.NewLine);
            RegistHisroty(WorkingImg);
        }
        #endregion

        private void squ_Click(object sender, EventArgs e)
        {
            var result = WorkingImg.MatchTemplate(TemplateImg, TemplateMatchingType.SqdiffNormed);
            WorkingImg = PaddingImage((255 - result * 255).Convert<Gray, byte>(), TemplateImg);
            rtxLog.AppendText("squ_Click" + Environment.NewLine);
            RegistHisroty(WorkingImg);
        }

        private void Coeff_Click(object sender, EventArgs e)
        {
            var result = WorkingImg.MatchTemplate(TemplateImg, TemplateMatchingType.CcoeffNormed);
            WorkingImg = PaddingImage((result * 255).Convert<Gray, byte>(), TemplateImg);
            rtxLog.AppendText("Coeff_Click" + Environment.NewLine);
            RegistHisroty(WorkingImg);
        }

        private void Corr_Click(object sender, EventArgs e)
        {
            rtxLog.AppendText("Corr_Click" + Environment.NewLine);
            var result = WorkingImg.MatchTemplate(TemplateImg, TemplateMatchingType.CcorrNormed);
            WorkingImg = PaddingImage((255 - 255 * result).Convert<Gray, byte>(), TemplateImg);
            RegistHisroty(WorkingImg);
        }

        private void btnLBP_Click(object sender, EventArgs e)
        {
            var imgdata = WorkingImg.Data;
            int rownum = imgdata.Len();
            int colnum = imgdata.Len(1);
            byte[][] lbpdata = rownum.JArray<byte>(colnum);
            for (int j = 0; j < rownum; j++)
            {
                var rows = new byte[colnum];
                for (int i = 0; i < colnum; i++)
                {
                    rows[i] = imgdata[j, i, 0];
                }
                lbpdata[j] = rows;
            }
            //WorkingImg.Data = Fe.LBP(lbpdata).ConvertToImgData();
            rtxLog.AppendText("btnLBP_Click" + Selected_Kernal().ToString() + Environment.NewLine);
            RegistHisroty(WorkingImg);
        }

        private void btnInverse_Click_1(object sender, EventArgs e)
        {

            //Parallel.For(0, WorkingImg.Height, j =>
            //{
            //    for (int i = 0; i < WorkingImg.Width; i++)
            //    {
            //        WorkingImg.Data[j, i, 0] = (byte)(255 - WorkingImg.Data[j, i, 0]);
            //    }
            //});
            WorkingImg = WorkingImg.Not();
            rtxLog.AppendText("btnInverse_Click  " + Environment.NewLine);
            RegistHisroty(WorkingImg);
        }



        Image<Gray, byte> PaddingImage(Image<Gray, byte> match_result, Image<Gray, byte> template)
        {
            Image<Gray, byte> padded = new Image<Gray, byte>(match_result.Width + TemplateImg.Width, match_result.Height + TemplateImg.Height);
            for (int j = 0; j < match_result.Height; j++)
            {
                for (int i = 0; i < match_result.Width; i++)
                {
                    padded[TemplateImg.Height / 2 + j, TemplateImg.Width / 2 + i] = match_result[j, i];

                }
            }
            return padded;
        }

        void RegistHisroty(Image<Gray, byte> img, bool display = true)
        {
            History();
            if (display) imageBox1.Image = img;
        }

        void RegistHisroty(Image<Bgr, byte> img)
        {
            History();
            imageBox1.Image = img;
        }


        private void btnReset_Click(object sender, EventArgs e)
        {
            rtxLog.AppendText("Reset" + Environment.NewLine);
            WorkingImg = RootImg.Clone();
            HistoryImg.Clear();
            RegistHisroty(WorkingImg);
        }

        kernal Selected_Kernal()
        {
            if (Hori.Checked)
            {
                return kernal.Horizontal;
            }
            else if (Verti.Checked)
            {
                return kernal.Vertical;
            }
            else if (Cross.Checked)
            {
                return kernal.Cross;
            }
            else
            {
                return kernal.Rect;
            }
        }

        void History()
        {
            if (WorkingImg != null)
            {
                if (HistoryImg.Count < 20)
                {
                    HistoryImg.Add(WorkingImg.Clone());
                }
                else
                {
                    HistoryImg.RemoveAt(0);
                    History();
                }
            }
        }

        void Back()
        {
            if (HistoryImg.Count > 1)
            {
                WorkingImg = HistoryImg[HistoryImg.Count - 2].Clone();
                HistoryImg.RemoveAt(HistoryImg.Count - 1);
                imageBox1.Image = WorkingImg;

                var num = rtxLog.Lines.Length - 2;
                rtxLog.Lines = rtxLog.Lines.Take(num).ToArray();
                rtxLog.AppendText(Environment.NewLine);

            }
            else if(HistoryImg.Count == 1)
            {
                WorkingImg = HistoryImg[0].Clone();
                HistoryImg.RemoveAt(0);
                imageBox1.Image = WorkingImg;
                rtxLog.AppendText(Environment.NewLine);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            Back();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            //var thres = (int)nudThres.Value;
            //Image<Gray, byte> imgoutput = new Image<Gray, byte>(WorkingImg.Data);
            //for (int i = 0; i < 20; i++)
            //{
            //    int thr = 190 + i;
            //    imgoutput = WorkingImg.ThresholdBinary(new Gray(thr), new Gray(255));
            //    imgoutput.Save(@"C:\Veeco_TestImg\Result" + "\\" + thr.ToString() + ".png");
            //}

            //rtxLog.AppendText( "ThresInv_Click  " + thres.ToString() + Environment.NewLine );
            //RegistHisroty( WorkingImg );


            var cluster = new Clustering();
            var resultlist = cluster.test(WorkingImg.ToBitmap());
            Image<Gray,byte> tetimg = new Image<Gray, byte>(resultlist["image"]);
            tetimg.Save( basepath + "Clustered.bmp" );
            label1.Text = ((int)resultlist["center"][0][0]).ToString();
            label2.Text = ((int)resultlist["center"][1][0]).ToString();
            label3.Text = ((int)resultlist["center"][2][0]).ToString();
            WorkingImg = tetimg;
            RegistHisroty( WorkingImg );
        }

        private void btnTestSomething_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Image<Bgr, byte> colorimg = new Image<Bgr, byte>(ofd.FileName);
                Processing cnv = new Processing();

                var rg = cnv.DrawContourAndAreaSize_RG( colorimg );
                try
                {
                    if ( rg.Item1 != null ) imageBox1.Image = rg.Item1;
                    Console.WriteLine( rg.Item2 );
                }
                catch ( Exception )
                {
                }
              
            }
        }

        private void btnhistogram_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2();
            f2.boxhisto.ClearHistogram();
            f2.boxhisto.GenerateHistograms(WorkingImg, 256);
            f2.boxhisto.Refresh();
            f2.Show();
        }

        private void btnAKAZE_Click(object sender, EventArgs e)
        {
            var temproot = RootImg.Clone();
            var tempimg1 = WorkingImg.Clone();
            Image<Bgr, byte> colorimg = tempimg1.Convert<Bgr, byte>();
            Image<Bgr, byte> tempOriImg = temproot.Convert<Bgr, byte>();
            var f2d = new AKAZE(
                descriptorChannels: 1);

            var keypoint = f2d.Detect(WorkingImg);
            foreach (var point in keypoint)
            {
                System.Drawing.Rectangle rect = new Rectangle();
                rect.X = (int)point.Point.X;
                rect.Y = (int)point.Point.Y;
                rect.Width = (int)point.Size;
                rect.Height = (int)point.Size;
                tempOriImg.Draw(rect, new Bgr(60, 200, 10), 2);
            }

            rtxLog.AppendText("btnAKAZE_Click" + Environment.NewLine);
            RegistHisroty(tempOriImg);

        }

        private void btnMSER_Click(object sender, EventArgs e)
        {
            var temproot = RootImg.Clone();
            var tempimg1 = WorkingImg.Clone();
            Image<Bgr, byte> colorimg = tempimg1.Convert<Bgr, byte>();
            Image<Bgr, byte> tempOriImg = temproot.Convert<Bgr, byte>();
            var f2d = new MSERDetector();

            var keypoint = f2d.Detect(WorkingImg);
            foreach (var point in keypoint)
            {
                System.Drawing.Rectangle rect = new Rectangle();
                rect.X = (int)point.Point.X;
                rect.Y = (int)point.Point.Y;
                rect.Width = (int)point.Size;
                rect.Height = (int)point.Size;
                tempOriImg.Draw(rect, new Bgr(60, 200, 10), 2);
            }

            rtxLog.AppendText("btnMSER_Click" + Environment.NewLine);
            RegistHisroty(tempOriImg);
        }

        private void btnORB_Click(object sender, EventArgs e)
        {
            var temproot = RootImg.Clone();
            var tempimg1 = WorkingImg.Clone();
            Image<Bgr, byte> colorimg = tempimg1.Convert<Bgr, byte>();
            Image<Bgr, byte> tempOriImg = temproot.Convert<Bgr, byte>();
            var f2d = new ORBDetector();

            var keypoint = f2d.Detect(WorkingImg);
            foreach (var point in keypoint)
            {
                System.Drawing.Rectangle rect = new Rectangle();
                rect.X = (int)point.Point.X;
                rect.Y = (int)point.Point.Y;
                rect.Width = (int)point.Size;
                rect.Height = (int)point.Size;
                tempOriImg.Draw(rect, new Bgr(60, 200, 10), 2);
            }

            rtxLog.AppendText("btnORB_Click" + Environment.NewLine);
            RegistHisroty(tempOriImg);
        }

        private void btnBrisk_Click(object sender, EventArgs e)
        {
            var temproot = RootImg.Clone();
            var tempimg1 = WorkingImg.Clone();
            Image<Bgr, byte> colorimg = tempimg1.Convert<Bgr, byte>();
            Image<Bgr, byte> tempOriImg = temproot.Convert<Bgr, byte>();
            //var f2d = new Brisk((int)nudbri1.Value ,(int)nudbri1.Value , (int)nudbri1.Value );
            var f2d = new Brisk();

            var keypoint = f2d.Detect(WorkingImg);
            foreach (var point in keypoint)
            {
                System.Drawing.Rectangle rect = new Rectangle();
                rect.X = (int)point.Point.X;
                rect.Y = (int)point.Point.Y;
                rect.Width = (int)point.Size;
                rect.Height = (int)point.Size;
                tempOriImg.Draw(rect, new Bgr(60, 200, 10), 2);
            }

            rtxLog.AppendText("btnBrisk_Click" + Environment.NewLine);
            RegistHisroty(tempOriImg);
        }

        private void btnBF_Click(object sender, EventArgs e)
        {
            var temproot = RootImg.Clone();
            var tempimg1 = WorkingImg.Clone();
            Image<Bgr, byte> colorimg = tempimg1.Convert<Bgr, byte>();
            Image<Bgr, byte> tempOriImg = temproot.Convert<Bgr, byte>();
            var f2d = new DAISY();
            var keypoint = f2d.Detect(WorkingImg);
            foreach (var point in keypoint)
            {
                System.Drawing.Rectangle rect = new Rectangle();
                rect.X = (int)point.Point.X;
                rect.Y = (int)point.Point.Y;
                rect.Width = (int)point.Size;
                rect.Height = (int)point.Size;
                tempOriImg.Draw(rect, new Bgr(60, 200, 10), 2);
            }

            rtxLog.AppendText("btnBrisk_Click" + Environment.NewLine);
            RegistHisroty(tempOriImg);
        }

        private void btnFreak_Click(object sender, EventArgs e)
        {
            var temproot = RootImg.Clone();
            var tempimg1 = WorkingImg.Clone();
            Image<Bgr, byte> colorimg = tempimg1.Convert<Bgr, byte>();
            Image<Bgr, byte> tempOriImg = temproot.Convert<Bgr, byte>();
            var f2d = new Freak();
            var keypoint = f2d.Detect(WorkingImg);
            foreach (var point in keypoint)
            {
                System.Drawing.Rectangle rect = new Rectangle();
                rect.X = (int)point.Point.X;
                rect.Y = (int)point.Point.Y;
                rect.Width = (int)point.Size;
                rect.Height = (int)point.Size;
                tempOriImg.Draw(rect, new Bgr(60, 200, 10), 2);
            }
            rtxLog.AppendText("btnFreak_Click" + Environment.NewLine);
            RegistHisroty(tempOriImg);
        }

        private void btnNIMean_Click(object sender, EventArgs e)
        {
            CvInvoke.FastNlMeansDenoising(WorkingImg, WorkingImg , (int)nudNlMean_h.Value , (int)nudNlMean_windowSize.Value , ( int )nudsearchw.Value );
            rtxLog.AppendText("btnNIMean_Click" + Environment.NewLine);
            RegistHisroty(WorkingImg);
        }

        private void btnEqualization_Click(object sender, EventArgs e)
        {
            WorkingImg._EqualizeHist();
            rtxLog.AppendText("btnEqualization_Click" + Environment.NewLine);
            RegistHisroty(WorkingImg);
        }

        private void btnMedian_Click(object sender, EventArgs e)
        {
            CvInvoke.MedianBlur(WorkingImg, WorkingImg, (int)nudMedianSize.Value);
            rtxLog.AppendText("btnMedian_Click" + Environment.NewLine);
            RegistHisroty(WorkingImg);
        }

        private void btnConourArea_Click(object sender, EventArgs e)
        {
            int up = (int)nudCntrUp.Value;
            int dw = (int)nudCntrDw.Value;


            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            var tempimg1 = WorkingImg.Clone();

            
            var colorimg = ColorMode == true ?  new Image<Bgr, byte>(RootImgColor.Data) : new Image<Bgr, byte>(RootImg.Bitmap);
            CvInvoke.FindContours(tempimg1, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);


            List<double> arealist = new List<double>();
            for (int i = 0; i < contours.Size; i++)
            {
                double areaSize = CvInvoke.ContourArea(contours[i], false);  //  Find the area of contour
                if (areaSize >= dw && areaSize <= up)
                {
                    CvInvoke.DrawContours(colorimg, contours, i, new MCvScalar(14, 200, 40),2);
                }
            }
          
            rtxLog.AppendText("Contour" + Environment.NewLine);
            RegistHisroty(WorkingImg, false);
            imageBox1.Image = colorimg;
            ///





        }

        private void btnNormalize_Click(object sender, EventArgs e)
        {
            var temp = new Image<Gray, byte>(WorkingImg.Data);
            CvInvoke.Normalize(WorkingImg, temp, alpha: 1f/255f,  beta: 255 , normType:NormType.L1);
            rtxLog.AppendText("btnNormalize_Click" + Environment.NewLine);
            RegistHisroty(WorkingImg, false);
            imageBox1.Image = temp;

            //temp.Data.Cast<byte>().Max();
            temp.Data.MaxArray().Print("temp max ");
            temp.Data.MinArray().Print("temp min ");
        }

        private void btnGamma_Click( object sender , EventArgs e )
        {
            WorkingImg._GammaCorrect( (double)nudGamma.Value );
            rtxLog.AppendText( "btnGamma_Click" + Environment.NewLine );
            RegistHisroty( WorkingImg );
        }

        private void btnLevel_Click( object sender , EventArgs e )
        {
            //WorkingImg = WorkingImg.Brightness( ( double ) nudlevel_a .Value , ( double ) nudlevel_b.Value , ( double )nudlevel_s.Value , ( double )nudlevel_E.Value );
            WorkingImg = WorkingImg.Brightness( ( double ) nudlevel_a .Value , ( double ) nudlevel_b.Value );
            rtxLog.AppendText(  $"btnLevel_Click alpha : {( double )nudlevel_a.Value }  ,  beta : {( double )nudlevel_b.Value } " + Environment.NewLine );
            RegistHisroty( WorkingImg );
        }

        private void btnNormalize2_Click( object sender , EventArgs e )
        {
            
            WorkingImg = WorkingImg.Normalize( (byte)nudNormalize.Value );
            rtxLog.AppendText( $"btnNormalize2_Click " + Environment.NewLine );
            RegistHisroty( WorkingImg );
        }

        private void btnNormalize3_Click( object sender , EventArgs e )
        {

            WorkingImg = WorkingImg.Normalize();
            rtxLog.AppendText( $"btnNormalize3_Click " + Environment.NewLine );
            RegistHisroty( WorkingImg );
        }


        private void btnCntrOrig_Click( object sender , EventArgs e )
        {
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            var tempimg1 = WorkingImg.Clone();
            var tempimg2 = RootImg.Clone();
            CvInvoke.FindContours( tempimg1 , contours , null , RetrType.External , ChainApproxMethod.ChainApproxNone );
            Image<Bgr, byte> colorimg = tempimg2.Convert<Bgr, byte>();
            for ( int i = 0 ; i < contours.Size ; i++ )
            {
                CvInvoke.DrawContours( colorimg , contours , i , new MCvScalar( 14 , 250 , 40 ) , 2 );
            }
            rtxLog.AppendText( "Contour" + Environment.NewLine );
            RegistHisroty( WorkingImg , false );
            imageBox1.Image = colorimg;
        }

        bool IsRoot = false;
        private void btnSwitch_Click( object sender , EventArgs e )
        {
            if ( !IsRoot )
            {
                imageBox1.Image = RootImg;
                IsRoot = true;
            } 
            else
            { 
                imageBox1.Image = WorkingImg;
                IsRoot = false;
            }
        }

        private void btnunion_max_Click( object sender , EventArgs e )
        {
            var temp = new Image<Gray, byte>(WorkingImg.Data).Inverse();

            var output = new Image<Gray, byte>(WorkingImg.Data);

            for ( int j = 0 ; j < output.Data.GetLength(0) ; j++ )
            {
                for ( int i = 0 ; i < output.Data.GetLength(1) ; i++ )
                {
                    output.Data [ j , i , 0 ] = Math.Max( temp.Data [ j , i , 0 ] , WorkingImg.Data [ j , i , 0 ] );
                }
            }

            WorkingImg = output;
            rtxLog.AppendText( "btnunion_max_Click" + Environment.NewLine );
            RegistHisroty( WorkingImg , false );
            imageBox1.Image = temp;
        }

        private void btnunion_min_Click( object sender , EventArgs e )
        {
            var temp = new Image<Gray, byte>(WorkingImg.Data).Inverse();

            var output = new Image<Gray, byte>(WorkingImg.Data);

            for ( int j = 0 ; j < output.Data.GetLength( 0 ) ; j++ )
            {
                for ( int i = 0 ; i < output.Data.GetLength( 1 ) ; i++ )
                {
                    output.Data [ j , i , 0 ] = Math.Min( temp.Data [ j , i , 0 ] , WorkingImg.Data [ j , i , 0 ] );
                }
            }

            WorkingImg = output;
            rtxLog.AppendText( "btnunion_max_Click" + Environment.NewLine );
            RegistHisroty( WorkingImg , false );
            imageBox1.Image = temp;
        }

        private void btnInvThres_Click( object sender , EventArgs e )
        {
            WorkingImg = WorkingImg.InvThres(( int )nudINvThres.Value );
            rtxLog.AppendText( "btnInvThres_Click" + Environment.NewLine );
            RegistHisroty( WorkingImg , false );
            imageBox1.Image = WorkingImg;
        }

        private void btnFindFeature_Click( object sender , EventArgs e )
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if ( ofd.ShowDialog() == DialogResult.OK )
            {
                var template = new Image<Gray,byte>(ofd.FileName);




            }


        }

        private void btnLoadColor_Click( object sender , EventArgs e )
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if ( ofd.ShowDialog() == DialogResult.OK )
            {
                Reset();
                ColorMode = true;

                RootImg = new Image<Gray , byte>( ofd.FileName );
                RootImgColor = new Image<Bgr , byte>( ofd.FileName );
                var bdata = RootImgColor.Data.BGRtoGray(0);
                var gdata = RootImgColor.Data.BGRtoGray(1);
                var rdata = RootImgColor.Data.BGRtoGray(2);
                var gimg = new Image<Gray, byte>(gdata);
                var rimg = new Image<Gray, byte>(rdata);
                WorkingImg = gimg + rimg / 2;
                
                rtxLog.AppendText( "Load" + Environment.NewLine );
                imageBox1.Image = WorkingImg;
                History();
            }

        }

        private void btnConv_Click( object sender , EventArgs e )
        {
            float convratio = (float)nudConv.Value;
            var kernel = rtbConv.Text.Split('\n')
                                .Select( f => f.Split(',')
                                                .Select( s => float.Parse(s)/convratio )
                                                .ToArray())
                                .ToArray()
                                .ToMat() ;

            
            

            var fimg = WorkingImg.Convolution( new ConvolutionKernelF( kernel ) );
            WorkingImg = fimg.Convert<Gray , byte>();
            rtxLog.AppendText( "btnConv_Click" + Environment.NewLine );
            RegistHisroty( WorkingImg , false );
            imageBox1.Image = WorkingImg;




        }

        private void btnSetHighpass_Click( object sender , EventArgs e )
        {
            StringBuilder sb = new StringBuilder();

            sb.Append( "-1," );
            sb.Append( "-1," );
            sb.Append( "-1" );
            sb.Append(Environment.NewLine);
            sb.Append( "-1," );
            sb.Append( " 9," );
            sb.Append( "-1" );
            sb.Append( Environment.NewLine );
            sb.Append( "-1," );
            sb.Append( "-1," );
            sb.Append( "-1" );
            rtbConv.Text = sb.ToString();
        }

        private void btnBlur_Click( object sender , EventArgs e )
        {
            WorkingImg = WorkingImg.SmoothGaussian((int)nudBlurKernelSize.Value);
            
            rtxLog.AppendText( "btnBlur_Click" + Environment.NewLine );
            RegistHisroty( WorkingImg , false );
            imageBox1.Image = WorkingImg;
        }

        private void btnSimpleBlu_Click( object sender , EventArgs e )
        {
            WorkingImg = WorkingImg.SmoothBlur( ( int )nudBlurKernelSize.Value , ( int )nudBlurKernelSize.Value );
            rtxLog.AppendText( "btnSimpleBlu_Click" + Environment.NewLine );
            RegistHisroty( WorkingImg , false );
            imageBox1.Image = WorkingImg;
        }


        private void btnHough_Click( object sender , EventArgs e )
        {
            imageBox1.Image = WorkingImg;
            var colimg = WorkingImg.Convert<Bgr , byte>();
            var data = colimg.HoughLines( 
                (int)nudhough1.Value , 
                (int)nudhough2.Value , 
                (int)nudhough3.Value ,
                Math.PI / (int)nudhough4.Value ,
                (int)nudhough5.Value,
                (double)nudhough6.Value ,
                (double)nudhough7.Value);

            LineSegment2D avgLine = new LineSegment2D();
            var flatdata =data.Flatten();

            var selectedlines = flatdata.Where( x => x.P1.X > 50 && x.P1.Y > 50 && x.P2.X > 50 && x.P2.Y > 50 ).Select(x => x).ToArray();

            if ( selectedlines.Count() != 0 )
            {
                var result = selectedlines.Aggregate( ( f , s ) =>
            {
                var p1 = new System.Drawing.Point
                (
                    f.P1.X + s.P1.X,
                    f.P1.Y + s.P1.Y
                );

                var p2 = new System.Drawing.Point
                (
                    f.P2.X + s.P2.X,
                    f.P2.Y + s.P2.Y
                );
                return new LineSegment2D(p1,p2);
            } );

                avgLine = new LineSegment2D(
                    new System.Drawing.Point
                        (
                        result.P1.X / selectedlines.Count() ,
                        result.P1.Y / selectedlines.Count()
                        ) ,
                     new System.Drawing.Point
                        (
                        result.P2.X / selectedlines.Count() ,
                        result.P2.Y / selectedlines.Count()
                        )
                    );

                var selectedline =  avgLine;

                colimg.Draw( selectedline , new Bgr( 100 , 200 , 10 ) , 2 );

                imageBox1.Image = colimg;





                //rtxLog.AppendText( "btnBlur_Click" + Environment.NewLine );
                //RegistHisroty( WorkingImg , false );
                //imageBox1.Image = WorkingImg;
            }
        }

        private void btnApThres_Click( object sender , EventArgs e )
        {
            WorkingImg = WorkingImg.ThresholdAdaptive( 
                new Gray( 255 ) , 
                AdaptiveThresholdType.GaussianC , 
                ThresholdType.Binary , 
                ( int )nudAdpThresBlockSize.Value ,
                new Gray((int)nudAdpSubstractVal.Value) );

            rtxLog.AppendText( "btnApThres_Click" + Environment.NewLine );
            RegistHisroty( WorkingImg , false );
            imageBox1.Image = WorkingImg;
        }

        private void btnAdpThres2Zero_Click( object sender , EventArgs e )
        {
            WorkingImg = WorkingImg.ThresholdAdaptive(
               new Gray( 255 ) ,
               AdaptiveThresholdType.GaussianC ,
               ThresholdType.ToZero ,
               ( int )nudAdpThresBlockSize.Value ,
               new Gray( ( int )nudAdpSubstractVal.Value ) );

            rtxLog.AppendText( "btnAdpThres2Zero_Click" + Environment.NewLine );
            RegistHisroty( WorkingImg , false );
            imageBox1.Image = WorkingImg;
        }

		private void btnLaplace_Click( object sender , EventArgs e )
		{
			WorkingImg = WorkingImg.Laplace( ( int )nudLapla.Value ).Convert<Gray , byte>();
			rtxLog.AppendText( "btnLaplace_Click" + Environment.NewLine );
			RegistHisroty( WorkingImg , false );
			imageBox1.Image = WorkingImg;
		}

		private void btnOpenResizer_Click( object sender , EventArgs e )
		{
			new Resizer().ShowDialog();
		}

		private void btnBackRemover_Click( object sender , EventArgs e )
		{
			new BackGroundCorrection().ShowDialog();
		}
	}
	public static class ExtensionP
    {
        public static TResult Measure<TSource, TResult>(
            this TSource @this,
            string msg,
            Func<TSource, TResult> fn)
        {
            Stopwatch stw = new Stopwatch();
            stw.Start();
            for (int i = 0; i < 10; i++)
            {
                fn(@this);
            }
            stw.Stop();
            Console.WriteLine($"{stw.ElapsedMilliseconds / 1.0}" + msg);
            return fn(@this);
        }
    }

  
}

