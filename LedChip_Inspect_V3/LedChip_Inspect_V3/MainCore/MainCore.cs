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
using static WaferandChipProcessing.MainCore_Extension;

namespace WaferandChipProcessing
{
    public partial class MainCore
    {
        #region Init
        public MainCore()
        {
            PData = new ImgPData();
            
        }
        #endregion


        public int BackgroundInten(Image<Gray,byte> img) {
            // cluster has error
            double min = 255;
            foreach ( var item in ClusterImg( img.ToBitmap() )["center"] )
            {
                if ( item[0] < min ) min = item[0];
            }
            return (int)min;
        }

		void SetFailColor( byte [ , , ] failchipDisplayData , int j , int i )
		{
			var color = new byte[] {
				( byte ) ( failchipDisplayData[j , i , 0] * 0.3 ),
			( byte )( failchipDisplayData [ j , i , 1 ] * 0.5 ),
			200};

			ColoringArea( ref failchipDisplayData , j , i , 3 , color ); // red

			//failchipDisplayData[j , i , 0] = ( byte ) ( failchipDisplayData[j , i , 0] * 0.3 );
			//failchipDisplayData[j , i , 1] = ( byte ) ( failchipDisplayData[j , i , 1] * 0.5 );
			//failchipDisplayData[j , i , 2] = 200;

		}

        void SetLowColor (byte[,,] failchipDisplayData , int j , int i)
        {
			var color = new byte[] {
				200,
				( byte ) ( failchipDisplayData[j , i , 0] * 0.5 ),
			( byte )( failchipDisplayData [ j , i , 1 ] * 0.3 )};
			ColoringArea( ref failchipDisplayData , j , i , 3 , color ); // blue
			//failchipDisplayData [j , i , 0] =  200 ;
			//failchipDisplayData[j , i , 1] = ( byte ) ( failchipDisplayData[j , i , 1] * 0.5 );
			//failchipDisplayData[j , i , 2] = ( byte ) ( failchipDisplayData[j , i , 2] * 0.3);
		}

        void SetOverColor (byte[,,] failchipDisplayData , int j , int i)
        {
			var color = new byte[] {
				( byte ) ( failchipDisplayData[j , i , 0] * 0.3 ),
			200,
				( byte ) ( failchipDisplayData[j , i , 2] * 0.5)};
			ColoringArea( ref failchipDisplayData , j , i , 3 , color ); // green
			//failchipDisplayData [j , i , 0] = ( byte ) ( failchipDisplayData[j , i , 0] * 0.3 );
			//failchipDisplayData[j , i , 1] = 200 ;
			//failchipDisplayData[j , i , 2] = ( byte ) ( failchipDisplayData[j , i , 2] * 0.5);
		}

        #region Save & Load

        //public void SaveImg ( Image<Bgr,byte> img , string path )
        //{
        //    img.Save( path );
        //}

        public void SaveImg( dynamic img , string path )
        {
            img.Save( path );
        }

