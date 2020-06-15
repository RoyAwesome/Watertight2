using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Watertight.Framework.Components;
using Watertight.Input;
using Watertight.Interfaces;
using Watertight.Rendering;
using Watertight.Rendering.Interfaces;
using Watertight.Rendering.Materials;
using Watertight.SFML.Input;
using Watertight.Tickable;

namespace Watertight.SFML
{
    public class SFMLRenderer : Renderer, IRendererResourceFactory
    {       
        const int DoRenderPriority = TickFunction.World;
        const int SortRenderingPriority = DoRenderPriority + 0x000F;
        const int PreRenderingPriority = SortRenderingPriority + 0x000F;


        public static SFMLRenderer Instance
        {
            get
            {
                return IEngine.Instance.Renderer as SFMLRenderer;
            }
        }

        public RenderWindow Window
        {
            get;
            set;
        }

        private SFMLKeyboardInputSource KeyboardInput
        {
            get;
        } = new SFMLKeyboardInputSource();

        private SFMLMouseInputSource MouseInput
        {
            get;
        } = new SFMLMouseInputSource();



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

        public override ICamera MainCamera
        {
            get
            {
                return _MainCamera;
            }
            set
            {
                _MainCamera = value as SFMLCamera;
                if (_MainCamera != null && Window != null)
                {
                    Window.SetView(_MainCamera);
                }
            }
        }

        public override ITextureFactory TextureFactory 
        { 
            get;
        } = new SFMLTextureFactory();

        public override IMaterialFactory MaterialFactory => throw new NotImplementedException();

        public override IRendererResourceFactory RendererResourceFactory => this;

        private SFMLCamera _MainCamera;

        private List<IRenderable> Renderables = new List<IRenderable>();

        private RenderingCommand ClearScreenCommand;

        public SFMLRenderer()
        {
        }

        public override void CreateRenderer()
        {
            //SFML doesn't have a seperate renderer.  

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



            
        }

        protected override void CreateWindow_Internal()
        {
            TickFunction WindowTick = new TickFunction
            {
                TickFunc = (DeltaTime) => { Window.DispatchEvents(); },
                TickPriority = TickFunction.HighPriority,
                CanTick = true,
            };

            IEngine.Instance.GameThreadTickManager.AddTick(WindowTick);


            Window = new RenderWindow(new VideoMode((uint)ScreenSize.X, (uint)ScreenSize.Y), IEngine.Instance.Name);
            Window.Closed += (s, e) => IEngine.Instance.Shutdown();

            Window.Resized += Window_Resized;

            Window.KeyPressed += KeyboardInput.Window_KeyPressed;
            Window.KeyReleased += KeyboardInput.Window_KeyReleased;

            Window.MouseButtonPressed += MouseInput.Window_KeyPressed;
            Window.MouseButtonReleased += MouseInput.Window_KeyReleased;

            Window.MouseWheelScrolled += MouseInput.Window_MouseWheelScrolled;

            Window.SetKeyRepeatEnabled(false);

            InputProcessor.RegisterInputSource(KeyboardInput);
            InputProcessor.RegisterInputSource(MouseInput);

            ClearScreenCommand = CreateRenderCommand()
                    .WithClearColor(System.Drawing.Color.CornflowerBlue);
        }

        private void Window_Resized(object sender, SizeEventArgs e)
        {
            ScreenSize = new Vector2(e.Width, e.Height);
        }

        public void RenderStart(float DeltaTime)
        {
            Window.SetTitle(string.Format("{0} - {2} - FPS: {1}", IEngine.Instance.Name, IEngine.Instance.FPS.ToString("0"), IEngine.Instance.Version));
            ExecuteRenderCommand(ClearScreenCommand);

            if (MainCamera != null && MainCamera is SFMLCamera)
            {
                Window.SetView(MainCamera as SFMLCamera);
            }
        }

        public void RenderEnd(float DeltaTime)
        {
            Window.Display();
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
            (MainCamera as SFMLCamera)?.PreRender(DeltaTime);
        }

        public void RenderAll(float DeltaTime)
        {
            foreach(RenderingCommand command in CommandQueue)
            {
                ExecuteRenderCommand(command);                            
            }
            CommandQueue.Clear();

            foreach (IRenderable renderable in Renderables)
            {
                renderable?.Render(this);
            }
        }

        public override void AddRenderable(IRenderable Renderable)
        {
            Renderables.Add(Renderable);
        }

        public override void RemoveRenderable(IRenderable Renderable)
        {
            Renderables.Remove(Renderable);
        }

        public  RenderingCommand CreateRenderCommand()
        {
            return new SFMLRenderingCommand();
        }

        public ICamera CreateCamera(CameraComponent Owner)
        {
            return new SFMLCamera()
            {
                Owner = Owner
            };
        }

        public override void ExecuteRenderCommand(RenderingCommand Command)
        {
            if (!(Command is SFMLRenderingCommand))
            {
                return;
            }
            Window.Draw(Command as SFMLRenderingCommand);
        }

        protected override Material ConstructDefaultMaterial()
        {
            return new SFMLMaterial();
        }

        public IVertexBuffer CreateVertexBuffer()
        {
            return new SFMLVertexBuffer();
        }

        protected override Material ConstructDefaultWireframeMaterial()
        {
            Material m = new SFMLMaterial();
            m.Topology = RenderTopology.Line_List;
            return m;
        }
    }
}
