using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Forms;
using static Emgu.CV.CvEnum.Inter;
using System.Diagnostics;
using EmguCV_Extension;
using SpeedyCoding;


namespace WaferandChipProcessing
{
	using static System.Math;
	using static Epi_Helper;
	using static WaferandChipProcessing.EpiProcessingParameter;
	/// <summary>
	/// Method container called by mainform
	/// </summary>
	public partial class EpiCore
    {
        public event ImgForDisplay evtDroppedImg;
		


        public EpiCore( int waferInxImgsize )
        {

            EpiGrayImgDic = new Dictionary<ImgIdxPos , Image<Gray , byte>>(); // Setted From Tuple
            EpiColorImgDic = new Dictionary<ImgIdxPos , Image<Bgr , byte>>(); // Setted ToDic

            Pos2EnumTable = new Dictionary<Tuple<int? , int?> , ImgIdxPos>(); // Setted Below
            EpiProcResultDict = new Dictionary<ImgIdxPos , EpiDataResult>(); // Setted Below

            // Create Dictionary < (position) , ImagePos Enum >         
            var pairEnumPos = Enum.GetValues(typeof(ImgIdxPos))
                                       .Cast<ImgIdxPos>()
                                       .ToArray()
                                       .ZipFlattenReshape(2, 3)
                                       .ActLoop(x =>
                                                Pos2EnumTable.Add(
                                                        Tuple.Create( (int?)x.Item2, (int?)x.Item3)
                                                        , x.Item1));
            // Regist Event on 
            EpiSeperatedImgTrsEvt = pairEnumPos
                         .Select( pos => new EpiSeperatedImgEvent( pos.Item2 , pos.Item3 ) )
                         .ToArray();

            waferIndexImgSize = waferInxImgsize;

            // Create Global Func
            Create_GlobalFunc();

            IndexViewImg = DrawWafer( new Image<Bgr , byte>( waferIndexImgSize , waferIndexImgSize )
                                      .Inverse() );

            Create_EpiProcessMethodList();
        }

