using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Watertight.Filesystem;
using Watertight.Framework.Components;
using Watertight.Interfaces;
using Watertight.Rendering.Interfaces;
using Watertight.Rendering.Materials;
using Watertight.Rendering.VeldridRendering.Input;
using Watertight.Rendering.VeldridRendering.ResourceFactories;
using Watertight.Tickable;

namespace Watertight.Rendering.VeldridRendering
{
    public class VeldridRenderer : Renderer
    {
        public static ResourcePtr DefaultShader_Fragment = new ResourcePtr("shader:DefaultShader_Fragment");
        public static ResourcePtr DefaultShaderUntextured_Fragment = new ResourcePtr("shader:DefaultShaderUntextured_Fragment");
        public static ResourcePtr DefaultShader_Vertex = new ResourcePtr("shader:DefaultShader_Vertex");

        public override ITextureFactory TextureFactory => _TextureFactory;
        ITextureFactory _TextureFactory = new VeldridTextureFactory();


        public override IMaterialFactory MaterialFactory => _MaterialFactory;
        IMaterialFactory _MaterialFactory = new VeldridMaterialFactory();

        public override IRendererResourceFactory RendererResourceFactory => _RendererResourceFactory;
        IRendererResourceFactory _RendererResourceFactory = new VeldridResourceFactory();

        #region Tick Management
        const int DoRenderPriority = TickFunction.World;
        const int SortRenderingPriority = DoRenderPriority + 0x000F;
        const int PreRenderingPriority = SortRenderingPriority + 0x000F;

        TickFunction RenderEndFunc = new TickFunction
        {
            CanTick = true,
            TickPriority = TickFunction.Last,
        };

        TickFunction SortRendererFunc = new TickFunction
        {
            CanTick = true,
            TickPriority = SortRenderingPriority,
        };

        TickFunction PreRenderFunc = new TickFunction
        {
            CanTick = true,
            TickPriority = PreRenderingPriority,
        };

        TickFunction DoRenderFunc = new TickFunction
        {
            CanTick = true,
            TickPriority = DoRenderPriority,
        };

        TickFunction RenderStartFunc = new TickFunction
        {
            CanTick = true,
            TickPriority = TickFunction.HighPriority,
        };
        #endregion

        #region Renderables
        private List<IRenderable> Renderables = new List<IRenderable>();


        public override void AddRenderable(IRenderable Renderable)
        {
            Renderables.Add(Renderable);
        }

        public override void RemoveRenderable(IRenderable Renderable)
        {
            Renderables.Remove(Renderable);
        }

        #endregion


        public Veldrid.GraphicsDevice GraphicsDevice;
        public Veldrid.ResourceFactory VeldridFactory => GraphicsDevice.ResourceFactory;
        internal Sdl2Window Window;

        public override void CreateRenderer()
        {
            RenderStartFunc.TickFunc = RenderStart;
            TickManager.AddTick(RenderStartFunc);

            PreRenderFunc.TickFunc = PreRender;
            TickManager.AddTick(PreRenderFunc);

            SortRendererFunc.TickFunc = SortRenderer;
            TickManager.AddTick(SortRendererFunc);

            DoRenderFunc.TickFunc = RenderAll;
            TickManager.AddTick(DoRenderFunc);

            RenderEndFunc.TickFunc = RenderEnd;
            TickManager.AddTick(RenderEndFunc);

            Watertight.Input.InputProcessor.RegisterInputSource(VeldridInputSource);

        }

        VeldridInputSource VeldridInputSource = new VeldridInputSource();

        protected override void CreateWindow_Internal()
        {
            TickFunction WindowTick = new TickFunction
            {
                TickFunc = (DeltaTime) => { 
                    var Snapshot = Window.PumpEvents();
                    VeldridInputSource.LastInputSnapshot(Snapshot);
                },
                TickPriority = TickFunction.HighPriority,
                CanTick = true,
            };
            IEngine.Instance.GameThreadTickManager.AddTick(WindowTick);

            Veldrid.StartupUtilities.WindowCreateInfo windowCI = new Veldrid.StartupUtilities.WindowCreateInfo
            {
                X = 100,
                Y = 100,
                WindowWidth = (int)ScreenSize.X,
                WindowHeight = (int)ScreenSize.Y,
                WindowInitialState = Veldrid.WindowState.Normal,
                WindowTitle = "Intro",
            };

            Window = VeldridStartup.CreateWindow(ref windowCI);
            Window.KeyDown += VeldridInputSource.OnWindowKeyEvent;
            Window.KeyUp += VeldridInputSource.OnWindowKeyEvent;
            Window.MouseDown += VeldridInputSource.OnWindowMouseEvent;
            Window.MouseUp += VeldridInputSource.OnWindowMouseEvent;
            Window.Resized += Window_Resized;
            Window.Closing += Window_Closing;
            Window.Closed += Window_Closed;
            Window.LimitPollRate = true;


            Veldrid.GraphicsDeviceOptions options = new Veldrid.GraphicsDeviceOptions()
            {
                PreferStandardClipSpaceYDirection = true,
                Debug = false,
            };

            GraphicsDevice = VeldridStartup.CreateVulkanGraphicsDevice(options, Window);


            ConstructVeldridResources();
        }

