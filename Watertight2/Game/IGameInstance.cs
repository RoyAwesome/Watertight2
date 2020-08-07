using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Interfaces;

namespace Watertight.Game
{
    public interface IGameInstance : IHasResources, INamed, ITickable
    {
        public string Version
        {
            get;
        }

        public void OnInit();

        public void OnStart();

        public void OnRendererCreated();
    }
}
