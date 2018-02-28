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
using AccordBased_processing.FeatureExtract;
using System.Diagnostics;
using SpeedyCoding;

namespace WaferandChipProcessing
{
    public partial class MainCore
    {
        void Register_ProcMethod()
        {
            Proc_Method_List = new Dictionary<SampleType , Func<Image<Gray , byte> , Image<Gray , byte>>>();
            Proc_Method_List.Add( SampleType.Thres , CreateMethod_Thres() );
            Proc_Method_List.Add( SampleType.AdpThres , CreateMethod_AdpThres() );
            Proc_Method_List.Add( SampleType._A , CreateMethod_A() );
            Proc_Method_List.Add( SampleType._B , CreateMethod_BC() );
            Proc_Method_List.Add( SampleType._C , CreateMethod_BC() );
            Proc_Method_List.Add( SampleType._D , CreateMethod_D() );
            Proc_Method_List.Add( SampleType._BlueLD , CreateMethod_BlueLD() );
            Proc_Method_List.Add( SampleType.Fullested , CreateMethod_FullEst() );
            Proc_Method_List.Add( SampleType.Lumence_0620 , CreateMethod_Lumence0620() );
            Proc_Method_List.Add( SampleType.Lumence_0620_2mm , CreateMethod_Lumence0620_2mm() );
            Proc_Method_List.Add( SampleType.AOTCW , CreateMethod_AOTCW() );
            Proc_Method_List.Add( SampleType.AOTTB , CreateMethod_AOTTB() );
            Proc_Method_List.Add( SampleType.Epistar , CreateMethod_Epistar );
            Proc_Method_List.Add( SampleType.LumenMapFront , CreateMethod_LumenMapFront );
            Proc_Method_List.Add( SampleType.LumenMapBack , CreateMethod_LumenMapBack );
            Proc_Method_List.Add( SampleType.LumenLineFront , CreateMethod_LumenLineFront );
            Proc_Method_List.Add( SampleType.LumenLineBack , CreateMethod_LumenLineBack );
            Proc_Method_List.Add( SampleType.Guang2 , CreateMethod_Guang2 );
            Proc_Method_List.Add( SampleType.Guang2Scatter , CreateMethod_Guang2Scatter );
            Proc_Method_List.Add( SampleType.Guang2Mapping , CreateMethod_GuangMapping );
            Proc_Method_List.Add( SampleType.Guang2MappingSC , CreateMethod_GuangMappingScatter );
            Proc_Method_List.Add( SampleType.SSDisplay1RGBSample , CreateMethod_SSDisplay1RGBSample );
			Proc_Method_List.Add( SampleType.PlaynittideB1 , CreateMethod_PlaynittideB1 );
			Proc_Method_List.Add( SampleType.PlaynittideB2 , CreateMethod_PlaynittideB2 );
			Proc_Method_List.Add( SampleType.PlaynittideG1 , CreateMethod_PlaynittideG1 );
		}

