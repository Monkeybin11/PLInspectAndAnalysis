using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaferandChipProcessing
{
    public static class MainCore_Extension
    {
		public static void ColoringArea( ref byte [ , , ] src , int j , int i , int size , byte[] color)
		{
			int hlimit = src.GetLength(0);
			int wlimit = src.GetLength(1);

			for ( int jj = 0 ; jj < size ; jj++ )
			{
				for ( int ii = 0 ; ii < size ; ii++ )
				{
					if ( i + ii < wlimit && j + jj < hlimit )
					{
						src [ j+jj , i+ii , 0 ] = color[0];
						src [ j+jj , i+ii , 1 ] = color[1];
						src [ j+jj , i+ii , 2 ] = color[2];
					}
				}
			}



		}


        public static TResult Map<TSource, TResult> (
            this TSource @this ,
            Func<TSource , TResult> fn)
            => fn( @this );

        public static T Act<T> (
            this T @this ,
            Action<T> action)
        {
            action( @this );
            return @this;
        }
      
        /// <summary>
        /// Act Loop on Estimated Chip Data.  
        /// </summary>
        /// <param name="this"></param>
        /// <param name="boxlsit">BoxList for compare with chip Est data</param>
        /// <param name="centerPoints"></param>
        /// <param name="loopAct"></param>
        /// <returns></returns>
        public static double[,,] Act_LoopChipPos (
            this double[,,] @this,
            List<System.Drawing.Rectangle> boxlsit,
            System.Drawing.Point[] centerPoints,
            Action<int,int,double,double,List<System.Drawing.Rectangle>,System.Drawing.Point[]> loopAct , 
			bool isParallel = false)
        {
            try
            {
				if ( isParallel )
				{
					Parallel.For( 0 , @this.GetLength( 0 ) , j => {
						for ( int i = 0 ; i < @this.GetLength( 1 ) ; i++ ) // col
						{
							loopAct(
								j , i
								, @this [ j , i , 0 ]
								, @this [ j , i , 1 ]
								, boxlsit
								, centerPoints );
						}
					} );
				}
				else
				{
					for ( int j = 0 ; j < @this.GetLength( 0 ) ; j++ ) // row
					{
						for ( int i = 0 ; i < @this.GetLength( 1 ) ; i++ ) // col
						{
							loopAct(
								j , i
								, @this [ j , i , 0 ]
								, @this [ j , i , 1 ]
								, boxlsit
								, centerPoints );
						}
					}
				}
              
                return @this;
            }
            catch
            {
                return @this;
            }
        }

        // need to fix
        public static List<System.Drawing.Rectangle> GetRectList(
            this double[,,] @this,
            int hSize
            , int wSize)
        {
            int hlimit = @this.GetLength(0), wlimit = @this.GetLength(1);

            return Enumerable.Range(0, @this.GetLength(0))
                        .SelectMany(j => Enumerable.Range(0, @this.GetLength(1))
                                     , (j, i) => new System.Drawing.Rectangle( 
                                         (int)(@this[j, i, 1] - wSize/2 > 0 ? @this[j, i, 1] - wSize / 2 : 0)
                                         , (int)(@this[j, i, 0] - hSize/2 > 0 ? @this[j, i, 0] - hSize / 2 : 0)
                                         , @this[j, i, 1] + wSize / 2 <= wlimit ? (int)(@this[j, i, 1] + wSize / 2) : (int)(wlimit - @this[j, i, 1])
                                         , @this[j, i, 0] + hSize / 2 <= hlimit ? (int)(@this[j, i, 0] + hSize / 2) : (int)(hlimit - @this[j, i, 0])))
                        .ToList();
        }

        public static List<System.Drawing.Rectangle> GetRectList(
          this double [ , , ] @this ,
          int hSize
          , int wSize
          , int height
          , int width
          )
        {
            int hlimit = @this.GetLength(0), wlimit = @this.GetLength(1);

            return Enumerable.Range( 0 , @this.GetLength( 0 ) )
                        .SelectMany( j => Enumerable.Range( 0 , @this.GetLength( 1 ) )
                                     , ( j , i ) => new System.Drawing.Rectangle(
                                         ( int )( @this [ j , i , 1 ] - wSize / 2 > 0 ? @this [ j , i , 1 ] - wSize / 2 : 0 )
                                         , ( int )( @this [ j , i , 0 ] - hSize / 2 > 0 ? @this [ j , i , 0 ] - hSize / 2 : 0 )
                                         , @this [ j , i , 1 ] + wSize / 2 <= width ? wSize  : ( int )( width - @this [ j , i , 1 ] )
                                         , @this [ j , i , 0 ] + hSize / 2 <= height ? hSize  : ( int )( height - @this [ j , i , 0 ] ) ) )
                        .ToList();
        }


        public static System.Drawing.Point[] GetMomnetList(
           this double[,,] @this)
        {
            return Enumerable.Range(0, @this.GetLength(0))
                        .SelectMany(j => Enumerable.Range(0, @this.GetLength(1))
                                     , (j, i) => new System.Drawing.Point(i, j))
                        .ToArray();
        }



        public static System.Drawing.Rectangle ExpendRect (
            this System.Drawing.Rectangle @this ,
            int margin 
            )
        {
            return new System.Drawing.Rectangle(
                @this.X - margin
                , @this.Y - margin
                , @this.Width + margin * 2
                , @this.Height + margin * 2 );
        }

        public static System.Drawing.Rectangle ShurinkRect(
            this System.Drawing.Rectangle @this,
            int margin
            )
        {
            return new System.Drawing.Rectangle(
                @this.X + margin
                , @this.Y + margin
                , @this.Width - margin * 2
                , @this.Height - margin * 2);
        }


        //not working
        public static TResult CreateFn<TSrc,TResult> (
            this TSrc @this,
            Func<dynamic[],TResult> fn,
            params object[] parameter
            )
        {
            return fn( parameter );
        }


        public static TSource Measure_Act<TSource> (
                this TSource @this ,
                string msg ,
                int iter ,
                Action<TSource > fn)
        {
            Stopwatch stw = new Stopwatch();
            stw.Start();
            for ( int i = 0 ; i < iter ; i++ )
            {
                fn( @this );
            }
            stw.Stop();
            Console.WriteLine( msg + $"{stw.ElapsedMilliseconds / 1.0}"  );
            return @this;
        }

        public static TResult Measure_Map<TSource, TResult> (
                this TSource @this ,
                string msg ,
                int iter,
                Func<TSource , TResult> fn)
        {
            Stopwatch stw = new Stopwatch();
            stw.Start();
            for ( int i = 0 ; i < iter ; i++ )
            {
                fn( @this );
            }
            stw.Stop();
            Console.WriteLine(msg + $"{stw.ElapsedMilliseconds / 1.0}" );
            return fn( @this );
        }

        public static TResult Measure<TSource, TSource2, TResult> (
            this TSource @this ,
            string msg ,
            int iter,
            TSource2 src2 ,
            Func<TSource , TSource2 , TResult> fn)
        {
            Stopwatch stw = new Stopwatch();
            stw.Start();
            for ( int i = 0 ; i < iter ; i++ )
            {
                fn( @this , src2 );
            }
            stw.Stop();
            Console.WriteLine( $"{stw.ElapsedMilliseconds / 1.0}" + msg );
            return fn( @this , src2 );
        }
    }

    public class MyEqualityComparer : IEqualityComparer<int[]>
    {
        //Dictionary<int[], bool> dict = new Dictionary<int[], bool>(new IntArrayComparer());
        public bool Equals (int[] x , int[] y)
        {
            if ( x.Length != y.Length )
            {
                return false;
            }
            for ( int i = 0 ; i < x.Length ; i++ )
            {
                if ( x[i] != y[i] )
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode (int[] obj)
        {
            int result = 17;
            for ( int i = 0 ; i < obj.Length ; i++ )
            {
                unchecked
                {
                    result = result * 23 + obj[i];
                }
            }
            return result;
        }
    }
}