		public async void EpiProcessing( int resolution ,int areaUpLimit ,int areaDwlimit, int outerexclude = 0 )
        {
            try
            {
                Stopwatch stw  = new Stopwatch();
                stw.Start();
                SampleResolution = resolution;
				if ( outerexclude > 0 ) OuterExcludeSize = outerexclude;
				AreaUp = areaUpLimit;
				AreaDw = areaDwlimit;

				var totalImgSize = EpiGrayImgDic[ImgIdxPos.TL].Width
                                   + EpiGrayImgDic[ImgIdxPos.TM].Width
                                   + EpiGrayImgDic[ImgIdxPos.TR].Width;

                if ( EpiGrayImgDic.Count != 6 ) return;
                ResetData();
                EpiProcedImgDic = EpiColorImgDic.ToDictionary( x => x.Key
                                                             , x => x.Value.Normalize( 64 ).Gamma( 1.4 ) );
                
                    Offset = CreateOffset( EpiGrayImgDic [ ImgIdxPos.TL ].Height
                              , EpiGrayImgDic [ ImgIdxPos.TL ].Width
                              , EpiGrayImgDic [ ImgIdxPos.TL ].Width + EpiGrayImgDic [ ImgIdxPos.TM ].Width );

                var dfInfoDic = EpiGrayImgDic.ToDictionary(
                                                 dic => dic.Key
                                               , dic => EpiProcFnList[ dic.Key.ImgIdx2EpiMethod() ]
                                                                         ( dic.Value ) )

                                             .ToDictionary(
                                                 dic => dic.Key
                                               , dic =>
                                                            dic.Value.FindContour(AreaUp, AreaDw))

                                             .ToDictionary(
                                                 dic => dic.Key
                                               , dic => dic.Value.FindDefectInfo()); //a
                                                      
                
                List<Task> resultTaskList = new List<Task>();
                
                    Enum.GetValues( typeof( ImgIdxPos ) )
                        .Cast<ImgIdxPos>()
                        .ActLoop( pos =>
                        {
                            resultTaskList.Add( Task.Run( ( Action )( () =>
                                                       DrawCircleSequance(
                                                            EpiProcedImgDic [ pos ] // Destination
                                                            , dfInfoDic [ pos ] // box list
                                                            , dfInfoDic [ pos ].Count() ) ) ) );
                        } )
                        .ActLoop( pos =>
                        {
                            resultTaskList.Add( Task.Run( ( Action )( () =>
                                                               CreateResult(
                                                                   EpiProcResultDict [ pos ].DefectList // Destination
                                                                   , dfInfoDic [ pos ]                  // Src
                                                                   , dfInfoDic [ pos ].Count()          
                                                                   , SampleResolution ) ) ) );
                        } );
                Task all = Task.WhenAll(resultTaskList.ToArray());

                try
                {
                    await all;   
                    // Result Combine
                    var combined =Enum.GetValues( typeof( ImgIdxPos ) )
                                   .Cast<ImgIdxPos>()
                                   .Select( pos => EpiProcResultDict [ pos ].DefectList
                                                     .ShiftDefectData( Offset , pos , SampleResolution) )
                                   .Aggregate( ( f , s ) => f.Concate_H( s ) )
								   .Where( x => VectorLen( (x.CenterX-Offset[OffsetPos.Row1]),(x.CenterY-Offset[OffsetPos.Row1]) ) < Offset[OffsetPos.Row1] - OuterExcludeSize ) 
								   .Select( x => x)
								   .ToList();
                                   //.Act( x=> evtProgressTime( 85 )); // 컴바인 result 

                     
                    EpiProcResult_FullScale.DefectList = new List<DefectData>( combined ); 
                    EpiProcResul_IdxScale.DefectList = combined.Convert2IdxPos( totalImgSize , totalImgSize , waferIndexImgSize , SampleResolution );
                    IndexViewImg = IndexViewImg.DrawIdxDefect( EpiProcResul_IdxScale.DefectList ); // Draw Index View Image

					/* Start Side Effect */
					evtTrsIdxImg( IndexViewImg );
                    evtTrsResizedProcedImg( Origin2ResizedImg( (double)waferIndexImgSize  / (double)50000
                                                        , EpiColorImgDic.First().Value.Width
                                                        , EpiColorImgDic.First().Value.Height
                                                        , EpiProcedImgDic )
                                            .StackSplitted());

                    CreateStatisticResult( EpiProcResult_FullScale );
                    evtStatistic( new int [ ] { EpiProcResult_FullScale.Size1Number
                                                , EpiProcResult_FullScale.Size2Number 
                                                , EpiProcResult_FullScale.Size3Number 
                                                , EpiProcResult_FullScale.Size4Number } );
                    stw.Stop();
                    stw.ElapsedMilliseconds.Print(" Processing Time");
                    evtProcTime( (int)stw.ElapsedMilliseconds );
                }
                catch ( Exception e )
                {
                    e.ToString().Print( " Main Processing Error " );
                }
            }
            catch ( Exception e )
            {
                e.ToString().Print( " Main Processing Error " );
            }
        }


        public void SetImageList()
        {
            new FolderBrowserDialog()
                .Act( ths => ths.RootFolder = Environment.SpecialFolder.MyComputer ) // for test
                .Act( ths => ths.SelectedPath = @"D:\03JobPro\2017\05_veeco\Mapping Data\5um Data 0602\VP_538\Resized" )// for test
                .Act( ofd =>
                        {
                            if ( ofd.ShowDialog() == DialogResult.OK )
                            {
                                string[] imgpathslist = Directory.GetFiles(ofd.SelectedPath, "*.bmp");
                                var imgpaths = imgpathslist
                                                .Select(Path.GetFileName)
                                                .ToArray();

                                if ( imgpaths.GetLength( 0 ) != 6 )
                                {
                                    MessageBox.Show( "Please check files or folder" );
                                    SetImageList();
                                }
                                else
                                {
                                    imgpathslist.ActLoop( path =>
                                                             EpiColorImgDic.Add( path.TrimFileNameOnly()
                                                                                    .Map2ImgPos()
                                                                                , new Image<Bgr , byte>( path ) ) );

                                    evtTrsFullImg(
                                           imgpathslist.Map2ImgZipedPos()
                                                   .ActLoop( pair => EpiGrayImgDic.Add( pair.Item1 , pair.Item2 ) )
                                                   .ToArray() );
                                }
                            }
                        } );
        }

