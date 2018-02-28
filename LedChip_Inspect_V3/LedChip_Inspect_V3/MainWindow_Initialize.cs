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

namespace WaferandChipProcessing
{
    enum ChipPostPattern { FirstChip, SecondChip , ThirdChip}
    public partial class MainWindow
    {
        void InitDisplay(MainCore core)
        {
            nudCWNum.Value = 787;
            nudCHNum.Value = 700;
            nudThresh.Value = 40;
            nudAreaUpLimit.Value = 210;
            nudAreaDWLimit.Value = 1;
            nudIntenSumUPLimit.Value = 15000;
            nudIntenSumDWLimit.Value = 2000;
            nudboxSizeH.Value = 100;
            nudboxSizeW.Value = 100;
            nudThickness.Value = 1;

            nudCropRatio.Value = 280;
            nudCropRatioAdv.Value = 5;

			nudTol.Value = 3;

			ckbEst4Pos_Rumbus.IsChecked = true;

            var sampleType2Idx = new Dictionary<string, SampleType>();
            sampleType2Idx.Add("0. Simple", SampleType.None);
            sampleType2Idx.Add("1. 1B6RB", SampleType._1B6R);
            sampleType2Idx.Add("2. A (not suppported)", SampleType._A);
            sampleType2Idx.Add("3. B", SampleType._B);
            sampleType2Idx.Add("4. C", SampleType._C);
            sampleType2Idx.Add("5. D", SampleType._D);
            sampleType2Idx.Add("6. BlueLD", SampleType._BlueLD);
            sampleType2Idx.Add("7. FullEsted", SampleType.Fullested);
            sampleType2Idx.Add("8. Lumence_0620" , SampleType.Lumence_0620);
            sampleType2Idx.Add("8. Lumence_0620_2mm" , SampleType.Lumence_0620_2mm);
            sampleType2Idx.Add("9. AOTCW" , SampleType.AOTCW);
            sampleType2Idx.Add("10. AOTTB" , SampleType.AOTTB);
            sampleType2Idx.Add("11. Epistar" , SampleType.Epistar);
            sampleType2Idx.Add("12. LumenMapFront" , SampleType.LumenMapFront);
            sampleType2Idx.Add("13. LumenMapBack" , SampleType.LumenMapBack);
            sampleType2Idx.Add("14. LumenLineFront" , SampleType.LumenLineFront);
            sampleType2Idx.Add("15. LumenLineBack" , SampleType.LumenLineBack);
            sampleType2Idx.Add("16. Gaung2" , SampleType.Guang2);
            sampleType2Idx.Add("17. Gaung2Scatter" , SampleType.Guang2Scatter);
            sampleType2Idx.Add("18. Gaung2Mapping" , SampleType.Guang2Mapping );
            sampleType2Idx.Add("19. Gaung2MappingSC" , SampleType.Guang2MappingSC );
            sampleType2Idx.Add( "20. SSDisplay1RGBSample" , SampleType.SSDisplay1RGBSample );
			sampleType2Idx.Add( "21. PlaynittideB1" , SampleType.PlaynittideB1 );
			sampleType2Idx.Add( "22. PlaynittideB2" , SampleType.PlaynittideB2 );
			sampleType2Idx.Add( "23. PlaynittideG1" , SampleType.PlaynittideG1 );
			sampleType2Idx.Select((s, ix) => Tuple.Create(ix, s)); // like List<tuple> 

            core.SampleTypeList = sampleType2Idx;
            cbSampleMethod.ItemsSource = sampleType2Idx.Select((v) => v.Key).ToList();
			cbSampleMethod.SelectedIndex = 22;

			//nudEpiYoffset.Value  = 25000;
            //nudEpiX1Offset.Value = 16666;
            //nudEpiX2Offset.Value = 33332;

			//nudEpiYoffset.Value = 15000;
			//nudEpiX1Offset.Value = 10000;
			//nudEpiX2Offset.Value = 20000;

			nudEpiYoffset.Value = 7499;
			nudEpiX1Offset.Value = 5000;
			nudEpiX2Offset.Value = 10000;

			nudEpiResolution .Value = 10;
			nudEdgeOffset	 .Value = 300;
			nudEpiAreaUpLimit.Value = 100000;
			nudEpiAreaDwLimit.Value = 4;
		}




		double[][] AdvPos = new double[3][];
        void LTClickEvt_Advanced( object ob , MouseButtonEventArgs ev )
        {
            double py = ev.GetPosition(this.canvasLT).Y;
            double px = ev.GetPosition(this.canvasLT).X;
            switch ( Core.ChipPosMode )
            {
                case AdvancedChipPos.First:
                    AdvPos [ 0 ] = new double [ ] { py , px };
                    Core.PData.LTPos_Img = Core.MapCanv2ImgLTRB( new double [ 2 ] { py , px } );
                    break;
                case AdvancedChipPos.Second:
                    AdvPos [ 1 ] = new double [ ] { py , px };
                    break;
                case AdvancedChipPos.Third:
                    AdvPos [ 2 ] = new double [ ] { py , px };
                    break;
            }
            SetFSTPos( canvasLT , AdvPos );
            Core.PData.AdvHChipPos = AdvPos.Select( (x , i) => AdvPos[i] != null 
                                                                ? Core.MapCanv2ImgLTRB( AdvPos[i] ) 
                                                                : null )
                                           .ToArray();
        }

