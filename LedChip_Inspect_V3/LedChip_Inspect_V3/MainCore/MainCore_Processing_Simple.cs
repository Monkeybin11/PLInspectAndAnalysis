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
using WaferandChipProcessing.Func;
using System.Windows.Media;
using System.Windows.Controls;
using System.IO;
using static EmguCV_Extension.Vision_Tool;
using static EmguCV_Extension.Preprocessing;
using static Util_Tool.UI.Corrdinate;
using System.Diagnostics;


namespace WaferandChipProcessing
{

    public partial class MainCore
    {
        public Action<Image<Gray, byte>, Image<Bgr, byte>> ProcessingStep1_Simple(
            SampleType sampletype,
            int cHnum,
            int cWnum,
            int estWSize,
            int estHSize)
        {
            return new Action<Image<Gray, byte>, Image<Bgr, byte>>((baseimg, colorimg) =>
        {
            try
            {
                Stopwatch stw = new Stopwatch();
                stw.Start();

                var color_visual_img = colorimg.Clone();
                var color_visual_img2 = colorimg.Clone();
                PResult = new ImgPResult(
                    PData.UPAreaLimit,
                    PData.DWAreaLimit,
                    PData.IntenSumUPLimit,
                    PData.IntenSumDWLimit
                );
                
                byte[,,] indexingImage = MatPattern(cHnum, cWnum, 3);
                byte[,,] passfailPosData = new byte[cHnum, cWnum, 1];
                double[,,] estedChipP= EstedChipPos(cHnum, cWnum);
               
                var boxlist = estedChipP.GetRectList(estHSize,estWSize,color_visual_img.Height,color_visual_img.Width);
                var centerMoment = estedChipP.GetMomnetList(); //  모멘트 및 박스 리스트 구함. 

                estedChipP.Act_LoopChipPos(
                                boxlist
                                , centerMoment
                                , CheckOkNg_Inten(
                                    indexingImage
                                    , color_visual_img
                                    , ref PResult));

                var clusterCenters = (ClusterData(
                    (from item in PResult.OutData
                     select new double[] { item.Intensity })
                      .ToArray<double[]>())["center"] as double[][])
                    .Select(x => x[0])
                    .OrderBy(x => x)
                    .ToArray(); // Background Intensity

                var updw = PResult.OutData
                                    .Select(x => x.Intensity)
                                    .ToArray<double>()
                                    .Map(intesns => FindIntensityUpDw(intesns));


                PResult.OutData.Act(CheckLowOver(
                                          estedChipP
                                          , indexingImage
                                          , color_visual_img
                                          , clusterCenters
                                          , LineThickness));

                DrawCenterPoint(color_visual_img, estedChipP);
                UpdateResult(PResult)(indexingImage, color_visual_img);
                evtProcessingResult();
                stw.Stop();
                Console.WriteLine("Process Time : " + stw.ElapsedMilliseconds);
            }
            catch (Exception er)
            {
                System.Windows.MessageBox.Show(er.ToString());
                evtProcessingDone(true);
            }
        });
        }

        Action<int, int, double, double, List<System.Drawing.Rectangle>, System.Drawing.Point[]> CheckOkNg_Inten(
            byte[,,] indexingImage
            , Image<Bgr, byte> color_visual_img
            , ref ImgPResult Presult)
        {
            return (j, i, yps, xps, boxlist, centerlist) =>
                   {
                       bool isFail = true;
                       for (int k = 0; k < boxlist.Count; k++)
                       {
                           /* Check Ested Chip Pos in Contour*/
                           Create_Inbox(boxlist[k], APBoxTolerance );
                           if (InBox(yps, xps))
                           {
                               PResult.OutData.Add(
                                       new ExResult(
                                           j, i
                                           , 0
                                           , 0
                                           , "OK"
                                           , SumInsideBox(boxlist[k])
                                           , boxlist[k].Width * boxlist[k].Height
                                           , boxlist[k]));
                               isFail = false;
                               color_visual_img.Draw(boxlist[k], ApOkChipColor );

                               var cirp = new CircleF();
                               cirp.Center = new System.Drawing.PointF(
                                                            (float)(boxlist[k].X + boxlist[k].Width / 2)
                                                            , (float)(boxlist[k].Y + boxlist[k].Height / 2));

                               color_visual_img.Draw(
                                   cirp
                                   , ApCenteBoxColor , 1);


                               break;
                           }
                       }
                       if (isFail)
                       {
                           double failboxInten = SumAreaPoint((int)yps, (int)xps);
                           PResult.OutData.Add(
                                   new ExResult(
                                       j, i
                                       , 0
                                       , 0
                                       , "NOPL"
                                       , failboxInten
                                       , 0));
                           SetFailColor(indexingImage, j, i);
                       }
                   };
        }



    }
}
