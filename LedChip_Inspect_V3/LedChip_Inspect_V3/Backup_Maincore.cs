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
    partial class MainCore
    {
        public Action<Image<Gray , byte> , Image<Bgr , byte>> ProcessingStep1_alt_bk (
            int threshold ,
            SampleType sampletype ,
            int cHnum ,
            int cWnum ,
            bool debugmode = false)
        {
            return new Action<Image<Gray , byte> , Image<Bgr , byte>>( (baseimg , colorimg) =>
            {
                try
                {
                    Stopwatch stw = new Stopwatch();
                    stw.Start();
                    var color_visual_img = colorimg.Clone(); 
                    PResult = new ImgPResult(
                         PData.UPAreaLimit ,
                         PData.DWAreaLimit ,
                         PData.IntenSumUPLimit ,
                         PData.IntenSumDWLimit

                        );
                    VectorOfVectorOfPoint contours;
                    if ( debugmode )
                    {
                        contours = baseimg
                                     .Map( img => Proc_Method_List[sampletype]( img ) )
                                     .Act( img => img.Save( TestFileSavePath.BasePath + "\\beforcntr.bmp" ) )
                                     .Map( img => FindContour( img ) )
                                     .Map( cntr => Sortcontours( cntr ) );
                    }
                    else
                    {
                        contours = baseimg
                                     .Map( img => Proc_Method_List[sampletype]( img ) )
                                     .Map( img => FindContour( img ) )
                                     .Map( cntr => Sortcontours( cntr ) );
                    }
                    var centerMoment = contours
                                        .Map(cntr => CalcCenter(cntr));

                    var boxlist = contours
                                    .Map( cntr => ApplyBox(cntr));

                    byte[,,]   indexingImage   = MatPattern( cHnum, cWnum , 3);
                    byte[,,]   passfailPosData = new byte[ cHnum, cWnum , 1];
                    double[,,] estedChipP      = EstedChipPos( cHnum, cWnum )
                                                 .Act( est => DrawCenterPoint(color_visual_img , est) )
                                                 .Act_LoopChipPos( boxlist 
                                                                   , centerMoment
                                                                   , CheckOkNg_SizeInInten(
                                                                       indexingImage 
                                                                       , contours
                                                                       , color_visual_img
                                                                       , ref PResult )
																   , isParallel: false);

                    var centers =  (ClusterData( 
                        ( from item in PResult.OutData
                            select new double[] { item.Intensity } )
                            .ToArray<double[]>())["center"] as  double[][])
                        .Select(x => x[0])
                        .OrderBy( x => x)
                        .ToArray();

                    PResult.OutData
                   .Act( CheckLowOver(
                          estedChipP
                          , indexingImage
                          , color_visual_img
                          , centers
                          , LineThickness) );

                    UpdateResult( PResult )( indexingImage , color_visual_img );
                    stw.Stop();
                    Console.WriteLine( "Process Time : " + stw.ElapsedMilliseconds );
                }
                catch ( Exception er )
                {
                    System.Windows.Forms.MessageBox.Show( er.ToString() );
                    evtProcessingDone( true );
                }
            } );
        }

        Action<byte[,,] , Image<Bgr , byte>> UpdateResult_bk (ImgPResult processResult)
        {
            return new Action<byte[,,] , Image<Bgr , byte>>(
            (indeximg , procedimg) =>
            {
                IndexViewImg.Data = indeximg;
                ProcedImg         = procedimg.Clone();
                

                processResult.ChipPassCount = ( from item in processResult.OutData where item.OKNG == "OK"   select item ).Count();
                processResult.ChipNOPLCount   = ( from item in processResult.OutData where item.OKNG == "NOPL"   select item ).Count();
                processResult.ChipLowCount  = ( from item in processResult.OutData where item.OKNG == "LOW"  select item ).Count();
                processResult.ChipOverCount = ( from item in processResult.OutData where item.OKNG == "OVER" select item ).Count();

                evtIntenAvg( ( int ) ( processResult.OutData.Select( (x) => x.Intensity ).ToList().Average() ) );
                evtAreaAvg( ( int ) ( processResult.OutData.Select( (x) => x.ContourSize ).ToList().Average() ) );
                evtProcessingDone( true );
            } );
        }


        Action<int , int , double , double , List<System.Drawing.Rectangle> , System.Drawing.Point[]> CheckOkNg_bk (
            byte[,,] indexingImage
            , VectorOfVectorOfPoint contours
            , Image<Bgr,byte> color_visual_img
            , ref ImgPResult Presult)
        {
            return  (j , i , yps , xps , boxlist, centerlist) =>
                    {
                        bool isFail = true;
                        for ( int k = 0 ; k < boxlist.Count ; k++ )
                        {
                            /* Check Ested Chip Pos in Contour*/
                            Create_Inbox( boxlist[k] , APBoxTolerance );
                            if ( InBox( yps , xps ) )
                            {
                                PResult.OutData.Add(
                                        new ExResult(
                                            j , i
                                            , (int)(int)yps - (int)centerlist[k].Y 
                                            , (int)(int)xps - (int)centerlist[k].X 
                                            , "OK"
                                            , SumInsideBox( boxlist[k] )
                                            , CvInvoke.ContourArea( contours[k] )
                                            , boxlist[k] ) );
                                isFail = false;
                                color_visual_img.Draw( boxlist[k] , ApOkChipColor , 1 );
                                CircleF cirp = new CircleF(new System.Drawing.PointF( (float)centerlist[k].X  , (float)centerlist[k].Y  ), 0);
                                color_visual_img.Draw( cirp , ApCenteBoxColor , 1 );
                                break;
                            }
                        }
                        if ( isFail )
                        {
                            double failboxInten = SumAreaPoint( (int)yps ,  (int)xps);
                            PResult.OutData.Add(
                                    new ExResult(
                                        j , i
                                        , 0
                                        , 0
                                        , "NOPL"
                                        , failboxInten
                                        , 0 ) );
                            SetFailColor( indexingImage , j , i );
                        }
                    };
        }

        Action<List<ExResult>> CheckLowOver_bk (
            double[,,] estedChipP,
            byte[,,] indexingImage,
            Image<Bgr,byte> targetimg,
            double[] center )
        {
            return new Action<List<ExResult>>( (list) =>
            {

                foreach ( var item in list )
                {
                    if ( item.OKNG == "OK" )
                    {
                        int xs = (int)estedChipP[ item.Hindex , item.Windex , 1] - 3;
                        int ys = (int)estedChipP[ item.Hindex , item.Windex , 0] - 3;

                        //if ( item.Intensity < 48000 && item.Intensity > 1 ) Console.WriteLine( item.Intensity );
                        if ( item.Intensity < PData.IntenSumDWLimit )
                        {
                            //Console.WriteLine( item.Intensity );
                            item.OKNG = "LOW";
                            SetLowColor( indexingImage , item.Hindex , item.Windex );
                            targetimg.Draw( item.BoxData.ExpendRect( 4 ) , ApLowColor , 3 );
                        }
                        else if ( item.Intensity > PData.IntenSumUPLimit )
                        {
                            item.OKNG = "OVER";
                            SetOverColor( indexingImage , item.Hindex , item.Windex );
                            targetimg.Draw( item.BoxData.ExpendRect( 4 ) , ApOverColor , 3 );
                        }
                    }
                }
            } );
        }
    }

     /*
        public Action<Image<Gray , byte> , Image<Bgr , byte>> ProcessingStep1_alt3 (
            int threshold ,
            SampleType sampletype ,
            int cHnum ,
            int cWnum ,
            bool debugmode = true)
        {
            return new Action<Image<Gray , byte> , Image<Bgr , byte>>( (baseimg , colorimg) =>
            {
                try
                {
                    var color_visual_img = colorimg.Clone();
                    PResult = new ImgPResult();
                    VectorOfVectorOfPoint contours;
                    if ( debugmode )
                    {
                        contours = baseimg
                                     .Map( img => Proc_Method_List[sampletype]( img ) )
                                     //.Act( img => img.Save( TestFileSavePath.BasePath + "\\beforcntr.bmp" ) )
                                     //.Map( img => FindContour( img ) )
                                     .Measure_Map( "FindContour x 10" , 10 , img => FindContour( img ) )
                                     //.Map( cntr => Sortcontours( cntr ) );
                                     .Measure_Map( "sort x 100" , 100 , cntr => Sortcontours( cntr ) );
                    }
                    else
                    {
                        contours = baseimg
                                     .Map( img => Proc_Method_List[sampletype]( img ) )
                                     .Map( img => FindContour( img ) )
                                     .Map( cntr => Sortcontours( cntr ) );
                    }

                    var boxlist = contours
                                    //.Map( cc);
                                    .Measure_Map(" appplybox x 100 " , 100 , cntr => ApplyBox(cntr));


                    Stopwatch sttw = new Stopwatch();
                    sttw.Start();
                    byte[,,]   indexingImage   = MatPattern( cHnum, cWnum , 3);
                    sttw.Stop();
                    Console.WriteLine( "IndexImage Time : " + sttw.ElapsedMilliseconds );
                    sttw.Reset();

                    //byte[,,]   passfailPosData = new byte[ cHnum, cWnum , 1];
                    //double[,,] estedChipP      = EstedChipPos( cHnum, cWnum )
                    //                             .Act( est => DrawCenterPoint(color_visual_img , est) )
                    //                             .Act_LoopChipPos( boxlist 
                    //                                               , CheckOkNg(
                    //                                                   indexingImage 
                    //                                                   , contours
                    //                                                   , color_visual_img
                    //                                                   , ref PResult ));
                    //

                    sttw.Start();
                    //byte[,,]   passfailPosData = new byte[ cHnum, cWnum , 1];
                    double[,,] estedChipP      = EstedChipPos( cHnum, cWnum );
                    sttw.Stop();
                    Console.WriteLine( "estedChipP Time : " + sttw.ElapsedMilliseconds );
                    sttw.Reset();


                    estedChipP.Measure_Act( "DrawCEnterPoint x 10 " , 10 , est => DrawCenterPoint( color_visual_img , est ) );

                    sttw.Start();
                    estedChipP.Act_LoopChipPos( boxlist
                                                , CheckOkNg(
                                                    indexingImage
                                                    , contours
                                                    , color_visual_img
                                                    , ref PResult ) );
                    sttw.Stop();
                    Console.WriteLine( "OKNG Time : " + sttw.ElapsedMilliseconds );
                    sttw.Reset();


                    sttw.Start();
                    var centers =  (ClusterData(
                        ( from item in PResult.OutData
                          select new double[] { item.Intensity } )
                            .ToArray<double[]>())["center"] as  double[][])
                        .Select(x => x[0])
                        .OrderBy( x => x)
                        .ToArray();
                    sttw.Stop();
                    Console.WriteLine( "Over Low Time : " + sttw.ElapsedMilliseconds );
                    sttw.Reset();



                    sttw.Start();
                    PResult.OutData
                   .Act( CheckLowOver(
                          estedChipP
                          , indexingImage
                          , color_visual_img
                          , centers ) );
                    sttw.Stop();
                    Console.WriteLine( "Over Low Time : " + sttw.ElapsedMilliseconds );
                    sttw.Reset();

                    sttw.Start();
                    UpdateResult( PResult )( indexingImage , color_visual_img );
                    sttw.Stop();
                    Console.WriteLine( "Update Time : " + sttw.ElapsedMilliseconds );
                    sttw.Reset();
                }
                catch ( Exception er )
                {
                    System.Windows.Forms.MessageBox.Show( er.ToString() );
                    evtProcessingDone( true );
                }
            } );
        }
        */
    
}