        public void SetImage( Nullable<int> row , Nullable<int> col , string path )
        {
            var pos = Pos2EnumTable[Tuple.Create(row, col)];
            var testimg = new Image<Bgr, byte>(path);

            

            EpiColorImgDic.Add( pos
                                , new Image<Bgr , byte>( path ) );

            new Image<Gray , byte>( path )
                    .Act( img => EpiGrayImgDic.Add(
                                     pos
                                     , img ) )
                    .Act( img => evtDroppedImg(
                                    pos
                                    , img ) );
        }

        public Dictionary<OffsetPos , int> CreateOffset( int row1 , int col1 , int col2 )
        {
            var output =  new Dictionary<OffsetPos , int>();
            output.Add( OffsetPos.Row1 , row1 );
            output.Add( OffsetPos.Col1 , col1 );
            output.Add( OffsetPos.Col2 , col2 );
            return output;
        }
        
        #region Internal Method

        private Func<double , double , double
                    , Dictionary<ImgIdxPos , Image<Bgr , byte>>
                    , Dictionary<ImgIdxPos , Image<Bgr , byte>>> Origin2ResizedImg
            => ( ratio , h , w , src ) =>
            {
                return src.MapDictionary( x =>
                                            x.Resize(
                                                ( int )( w * ratio )
                                                , ( int )( h * ratio )
                                                , Inter.Linear ) );


            };  // out resized dic

        private Func<Dictionary<ImgIdxPos , Image<Bgr , byte>> , Image<Bgr , byte>> StackEpiImg
            => splited =>
            {
                return splited [ ImgIdxPos.TL ]
                        .HStack( splited [ ImgIdxPos.TM ] )
                        .HStack( splited [ ImgIdxPos.TR ] )
                        .VStack( splited [ ImgIdxPos.BL ]
                                 .HStack( splited [ ImgIdxPos.BM ] )
                                 .HStack( splited [ ImgIdxPos.BR ] ) ); // out stacked img
            };



        #region FindDefect Helper
        //void DrawBoxSequance( Image<Bgr , byte> dst, List<RotatedRect> rects , int iterNum)
        //{
        //    if ( rects == null ) return;
        //    dst = dst?.DrawRotatedRect( rects [ iterNum - 1 ] , new Bgr(50,240,10));
        //    if ( iterNum == 0 ) return;
        //    DrawBoxSequance( dst , rects , iterNum - 1 );
        //}

        void DrawBoxSequance( Image<Bgr , byte> dst , List<RotatedRect> rects , int iterNum )
        {
            for ( int i = 0 ; i < rects.Count ; i++ )
            {
                var points = CvInvoke.BoxPoints(rects [i]);
                var lines = Enumerable.Range(0 , points.GetLength(0))
                            .Select( j =>
                                        new LineSegment2DF( points[j] , points [ ( j + 1 ) % 4 ] ) )
                            .ToArray();
                foreach ( var line in lines )
                {
                    dst.Draw( line , new Bgr( 50 , 240 , 10 ) , 2 );
                }
            }
        }

