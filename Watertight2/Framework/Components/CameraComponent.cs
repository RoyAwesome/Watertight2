using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Numerics;
using System.Text;
using Watertight.Math;
using Watertight.Rendering.Interfaces;

namespace Watertight.Framework.Components
{
    public class CameraComponent : SceneComponent
    {
        public enum ProjectionMode
        {
            Orthographic,
            Perspective,
            Custom,
        }

        public ProjectionMode Mode
        {
            get => _Mode;
            set
            {
                _Mode = value;               
                RebuildProjection();                               
            }
        }
        ProjectionMode _Mode = ProjectionMode.Orthographic;

        public float FieldOfView
        {
            get => _FieldOfView;
            set
            {
                _FieldOfView = value;
                RebuildProjection();
            }
        }
        float _FieldOfView = 85;

        Vector2 ClippingBounds
        {
            get => _ClippingBounds;
            set
            {
                _ClippingBounds = value;
                RebuildProjection();
            }
        } 
        Vector2 _ClippingBounds = new Vector2(0.01f, 100000);

        public ICamera Camera
        {
            get;
            private set;
        }

        public CameraComponent(Actor owner)
            : base(owner)
        {

        }

        public CameraComponent()
            : base() 
        {

        }

        public void MakeActive()
        {
            Camera.MakeActive();
        }

        protected override void OnRegister()
        {
            Camera =  IEngine.Instance.Renderer.RendererResourceFactory.CreateCamera(this);
            RebuildProjection();

            IEngine.Instance.Renderer.WindowResized += Renderer_WindowResized;
        }

        private void Renderer_WindowResized(Vector2 OldSize)
        {
            RebuildProjection();
        }

        public void RebuildProjection()
        {
            if (Camera != null)
            {
                if (Mode == ProjectionMode.Orthographic)
                {
                    Camera.Projection = Matrix4x4.CreateOrthographic(IEngine.Instance.Renderer.ScreenSize.X, IEngine.Instance.Renderer.ScreenSize.Y, -ClippingBounds.Y, ClippingBounds.Y);
                }
                if (Mode == ProjectionMode.Perspective)
                {
                    Camera.Projection = Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView.ToRadians(), IEngine.Instance.Renderer.ScreenSize.X / IEngine.Instance.Renderer.ScreenSize.Y, ClippingBounds.X, ClippingBounds.Y);
                }
            }
        }

        public void LookAt(Vector3 Target)
        {
            var LookAtMatrix = Matrix4x4.CreateLookAt(GetLocation_WorldSpace(), Target, Vector3.UnitZ);
            this.Location = LookAtMatrix.Translation;
            this.Rotation = Quaternion.CreateFromRotationMatrix(LookAtMatrix);
        }

    }
}
