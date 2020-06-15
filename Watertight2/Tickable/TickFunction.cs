using System;
using System.Collections.Generic;
using System.Text;

namespace Watertight.Tickable
{

    public class TickFunction 
    {
        public const int HighPriority = int.MaxValue - 0x00FF;
        public const int InputPoll = int.MaxValue - 0x01FF;
        public const int World = int.MaxValue - 0x05FF;
        public const int Network = int.MaxValue - 0x07FF;
        public const int Last = 0;

        public delegate void Tick(float DeltaTime);

        public Tick TickFunc
        {
            get;
            set;
        }       

        public int TickPriority
        {
            get;
            set;
        }

        public void TickBefore(TickFunction otherTickFunction)
        {
            TickPriority = otherTickFunction?.TickPriority + 1 ?? TickPriority;
        }

        public void TickAfter(TickFunction otherTickfunc)
        {
            TickPriority = otherTickfunc?.TickPriority - 1 ?? TickPriority;
        }

        internal long LastTickTime
        {
            get;
            set;
        }

        public bool CanTick
        {
            get;
            set;
        }

    }
}
