using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;

namespace EmguCV_Extension
{
    public static class Covertor_Extension
    {
        public static int [ ] [ ] [ ] Points2Arrays( 
            this Point [ ] [ ] input )
        {
            return ( from rows in input
                     select ( from row in rows

                              select new int [ ] { row.X , row.Y } ).ToArray() ).ToArray();
        }

        public static Point [ ] [ ] Arrays2Points( 
            this int [ ] [ ] [ ] input )
        {
            return ( from rows in input
                     select ( from row in rows
                              select new Point( row [ 0 ] , row [ 1 ] ) ).ToArray() ).ToArray();

        }

        public static byte[,,] Gray2Bgr(
            this byte[,,] graysrc
            )
        {
            return new Image<Gray , byte>( graysrc ).Convert<Bgr,byte>().Data;
        }

        public static byte [ , , ] Bgr2Gray(
           this byte [ , , ] graysrc
           )
        {
            return new Image<Bgr , byte>( graysrc ).Convert<Gray , byte>().Data;
        }




    }
}
