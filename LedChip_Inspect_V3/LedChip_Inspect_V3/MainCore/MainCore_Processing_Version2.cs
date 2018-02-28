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
using static ModelLib.AmplifiedType.Handler;
using static EmguCV_Extension.Vision_Tool;
using static EmguCV_Extension.Preprocessing;
using static Util_Tool.UI.Corrdinate;
using static System.Console;
using static System.Linq.Enumerable;
using System.Diagnostics;
using System.Drawing;
using EmguCV_Extension;
using ModelLib.AmplifiedType;
using Unit = System.ValueTuple;
using static ModelLib.AmplifiedType.PartialApplication;

using SpeedyCoding;

namespace WaferandChipProcessing
{
	using static WaferandChipProcessing.Handler;
	public partial class MainCore
	{
		public Action<Image<Gray , byte> , Image<Bgr , byte>> ProcessingStep1_Version2(
			int threshold ,
			SampleType sampletype ,
			int cHnum ,
			int cWnum ,
			bool whiteGrid ,
			bool debugmode = false )
		{
			return ( originalimg , colorimg ) =>
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
						PData.IntenSumDWLimit );

					VectorOfVectorOfPoint contours;

					if ( debugmode )
					{
						contours = baseimg
									 .Map( Proc_Method_List [ sampletype ] )
									 .Act( img => img.Save( TestFileSavePath.BasePath + "\\beforcntr.bmp" ) )
									 .Map( FindContour)
									 .Map( Sortcontours);
					}
					else
					{
						contours = baseimg
									 .Map( Proc_Method_List [ sampletype ] )
									 .Map( FindContour )
									 .Map( Sortcontours );
					}
					stw2.ElapsedMilliseconds.Print( "Pre Contour : " );
					stw2.Restart();
				
		
					var centerMoment = contours.Map( CalcCenter);
					stw2.ElapsedMilliseconds.Print( "moment : " );
					stw2.Restart();
					var boxlist = contours.Map( ApplyBox );
				
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

					var estRes = EstedChipPos_Ver2( cHnum , cWnum ); // Y , X
									
					var estedChipP =  estRes.IndexPos
										 .Act( est => DrawCenterPoint( color_visual_img , est ) );

					// Index list pair with boxlist
					var indexres = GetIndexOf(APBoxTolerance ,boxlist , estRes.HLineEQs , estRes.VLineEQs );

					//var idxtemp = indexres.ToArray();
					//
					//for ( int i = 0 ; i < boxlist.Len() ; i++ )
					//{
					//	Console.WriteLine( $"index {idxtemp [ i].Value.j} , {idxtemp [ i].Value.i}    Box : {boxlist[i].Y}  {boxlist [ i ].X}" );
					//}

					var resultGenerator = ImportResult.Apply( estRes.IndexPos )
													  .Apply( boxlist)
													  .Apply( indexres );
					
					var drawing = DrawProcIdx.Apply(LineThickness)
											 .Apply(color_visual_img)
											 .Apply(indexingImage);
					
					
					//var temp1 = NgResultInitializer(cHnum , cWnum);
					//var temp2 = resultGenerator(temp1);
					//
					//
					//var res2 = ImportResult( estRes.IndexPos , boxlist , indexres , temp1 );



					PResult.OutData = NgResultInitializer(cHnum , cWnum)
										.Map( resultGenerator )
										.Flatten()
										.ToList();

					PResult.OutData.ForEach( drawing );

