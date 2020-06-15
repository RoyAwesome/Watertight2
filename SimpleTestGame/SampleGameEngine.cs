using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using Watertight;
using Watertight.Filesystem;
using Watertight.Framework.Components;
using Watertight.Interfaces;
using Watertight.Math;
using Watertight.Rendering;
using Watertight.Rendering.Interfaces;
using Watertight.Rendering.Materials;
using Watertight.VulkanRenderer;

//using UnderlyingRenderer = Watertight.VulkanRenderer.VulkanRenderer;
using UnderlyingRenderer = Watertight.Rendering.VeldridRendering.VeldridRenderer;

namespace SimpleTestGame
{
    public class SampleRenderable : IRenderable
    {       

        RenderingCommand _Command;

        public SampleRenderable(RenderingCommand Command)
        {
            this._Command = Command;
        }

        public void PreRender(Renderer renderer)
        {
        }

      

        public void Render(Renderer renderer)
        {
            renderer.ExecuteRenderCommand(_Command);
        }       
    }

    internal class SampleGameEngine : Engine<UnderlyingRenderer>
    {
        public override IEnumerable<ResourcePtr> PreloadResources => new ResourcePtr[] {
                TestMaterial,
                TestTexture,
            };

        public ResourcePtr TestMaterial = new ResourcePtr("material:TestMaterial.mat");
        public ResourcePtr TestTexture = new ResourcePtr("texture:pikaoh.png");

        public SampleGameEngine(string Name, string version) : base(Name, version)
        {

        }

        public override void OnInit()
        {
            base.OnInit();

        }

        RenderingCommand Command;
        CameraComponent CameraComp;

        public override void OnStart()
        {
            base.OnStart();

            Vertex[] vertices = new Vertex[]
            {
                 new Vertex
                {
                    Location = new System.Numerics.Vector3(-0.5f, -0.5f, 0),
                    Color = Color.Gold,
                    UV = Vector2.Zero,
                },
                new Vertex
                {
                    Location = new System.Numerics.Vector3(0.5f, -0.5f, 0),
                    Color = Color.Red,
                    UV = Vector2.Zero,
                },
                new Vertex
                {
                    Location = new System.Numerics.Vector3(0.5f, 0.5f, 0),
                    Color = Color.Blue,
                    UV = Vector2.Zero,
                },
                new Vertex
                {
                    Location = new System.Numerics.Vector3(-0.5f, 0.5f, 0),
                    Color = Color.Green,
                    UV = Vector2.Zero,
                },
           };

            ushort[] ind = new ushort[]
            {
                0, 1, 2,
                2, 3, 0,
            };

            CameraComp = new CameraComponent(null);
            CameraComp.Register();
            CameraComp.MakeActive();

            const bool TestPerspective = false;

            if(TestPerspective)
            {
                CameraComp.Mode = CameraComponent.ProjectionMode.Perspective;
                CameraComp.Location =new Vector3(200, 100, 500);
                CameraComp.LookAt(Vector3.Zero);
            }
            else
            {
                CameraComp.Mode = CameraComponent.ProjectionMode.Orthographic;
            }


            IVertexBuffer VertexBuffer = Renderer.RendererResourceFactory.CreateVertexBuffer();
            VertexBuffer.SetVertexData(vertices, ind);

            Command = Renderer.RendererResourceFactory.CreateRenderCommand()
                .WithDebugName("Sample Game Render")
                .WithMaterial(Renderer.DefaultMaterialPtr.Get<Material>())
                .WithVertexBuffer(VertexBuffer);
            

            SampleRenderable renderable = new SampleRenderable(Command);

            Renderer.AddRenderable(renderable);
        }

        float TotalTime = 0;

        public override void Tick(float DeltaTime)
        {
            base.Tick(DeltaTime);
            TotalTime += DeltaTime;

            float sin = MathF.Sin(TotalTime.ToRadians() / 4);
            float cos = MathF.Cos(TotalTime.ToRadians() / 4);


            Command.WithTransform(new Vector3(sin, cos, 1), Quaternion.Identity, Vector3.One * 100);
        }
    }


}