        Func<Image<Gray, byte>, Image<Gray, byte>> CreateMethod_Thres()
        {
            var method = new Func<Image<Gray, byte>, Image<Gray, byte>>((img) =>
            {
                var thresImg      = DoThreshold(img , PData.ThresholdV );
                return thresImg;
            });
            return method;
        }

        Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_AdpThres()
        {
            var method = new Func<Image<Gray,byte>,Image<Gray,byte>>((img)=>
            {
                if (PData.ThresholdV % 2.0 == 0) PData.ThresholdV = PData.ThresholdV + 1;
                var thresImg  = img.ThresholdAdaptive(new Gray(255), AdaptiveThresholdType.GaussianC , ThresholdType.Binary , PData.ThresholdV ,new Gray(0));
                return thresImg;//.DilateCross();
            } );
            return method;
        }

       
        Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_A()
        {
            var method = new Func<Image<Gray,byte>,Image<Gray,byte>>((img)=>
            {
                // Not Work
                return null;
            } );
            return method;
        }
        Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_BC()
        {
            var method = new Func<Image<Gray,byte>,Image<Gray,byte>>((img)=>
            {
                //var morped =  OpenRect(CloseRect( DoThreshold( img , 23) , 3) ,3);
                var morped =  DoThreshold( TempMatch_Ce( img , TemplateImg) , 120) ;

                for (int i = 0; i < 8; i++)
                {
                    morped = ErodeHori(morped , 3);
                }


                for (int i = 0; i < 8; i++)
                {
                    morped = DilateRect(morped , 3);
                }
                //
                //for (int i = 0; i < 6; i++)
                //{
                //    morped = DilateVerti(morped , 3);
                //}
                return morped;
            } );
            return method;
        }
        Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_D()
        {
            var method = new Func<Image<Gray,byte>,Image<Gray,byte>>((img)=>
            {
                var imgg = DilateRect(DilateRect(CloseRect(DoThreshold(img , BackgroundInten(img) + 1) , 5),3),3);
                return DilateRect(DilateRect(CloseRect(DoThreshold(img , BackgroundInten(img) + 1) , 5),3),3);
            } );
            return method;
        }
        Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_BlueLD()
        {
            var method = new Func<Image<Gray,byte>,Image<Gray,byte>>((img)=>
            {
                var backInten     = BackgroundInten(img);
                var thresImg      = DoThreshold(img , (int)backInten + 5 );
                return thresImg;
            } );
            return method;
        }

        Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_FullEst()
        {
            var method = new Func<Image<Gray, byte>, Image<Gray, byte>>((img) =>
            {
                return img;
            });
            return method;
        }

        Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_Lumence0620()
        {
            var method = new Func<Image<Gray, byte>, Image<Gray, byte>>((img) =>
            {
                return img.Median(5)
                         //.Normalize(120)
                         .Threshold(120)
                         .DilateRect();
            });
            return method;
        }

        Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_Lumence0620_2mm()
        {
            var method = new Func<Image<Gray, byte>, Image<Gray, byte>>((img) =>
            {
                return img.Threshold(60)
                          .DilateRect();
            });
            return method;
        }

        Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_Lumence0620_2mm_bck()
        {
            var method = new Func<Image<Gray, byte>, Image<Gray, byte>>((img) =>
            {
                //var clustered = ( img.ToBitmap() );
                Feature_Extract<byte,byte> Fe = new Feature_Extract<byte, byte>();
                K_Mean kmean = new K_Mean();

                var clustered = kmean.test(img.ToBitmap());

                double min = 255;
                foreach ( var item in clustered[ "center" ] )
                {
                    if ( item [ 0 ] < min ) min = item [ 0 ];
                }
                var clustedImg = new Image<Gray,byte>( clustered["image"]);


                var imgdata = clustedImg.Data;
                int rownum = imgdata.Len();
                int colnum = imgdata.Len(1);
                byte[][] lbpdata = rownum.JArray<byte>(colnum);
                for ( int j = 0 ; j < rownum ; j++ )
                {
                    var rows = new byte[colnum];
                    for ( int i = 0 ; i < colnum ; i++ )
                    {
                        rows [ i ] = imgdata [ j , i , 0 ];
                    }
                    lbpdata [ j ] = rows;
                }

                var lbp = new Image<Gray,byte> ( Fe.LBP(lbpdata)
                                                    .ConvertToImgData() );

                var tempimg1 = lbp.Normalize(60);
                var tempimg2 = tempimg1.Normalize(60);
                var tempimg3 = tempimg2.Normalize(60);




                return lbp.Normalize(60)
                          .Normalize(60)
                          .DilateRect()
                          .DilateRect()
                          .ErodeRect()
                          .ErodeRect();
            });
            return method;
        }


        Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_AOTCW()
        {
            var method = new Func<Image<Gray, byte>, Image<Gray, byte>>((img) =>
            {
                return img
                .Median(9)
                .Normalize(70)
                .Gamma(3)
                .Median(9)
                .Normalize(180)
                .Median(15)
                .Brightness(1,100)
                .Gamma(2)
                .Gamma(2)
                .Threshold(120);
            });
            return method;
        }


        Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_AOTTB()
        {
            var method = new Func<Image<Gray, byte>, Image<Gray, byte>>((img) =>
            {


                return img
                    .Median(5)
                    .Normalize(120)
                    .Gamma(5)
                    .Median(11)
                    .Threshold(120)
                    .DilateRect()
                    .DilateRect()
                    .DilateRect();
                          
            });
            return method;
        }

        Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_Epistar =>
            img =>
            {
				return img
					.SmoothGaussian( 3 )
					.Gamma( 1.3 )
					.ThresholdAdaptive( new Gray( 255 ) , AdaptiveThresholdType.GaussianC , ThresholdType.Binary , 151 , new Gray( 3 ) )
					.ErodeCross()
					.DilateRect();
					//.SaveImg( @"D:\03JobPro\2017\012_Epistar\20170824_\Region1\Debug\afterProcessing.bmp" ); // sjwtemp
            };

		Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_LumenMapFront =>
		 img =>
		 {
			 return img
				.Normalize( 60 )
				.Median( 5 )
				 .Gamma( 1.5 )
				 .Gamma( 1.5 )
				 .Threshold( 120 )
				 .ErodeRect();
		 };

		Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_LumenMapBack =>
		 img =>
		 {
			 return img
				.Normalize( 60 )
				.Median( 5 )
				.Gamma( 2.5 )
				.Threshold( 200 );
		 };

		Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_LumenLineFront =>
		img =>
		{
			return img
				.SmoothGaussian( 3 )
				.Laplace( 5 ).Convert<Gray , byte>()
				.Median( 3 )
				.Gamma( 2 )
				.Normalize( 120 )
				.Gamma( 2 )
				.Gamma( 2 )
				.Median( 5 )
				.Normalize( 120 )
				.Gamma( 2 )
				.Normalize( 120 )
				.Inverse()
				.Gamma( 2 )
				.Normalize( 120 )
				.Gamma( 2 )
				.Gamma( 2 )
				.Threshold( 180 )
				.ErodeCross()
				.DilateRect();
		};

		Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_LumenLineBack =>
		 img =>
		 {
			 return img
				.Normalize( 60 )
				.Median( 5 )
				.Gamma( 2.5 )
				.Threshold( 200 );
		 };

		Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_Guang2 =>
		 img =>
		 {
			 return img
			  .Median( 5 )
			  .ThresholdAdaptive(
				new Gray( 255 ) ,
				AdaptiveThresholdType.GaussianC ,
				ThresholdType.Binary ,
				33 ,
				new Gray( 0 ) )
			.OpenRect()
			.DilateRect()
			.DilateRect()
			.ErodeRect()
			.ErodeRect();
		 };

		Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_Guang2Scatter =>
		 img =>
		 {
			 return img
			  .Median( 5 )
			  .ThresholdAdaptive(
				new Gray( 255 ) ,
				AdaptiveThresholdType.GaussianC ,
				ThresholdType.Binary ,
				55 ,
				new Gray( 0 ) )
			.OpenRect()
			.CloseRect();
			;
		 };

		Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_GuangMapping =>
		 img =>
		 {
			 return img
			  .Median( 5 )
			  .ThresholdAdaptive(
				new Gray( 255 ) ,
				AdaptiveThresholdType.GaussianC ,
				ThresholdType.Binary ,
				91 ,
				new Gray( 0 ) )
			.OpenRect()
			.DilateRect();
		 };

		Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_GuangMappingScatter =>
		 img => CreateMethod_Guang2Scatter( img );

		Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_SSDisplay1RGBSample =>
			img => img.Threshold(20);
		//  TColor maxValue , AdaptiveThresholdType adaptiveType , ThresholdType thresholdType , int blockSize , TColor param1
		Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_PlaynittideB1 =>
		img => img.ThresholdAdaptive(new Gray(255) , AdaptiveThresholdType.GaussianC , ThresholdType.Binary , 555 , new Gray(0) );
		//.Normalize(120)
		//.Gamma( 2.0 )
		//.Normalize(120)
		//.Gamma( 2.0 )
		//.Normalize(120)
		//.Gamma( 2.0 )
		//.Normalize(120)
		//.Gamma( 2.0 )
	    //.Normalize( 120 )
		//.Gamma( 2.0 )
		//.Normalize( 120 )
		//.Gamma( 2.0 )
		//.Normalize( 120 )
		//.Gamma( 2.0 )
		//.Normalize( 120 )
		//.Gamma( 2.0 )
		//.DilateCross()
		//.ErodeCross()
		//.DilateRect()
		//.Threshold(200);

		Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_PlaynittideB2 =>
		img => img.Threshold( 20 );

		Func<Image<Gray , byte> , Image<Gray , byte>> CreateMethod_PlaynittideG1 =>
		img => img.Threshold( 20 );



	}
}
