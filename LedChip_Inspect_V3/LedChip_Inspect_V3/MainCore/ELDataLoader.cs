using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaferandChipProcessing
{
    public static class ELDataLoader
    {
        public static IEnumerable<ELData> LoadELData(string path)
        {
            var lines = File.ReadAllLines(path, Encoding.UTF8);

            var lineSplited = lines.Select(x => x.Split(',')).ToArray();

            var eldata = lineSplited.Select(x => 
            {
                var xidx = int.Parse(x[0]);
                var yidx = int.Parse(x[1]);
                var clss = x.Last();

                return new ELData(xidx, yidx, clss);
            });

            return eldata;
        }

    }

    public struct ELData
    {
        public int Xidx;
        public int Yidx;
        public string Class;

        public ELData(int x, int y, string clss)
        {
            Xidx = x;
            Yidx = y;
            Class = clss;
        }
    }
}
