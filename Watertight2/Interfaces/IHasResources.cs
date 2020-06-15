using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Filesystem;

namespace Watertight.Interfaces
{
    public interface IHasResources
    {
        public void CollectResources(IList<ResourcePtr> ResourceCollector);

    }
}
