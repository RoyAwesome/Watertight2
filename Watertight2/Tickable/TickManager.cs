using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Threading;

namespace Watertight.Tickable
{
    public class TickManager
    {


        private List<TickFunction> TickList = new List<TickFunction>();
        private List<TickFunction> RemoveTickList = new List<TickFunction>();
        private Stopwatch StopWatch = new Stopwatch();


        public float MinFrameTime
        {
            get;
            set;
        }

        public bool ShouldGCIfAble
        {
            get;
            set;
        } = false;
        
        public void AddTick(TickFunction tickFunction)
        {
            if(TickList.Contains(tickFunction))
            {
                return;
            }

            TickList.Add(tickFunction);
        }

        public void RemoveTick(TickFunction tickFunction)
        {
            if(tickFunction == null)
            {
                return;
            }
            tickFunction.CanTick = false;
            RemoveTickList.Add(tickFunction);
        }




        public void Init()
        {
            StopWatch.Start();
        }

        public float ExecuteSingleTick()
        {
            long FrameStart = StopWatch.ElapsedTicks;
            TickList.Sort((x, y) => {
                return y.TickPriority - x.TickPriority;
            });
            for (int i = 0; i < TickList.Count; i++)
            {
                TickFunction tf = TickList[i];
                long TickStart = StopWatch.ElapsedTicks;
                long DeltaTick = TickStart - tf.LastTickTime;
                float DeltaTimeMs = (float)DeltaTick / (float)TimeSpan.TicksPerMillisecond;

                if (tf.CanTick)
                {
                    tf.TickFunc?.Invoke(DeltaTimeMs / 1000.0f);
                }

                tf.LastTickTime = TickStart;
            }

            foreach (TickFunction tickfunc in RemoveTickList)
            {
                TickList.Remove(tickfunc);
            }
            RemoveTickList.Clear();


            long FrameDelta = StopWatch.ElapsedTicks - FrameStart;
            if (MinFrameTime > 0)
            {                
                float BeforeSleepDeltaTime = (float)FrameDelta / (float)TimeSpan.TicksPerMillisecond;

                if(ShouldGCIfAble && BeforeSleepDeltaTime < MinFrameTime)
                {                    
                    GC.Collect();
                    FrameDelta = StopWatch.ElapsedTicks - FrameStart;
                    BeforeSleepDeltaTime = (float)FrameDelta / (float)TimeSpan.TicksPerMillisecond;                    
                }
              

                if (BeforeSleepDeltaTime < MinFrameTime)
                {                    

                    Thread.Sleep((int)(MinFrameTime - BeforeSleepDeltaTime));
                }

                FrameDelta = StopWatch.ElapsedTicks - FrameStart;
            }
             
            //Return the total time that this tick took
            return (float)FrameDelta / (float)TimeSpan.TicksPerMillisecond;
        }

    }
}