        void DrawCircleSequance( Image<Bgr , byte> dst , List<RawDefectInfo> defectlist , int iterNum )
        {
            foreach ( var df in defectlist )
            {
                MCvScalar color;
                //if(df.Size < 3000 )
                //{
                //    color = new MCvScalar( 0 , 255 , 0 );
                //}
                //else if ( df.Size < 4000 )
                //{
                //    color = new MCvScalar( 255 , 0 , 0 );
                //}
                //else if ( df.Size < 5000 )
                //{
                //    color = new MCvScalar( 0 , 0 , 255 );
                //}
                //else
                //{
                //    color = new MCvScalar( 0 , 255 , 255 );
                //}
                color = new MCvScalar( 0 , 0 , 255 );
                CvInvoke.Circle( dst
                                , new System.Drawing.Point( (int)df.CenterX , (int)df.CenterY) 
                                , (int)df.Radius
                                , color
                                , 2);
            }
        }

        void CreateResult( List<DefectData> dst , List<RawDefectInfo> info , int iterNum , int resolution)
        {
            dst.Clear();
            for ( int i = 0 ; i < info.Count ; i++ )
            {
                dst.Add( info[i].Cvt2DefectData( resolution ) );
            }
        }

        void CreateStatisticResult( EpiDataResult src)
        {
            // Check With RealSize
            var sizelist = src.DefectList.Select( x => x.RealSize  ).ToList();
            
            for ( int i = 0 ; i < sizelist.Count ; i++ )
            {
                if (sizelist[i] < src.Size1)
                {
                    src.Size1Number++;
                    continue;
                }

                if ( sizelist [ i ] < src.Size2 )
                {
                    src.Size2Number++;
                    continue;
                }

                if ( sizelist [ i ] < src.Size3 )
                {
                    src.Size3Number++;
                    continue;
                }
                src.Size4Number++;
            }
           
        }

        #endregion

        #endregion

        #region Ssave

        public void SaveEpiImage(string basepath)
        {
            try
            {
                basepath = basepath + "_";

                foreach ( var item in EpiProcedImgDic )
                {
                    string name =  Enum.GetName(typeof(ImgIdxPos),item.Key);
                    item.Value.Save( basepath + name + "_splited.bmp" );
                }
                IndexViewImg.Save( basepath + "Index.bmp" );
            }
            catch ( Exception e)
            {
                e.ToString().Print( " SsveEpiImage Error" ); ;
            }
            
        }

        public void SaveEpiResult(string path)
        {
            try
            {

            var dftlist = EpiProcResult_FullScale.DefectList;
            var number = EpiProcResult_FullScale;
            string delimiter = ",";
            StringBuilder csvExport = new StringBuilder();
                csvExport.Append( "< 10 (um)" );
                csvExport.Append( delimiter );
                csvExport.Append( ( int )number.Size1Number );
                csvExport.Append( Environment.NewLine );
                csvExport.Append( "< 30 (um)" );
                csvExport.Append( delimiter );
                csvExport.Append( ( int )number.Size2Number );
                csvExport.Append( Environment.NewLine );
                csvExport.Append( " < 100(um)" );
                csvExport.Append( delimiter );
                csvExport.Append( ( int )number.Size3Number );
                csvExport.Append( Environment.NewLine );
                csvExport.Append( " > 100(um)" );
                csvExport.Append( delimiter );
                csvExport.Append( ( int )number.Size4Number );
                csvExport.Append( Environment.NewLine );

            csvExport.Append( "Y (um) " );
            csvExport.Append( delimiter );
            csvExport.Append( "X (um)" );
            csvExport.Append( delimiter );
            csvExport.Append( "Size (um^2)" );
            csvExport.Append( delimiter );
            csvExport.Append( Environment.NewLine );

            for ( int i = 0 ; i < dftlist.Count ; i++ )
            {
                csvExport.Append( (int)dftlist[ i ].RealY  );
                csvExport.Append( delimiter );
                csvExport.Append( (int)dftlist [ i ].RealX );
                csvExport.Append( delimiter );
                csvExport.Append( (int)dftlist [ i ].RealSize );
            
                csvExport.Append( Environment.NewLine );
            }
            System.IO.File.WriteAllText( path , csvExport.ToString() );

            }
            catch ( Exception e)
            {
                e.ToString().Print( " SsveEpiResult Error" );
            }
        }

		#endregion
	}
}
