using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MahApps.Metro;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using Emgu.CV.Util;
using WaferandChipProcessing.Data;
using WaferandChipProcessing.Func;
using System.Diagnostics;
using Accord.Math.Metrics;
using Microsoft.VisualStudio.DebuggerVisualizers;
using System.IO;
using LedChipPassFail_first;
using EmguCV_Extension;
using SpeedyCoding;

namespace WaferandChipProcessing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        MainCore Core;
        EpiCore CoreEpi;
        HistogramBox HistoBox;
        DenseHistogram[] HistogramList;
        WindowsFormsHost WinHost;
        Stopwatch timer = new Stopwatch();

        ImageBrush[] CornerImgGroup;
        Canvas[] CornerCanvsGroup   ;
        Border[] CornerBorderGroup  ;
        Canvas[] EpiCanvasGroup ;
        ImageBrush[] EpiImgBrushGroup;

        Func<Image<Gray , byte> , Image<Gray , byte>>[] CornerImgCrops;
        Action<object, MouseButtonEventArgs>[] CornerClickEvt;
        Dictionary<ImgIdxPos,System.Windows.Controls.Button> ZoomClickBtn;
        Dictionary<ImgIdxPos,MouseButtonEventHandler> ZoomClickBtnMethod;

        public readonly int BoxLimtiAreaUP = 20;
        public readonly int BoxLimtiAreaDW = 3;
        bool ProcessDone = false;

        public MainWindow()
        {
            InitializeComponent();
            ControlGrouping();
            Core = new MainCore()
                .Act(core =>
                {
                    core.evtIntenAvg += new TrsInt(DisplayIntenAvg);
                    core.evtAreaAvg += new TrsInt(DisplayAreaAvg);
                    core.evtProcessingDone += new TrsBool(DisplayProcessingResult);
                    core.evtProcessingResult += new Trsvoid(() =>
                    {
                        this.BeginInvoke(() =>
                        {
                            lblTotalChip.Content   = core.PResult.ChipTotalCount;
                            lblPassChipnum.Content = core.PResult.ChipPassCount;
                            lblLowChipnum.Content  = core.PResult.ChipLowCount;
                            lblOverChipnum.Content = core.PResult.ChipOverCount;
                            lblFailChipnum.Content = core.PResult.ChipNOPLCount;
                        });
                    });
                })
                .Act(core => InitDisplay(core)); //dependancy

            InitCoreEpi();
            RegisterZoomGrid( gridepiIdx , ZoomClickBtn , ZoomClickBtnMethod );


        }

        void InitCoreEpi() {
            CoreEpi = new EpiCore( 7500 );


            // Regist Event . Event Flow : Drop => Run Event Method on EpiCore.Epi_Event[i].DropEventMethod 
            //                                                -> evtDropDone Triggerd :CoreEpi.SetImage Run
            CoreEpi.Act( core =>
            {
                for ( int i = 0 ; i < core.EpiSeperatedImgTrsEvt.Count() ; i++ )
                {
                    EpiCanvasGroup [ i ].Drop
                            += new System.Windows.DragEventHandler(
                                core.EpiSeperatedImgTrsEvt [ i ]
                                         .Act( ths => ths.evtDropDone
                                             += new DropDone( CoreEpi.SetImage ) )
                                    .DropEventMethod );
                }
            } )
                 .Act( core => core.evtDroppedImg += new ImgForDisplay( EpiDisplayOrigImg ) )
                 .Act( core => core.evtTrsFullImg += new TrsFullImage( EvtTransfullEpiImg ) )
                 .Act( core => core.evtTrsResizedProcedImg += new TrsProcedImage( EpiDisplayProcedImg ) )
                 .Act( core => core.evtTrsIdxImg += new TrsProcedImage( EpiDisplayDefcetIdxImg ) )
                 //.Act( core => core.evtProgressTime += new TrsProgress( EpiProgress ) )
                 .Act( core => core.evtStatistic += new TrsStatistic( EpiDisplayStatisticResult ) )
                 .Act( core => core.evtProcTime += new TrsProgress( EpiDisplayProcTime ) )
                 .Act( core => imgEpIndex.ImageSource = core.IndexViewImg.ToBitmapSource() );
        }

        #region Epi Event
        void EvtTransfullEpiImg(Tuple<ImgIdxPos, Image<Gray, byte>>[] zippedlist)
        {
            this.Dispatcher.Invoke( ( Action )( () => zippedlist.ActLoop( ths => EpiDisplayOrigImg(ths.Item1, ths.Item2))));
        }

        void EpiDisplayProcedImg( Image<Bgr , byte> procedidximg )
        {
            this.Dispatcher.Invoke( ( Action )( () => imgEpProced.ImageSource = procedidximg.ToBitmapSource()));
        }
      
        

        void EpiDisplayDefcetIdxImg( Image<Bgr , byte> procedIdxImg )
        {
            this.Dispatcher.Invoke( ( Action )( () => imgEpIndex.ImageSource = procedIdxImg.ToBitmapSource()));
            //imboxEmgu.Image = procedIdxImg;
            //imboxEmgu.Refresh();
        }
        void EpiProgress( int progress )
        {
            //this.Dispatcher.Invoke( ( Action )( () => lblprgEpi.Content = progress.ToString() + " %" ) );
        }

        void EpiDisplayProcTime( int procTime )
        {
            this.Dispatcher.Invoke( ( Action )( () => lblProcTime.Content = procTime.ToString() + " (um)" ) );
        }


        void EpiDisplayStatisticResult( int[] statisticResult )
        {
            this.Dispatcher.Invoke( ( Action )( () =>
            {
                lblSize1.Content = statisticResult [ 0 ].ToString();
                lblSize2.Content = statisticResult [ 1 ].ToString();
                lblSize3.Content = statisticResult [ 2 ].ToString();
                lblSize4.Content = statisticResult [ 3 ].ToString();
            } ) );
        }


        #endregion


        void ControlGrouping()
        {
            CornerImgGroup     = new ImageBrush[4] { imgLT, imgLB, imgRT, imgRB };
            CornerCanvsGroup   = new Canvas[4] { canvasLT, canvasLB, canvasRT, canvasRB };
            CornerBorderGroup  = new Border[4] { borderLT, borderLB, borderRT, borderRB };
            EpiCanvasGroup     = new Canvas[6] {
                                          canvasEpOriginTL
                                          , canvasEpOriginTM
                                          , canvasEpOriginTR
                                          , canvasEpOriginBL
                                          , canvasEpOriginBM
                                          , canvasEpOriginBR };

            EpiImgBrushGroup = new ImageBrush[6] {
                                          imgEpOriginTL
                                          , imgEpOriginTM
                                          , imgEpOriginTR
                                          , imgEpOriginBL
                                          , imgEpOriginBM
                                          , imgEpOriginBR };
        }

        void EpiDisplayOrigImg(ImgIdxPos pos, Image<Gray,byte> img)
        {
            this.BeginInvoke( ( Action )( () =>
            {
                var src  = img.Resize(0.1,Inter.Cubic).Normalize(64).Gamma(1.4).ToBitmapSource();
                switch ( pos )
                {
                    case ImgIdxPos.TL:
                        imgEpOriginTL.ImageSource = src;
                        break;
                    case ImgIdxPos.TM:
                        imgEpOriginTM.ImageSource = src;
                        break;
                    case ImgIdxPos.TR:
                        imgEpOriginTR.ImageSource = src;
                        break;
                    case ImgIdxPos.BL:
                        imgEpOriginBL.ImageSource = src;
                        break;
                    case ImgIdxPos.BM:
                        imgEpOriginBM.ImageSource = src;
                        break;
                    case ImgIdxPos.BR:
                        imgEpOriginBR.ImageSource = src;
                        break;
                }
            } ) );
        }

        void DisplayProcessingResult (bool istrue) {
            this.BeginInvoke( ( Action )( () =>
            {
                if ( istrue )
                {
                    timer.Stop();
                    this.BeginInvoke( () => lblRunningTime.Content = timer.ElapsedMilliseconds / 1000 );
                }
            } ) );
        }

        void ClearLRFrame()
        {
            canvasLT.Children.Clear();
            canvasLB.Children.Clear();
            canvasRT.Children.Clear();
            canvasRB.Children.Clear();
            canvasProced.Children.Clear();
            canvasIndex.Visibility = Visibility.Hidden;
            borderIndex.Visibility = Visibility.Hidden;
            OpenCornerImg( CornerCanvsGroup );
        }

        void SetInitImg(ImageBrush[] cornerImg ,Canvas oriCanvas , Canvas Pro , Canvas[] cornerCanv)
        {
            List<Rectangle> rectList = new List<Rectangle>();

            /*Canvas Setting*/
            while ( oriCanvas.Children.Count > 0 )
            {
                oriCanvas.Children.Clear();
            }
            double[] canvXYLen = Core.MapImg2Canv( new double[2] { Core.LTRBPixelNumberH , Core.LTRBPixelNumberW} );
            Core.SetCornerRect( oriCanvas , canvXYLen[0] , canvXYLen[1] );
            RenderOptions.SetBitmapScalingMode( Pro , BitmapScalingMode.NearestNeighbor );

            for ( int i = 0 ; i < 4 ; i++ )
            {
                RenderOptions.SetBitmapScalingMode( cornerCanv[i] , BitmapScalingMode.HighQuality );
                //var temp =  CornerImgCrops[i]( Core.OriginImg );
                cornerImg[i].ImageSource = BitmapSrcConvert.ToBitmapSource( CornerImgCrops[i]( Core.OriginImg ) );
                cornerCanv[i].MouseLeftButtonUp += new MouseButtonEventHandler( CornerClickEvt[i] );
            }
            imgOri.ImageSource = BitmapSrcConvert.ToBitmapSource( Core.OriginImg );
        }

   

        // load -<> ready -> start
        #region Process Ready
        bool ReadyProc()
        {
            foreach ( var pos in Core.PData.CornerPos_Img )
            {
                if ( pos == null) 
                {
                    System.Windows.Forms.MessageBox.Show( "Set First and Last Chip Position First" );
                    Mouse.OverrideCursor = null;
                    return false;
                }
            }

            SetProcessingData();
            ChangeFront2ImgProcStep();
            CreateFuncofProc();
            HideCornerImg( CornerCanvsGroup );
            canvasIndex.Visibility = Visibility.Visible;
            borderIndex.Visibility = Visibility.Visible;
            return true;
        }

        void HideCornerImg(Canvas[] canvases) {
            for ( int i = 0 ; i < 4 ; i++ )
            {
                canvases[i].Visibility = Visibility.Hidden;
                CornerBorderGroup[i].Visibility = Visibility.Hidden;
            }
        }

        void OpenCornerImg( Canvas[] canvases ) {
            for ( int i = 0 ; i < 4 ; i++ )
            {
                canvases[i].Visibility = Visibility.Visible;
                CornerBorderGroup[i].Visibility = Visibility.Visible;
            }
        }
        
        void SetProcessingData()
        {
            Core.PData.ImgRealH = Core.OriginImg.Height;
            Core.PData.ImgRealW = Core.OriginImg.Width;
            Core.PData.CanvasH = ( int ) canvas.ActualHeight;
            Core.PData.CanvasW = ( int ) canvas.ActualWidth;

            Core.PData.ChipWNum = ( int ) nudCWNum.Value;
            Core.PData.ChipHNum = ( int ) nudCHNum.Value;

            Core.PData.ThresholdV = ( int ) nudThresh.Value;
            Core.PData.UPAreaLimit = ( int ) ( nudAreaUpLimit.Value );
            Core.PData.DWAreaLimit = ( int ) ( nudAreaDWLimit.Value );

            Core.PData.IntenSumUPLimit = ( int ) ( nudIntenSumUPLimit.Value );
            Core.PData.IntenSumDWLimit = ( int ) ( nudIntenSumDWLimit.Value );

            Core.LineThickness = (int)(nudThickness.Value);
        }
        void ChangeFront2ImgProcStep()
        {
            btnStartProcssing.IsEnabled = true;
            Removeevent( CornerCanvsGroup );
            ClearLRFrame();
            while ( canvas.Children.Count > 0 ) { canvas.Children.RemoveAt( canvas.Children.Count - 1 ); } // delect rect
            titleRB.Text = "Histogram";
            titleLT.Text = "Indexing View";

            if ( Core.ChipPosMode == AdvancedChipPos.None )
            {
                Core.CreateEstedChipFunc( Core.PData.CornerPos_Img , ckbEst4Pos.IsChecked.Value ?		 EstChipPosMode.With4Point : 
																	 ckbEst4Pos_Rumbus.IsChecked.Value ? EstChipPosMode.With4PointLineEquation :
																										 EstChipPosMode.With2Point );
            }
            else
            {
                Core.CreateEstedChipFunc( Core.PData.AdvHChipPos , Core.PData.CornerPos_Img , EstChipPosMode.WithPatternPoint );
            }

            Core.IndexViewImg = new Image<Bgr , byte>( Core.PData.ChipWNum , Core.PData.ChipHNum );
            Core.IndexViewImg.Data = MatPattern( Core.PData.ChipHNum , Core.PData.ChipWNum , 3 );
            imgIndex.ImageSource = BitmapSrcConvert.ToBitmapSource( Core.IndexViewImg );
            imgRB.ImageSource = null;

            WinHost = CreateWinHost(canvasLT);
            //HistoBox = new HistogramBox();

            canvasRB.Children.Clear();
            //AddHist2Box( HistoBox , ref HistogramList, HistoFromImage( Core.OriginImg , Core.BinSize ) ,
            //                 ( bool ) ckbSetHistRange.IsChecked ? float.Parse( nudHistDW.Text ) : 0 ,
            //                 ( bool ) ckbSetHistRange.IsChecked ? float.Parse( nudHistUP.Text ) : 255 );
            //HistoBox.Refresh();
            WinHost.Child = HistoBox;
            canvasRB.Children.Add( WinHost );
        }
        void CreateFuncofProc()
        {
            ThresholdMode mode = ckbThresMode.IsChecked.Value ? ThresholdMode.Auto : ThresholdMode.Manual;
            Core.CreateProcFun( mode );
        }
        
        System.Drawing.Rectangle CenterDotForDrawing( double px , double py )
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle();
            rect.Width = 2;
            rect.Height = 2;
            return rect;
        }
        #endregion

        #region After Setting Function

        Image<Bgr,byte> DrawContour(Image<Bgr,byte> img,VectorOfVectorOfPoint contr) {
            for ( int i = 0 ; i < contr.Size ; i++ )
            {
                CvInvoke.DrawContours( img , contr , i , new MCvScalar( 0 , 255 , 0 ) );
            }
            return img;
        }

        Image<Bgr , byte> DrawCenterPoint( Image<Bgr , byte> img , double[,,] centrPoint )
        {
            Parallel.For( 0 , centrPoint.GetLength( 0 ) , i =>
            {
                Parallel.For( 0 , centrPoint.GetLength( 1 ) , j =>
                {
                    img.Data[( int ) centrPoint[i , j , 0] , ( int ) centrPoint[i , j , 1] , 0] = 0;
                    img.Data[( int ) centrPoint[i , j , 0] , ( int ) centrPoint[i , j , 1] , 1] = 0;
                    img.Data[( int ) centrPoint[i , j , 0] , ( int ) centrPoint[i , j , 1] , 2] = 255;
                } );
            } );
            return img;
        }

        Image<Bgr , byte> DrawBox( Image<Bgr , byte> img , List<System.Drawing.Rectangle> rclist )
        {
            Parallel.For( 0 , rclist.Count , i =>
            {
                img.Draw( rclist[i] , new Bgr( 40 , 165 , 5 ) , 1 );
            } );
            return img;
        }

        void Removeevent(Canvas[] canvases )
        {
            for ( int i = 0 ; i < 4 ; i++ )
            {
                canvases[i].MouseLeftButtonUp -= new MouseButtonEventHandler( CornerClickEvt[i] );
            }
        }

        byte[,,] MatZeros( int channal1 , int channal2 , int channal3 )
        {
            byte[,,] output = new byte[channal1,channal2,channal3];
            for ( int i = 0 ; i < channal1 ; i++ )
            {
                for ( int j = 0 ; j < channal2 ; j++ )
                {
                    for ( int k = 0 ; k < channal3 ; k++ )
                    {
                        output[i , j , k] = 150;
                    }
                }
            }
            return output;
        }

        byte[,,] MatPattern( int channal1 , int channal2 , int channal3 )
        {
            byte[,,] output = new byte[channal1,channal2,channal3];

            Parallel.For( 0 , channal1 , i => {
                Parallel.For( 0 , channal2 , j => {

                    if ( i % 2 == 0 ) {
                        if ( j % 2 == 0 )
                        {
                            output[i , j , 0] = 250;
                            output[i , j , 1] = 250;
                            output[i , j , 2] = 250;
                        }
                        else
                        {
                            output[i , j , 0] = 150;
                            output[i , j , 1] = 150;
                            output[i , j , 2] = 150;
                        }
                    }
                    else if ( j%2 == 0) {
                        output[i , j , 0] = 200;
                        output[i , j , 1] = 200;
                        output[i , j , 2] = 200;
                    }
                    else
                    {
                        output[i , j , 0] = 100;
                        output[i , j , 1] = 100;
                        output[i , j , 2] = 100;
                    }

                } );
            } );

            
            return output;
        }
        #endregion

        #region Histogram

        private void ckbSetHistRange_Checked(object sender, RoutedEventArgs e)
        {
            RefreshHistogram();
        }

        private void ckbSetHistRange_Unchecked(object sender, RoutedEventArgs e)
        {

            RefreshHistogram();
        }

        void RefreshHistogram()
        {
            try
            {
                if (!ProcessDone && HistogramList != null)
                {
                    HistoBox.ClearHistogram();
                    AddHist2Box(HistoBox, ref HistogramList, HistoFromImage(Core.OriginImg, Core.BinSize),
                            (bool)ckbSetHistRange.IsChecked ? float.Parse(nudHistDW.Text) : 0,
                            (bool)ckbSetHistRange.IsChecked ? float.Parse(nudHistUP.Text) : 255);
                }
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("Please Input only Number on Histogram Range");
            }
        }

        void DisplayResultHisto(ImgPResult data)
        {
            HistoBox.ClearHistogram();
            HistoBox.Name = "AreaSize";
            AddHist2Box( HistoBox , ref HistogramList, HistoFromResult( data ) ,
                             ( bool ) ckbSetHistRange.IsChecked ? float.Parse( nudHistDW.Text ) : 0 ,
                             ( bool ) ckbSetHistRange.IsChecked ? float.Parse( nudHistUP.Text ) : 255 );
            HistoBox.Refresh();
            WinHost.Child = HistoBox;
            //canvasHist.Children.Clear();
            //canvasHist.Children.Add( WinHost );
            
        }

        void AddHist2Box( HistogramBox box , ref DenseHistogram[] histogramArr,dynamic createhist, float dw, float up)
        {
            histogramArr = createhist( dw , up );
            for ( int i = 0 ; i < histogramArr.GetLength(0) ; i++ )
            {
                if ( histogramArr[i] != null )
                {
                    box.AddHistogram( null , System.Drawing.Color.Black , histogramArr[i] , Core.BinSize , new float[] { dw , up } );
                }
            }
        }
        #endregion

        #region Helper
        WindowsFormsHost CreateWinHost( Canvas targcanv )
        {
            WindowsFormsHost wh = new WindowsFormsHost();
            wh.Width = targcanv.Width;
            wh.Height = targcanv.Height;
            wh.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            wh.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            return wh;
        }

        DenseHistogram CreateHisto( ImgPResult result , Func<List<ExResult> , int[]> func )
        {
            List<int> temp = new List<int>();
            var item = func(result.OutData);

            DenseHistogram hist = new DenseHistogram(20,new RangeF((float)item.Min(),(float)item.Max()));
            Matrix<float> farr = new Matrix<float>(1,item.GetLength(0));
            for ( int i = 0 ; i < item.GetLength( 0 ) ; i++ )
            {
                farr.Data[0 , i] = item[i];
            }
            Matrix<float>[] histData = new Matrix<float>[1] { farr }; // Histogram data is Matrix<float>
            hist.Calculate( histData , true , null );
            return hist;
        }

        Func< float , float , DenseHistogram[]> HistoFromImage( Image<Gray , byte> img , int binsize )
        {
            var fromimg = new Func< float , float , DenseHistogram[]> ( ( dw , up ) =>
            {
                DenseHistogram[] hist = new DenseHistogram[] { };
                hist = new DenseHistogram[1];
                hist[0] = new DenseHistogram( binsize , new RangeF( dw , up ) );
                hist[0].Calculate<byte>( new Image<Gray , byte>[] { img } , true , null );
                return hist;
            } );
            return fromimg;
        }

        Func<float , float ,DenseHistogram[]> HistoFromResult( ImgPResult result )
        {
            var fromresult = new Func<float , float ,DenseHistogram[]>((float dw, float up)=>
            {
                var item = result.OutData.Select( i => ( int ) i.Intensity ).ToArray();
                DenseHistogram histIntes = CreateHisto(Core.PResult, new Func<List<ExResult>,int[]>( j => j.Select(i => ( int ) i.Intensity).ToArray() ));
                DenseHistogram histSize  = CreateHisto(Core.PResult, new Func<List<ExResult>,int[]>( j => j.Select(i => ( int ) i.ContourSize).ToArray() ));
                return new DenseHistogram[2] { histIntes , histSize };
            } );
            return fromresult;
        }


        #endregion



        #region Epi Region

        private void btnEpiLoadImgs_Click(object sender, RoutedEventArgs e)
        {
            
            Stopwatch stw = new Stopwatch();
            stw.Start();
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            //await Task.Run(()=> CoreEpi.SetImageList());
            CoreEpi.SetImageList();
            Mouse.OverrideCursor = null;
            stw.Stop();
            stw.ElapsedMilliseconds.Print( "Load Time" );
        }

        private void btnEpiProcStart_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch stw = new Stopwatch();
            stw.Start();
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
			CoreEpi.EpiProcessing(
													( int )nudEpiResolution.Value ,
													( int )nudEpiAreaUpLimit.Value ,
													( int )nudEpiAreaDwLimit.Value ,
													( int )nudEdgeOffset.Value );
			//Task proc = new Task(()=>CoreEpi.EpiProcessing(
			//										( int )nudEpiResolution.Value ,
			//										( int )nudEpiAreaUpLimit.Value ,
			//										( int )nudEpiAreaDwLimit.Value ,
			//										( int )nudEdgeOffset.Value ));
            //proc.Start();
            //proc.Wait();

            Mouse.OverrideCursor = null;
            stw.Stop();
            stw.ElapsedMilliseconds.Print( "Load Time" );
            //lblprgEpi.Content = "100 %";
        }

    

        private void btnEpiSaveImg_Click( object sender , RoutedEventArgs e )
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if ( sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK )
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                CoreEpi.SaveEpiImage( sfd.FileName );
                Mouse.OverrideCursor = null;
            }
        }

        private void btnEpiSaveResult_Click( object sender , RoutedEventArgs e )
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if ( sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK )
            {
                CoreEpi.SaveEpiResult( sfd.FileName + ".csv" );
            }
        }

        private void RegisterZoomGrid(  System.Windows.Controls.Primitives.UniformGrid grid 
                                       , Dictionary<ImgIdxPos , System.Windows.Controls.Button> btns
                                       , Dictionary<ImgIdxPos , MouseButtonEventHandler> methods )
        {
            btns = new Dictionary<ImgIdxPos , System.Windows.Controls.Button>();
            methods = new Dictionary<ImgIdxPos , MouseButtonEventHandler>();
            Enum.GetValues( typeof( ImgIdxPos ) )
                    .Cast<ImgIdxPos>()
                    .ActLoop( pos => btns.Add( pos , new System.Windows.Controls.Button()
                                                        .Act( x => x.Background = Brushes.Transparent )
                                                        .SetImgPos2GridPos( pos)) )
                    .ActLoop( pos => methods.Add( pos , CreateZoomClickMethod( pos ) ) )
                    .ActLoop( pos => btns[pos].PreviewMouseDoubleClick 
                                     += new MouseButtonEventHandler ( methods[ pos ] ) )
                    .ActLoop( pos => grid.Children.Add( btns[pos] ) );
        
            Console.ReadLine();
        
        }
        
        private MouseButtonEventHandler CreateZoomClickMethod( ImgIdxPos pos)
        {
            return ( MouseButtonEventHandler )
                        ( ( sender , e ) => 
                        {
                            new ZoomWindow()
                                    .Act( win => win.ShowImage( CoreEpi.GetProcedImg( pos ) ) )
                                    .Act( win => win.BringToFront() )
                                    .Act( win => win.evtClosed += new EvtClosed( RestoreWindowSize ))
                                    .Act( win => win.TopMost = true)
                                    .Show(); 
                        } );
        }

        void RestoreWindowSize()
        {
            this.WindowState = WindowState.Normal;
        }

        #endregion

        private void ChipPosCheckbox( object sender , RoutedEventArgs e )
        {
        }

        private void MetroWindow_Loaded( object sender , RoutedEventArgs e )
        {
            

        }

       
    }
}
