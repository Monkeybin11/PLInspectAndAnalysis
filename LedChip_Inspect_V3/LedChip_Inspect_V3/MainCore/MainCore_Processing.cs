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
using SpeedyCoding;


namespace WaferandChipProcessing
{

	public partial class MainCore
	{
		public Action<Image<Gray , byte> , Image<Bgr , byte>> ProcessingStep1_Normal(
			int threshold ,
			SampleType sampletype ,
			int cHnum ,
			int cWnum ,
			bool whiteGrid ,
			bool debugmode = false )
		{
			return new Action<Image<Gray , byte> , Image<Bgr , byte>>( ( originalimg , colorimg ) =>
			{
				try
				{
					Stopwatch stw = new Stopwatch();
					Stopwatch stw2 = new Stopwatch();
					stw.Start();
					stw2.Start();
					var color_visual_img = colorimg.Clone();
					var color_visual_img2 = colorimg.Clone();
					var baseimg = originalimg.Clone();
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
									 .Map( img => Proc_Method_List [ sampletype ]( img ) )
									 .Act( img => img.Save( TestFileSavePath.BasePath + "\\beforcntr.bmp" ) )
									 .Map( img => FindContour( img ) )
									 .Map( cntr => Sortcontours( cntr ) );
					}
					else
					{
						contours = baseimg
									 .Map( img => Proc_Method_List [ sampletype ]( img ) )
									 .Map( img => FindContour( img ) )
									 .Map( cntr => Sortcontours( cntr ) );
					}
					stw2.ElapsedMilliseconds.Print("Pre Contour : ");
					stw2.Restart();
					//DrawContour( color_visual_img2 , contours );
					//if ( debugmode ) color_visual_img2.Save( TestFileSavePath.BasePath + "\\Aftercntr.bmp" );

					//var areaupdown = FindAreaUpDwBoundaryt(contours);
					//Console.WriteLine( $"Area Down : {areaupdown[0]} // UP : {areaupdown[1]} ");

					var centerMoment = contours
										.Map(cntr => CalcCenter(cntr));
					stw2.ElapsedMilliseconds.Print( "moment : " );
					stw2.Restart();
					var boxlist = contours
									.Map( cntr => ApplyBox(cntr));

					//StringBuilder stbbox = new StringBuilder();
					//
					//for ( int i = 0 ; i < boxlist.Count ; i++ )
					//{
					//	stbbox.AppendLine( boxlist [ i ].X.ToString() + "," 
					//		+ boxlist [ i ].Y.ToString() + "," 
					//		+ boxlist [ i ].Width.ToString() + "," 
					//		+ boxlist [ i ].Height.ToString() );
					//}
					//
					//var boxpath = @"C:\Temp\BoxList.csv";
					//File.WriteAllText(boxpath , stbbox.ToString());
					//
					//stw2.ElapsedMilliseconds.Print( "box : " );
					//stw2.Restart();

					var color_visual_img3 = colorimg.Clone();
					DrawBox( color_visual_img3 , boxlist );
					if ( debugmode ) color_visual_img3.Save( TestFileSavePath.BasePath + "\\AftercntrBox.bmp" );

					byte[,,]   indexingImage = null;
					if ( whiteGrid )
					{ indexingImage = MatWhitePattern( cHnum , cWnum , 3 ); }
					else
					{ indexingImage = MatPattern( cHnum , cWnum , 3 ); }

					stw2.ElapsedMilliseconds.Print( "Create Pattern : " );
					stw2.Restart();

					byte[,,]   passfailPosData = new byte[ cHnum, cWnum , 1];
					
					//double[,,] estedChipP      = EstedChipPos( cHnum, cWnum )
					//.Act(ext => {
					//	StringBuilder stby = new StringBuilder();
					//	StringBuilder stbx = new StringBuilder();
					//	int d0 = ext.GetLength(0);
					//	int d1 = ext.GetLength(0);
					//	int d2 = ext.GetLength(0);
					//
					//	for (int i = 0; i < d0; i++)
					//	{
					//		for (int j = 0; j < d1; j++)
					//		{
					//			if(j == d1 -1)
					//			{
					//				stby.Append( ext[i,j,0].ToString()  );
					//				stbx.Append( ext[i,j,1].ToString() );
					//			}
					//			else
					//			{
					//				stby.Append( ext[i,j,0].ToString() + "," );
					//				stbx.Append( ext[i,j,1].ToString() + ",");
					//			}
					//			
					//		}
					//		stby.Append( Environment.NewLine );
					//		stbx.Append( Environment.NewLine );
					//	}
					//	var ypath = @"C:\Temp\yest.csv";
					//	var xpath = @"C:\Temp\xest.csv";
					//	File.WriteAllText(ypath,stby.ToString());
					//	File.WriteAllText(xpath,stby.ToString());
					//
					//
					//} )
					double[,,] estedChipP      = EstedChipPos( cHnum, cWnum )
												 .Act( est => DrawCenterPoint(color_visual_img , est) )
												 .Act(x => boxlist.Count.Print("ToTal Box Num"))
                                                 // Operate Action on Each estedChipPos. Operation Action use Boxlist and Moment as "Closure Variable" ,  Main Operation is CheckOkNg
                                                .Act_LoopChipPos( boxlist
																   , centerMoment
																   , CheckOkNg_SizeInInten(
																	   indexingImage
																	   , contours
																	   , color_visual_img
																	   , ref PResult )
																   , isParallel: true);
					stw2.ElapsedMilliseconds.Print( "In Out Check : " );
					stw2.Restart();

					for ( int i = 0 ; i < estedChipP.GetLength( 0 ) ; i++ )
					{
						for ( int j = 0 ; j < estedChipP.GetLength( 1 ) ; j++ )
						{
							var cirp =  new CircleF();
							cirp.Center = new System.Drawing.PointF(
														 ( float )( estedChipP [ i , j , 1 ] )
														 , ( float )( estedChipP [ i , j , 0 ] ) );

							color_visual_img3.Draw(
								cirp
								, ApCenteBoxColor , 1 );
						}
					}
					stw2.ElapsedMilliseconds.Print( "Low Over Check : " );
					stw2.Restart();
					if ( debugmode ) color_visual_img3.Save( TestFileSavePath.BasePath + "\\AftercntrBoxandEsted.bmp" );


					//var centers =  (ClusterData( 
					//    ( from item in PResult.OutData
					//      select new double[] { item.Intensity } )
					//      .ToArray<double[]>())["center"] as  double[][])
					//    .Select(x => x[0])
					//    .OrderBy( x => x)
					//    .ToArray();


					//var updw = PResult.OutData
					//                .Select(x => x.Intensity)
					//                .ToArray<double>()
					//                .Map( intesns => FindIntensityUpDw(intesns));
					//
					//Console.WriteLine( $"Intensity Down : {updw[0]} // UP : {updw[1]} " );

					PResult.OutData
				   .Act( CheckLowOver(
						  estedChipP
						  , indexingImage
						  , color_visual_img
						  , LineThickness ) );

					DrawCenterPoint( color_visual_img , estedChipP );
					UpdateResult( PResult )( indexingImage , color_visual_img );
					evtProcessingResult();
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

		Action<byte [ , , ] , Image<Bgr , byte>> UpdateResult( ImgPResult processResult )
		{
			return new Action<byte [ , , ] , Image<Bgr , byte>>(
			( indeximg , procedimg ) =>
			{
				IndexViewImg.Data = indeximg;
				ProcedImg = procedimg.Clone();


				processResult.ChipPassCount = ( from item in processResult.OutData where item.OKNG == "OK" select item ).Count();
				processResult.ChipNOPLCount = ( from item in processResult.OutData where item.OKNG == "NOPL" select item ).Count();
				processResult.ChipLowCount  = ( from item in processResult.OutData where item.OKNG == "LOW" select item ).Count();
				processResult.ChipOverCount = ( from item in processResult.OutData where item.OKNG == "OVER" select item ).Count();

				evtIntenAvg( ( int )( processResult.OutData.Select( ( x ) => x.Intensity ).ToList().Average() ) );
				evtAreaAvg( ( int )( processResult.OutData.Select( ( x ) => x.ContourSize ).ToList().Average() ) );
				evtProcessingDone( true );
			} );
		}
		object addkey = new object();
		Action<int , int , double , double , List<System.Drawing.Rectangle> , System.Drawing.Point [ ]> CheckOkNg_SizeInInten(
			byte [ , , ] indexingImage
			, VectorOfVectorOfPoint contours
			, Image<Bgr , byte> color_visual_img
			, ref ImgPResult Presult )
		{
			return ( j , i , yps , xps , boxlist , centerlist ) =>
				   {
					   List<bool> IsFailList = new List<bool>();
					   IsFailList.Add( true );
					   // Parallel.For( 0 , boxlist.Count , (k,ParallelLoopState) =>
					   // {
					   //   /* Check Ested Chip Pos in Contour*/
					   //   Create_Inbox( boxlist [ k ] , APBoxTolerance );
					   //   if ( InBox( yps , xps ) )
					   //   {
					   //	   lock ( addkey )
					   //	   {
					   //		   PResult.OutData.Add(
					   //			   new ExResult(
					   //				   j , i
					   //				   , ( int )yps - ( int )( boxlist [ k ].Y + boxlist [ k ].Height / 2 )
					   //				   , ( int )xps - ( int )( boxlist [ k ].X + boxlist [ k ].Width / 2 )
					   //				   , "OK"
					   //				   , SumInsideBox( boxlist [ k ] )
					   //				   , boxlist [ k ].Width * boxlist [ k ].Height
					   //				   , boxlist [ k ] ) );
					   //	   }
					   //	  
					   //	   IsFailList.Add( false );
					   //	   color_visual_img.Draw( boxlist [ k ] , ApOkChipColor , 1 );
					   //
					   //	   var cirp =  new CircleF();
					   //	   cirp.Center = new System.Drawing.PointF(
					   //									 ( float )( boxlist [ k ].X + boxlist [ k ].Width / 2 )
					   //									 , ( float )( boxlist [ k ].Y + boxlist [ k ].Height / 2 ) );
					   //
					   //	   color_visual_img.Draw(
					   //			cirp
					   //			, ApCenteBoxColor , 1 );
					   //	   ParallelLoopState.Stop();
					   //
					   //   }} );
					   for ( int k = 0 ; k < boxlist.Count ; k++ )
					   {
						   /* Check Ested Chip Pos in Contour*/
						   Create_Inbox( boxlist [ k ] , APBoxTolerance );
						   if ( InBox( yps , xps ) )
						   {
							   lock ( addkey )
							   {
								   PResult.OutData.Add(
									   new ExResult(
										   j , i
										   , ( int )yps - ( int )( boxlist [ k ].Y + boxlist [ k ].Height / 2 )
										   , ( int )xps - ( int )( boxlist [ k ].X + boxlist [ k ].Width / 2 )
										   , "OK"
										   , SumInsideBox( boxlist [ k ] )
										   , boxlist [ k ].Width * boxlist [ k ].Height
										   , boxlist [ k ] ) );
							   }

							   IsFailList.Add( false );
							   color_visual_img.Draw( boxlist [ k ] , ApOkChipColor , 1 );

							   var cirp =  new CircleF();
							   cirp.Center = new System.Drawing.PointF(
															 ( float )( boxlist [ k ].X + boxlist [ k ].Width / 2 )
															 , ( float )( boxlist [ k ].Y + boxlist [ k ].Height / 2 ) );

							   color_visual_img.Draw(
									cirp
									, ApCenteBoxColor , 1 );

							   break;
						   }
					   }
					  
					   var isFail = IsFailList.Aggregate((f,s) => f&&s);
					 
					   if ( isFail )
					   {
						   double failboxInten = SumAreaPoint( (int)yps ,  (int)xps);
						   lock ( addkey )
						   {
							   PResult.OutData.Add(
								   new ExResult(
									   j , i
									   , 0
									   , 0
									   , "NOPL"
									   , failboxInten
									   , 0 ) );
						   }
						   SetFailColor( indexingImage , j , i );
					   }

					   //var res = PResult.OutData;
					   //
					   //for ( int l = 0 ; l < res.Count ; l++ )
					   //{
						//   if ( res [ l ] == null )
						//	   Console.WriteLine( "Outdata Null in {0}" , l );
					   //}
				   };
		}




		Action<List<ExResult>> CheckLowOver(
			double [ , , ] estedChipP ,
			byte [ , , ] indexingImage ,
			Image<Bgr , byte> targetimg ,
			double [ ] center ,
			int thickness
			)
		{
			return new Action<List<ExResult>>( ( list ) =>
			{

				foreach ( var item in list )
				{
					if ( item.OKNG == "OK" )
					{
						int xs = (int)estedChipP[ item.Hindex , item.Windex , 1] - 3;
						int ys = (int)estedChipP[ item.Hindex , item.Windex , 0] - 3;

						if ( item.Intensity < PData.IntenSumDWLimit )
						{
							//Console.WriteLine( item.Intensity );
							item.OKNG = "LOW";
							SetLowColor( indexingImage , item.Hindex , item.Windex );
							targetimg.Draw( item.BoxData.ExpendRect( thickness + 1 ) , ApLowColor , thickness );
							//targetimg.Draw( item.BoxData.ExpendRect(-(thickness+1)) , ApLowColor , thickness);
						}
						else if ( item.Intensity > PData.IntenSumUPLimit )
						{
							item.OKNG = "OVER";
							SetOverColor( indexingImage , item.Hindex , item.Windex );
							targetimg.Draw( item.BoxData.ExpendRect( ( thickness + 1 ) ) , ApOverColor , thickness );
						}
					}
				}
			} );
		}

		Action<List<ExResult>> CheckLowOver(
		   double [ , , ] estedChipP ,
		   byte [ , , ] indexingImage ,
		   Image<Bgr , byte> targetimg ,
		   int thickness
		   )
		{
			return new Action<List<ExResult>>( ( list ) =>
			{
				int idx = 0;
				try
				{

			
				foreach ( var item in list )
				{
						if ( item == null ) continue;
					if ( item.OKNG == "OK" )
					{
						int xs = (int)estedChipP[ item.Hindex , item.Windex , 1] - 3;
						int ys = (int)estedChipP[ item.Hindex , item.Windex , 0] - 3;

						if ( item.Intensity < PData.IntenSumDWLimit )
						{
							//Console.WriteLine( item.Intensity );
							item.OKNG = "LOW";
							SetLowColor( indexingImage , item.Hindex , item.Windex );
							targetimg.Draw( item.BoxData.ExpendRect( thickness + 1 ) , ApLowColor , thickness );
							//targetimg.Draw( item.BoxData.ExpendRect(-(thickness+1)) , ApLowColor , thickness);
						}
						else if ( item.Intensity > PData.IntenSumUPLimit )
						{
							item.OKNG = "OVER";
							SetOverColor( indexingImage , item.Hindex , item.Windex );
							targetimg.Draw( item.BoxData.ExpendRect( ( thickness + 1 ) ) , ApOverColor , thickness );
						}
					}
						idx++;
				}
				}
				catch ( Exception ex)
				{
					Console.WriteLine( "Error Pos : {0}" , idx );
					Console.WriteLine( ex.ToString() );
				}

			} );
		}

	}
}
