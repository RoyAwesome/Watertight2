using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Watertight.Framework.Components;
using Watertight.Interfaces;
using Watertight.Math;
using Watertight.Rendering.Interfaces;
using Watertight.Util;

namespace Watertight.Rendering.VeldridRendering
{
    class VeldridCamera : ICamera
    {
        static VeldridRenderer Renderer => IEngine.Instance.Renderer as VeldridRenderer;


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
                if((Owner as ITransformable) == null)
                {
                    return Matrix4x4.Identity;
                }

                Transform tform = (Owner as ITransformable).GetTransform_WorldSpace();
                tform.Location *= 0.5f;

                return tform.ToTransformMatrix();
            }
        }
        public void MakeActive()
        {
            Renderer.MainCamera = this;
        }

        public VeldridCamera(CameraComponent Owner)
        {
            this.Owner = Owner;
        }

        public byte[] ViewProjection()
        {
            Transform View = (Owner as ITransformable)?.GetTransform_WorldSpace() ?? Transform.Identity;
            return (View.ToTransformMatrix() * Projection).ToBytes();
        }

        Veldrid.DeviceBuffer ViewProjectionBuffer;
        public Veldrid.ResourceSet ProjectionViewResourceSet;

       

        private void CreateBuffers()
        {
            ViewProjectionBuffer = Renderer.VeldridFactory.CreateBuffer(new Veldrid.BufferDescription(sizeof(float) * 16, Veldrid.BufferUsage.UniformBuffer | Veldrid.BufferUsage.Dynamic));
            ProjectionViewResourceSet = Renderer.VeldridFactory.CreateResourceSet(new Veldrid.ResourceSetDescription(
                ProjectionViewResourceLayout,
                ViewProjectionBuffer));
        }

        public void Update()
        {
            if(ViewProjectionBuffer == null)
            {
                CreateBuffers();
            }
            Matrix4x4 v = View;
            v.M22 *= -1;
            byte[] ViewArr = v.ToBytes();
            byte[] ProjArr = Projection.ToBytes();
            Renderer.GraphicsDevice.UpdateBuffer(ViewProjectionBuffer, 0, (Projection * v).ToBytes());
        }

        private static Veldrid.ResourceLayoutDescription ProjectionViewLayout = new Veldrid.ResourceLayoutDescription(
                new Veldrid.ResourceLayoutElementDescription("ProjectionView", Veldrid.ResourceKind.UniformBuffer, Veldrid.ShaderStages.Vertex));
        

        public static Veldrid.ResourceLayout ProjectionViewResourceLayout
        {
            get
            {
                if(_ResourceLayout == null)
                {
                    _ResourceLayout = Renderer.VeldridFactory.CreateResourceLayout(ProjectionViewLayout);
                }
                return _ResourceLayout;
            }
        }
        static Veldrid.ResourceLayout _ResourceLayout;
    }
}