        void LTClickEvt(object ob, MouseButtonEventArgs ev)
        {
            while (canvasLT.Children.Count > 0) { canvasLT.Children.RemoveAt(canvasLT.Children.Count - 1); }

            double py = ev.GetPosition(this.canvasLT).Y;
            double px = ev.GetPosition(this.canvasLT).X;
            canvasLT.Children.Add(StartEndDot(py, px));
            Core.PData.LTPos_Img = Core.MapCanv2ImgLTRB(new double[2] { py, px });
			Console.WriteLine( $" y: { Core.PData.LTPos_Img [ 0 ]}  x : { Core.PData.LTPos_Img [ 1 ]} " );
		}

        void LBClickEvt(object ob, MouseButtonEventArgs ev)
        {
            while (canvasLB.Children.Count > 0) { canvasLB.Children.RemoveAt(canvasLB.Children.Count - 1); }

            double py = ev.GetPosition(this.canvasLB).Y;
            double px = ev.GetPosition(this.canvasLB).X;
            canvasLB.Children.Add(StartEndDot(py, px));

            double[] onCropImgPos = Core.MapCanv2ImgLTRB(new double[2] { py, px });
            Core.PData.LBPos_Img = new double[2] { Core.OriginImg.Height + onCropImgPos[0] - Core.LTRBPixelNumberH, onCropImgPos[1] };
			Console.WriteLine( $" y: { Core.PData.LBPos_Img[0]}  x : { Core.PData.LBPos_Img[1]} " );
		}

        void RTClickEvt(object ob, MouseButtonEventArgs ev)
        {
            while (canvasRT.Children.Count > 0) { canvasRT.Children.RemoveAt(canvasRT.Children.Count - 1); }

            double py = ev.GetPosition(this.canvasRT).Y;
            double px = ev.GetPosition(this.canvasRT).X;
            canvasRT.Children.Add(StartEndDot(py, px));
			
			double[] onCropImgPos = Core.MapCanv2ImgLTRB(new double[2] { py, px });
            Core.PData.RTPos_Img = new double[2] { onCropImgPos[0], Core.OriginImg.Width + onCropImgPos[1] - Core.LTRBPixelNumberW };
			Console.WriteLine( $" y: { Core.PData.RTPos_Img [ 0 ]}  x : { Core.PData.RTPos_Img [ 1 ]} " );
		}

        void RBClickEvt(object ob, MouseButtonEventArgs ev)
        {
            while (canvasRB.Children.Count > 0) { canvasRB.Children.RemoveAt(canvasRB.Children.Count - 1); }

            double py = ev.GetPosition(this.canvasRB).Y;
            double px = ev.GetPosition(this.canvasRB).X;
            canvasRB.Children.Add(StartEndDot(py, px));
			double[] onCropImgPos = Core.MapCanv2ImgLTRB(new double[2] { py, px });
            Core.PData.RBPos_Img = new double[2] { onCropImgPos[0] - Core.LTRBPixelNumberH + Core.OriginImg.Height, onCropImgPos[1] - Core.LTRBPixelNumberW + Core.OriginImg.Width };
			Console.WriteLine( $" y: { Core.PData.RBPos_Img [ 0 ]}  x : { Core.PData.RBPos_Img [ 1 ]} " );
		}



        void DisplayIntenAvg(int input) { this.BeginInvoke(() => lblIntenAvg.Content = input.ToString()); }
        void DisplayAreaAvg(int input) { this.BeginInvoke(() => lblAreaAvg.Content = input.ToString()); }

        #region Helper
        Rectangle StartEndDot(double py, double px)
        {
            Rectangle rect = new Rectangle();
            rect.Width = 4;
            rect.Height = 4;
            rect.StrokeThickness = 1;
            rect.Fill = new SolidColorBrush(Colors.OrangeRed);
            rect.Stroke = new SolidColorBrush(Colors.OrangeRed);
            Canvas.SetLeft(rect, px - rect.Width / 2);
            Canvas.SetTop(rect, py - rect.Height / 2);
            return rect;
        }

        Rectangle StartEndDot( double[] pos , AdvancedChipPos chippos )
        {
            if ( pos == null ) return new Rectangle();
            Rectangle rect = new Rectangle();
            rect.Width = 4;
            rect.Height = 4;
            rect.StrokeThickness = 1;
            switch ( chippos )
            {
                case AdvancedChipPos.First:
                    
                    rect.Fill = new SolidColorBrush( Colors.Red );
                    rect.Stroke = new SolidColorBrush( Colors.Red );
                    break;
                case AdvancedChipPos.Second:
                    rect.Fill = new SolidColorBrush( Colors.Orange );
                    rect.Stroke = new SolidColorBrush( Colors.Orange );
                    break;
                case AdvancedChipPos.Third:
                    rect.Fill = new SolidColorBrush( Colors.ForestGreen );
                    rect.Stroke = new SolidColorBrush( Colors.ForestGreen );
                    break;
            }
            Canvas.SetLeft( rect , pos [ 1 ] - rect.Width / 2 );
            Canvas.SetTop( rect , pos [ 0 ] - rect.Height / 2 );
            return rect;
        }


        void SetFSTPos(Canvas cvs , double[][] pos )
        {
            while ( cvs.Children.Count > 0 ) { cvs.Children.RemoveAt( cvs.Children.Count - 1 ); }
            cvs.Children.Add( StartEndDot( pos [ 0 ] , AdvancedChipPos.First  ) );
            cvs.Children.Add( StartEndDot( pos [ 1 ] , AdvancedChipPos.Second ) );
            cvs.Children.Add( StartEndDot( pos [ 2 ] , AdvancedChipPos.Third  ) );
        }

        #endregion

    }
}
