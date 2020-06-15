using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Watertight.Framework.Components;
using Watertight.Interfaces;
using Watertight.Math;

namespace Watertight.Rendering.Interfaces
{
    public interface ICamera : IHasOwner<CameraComponent>
    {


        public void MakeActive();

        public Rectangle ViewRectangle
        {
            get;
        }

        public Matrix4x4 Projection
        {
            get;
            set;
        }

        public Matrix4x4 View
        {
            get;
        }

    }
}
