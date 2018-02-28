using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image_Processing_Test
{
    public static class Extension
    {
        public static Single [ , ] ToSingleMat(
            this byte [ , ] src )
        {
            float[,] output = new float[src.GetLength(0),src.GetLength(1)];

            for ( int j = 0 ; j < src.GetLength(0) ; j++ )
            {
                for ( int i = 0 ; i < src.GetLength(1) ; i++ )
                {
                    output [ j , i ] = BitConverter.ToSingle( new byte [ ] { src [ j , i ] } , 0 ); 
                }

            }

            return output;
        }

        public static TSrc [ , , ] BGRtoGray<TSrc>(
           this TSrc [ , , ] src
           , int idx )
        {
            int w = src.GetLength(0);
            int h = src.GetLength(1);
            int c = src.GetLength(2);

            TSrc[,,] output = new TSrc[w, h, 1];

            for ( int j = 0 ; j < w ; j++ )
            {
                for ( int i = 0 ; i < h ; i++ )
                {
                    output [ j , i , 0 ] = src [ j , i , idx ];
                }
            }
            return output;
        }


    }
}