        private void Window_Closed()
        {
            IEngine.Instance.Shutdown();
        }

        private void Window_Closing()
        {
            GraphicsDevice.WaitForIdle();
        }

        private void Window_Resized()
        {
            Vector2 NewSize = new Vector2(Window.Width, Window.Height);
            GraphicsDevice.ResizeMainWindow((uint)Window.Width, (uint)Window.Height);

            PostResize(NewSize);
        }

        RenderingCommand ClearScreenCommand;

        private void ConstructVeldridResources()
        {
            ClearScreenCommand = RendererResourceFactory.CreateRenderCommand();
            ClearScreenCommand.WithClearColor(Color.CornflowerBlue);
        }

        #region Renderer Ticking
        public void RenderStart(float DeltaTime)
        {
            Window.Title = string.Format("{0} - {2} - FPS: {1}", IEngine.Instance.Name, IEngine.Instance.FPS.ToString("0"), IEngine.Instance.Version);
            ExecuteRenderCommand(ClearScreenCommand);
        }

        public void RenderEnd(float DeltaTime)
        {
            GraphicsDevice.WaitForIdle();
            GraphicsDevice.SwapBuffers();
            GraphicsDevice.WaitForIdle();
        }

        public void SortRenderer(float DeltaTime)
        {
            CommandQueue.Sort((x, y) => (int)x.Transform.Translation.Z - (int)y.Transform.Translation.Z);
        }

        public void PreRender(float DeltaTime)
        {
            foreach (IRenderable renderable in Renderables)
            {
                renderable?.PreRender(this);
            }           
        }

        public void RenderAll(float DeltaTime)
        {     
            
            foreach (RenderingCommand command in CommandQueue)
            {
                ExecuteRenderCommand(command);
            }
            CommandQueue.Clear();
            
            foreach (IRenderable renderable in Renderables)
            {
                renderable?.Render(this);
            }
        }
        #endregion

        public override void ExecuteRenderCommand(RenderingCommand Command)
        {
            GraphicsDevice.SubmitCommands((Command as VeldridRenderingCommand).BuildStandardCommandList());
        }
       

        protected override Material ConstructDefaultMaterial()
        {
            //Manually load the shaders
            {
                VeldridShader frag = MaterialFactory.CreateShader(Shader.Stage.Fragment,
                 Assembly.GetExecutingAssembly().GetManifestResourceStream("Watertight.Rendering.VeldridRendering.Resources.Shaders.DefaultShader.glsl.frag")) as VeldridShader;
                FileSystem.EmplaceInMemoryResource(DefaultShader_Fragment, frag);
                frag.Bind();
            }
            {
                Shader frag = MaterialFactory.CreateShader(Shader.Stage.Fragment,
                Assembly.GetExecutingAssembly().GetManifestResourceStream("Watertight.Rendering.VeldridRendering.Resources.Shaders.DefaultShaderUntextured.glsl.frag"));
                FileSystem.EmplaceInMemoryResource(DefaultShaderUntextured_Fragment, frag);
                (frag as VeldridShader).Bind();
            }
            {
                VeldridShader vert = MaterialFactory.CreateShader(Shader.Stage.Vertex,
                    Assembly.GetExecutingAssembly().GetManifestResourceStream("Watertight.Rendering.VeldridRendering.Resources.Shaders.DefaultShader.glsl.vert")) as VeldridShader;
                FileSystem.EmplaceInMemoryResource(DefaultShader_Vertex, vert);
                vert.Bind();
            }

            VeldridMaterial m = MaterialFactory.Create() as VeldridMaterial;
            m.PostLoad();

            return m;
        }

        protected override Material ConstructDefaultWireframeMaterial()
        {
            Material m = DefaultMaterialPtr.Get<Material>().Clone() as Material;
            m.Topology = RenderTopology.Line_List;

            return m;
        }
    }
}
