using System;
using System.Collections.Generic;
using System.Text;

namespace Watertight.Interfaces
{
    public interface IHasOwner<T>
    {
        T Owner
        {
            get;
        }        
    }
}
