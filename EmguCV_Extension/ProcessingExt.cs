using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeedyCoding;

namespace EmguCV_Extension
{
	// --- Based  SpeedyCoding  ----
	class ProcessingExt
	{
		public static Func<double , double , double [ , , ]> FnEstChipPos_4PointP_rhombus( double [ ] realLT , double [ ] realLB , double [ ] realRT , double [ ] realRB )
		{
			var createEsted = new Func<double , double , double[,,]>( (double hChipN,double wChipN) =>
			{
				double[,,] output = new double[(int)hChipN , (int)wChipN,2];



				// wsplit num , hsplit num
				var leftSplitedY = realLT[0].xRange(
											(int)hChipN ,
											( realLB[0] - realLT[0])/hChipN);

				var rghtSplitedY = realRT[0].xRange(
											(int)hChipN ,
											( realRB[0] - realRT[0])/hChipN);

				var lEq = Calc_YXAxis(realLT , realLB);
				var rEq = Calc_YXAxis(realRT , realRB);

				// (y,x)
				var leftXY = leftSplitedY.Select( y => new double[] { y , lEq[0]*y + lEq[1] } ).ToList();
				var rghtXY = rghtSplitedY.Select( y => new double[] { y , rEq[0]*y + rEq[1] } ).ToList();



				// [ List(yl,xl) , List(yr,xr) ]
				var zippedSplited = leftXY.Zip(rghtXY , (f,s) => new { L = f , R = s } ).ToArray(); 

				// List of gradient of each singleline 
				var gradientList = zippedSplited.Select( x => Calc_XYAxis( x.L , x.R )).ToArray();


				int count = zippedSplited.Count();

				var res = zippedSplited.Select((crd,i) =>
				{
					double step = (crd.R[1] - crd.L[1])/count;

					var xlist = crd.L[1].xRange(count , step ).ToList();

					var ylist = xlist.Select( x => gradientList[i][0]* (i*step + zippedSplited[i].L[1]) + gradientList[i][1]).ToList();

					var singleLineZiped = ylist.Zip(xlist , (y,x) => new { Y = y , X = x } ).ToArray();

					return singleLineZiped;
				} ).ToList();
				//(int)hChipN , (int)wChipN,2];
				for (int j = 0; j < res.Count; j++)
				{
					for (int i = 0; i < res[j].Length; i++)
					{
						var x = res[j][i].X;
						var y = res[j][i].Y;
						output[i,j,0] = y;
						output[i,j,1] = x;
					}
				}		
				return output;
			} );
			return createEsted;
		}


		static double [ ] Calc_YXAxis( double [ ] first , double [ ] second )
		{
			( second [ 1 ] - first [ 1 ] ).Print( "x " );
			( second [ 0 ] - first [ 0 ] ).Print( " y " );

			double gradient = (second[1] - first[1])/(second[0] - first[0]);
			double biasf = first[1] - gradient * first[0];
			double biass = second[1] - gradient * second[0];
			return new double [ ] { gradient , ( biasf + biass ) / 2.0 };
		}
		static double [ ] Calc_XYAxis( double [ ] first , double [ ] second )
		{
			double gradient = (second[0] - first[0])/(second[1] - first[1]);
			double bias = first[0] - gradient * first[1];
			return new double [ ] { gradient , bias };
		}


	}
}
