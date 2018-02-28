using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaferandChipProcessing
{
    public delegate void Trsvoid  ();
    public delegate void TrsInt  (int input);
    public delegate void TrsBool (bool input);
    public partial class MainCore
    {
        public event Trsvoid evtProcessingResult;
        public event TrsInt evtIntenAvg;
        public event TrsInt evtAreaAvg;
        public event TrsBool evtProcessingDone;

    }
}
