using System;
namespace CompanioNc.View
{
    public class FrameLoadCompleteEventArgs : EventArgs
    {
        public FrameLoadStates Message { get; set; }
    }
}
