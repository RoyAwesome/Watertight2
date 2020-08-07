using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Watertight.Filesystem;
using Watertight.Input;
using Watertight.Interfaces;
using Watertight.Math;
using Watertight.Rendering;
using Watertight.Rendering.Interfaces;
using Watertight.Rendering.Materials;
using Watertight.Tickable;

namespace Watertight.Middleware.DearImGUI
{
    public class ImGUIMiddleware : IMiddleware, IRenderable
    {
        public static ResourcePtr FontAtlasResourcePtr = new ResourcePtr("texture:ImGUI/FontAtlas");
        public static ResourcePtr ImGUIMaterialPrt = new ResourcePtr("mat:ImGUI/RenderMat");

        const float OrthoOffset = 0.0f;

        public static int ImGUI_FrameStartTick = TickFunction.InputPoll + 0x000F;
        TickFunction StartFrameTickFunc = new TickFunction
        {
            CanTick = true,
            TickPriority = ImGUI_FrameStartTick,
        };

        Dictionary<IntPtr, ITexture> _ImGUITextureMap = new Dictionary<IntPtr, ITexture>();

        ICamera UICamera;

        public ImGUIMiddleware()
        {
            StartFrameTickFunc.TickFunc = FrameStart;

        }

        public void Initialize(IEngine EngineInstance)
        {           
            EngineInstance.GameThreadTickManager.AddTick(StartFrameTickFunc);
         

            var Context = ImGui.CreateContext();
            ImGui.SetCurrentContext(Context);

            ImGui.GetIO().Fonts.AddFontDefault();
            ImGui.GetIO().DisplaySize = EngineInstance.Renderer.ScreenSize;

        }

        public void PostRenderInitialize(Renderer Renderer)
        {
            RebuildFontAtlas(Renderer);

            Renderer.AddRenderable(this);

            UICamera = Renderer.RendererResourceFactory.CreateCamera(null);
            Renderer.WindowResized += (Vector2 OldSize) => { RefreshCamera(Renderer); };

            RefreshCamera(Renderer);

            Material imguiMat = Renderer.DefaultMaterialPtr.Get<Watertight.Rendering.Materials.Material>().Clone() as Material;
            FileSystem.EmplaceInMemoryResource(ImGUIMaterialPrt, imguiMat);
            imguiMat.DepthTest = false;


        }

        protected void RefreshCamera(Renderer Renderer)
        {
            //IMGUI Assumes 0 is the bottom and ScreenBuffer.y is top
            UICamera.Projection = Matrix4x4.CreateOrthographicOffCenter(OrthoOffset, Renderer.ScreenSize.X + OrthoOffset, OrthoOffset, Renderer.ScreenSize.Y + OrthoOffset, -1f, 1f);

        }

        public void FrameStart(float DeltaTime)
        {
            ImGui.GetIO().DeltaTime = DeltaTime;
            ImGui.GetIO().DisplaySize = IEngine.Instance.Renderer.ScreenSize;
            //ImGui.GetIO().DisplayFramebufferScale = new Vector2(1, 1);

            UpdateInput();

            ImGui.NewFrame();

            //Create a simple box
            {
                ImGui.Text("Hello World");

                ImGui.ShowDemoWindow();

            }

        }

        private void UpdateInput()
        {
            var io = ImGui.GetIO();

            io.KeyShift = InputProcessor.IsKeyDown(Key.LShiftKey) || InputProcessor.IsKeyDown(Key.RShiftKey);
            io.KeyCtrl = InputProcessor.IsKeyDown(Key.LControlKey) || InputProcessor.IsKeyDown(Key.RControlKey);
            io.KeyAlt = InputProcessor.IsKeyDown(Key.LAltKey) || InputProcessor.IsKeyDown(Key.RAltKey);

            io.MouseDown[0] = InputProcessor.IsKeyDown(Key.LeftMouse);
            io.MouseDown[1] = InputProcessor.IsKeyDown(Key.RightMouse);
            io.MouseDown[2] = InputProcessor.IsKeyDown(Key.MiddleMouse);

            io.MousePos = new Vector2(InputProcessor.PollAxis(Key.MouseX), InputProcessor.PollAxis(Key.MouseY));

            
        }

        public void PreRender(Renderer renderer)
        {
                       
        }

