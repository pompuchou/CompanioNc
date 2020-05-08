using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanioNc.View
{
    public enum FrameLoadStates
    {
        FrameLoadCompleted = 0,
        DocumentLoadCompletedButNotFrame = 1,
        DirectCall = 2
    }
}
