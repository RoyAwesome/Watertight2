using System;
using System.Linq;
using System.Numerics;
using Watertight.Filesystem;
using Watertight.FlowUI;
using Watertight.FlowUI.Draw;
using Watertight.Interfaces;
using Watertight.Math;
using Watertight.Modules;
using Watertight.Rendering;
using Watertight.Rendering.Interfaces;
using Watertight.Rendering.Materials;
using Watertight.Tickable;

namespace Watertight.FlowModule
{
    public class FlowModule : Module
    {
        public override string ModuleName => "Flow Module";

        public override string ModuleVersion => "1.0.0.0";

        public static int Flow_FrameStartTick = TickFunction.InputPoll + 0x000F;
        TickFunction StartFrameTickFunc = new TickFunction
        {
            CanTick = true,
            TickPriority = Flow_FrameStartTick,
        };

        private FlowRenderable RenderObject;

        public override void ShutdownModule()
        {
            
        }

        public override void StartupModule(StartupPhase Phase)
        {
            /*
           if(Phase == StartupPhase.EngineInit)
           {
                RenderObject = new FlowRenderable();
                StartFrameTickFunc.TickFunc = RenderObject.Tick;

                IEngine.Instance.GameThreadTickManager.AddTick(StartFrameTickFunc);
           }
           if(Phase == StartupPhase.PostRendererInit)
           {
               
                RenderObject.PostRendererInit(IEngine.Instance.Renderer);
           }
            */
        }

        class FlowRenderable : IRenderable, ITickable
        {
            const float OrthoOffset = 0.0f;
            ICamera UICamera;

            public static ResourcePtr FlowMaterialPtr = new ResourcePtr("mat:Flow/RenderMat");

            public void PostRendererInit(Renderer renderer)
            {
                renderer.AddRenderable(this);

                UICamera = renderer.RendererResourceFactory.CreateCamera(null);

                RefreshCamera(renderer);

                Material FlowMat = Renderer.DefaultMaterialPtr.Get<Watertight.Rendering.Materials.Material>().Clone() as Material;
                FileSystem.EmplaceInMemoryResource(FlowMaterialPtr, FlowMat);
            }

            private void RefreshCamera(Renderer renderer)
            {
                UICamera.Projection = Matrix4x4.CreateOrthographicOffCenter(OrthoOffset, renderer.ScreenSize.X + OrthoOffset, OrthoOffset, renderer.ScreenSize.Y + OrthoOffset, -1f, 1f);

            }

            public void Tick(float DeltaTime)
            {
                Flow.NewFrame();
                Flow.TestDrawing();
            }

            public void PreRender(Renderer renderer)
            {
                
            }

            public void Render(Renderer renderer)
            {
                Flow.Render();
                DrawData drawData = Flow.DrawData;

                for(int i = 0; i < drawData.DrawLists.Count; i++)
                {
                    DrawList drawList = drawData.DrawLists[i];

                    IVertexBuffer vertexBuffer = BindVertexBuffer(drawList);
                    int IndexOffset = 0;

                    for(int cmdi = 0; cmdi < drawList.CommandBuffer.Count; cmdi++)
                    {
                        DrawList.DrawCmd DrawCommand = drawList.CommandBuffer.ElementAt(cmdi);

                        using (RenderingCommand cmd = renderer.RendererResourceFactory.CreateRenderCommand())
                        {
                            cmd.WithVertexBuffer(vertexBuffer)
                                .WithMaterial(FlowMaterialPtr.Get<Material>())
                                .WithCamera(UICamera);

                            //TODO: Textures

                            cmd.WithStartIndex(IndexOffset)
                                .WithPrimitveCount((int)DrawCommand.Elements / 3)
                                .WithDebugName("Flow UI Rendering");

                            renderer.ExecuteRenderCommand(cmd);
                        }


                        IndexOffset += (int)DrawCommand.IndexOffset;
                    }

                   
                }
            }

            IVertexBuffer InternalVertexBuffer;
            private IVertexBuffer BindVertexBuffer(DrawList drawList)
            {
                Watertight.Math.Vertex[] Verts = new Math.Vertex[drawList.VertexBuffer.Count];
                ushort[] Indicies = new ushort[drawList.IndexBuffer.Count];

                for(int x = 0; x < drawList.VertexBuffer.Count; x++)
                {
                    FlowUI.Draw.Vertex FlowVtx = drawList.VertexBuffer.ElementAt(x);
                    Verts[x] = new Math.Vertex
                    {
                        Location = new Vector3(FlowVtx.Pos, 0),
                        Color = FlowVtx.Color,
                        UV = FlowVtx.UV
                    };
                }

                for(int i = 0; i < drawList.IndexBuffer.Count; i++)
                {
                    Indicies[i] = drawList.IndexBuffer.ElementAt(i);
                }

                if(InternalVertexBuffer == null)
                {
                    InternalVertexBuffer = IEngine.Instance.Renderer.RendererResourceFactory.CreateVertexBuffer();
                }

                InternalVertexBuffer.SetVertexData(Verts, Indicies);

                return InternalVertexBuffer;

            }
            
        }

    }
}