        public void Render(Renderer renderer)
        {
            ImGui.EndFrame();
            RenderIMGui(renderer);
        }
               
        private void RenderIMGui(Renderer renderer)
        {
            ImGui.Render();
            ImDrawDataPtr drawData = ImGui.GetDrawData();
                      
            for (int i = 0; i < drawData.CmdListsCount; i++)
            {
                ImDrawListPtr cmdList = drawData.CmdListsRange[i];

                IVertexBuffer vertexBuffer = CreateVertexBuffer(cmdList);

                int IndexOffset = 0;              


                for(int cmdi = 0; cmdi < cmdList.CmdBuffer.Size; cmdi++)
                {
                    ImDrawCmdPtr DrawCommand = cmdList.CmdBuffer[cmdi];

                    using(RenderingCommand cmd = renderer.RendererResourceFactory.CreateRenderCommand())
                    {
                        cmd.WithVertexBuffer(vertexBuffer)
                          .WithMaterial(Renderer.DefaultMaterialPtr.Get<Watertight.Rendering.Materials.Material>())
                          .WithCamera(UICamera);

                        if (_ImGUITextureMap.ContainsKey(DrawCommand.TextureId))
                        {
                            cmd.WithTexture(_ImGUITextureMap[DrawCommand.TextureId]);
                        }

                        cmd.WithStartIndex(IndexOffset)
                            .WithPrimitveCount((int)DrawCommand.ElemCount / 3)
                            .WithDebugName("IMGUI Middleware");

                        renderer.ExecuteRenderCommand(cmd);
                    }                       

                    IndexOffset += (int)DrawCommand.ElemCount;
                }               
            }
        }

        IVertexBuffer VertexBuffer;
        private IVertexBuffer CreateVertexBuffer(ImDrawListPtr DrawList)
        {

            Vertex[] Verts = new Vertex[DrawList.VtxBuffer.Size];
 
            for (int x = 0; x < DrawList.VtxBuffer.Size; x++)
            {
                Verts[x] = new Vertex
                {
                    Location = new System.Numerics.Vector3(DrawList.VtxBuffer[x].pos, 0),
                    UV = DrawList.VtxBuffer[x].uv,
                    Color = Color.FromArgb((int)DrawList.VtxBuffer[x].col)
                };
            }

            ushort[] Ind = new ushort[DrawList.IdxBuffer.Size];
            for (int x = 0; x < DrawList.IdxBuffer.Size; x++)
            {
                Ind[x] = DrawList.IdxBuffer[x];
            }

            if(VertexBuffer == null)
            {
                VertexBuffer = IEngine.Instance.Renderer.RendererResourceFactory.CreateVertexBuffer();
            }
            VertexBuffer.SetVertexData(Verts, Ind);

            return VertexBuffer;
        }
        

        static int TextureId = 1;

        public IntPtr BindTexture(ITexture Texture)
        {
            IntPtr id = new IntPtr(TextureId++);
            _ImGUITextureMap.Add(id, Texture);
            return id;
        }

        public void UnbindTexture(IntPtr intPtr)
        {
            _ImGUITextureMap.Remove(intPtr);
        }


      

        IntPtr? FontTexture;
        private void RebuildFontAtlas(Renderer renderer)
        {
            var Io = ImGui.GetIO();

            IntPtr PixelData;
            int Width;
            int Height;
            int BytesPerPixel;

            Io.Fonts.GetTexDataAsRGBA32(out PixelData, out Width, out Height, out BytesPerPixel);

            int Bytes = Width * Height * BytesPerPixel;

            byte[] ColorInts = new byte[Bytes];
            Marshal.Copy(PixelData, ColorInts, 0, Width * Height * BytesPerPixel);

            ITexture Texture = renderer.TextureFactory.Create(ColorInts, BytesPerPixel, new Vector2(Width, Height));

            Filesystem.FileSystem.EmplaceInMemoryResource(FontAtlasResourcePtr, Texture);

            if(FontTexture.HasValue)
            {
                UnbindTexture(FontTexture.Value);
            }

            FontTexture = BindTexture(Texture);

            Io.Fonts.SetTexID(FontTexture.Value);
            Io.Fonts.ClearTexData();
        }

        
    }
}
