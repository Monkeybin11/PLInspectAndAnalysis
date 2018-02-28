using Emgu.CV;
using Emgu.CV.Structure;
using EmguCV_Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaferandChipProcessing
{

	public interface PreProcFunc
	{
		Func<Image<Gray , byte> , Image<Gray , byte>> EpiProc_Side { get; set; }
		Func<Image<Gray , byte> , Image<Gray , byte>> EpiProc_Mid { get; set; }
		Func<Image<Gray , byte> , Image<Gray , byte>> EpiProc_MidTop { get; set; }

	}

	public class Veeco6Inch583 : PreProcFunc
	{
		Func<Image<Gray , byte> , Image<Gray , byte>> EpiCommonProcessing
		=> src =>
		{
			return src.Inverse()
					  .HistEqualize()
					  .Median( 5 )
					  .Median( 5 );
		};

		public Func<Image<Gray , byte> , Image<Gray , byte>> EpiProc_Mid
		{
			get
			{
				return src => EpiCommonProcessing( src )
								  .Threshold( 235 )
								  .OpenCross();
			}
			set { }
		}

		public Func<Image<Gray , byte> , Image<Gray , byte>> EpiProc_MidTop
		{
			get
			{
				return src => EpiCommonProcessing( src )
					.Threshold( 240 )
					.OpenCross();
			}
			set { }
		}

		public Func<Image<Gray , byte> , Image<Gray , byte>> EpiProc_Side
		{
			get
			{
				return src => EpiCommonProcessing( src )
					.Threshold( 235 )
					.OpenCross();
			}
			set { }
		}
	}

	public class Veeco6Inch583Scattering06_02 : PreProcFunc
	{
		Func<Image<Gray , byte> , Image<Gray , byte>> EpiCommonProcessing
		=> src =>
		{
			return src.Threshold(120);
		};

		public Func<Image<Gray , byte> , Image<Gray , byte>> EpiProc_Mid
		{
			get
			{
				return src => EpiCommonProcessing( src );
			}
			set { }
		}

		public Func<Image<Gray , byte> , Image<Gray , byte>> EpiProc_MidTop
		{
			get
			{
				return src => EpiCommonProcessing( src );
			}
			set { }
		}

		public Func<Image<Gray , byte> , Image<Gray , byte>> EpiProc_Side
		{
			get
			{
				return src => EpiCommonProcessing( src );
			}
			set { }
		}
	}

	public class Veeco6Inch583PL06_02 : PreProcFunc
	{
		Func<Image<Gray , byte> , Image<Gray , byte>> EpiCommonProcessing
		=> src =>
		{
			return src;
		};

		public Func<Image<Gray , byte> , Image<Gray , byte>> EpiProc_Mid
		{
			get
			{
				return src => EpiCommonProcessing( src )
								.Inverse()
								.Gamma( 2 )
								.Gamma( 2 )
								//.Median( 5 )
								.Brightness( 1 , 50 )
								.Gamma( 2 )
								.Brightness( 1 , 50 )
								.Gamma( 2 )
								.Brightness( 1 , 50 )
								.Gamma( 2 )
								.Brightness( 1 , 50 )
								.Gamma( 2 )
								.Normalize( 120 )
								.Gamma( 2 )
								.Gamma( 2 )
								.Median( 3 )
								.Threshold( 120 )
								.DilateRect()
								.DilateRect()
								.DilateRect();
			}
			set { }
		}

		public Func<Image<Gray , byte> , Image<Gray , byte>> EpiProc_MidTop
		{
			get
			{
				return src => EpiCommonProcessing( src )
								.Inverse()
								.Gamma( 2 )
								.Gamma( 2 )
								.Brightness( 1 , 50 )
								.Gamma( 2 )
								.Brightness( 1 , 50 )
								.Gamma( 2 )
								.Brightness( 1 , 50 )
								.Gamma( 2 )
								.Brightness( 1 , 50 )
								.Gamma( 2 )
								.Normalize( 120 )
								.Gamma( 2 )
								.Gamma( 2 )
								
								.Threshold( 120 )
								.ErodeCross()
								.DilateRect()
								.DilateRect()
								.DilateRect().Act( x => Console.WriteLine(x.Width) );
			}
			set { }
		}

		public Func<Image<Gray , byte> , Image<Gray , byte>> EpiProc_Side
		{
			get
			{
				return src => EpiCommonProcessing( src )
								.Inverse()
								.Gamma( 2 )
								.Gamma( 2 )
								//.Median( 5 )
								.Brightness( 1 , 50 )
								.Gamma( 2 )
								.Brightness( 1 , 50 )
								.Gamma( 2 )
								.Brightness( 1 , 50 )
								.Gamma( 2 )
								.Brightness( 1 , 50 )
								.Gamma( 2 )
								.Normalize( 120 )
								.Gamma( 2 )
								.Gamma( 2 )
								.Median( 3 )
								.Threshold( 120 )
								.DilateRect()
								.DilateRect()
								.DilateRect();
			}
			set { }
		}
	}







}