					//// ==== result done  ====

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
			};
		}

        public Action<Image<Gray, byte>, Image<Bgr, byte>> ProcessingStep1_Version2_WithEL(
            int threshold,
            SampleType sampletype,
            int cHnum,
            int cWnum,
            bool whiteGrid,
            List<ELData> eldata ,
            bool debugmode = false
            )
        {
            return (originalimg, colorimg) =>
            {

                try
                { 
                    var color_visual_img = colorimg.Clone();
                    var color_visual_img2 = colorimg.Clone();
                    var baseimg = originalimg.Clone();
                    PResult = new ImgPResult(
                        PData.UPAreaLimit,
                        PData.DWAreaLimit,
                        PData.IntenSumUPLimit,
                        PData.IntenSumDWLimit);

                    VectorOfVectorOfPoint contours;

                    if (debugmode)
                    {
                        contours = baseimg
                                     .Map(Proc_Method_List[sampletype])
                                     .Act(img => img.Save(TestFileSavePath.BasePath + "\\beforcntr.bmp"))
                                     .Map(FindContour)
                                     .Map(Sortcontours);
                    }
                    else
                    {
                        contours = baseimg
                                     .Map(Proc_Method_List[sampletype])
                                     .Map(FindContour)
                                     .Map(Sortcontours);
                    }
                 


                    var centerMoment = contours.Map(CalcCenter);
                  
                    var boxlist = contours.Map(ApplyBox);

                    var color_visual_img3 = colorimg.Clone();
                    DrawBox(color_visual_img3, boxlist);
                    if (debugmode) color_visual_img3.Save(TestFileSavePath.BasePath + "\\AftercntrBox.bmp");

                    byte[,,] indexingImage = null;
                    if (whiteGrid)
                    { indexingImage = MatWhitePattern(cHnum, cWnum, 3); }
                    else
                    { indexingImage = MatPattern(cHnum, cWnum, 3); }


                    byte[,,] passfailPosData = new byte[cHnum, cWnum, 1];

                    var estRes = EstedChipPos_Ver2(cHnum, cWnum); // Y , X

                    var estedChipP = estRes.IndexPos
                                         .Act(est => DrawCenterPoint(color_visual_img, est));

                    // Index list pair with boxlist
                    var indexres = GetIndexOf(APBoxTolerance, boxlist, estRes.HLineEQs, estRes.VLineEQs);

                

                    var resultGenerator = ImportResult.Apply(estRes.IndexPos)
                                                      .Apply(boxlist)
                                                      .Apply(indexres);

                    var drawing = DrawProcIdx.Apply(LineThickness)
                                             .Apply(color_visual_img)
                                             .Apply(indexingImage);

                    PResult.OutData = NgResultInitializer(cHnum, cWnum)
                                        .Map(resultGenerator)
                                        .Flatten()
                                        .ToList();

                    PResult.OutData.ForEach(drawing);

                    //// ==== result done  ====

                    for (int i = 0; i < estedChipP.GetLength(0); i++)
                    {
                        for (int j = 0; j < estedChipP.GetLength(1); j++)
                        {
                            var cirp = new CircleF();
                            cirp.Center = new System.Drawing.PointF(
                                                         (float)(estedChipP[i, j, 1])
                                                         , (float)(estedChipP[i, j, 0]));

                            color_visual_img3.Draw(
                                cirp
                                , ApCenteBoxColor, 1);
                        }
                    }

                    if (debugmode) color_visual_img3.Save(TestFileSavePath.BasePath + "\\AftercntrBoxandEsted.bmp");

                    DrawCenterPoint(color_visual_img, estedChipP);
                    color_visual_img = DrawELData(color_visual_img, estedChipP, eldata);
                    UpdateResult(PResult)(indexingImage, color_visual_img);
                    evtProcessingResult();
                }
                catch (Exception er)
                {
                    System.Windows.Forms.MessageBox.Show(er.ToString());
                    evtProcessingDone(true);
                }
            };
        }

        Image<Bgr, byte> DrawELData(Image<Bgr,byte> src , double[,,] centrPoint , List<ELData> eldata)
        {
            var nglist = eldata.Where(x => x.Class == "NG").Select(x => x).ToList();
            foreach (var item in nglist)
            {
                var x = item.Xidx-1;
                var y = item.Yidx-1;

                CircleF cirp = new CircleF();
                cirp.Center = new PointF((float)(int)centrPoint[y, x, 1], (float)(int)centrPoint[y, x, 0]);
                src.Draw(cirp, new Bgr(10,255,255), 3);
            }
            return src;
        }



        public Action<Image<Gray , byte> , Image<Bgr , byte>> ProcessingStep1_Version3(
			int threshold ,
			SampleType sampletype ,
			int cHnum ,
			int cWnum ,
			bool whiteGrid ,
			bool debugmode = false )
		{
			return ( originalimg , colorimg ) =>
			{
				Stopwatch stwtotal = new Stopwatch();
				stwtotal.Start();

				for ( int qq = 0 ; qq < 10 ; qq++ )
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
							PData.IntenSumDWLimit );

						VectorOfVectorOfPoint contours;


						//color_visual_img.Save( @"E\:001_Job\016_Samsung_Display_second\Test" +\\beforcntr.bmp" );

						if ( debugmode )
						{
							contours = baseimg
										 .Map( Proc_Method_List [ sampletype ] )
										 .Act( img => img.Save( TestFileSavePath.BasePath + "\\beforcntr.bmp" ) )
										 .Map( FindContour )
										 .Map( Sortcontours );
						}
						else
						{
							contours = baseimg
										 .Map( Proc_Method_List [ sampletype ] )
										 .Map( FindContour )
										 .Map( Sortcontours );
						}
						stw2.ElapsedMilliseconds.Print( "Pre Contour : " );
						stw2.Restart();


						var centerMoment = contours.Map( CalcCenter);
						stw2.ElapsedMilliseconds.Print( "moment : " );
						stw2.Restart();
						var boxlist = contours.Map( ApplyBox );

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

						var estRes = EstedChipPos_Ver2( cHnum , cWnum ); // Y , X

						var estedChipP =  estRes.IndexPos
										 .Act( est => DrawCenterPoint( color_visual_img , est ) );

						// Index list pair with boxlist
						var indexres = GetIndexOf(APBoxTolerance ,boxlist , estRes.HLineEQs , estRes.VLineEQs );

						//var idxtemp = indexres.ToArray();
						//
						//for ( int i = 0 ; i < boxlist.Len() ; i++ )
						//{
						//	Console.WriteLine( $"index {idxtemp [ i].Value.j} , {idxtemp [ i].Value.i}    Box : {boxlist[i].Y}  {boxlist [ i ].X}" );
						//}

						var resultGenerator = ImportResult.Apply( estRes.IndexPos )
													  .Apply( boxlist)
													  .Apply( indexres );

						var drawing = DrawProcIdx.Apply(LineThickness)
											 .Apply(color_visual_img)
											 .Apply(indexingImage);


						//var temp1 = NgResultInitializer(cHnum , cWnum);
						//var temp2 = resultGenerator(temp1);
						//
						//
						//var res2 = ImportResult( estRes.IndexPos , boxlist , indexres , temp1 );



						PResult.OutData = NgResultInitializer( cHnum , cWnum )
											.Map( resultGenerator )
											.Flatten()
											.ToList();

						PResult.OutData.ForEach( drawing );

						//// ==== result done  ====

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
				}
				stwtotal.Stop();
				Console.WriteLine( stwtotal.ElapsedMilliseconds );
			};
		}

		private Action<int , Image<Bgr , byte> , byte [ , , ] , ExResult> DrawProcIdx
			=> ( thickness , targetimg , idximg , res )
			=>
			{
				switch ( res.OKNG )
				{
					case "OK":
						targetimg.Draw( res.BoxData , ApOkChipColor , 1 );
						break;

					case "NOPL":
						SetFailColor( idximg , res.Hindex , res.Windex );
						break;

					case "LOW":
						SetLowColor( idximg , res.Hindex , res.Windex );
						targetimg.Draw( res.BoxData , ApLowColor , 1 );
						//targetimg.Draw( res.BoxData.ExpendRect( -( thickness + 1 ) ) , ApLowColor , thickness );
						break;


					case "OVER":
						res.OKNG = "OVER";
						SetOverColor( idximg , res.Hindex , res.Windex );
						targetimg.Draw( res.BoxData , ApOverColor , 1 );
						//targetimg.Draw( res.BoxData.ExpendRect( ( thickness + 1 ) ) , ApOverColor , thickness );
						break;
				}
			};



		private ExResult [ ] [ ] NgResultInitializer( int h , int w )
		{
			var temp = Range( 0 , h ).Select( j =>
								 Range( 0 , w ).Select( i =>
						 				new ExResult( j , i )).ToArray() )
								   .ToArray();
			return temp;
		}
			
			

		private Func<double [ , , ] ,
				List<Rectangle> ,
				IEnumerable<Maybe<Indexji>> ,
				ExResult [ ] [ ] ,
				ExResult [ ] [ ]> ImportResult
			=> ( ested , boxlist , boxindices , src )
			=>
		{
			var updator = ResultUpdater.Apply(ested)
									   .Apply(src)
									   .Apply( Constrain(PData.IntenSumUPLimit , PData.IntenSumDWLimit));

			PairIndexRect( boxlist , boxindices )
						 .ForEach( updator );

			return src;
		};

		Action<double [ , , ], ExResult[][] , Constrain , IndexRect > ResultUpdater
			=> ( ested , src , constrain , idxrec )
			=> idxrec.Index.Match(
				() => Unit() ,
				idx =>
				{
					var j = idx.j;
					var i = idx.i;
					var ypos = ested[ j,i,0];
					var xpos = ested[ j,i,1];
					var rec = idxrec.Rectangle;
					var intenSum = SumInsideBox( rec );
					src [ j ] [ i ] = new ExResult( j , i
										 , ( int )ypos - ( int )( rec.Y + rec.Height / 2 )
										 , ( int )xpos - ( int )( rec.X + rec.Width / 2 )
										 , Classifier( intenSum  , constrain)
										 , intenSum
										 , rec.Width * rec.Height
										 , rec );
					var temp = src;
					return Unit();
				});

		private Func<double , Constrain , string> Classifier
			=> ( inten , constrain) 
			=> inten < constrain.DwInten/10 ? "NOPL" :
			   inten < constrain.DwInten    ? "LOW" :
			   inten > constrain.UpInten    ? "OVER":
										      "OK";

		private Func<   IEnumerable<Rectangle> ,
				    	IEnumerable<Maybe<Indexji>> ,
						IEnumerable<IndexRect> > PairIndexRect
			=>( rects , idxs )
				=> idxs.Zip( rects , ToIndexRect );

		private Func< Maybe<Indexji>,Rectangle,IndexRect> ToIndexRect
			=> ( idx , rec )
			=> new IndexRect() { Rectangle = rec , Index = idx };
	}

	public static class Handler
	{
		public static Constrain Constrain( int up , int dw ) => new Constrain() { UpInten = up , DwInten = dw };
	}

	public class IndexRect
	{
		public Rectangle Rectangle;
		public Maybe<Indexji> Index;
	}

	public class Constrain
	{
		public int UpInten;
		public int DwInten;
	}

}
