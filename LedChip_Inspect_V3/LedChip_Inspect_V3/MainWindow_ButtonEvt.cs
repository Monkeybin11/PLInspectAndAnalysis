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
using static EmguCV_Extension.ThresholdMode;
using Microsoft.VisualStudio.DebuggerVisualizers;
using EmguCV_Extension;
using System.IO;


using static WaferandChipProcessing.ELDataLoader;

namespace WaferandChipProcessing
{
	public partial class MainWindow
	{
		#region MainFunction Button Evt

		private async void btnLoad_Click( object sender , RoutedEventArgs e )
		{
			OpenFileDialog ofd = new OpenFileDialog();
			if ( ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK )
			{
				// Init Core.PData

				Core.PData = new ImgPData();
				bool ischecked = (bool)ckbAdvancedPos.IsChecked;
				Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
				Core.ChipPosMode = ischecked ? AdvancedChipPos.First : AdvancedChipPos.None;

				ClearLRFrame();

				int cropRatio = (int)nudCropRatio.Value;
				int cropRatioAdv = (int)nudCropRatioAdv.Value;


				await Task.Run( () =>
				{
					ofd.FileName
					.Act( name =>
					 {
						 TestFileSavePath.Setting( name );
						 Core.OriginImg = new Image<Gray , byte>( name );
						 Core.ColorOriImg = new Image<Bgr , byte>( name );
						 try
						 {
							 Core.TemplateImg = new Image<Gray , byte>(
							 System.IO.Path.GetDirectoryName( name ) + "\\template.bmp" );
						 }
						 catch ( Exception )
						 {
						 }
					 } );

					Core.OriginImg.
					Act( img =>
					{
						double ratio = 0;
						if ( Math.Max( img.Width , img.Height ) < 600 ) ratio = 8;
						else if ( ischecked == true ) ratio = cropRatioAdv;
						else ratio = cropRatio;

						var cropsize = ( Math.Max(img.Width, img.Height) / ratio )
						.Act(size =>
						{
							Core.LTRBPixelNumberW = size;
							Core.LTRBPixelNumberH = size;
						});
					} );
				} );

				Core
					.Act( core =>
					{
						core.InitFunc( canvas , CornerCanvsGroup [ 0 ] );
						CornerImgCrops =
						new Func<Image<Gray , byte> , Image<Gray , byte>> [ 4 ] {
							core.CropImgLT
							, core.CropImgLB
							, core.CropImgRT
							, core.CropImgRB };
					} );

				if ( Core.ChipPosMode != AdvancedChipPos.None )
				{
					CornerClickEvt = new Action<object , MouseButtonEventArgs> [ 4 ] {
							LTClickEvt_Advanced
							, LBClickEvt
							, RTClickEvt
							, RBClickEvt };
				}
				else
				{
					CornerClickEvt = new Action<object , MouseButtonEventArgs> [ 4 ] {
							LTClickEvt
							, LBClickEvt
							, RTClickEvt
							, RBClickEvt };
				}
				SetInitImg( CornerImgGroup , canvas , canvasProced , CornerCanvsGroup );
				Core.PData.SetFrame( canvas.ActualHeight , canvas.ActualWidth , Core.OriginImg.Height , Core.OriginImg.Width );
				Mouse.OverrideCursor = null;
			}
		}