        public void SaveData( ImgPResult result , string path) {
            string delimiter = ",";
            StringBuilder csvExport = new StringBuilder(); //
            // 1
            csvExport.Append( "Result" );
            csvExport.Append( delimiter );
            csvExport.Append( " " );
            csvExport.Append( delimiter );
            csvExport.Append( " " );
            csvExport.Append( delimiter );
            csvExport.Append( " " );
            csvExport.Append( delimiter );
            csvExport.Append( "Condition" );
            csvExport.Append( delimiter );
            csvExport.Append( Environment.NewLine );

            //2
            csvExport.Append( " " );
            csvExport.Append( delimiter );
            csvExport.Append( "Total Chip number" );
            csvExport.Append( delimiter );
            csvExport.Append( result.ChipTotalCount.ToString() );
            csvExport.Append( delimiter );
            csvExport.Append( " " );
            csvExport.Append( delimiter );
            csvExport.Append( "AreaUPLimit" );
            csvExport.Append( delimiter );
            csvExport.Append( result.AreaUpLimit.ToString() );
            csvExport.Append( Environment.NewLine );

            //3
            csvExport.Append( " " );
            csvExport.Append( delimiter );
            csvExport.Append( "OK" );
            csvExport.Append( delimiter );
            csvExport.Append( result.ChipPassCount.ToString() );
            csvExport.Append( delimiter );
            csvExport.Append( " " );
            csvExport.Append( delimiter );
            csvExport.Append( "AreaDWLimit" );
            csvExport.Append( delimiter );
            csvExport.Append( result.AreaDwLimit.ToString() );
            csvExport.Append( Environment.NewLine );

            //4
            csvExport.Append( " " );
            csvExport.Append( delimiter );
            csvExport.Append( "NG" );
            csvExport.Append( delimiter );
            csvExport.Append( result.ChipTotalNgCount.ToString() );
            csvExport.Append( delimiter );
            csvExport.Append( " " );
            csvExport.Append( delimiter );
            csvExport.Append( "IntensityUPLimit" );
            csvExport.Append( delimiter );
            csvExport.Append( result.IntenUpLimit.ToString() );
            csvExport.Append( Environment.NewLine );

            //5
            csvExport.Append( " " );
            csvExport.Append( delimiter );
            csvExport.Append( "No Signal" );
            csvExport.Append( delimiter );
            csvExport.Append( result.ChipNOPLCount.ToString() );
            csvExport.Append( delimiter );
            csvExport.Append( " " );
            csvExport.Append( delimiter );
            csvExport.Append( "IntensityDWLimit" );
            csvExport.Append( delimiter );
            csvExport.Append( result.IntenDwLimit.ToString() );
            csvExport.Append( Environment.NewLine );

            //6
            csvExport.Append( " " );
            csvExport.Append( delimiter );
            csvExport.Append( "Low" );
            csvExport.Append( delimiter );
            csvExport.Append( result.ChipLowCount.ToString() );
            csvExport.Append( Environment.NewLine );

            //7
            csvExport.Append( " " );
            csvExport.Append( delimiter );
            csvExport.Append( "Over" );
            csvExport.Append( delimiter );
            csvExport.Append( result.ChipOverCount.ToString() );
            csvExport.Append( Environment.NewLine );

            //8
            csvExport.Append( Environment.NewLine );

            //9
            
            csvExport.Append( "Y " );
            csvExport.Append( delimiter );
            csvExport.Append( "X " );
            csvExport.Append( delimiter );
            csvExport.Append( "Y Error " );
            csvExport.Append( delimiter );
            csvExport.Append( "X Error " );
            csvExport.Append( delimiter );
            csvExport.Append( "OK/NG/LOW/OVER" );
            csvExport.Append( delimiter );
            csvExport.Append( "Size" );
            csvExport.Append( delimiter );
            csvExport.Append( "Integrated Intensity" );
            csvExport.Append( delimiter );
            csvExport.Append( Environment.NewLine );

            csvExport.Append( "(Row)" );
            csvExport.Append( delimiter );
            csvExport.Append( "(Column)" );
            csvExport.Append( delimiter );
            csvExport.Append( "(pixel)" );
            csvExport.Append( delimiter );
            csvExport.Append( "(pixel)" );
            csvExport.Append( delimiter );
            csvExport.Append( " " );
            csvExport.Append( delimiter );
            csvExport.Append( "(pixel^2)" );
            csvExport.Append( delimiter );
            csvExport.Append( "(a.u)" );
            csvExport.Append( delimiter );
            csvExport.Append( Environment.NewLine );

			result.OutData = result.OutData.OrderBy( x => x.Hindex ).ThenBy( x => x.Windex ).ToList();

			for ( int i = 0 ; i < result.OutData.Count ; i++ )
            {
                csvExport.Append( result.OutData[i].Hindex+1);
                csvExport.Append( delimiter );
                csvExport.Append( result.OutData[i].Windex+1 );
                csvExport.Append( delimiter );
                csvExport.Append( result.OutData[i].HindexError );
                csvExport.Append( delimiter );
                csvExport.Append( result.OutData[i].WindexError );
                csvExport.Append( delimiter );
                csvExport.Append( result.OutData[i].OKNG);
                csvExport.Append( delimiter );
                csvExport.Append( result.OutData[i].ContourSize );
                csvExport.Append( delimiter );
                //csvExport.Append( result.OutData[i].Intensity / result.OutData[i].ContourSize);
                csvExport.Append( result.OutData[i].Intensity );
                csvExport.Append( Environment.NewLine );
            }
            System.IO.File.WriteAllText( path , csvExport.ToString() );
        }
        #endregion
    }
}
