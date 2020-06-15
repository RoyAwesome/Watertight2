using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Watertight.Framework.Components;
using Watertight.Interfaces;
using Watertight.Math;
using Watertight.Rendering;
using Watertight.Rendering.Interfaces;

namespace Watertight.VulkanRenderer
{
    class VulkanCamera : ICamera
    {
        public Rectangle ViewRectangle
        {
            get;
            set;
        }

        public CameraComponent Owner
        {
            get;
            set;
        }
        public Matrix4x4 Projection
        {
            get;
            set;
        } = Matrix4x4.Identity;

        public Matrix4x4 View
        {
            get
            {
                return (Owner as ITransformable)?.GetTransform_WorldSpace().ToTransformMatrix() ?? Matrix4x4.Identity;
            }
        }
        public void MakeActive()
        {
            VulkanRenderer.Instance.MainCamera = this;
        }

        public VulkanCamera(CameraComponent Owner)
        {
            this.Owner = Owner;
        }

        public byte[] ViewProjection()
        {
            Transform View = (Owner as ITransformable)?.GetTransform_WorldSpace() ?? Transform.Identity;
            return (View.ToTransformMatrix() * Projection).ToBytes();
        }
    }
}