		private async void btnStartProcssing_Click( object sender , RoutedEventArgs e )
		{
			try
			{

				if ( ckbAdvancedPos.IsChecked == true && ( Core.PData.AdvHChipPos [ 0 ] == null || Core.PData.AdvHChipPos [ 1 ] == null || Core.PData.AdvHChipPos [ 2 ] == null ) )
				{
					System.Windows.MessageBox.Show( "Select Position of Chip first" );
					return;
				}

				timer.Start();
				this.BeginInvoke( () => Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait );
				var thresvalue = (int)nudThresh.Value;
				Core.APBoxTolerance = nudTol.Value == null ? 1 : ( int )nudTol.Value;

				var gridstyle = (bool)chbGridStyle.IsChecked;
				var needdebug = (bool)chbDebugImg.IsChecked;
				Core.SelectedSample = Core.SampleTypeList [ cbSampleMethod.SelectedItem as string ];

                if ((bool)ckbUseEl.IsChecked)
                {
                    List<ELData> output = new List<ELData>();


                    OpenFileDialog fd = new OpenFileDialog();

                    if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        output = LoadELData(fd.FileName).ToList();
                    }

                    if (ReadyProc()) await Task.Run(()
                                //=> Core.ProcessingStep1_Normal(
                                => Core.ProcessingStep1_Version2_WithEL(
                                   thresvalue
                                   , Core.SelectedSample // automated
                                   , (int)Core.PData.ChipHNum
                                   , (int)Core.PData.ChipWNum
                                   , gridstyle
                                   , output
                                   , needdebug)
                                   (
                                       Core.OriginImg,
                                       Core.ColorOriImg
                                   ));
                }
                else
                {
                    if (ReadyProc()) await Task.Run(()
                                 //=> Core.ProcessingStep1_Normal(
                                 => Core.ProcessingStep1_Version2(
                                    thresvalue
                                    , Core.SelectedSample // automated
                                    , (int)Core.PData.ChipHNum
                                    , (int)Core.PData.ChipWNum
                                    , gridstyle
                                    , needdebug)
                                    (
                                        Core.OriginImg,
                                        Core.ColorOriImg
                                    ));
                }
				

				this.BeginInvoke( () =>
				{
					imgPro.ImageSource = BitmapSrcConvert.ToBitmapSource( Core.ProcedImg );
					imgIndex.ImageSource = BitmapSrcConvert.ToBitmapSource( Core.IndexViewImg );
					Mouse.OverrideCursor = null;
				} );

			}
			catch ( Exception ex )
			{
				System.Windows.Forms.MessageBox.Show( "Main Processing Error : " + ex.ToString() );

			}
		}
		private void btnSaveData_Click( object sender , RoutedEventArgs e )
		{
			SaveFileDialog sfd = new SaveFileDialog();
			if ( sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK )
			{
				Core.SaveData( Core.PResult , sfd.FileName + ".csv" );
			}
		}
		private void btnSaveImg_Click( object sender , RoutedEventArgs e )
		{
			try
			{
				SaveFileDialog sfd = new SaveFileDialog();
				if ( sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK )
				{
					var resizedimg = Core.IndexViewImg.Resize(Core.ProcedImg.Width, Core.ProcedImg.Height, Inter.Nearest);
					Core.SaveImg( Core.IndexViewImg , sfd.FileName + "_OverView_Point2Chip.png" );
					Core.SaveImg( resizedimg , sfd.FileName + "_OverView_SameSize.png" );
					Core.SaveImg( Core.ProcedImg , sfd.FileName + "_Proced.png" );

					//HistogramList[0]?.Save( sfd.FileName + "_Histogram1.png" );
					//HistogramList[1]?.Save( sfd.FileName + "_Histogram2.png" );
				}

			}
			catch ( Exception )
			{
			}
		}


        private void btnLoadELData_Click(object sender, RoutedEventArgs e)
        {
           



        }

        #endregion

        private void cbSampleMethod_SelectionChanged( object sender , SelectionChangedEventArgs e )
		{
			if ( Core != null && Core.SelectedSample != null ) Core.SelectedSample = Core.SampleTypeList [ cbSampleMethod.SelectedItem as string ];
		}

		private async void btnfulleste_Click( object sender , RoutedEventArgs e )
		{
			try
			{
				this.BeginInvoke( () => Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait );

				int hsize = (int)nudboxSizeH.Value;
				int wsize = (int)nudboxSizeW.Value;

				if ( ReadyProc() ) await Task.Run( ()
					=> Core.ProcessingStep1_Simple(
						Core.SelectedSample // automated
					   , ( int )Core.PData.ChipHNum
					   , ( int )Core.PData.ChipWNum
					   , hsize
					   , wsize )
					   (
						   Core.OriginImg ,
						   Core.ColorOriImg
					   ) );

				this.BeginInvoke( () =>
				 {
					 imgPro.ImageSource = BitmapSrcConvert.ToBitmapSource( Core.ProcedImg );
					 imgIndex.ImageSource = BitmapSrcConvert.ToBitmapSource( Core.IndexViewImg );
					 Mouse.OverrideCursor = null;
				 } );
			}
			catch ( Exception ex )
			{
				System.Windows.Forms.MessageBox.Show( ex.ToString() );
			}
		}

		#region CheckBox Event
		private void ChipPosRadioBtn( object sender , RoutedEventArgs e )
		{
			System.Windows.Controls.RadioButton ckb = sender as System.Windows.Controls.RadioButton;

			switch ( ckb.Name )
			{
				case "ckbAdvancedPos":
					Core.ChipPosMode = AdvancedChipPos.First;
					ckbFirstChip.IsChecked = true;
					break;

				case "ckbFirstChip":
					Core.ChipPosMode = AdvancedChipPos.First;
					break;

				case "ckbSecondChip":
					Core.ChipPosMode = AdvancedChipPos.Second;
					break;

				case "ckbThirdChip":
					Core.ChipPosMode = AdvancedChipPos.Third;
					break;
			}
		}

		private void ChipPosChecked( object sender , RoutedEventArgs e )
		{
		}
		#endregion


	}
}
