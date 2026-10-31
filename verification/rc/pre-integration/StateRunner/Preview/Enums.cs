using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.StateRunner.Preview
{
    enum FsmReserved { NoAuthority = int.MinValue, }
    enum FsmReservedState
    {
        None = 0,
        FastTrack = None - 1,
        Next = FastTrack - 1,
        Canceled = Next - 1,
        Exception = Canceled - 1,
        MaxOOB = Exception - 1,
    }
}
