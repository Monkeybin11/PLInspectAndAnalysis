using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WaferandChipProcessing
{
    public delegate void DropDone(Nullable<int> rowN , Nullable<int> colN , string path);

    public class EpiSeperatedImgEvent
    {
        public event DropDone evtDropDone;

        public Nullable<int> RowNum, ColNum;
        public string Path;
        

        public EpiSeperatedImgEvent(int rownum, int colnum)
        {
            RowNum = rownum;
            ColNum = colnum;
        }

        public void DropEventMethod(object ss, DragEventArgs ee)
        {
            string[] files = (string[])ee.Data.GetData(DataFormats.FileDrop);
            evtDropDone( RowNum , ColNum , files[0] );
        }
    }
}
