using System;
using System.Collections.Generic;
using System.Text;

namespace Watertight.Interfaces
{
    public interface ITickable
    {
        public void Tick(float DeltaTime);
    }
}
