using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaferandChipProcessing
{
	using static System.Math;
	public static class Epi_Helper
	{
		public static double VectorLen( double a , double b )
				=> Pow( a * a + b * b , 0.5 );
	}
}
