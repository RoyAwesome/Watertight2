using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using Watertight.Filesystem;
using Watertight.Framework.Components;
using Watertight.Interfaces;
using Watertight.Rendering.Interfaces;
using Watertight.Rendering.Materials;
using Watertight.ResourceLoaders.Rendering;
using Watertight.Tickable;

namespace Watertight.Rendering
{
    public delegate void OnResize(Vector2 OldSize);

    public abstract class Renderer
    {
        static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static ResourcePtr DefaultMaterialPtr
        {
            get => new ResourcePtr("mat:Default");
        }

        public static ResourcePtr DefaultWireframeMaterialPtr
        {
            get => new ResourcePtr("mat:DefaultWireframe");
        }
        public static ResourcePtr DefaultTexturePtr
        {
            get => new ResourcePtr("texture:Default");
        }

        public event OnResize WindowResized;

        public TickManager TickManager
        {
            get;
            set;
        } = new TickManager();

        internal TickFunction RenderTickFunction
        {
            get;
        } = new TickFunction
        {
            TickPriority = TickFunction.World - 0x007F,
            CanTick = true,
        };

        public virtual ICamera MainCamera
        {
            get;
            set;
        }

        public Vector2 ScreenSize
        {
            get;
            protected set;
        } = new Vector2(800, 600);

        public abstract ITextureFactory TextureFactory
        {
            get;
        }

        public abstract IMaterialFactory MaterialFactory
        {
            get;
        }

        public abstract IRendererResourceFactory RendererResourceFactory
        {
            get;
        }
        public event Action OnWindowCreated;

        protected List<RenderingCommand> CommandQueue = new List<RenderingCommand>();
        
        public Renderer()
        {
            RenderTickFunction.TickFunc = RenderScene;
        }

               
        public void CreateWindow()
        {
            CreateWindow_Internal();

            OnWindowCreated?.Invoke();
        }

        protected void PostResize(Vector2 NewScrensize)
        {
            Vector2 OldSize = this.ScreenSize;
            this.ScreenSize = NewScrensize;
            WindowResized?.Invoke(OldSize);
        }


        protected abstract void CreateWindow_Internal();

        public abstract void CreateRenderer();

        internal virtual void ConstructRendererDefaults()
        {
           

            {
                Logger.Info("Constructing Default Engine Texture.");
                ITexture EngineDefault = ConstructDefaultTexture();
                if(EngineDefault == null)
                {
                    throw new Exception("Engine Default Texture cannot be null. Something went horribly wrong");
                }
                FileSystem.EmplaceInMemoryResource(DefaultTexturePtr, EngineDefault);
            }

            {
                Logger.Info("Constructing Default Engine Material.");
                Material EngineDefault = ConstructDefaultMaterial();
                if (EngineDefault == null)
                {
                    throw new Exception("Engine Default Material cannot be null.  Override Renderer.ConstructDefaultMaterial and return a default material!");
                }
                FileSystem.EmplaceInMemoryResource(DefaultMaterialPtr, EngineDefault);
            }

            {
                Logger.Info("Constructing Default Engine Wireframe Material.");
                Material EngineDefault = ConstructDefaultWireframeMaterial();
                if (EngineDefault == null)
                {
                    throw new Exception("Engine Default Wireframe Material cannot be null.  Override Renderer.ConstructDefaultWireframeMaterial and return a default material!");
                }
                FileSystem.EmplaceInMemoryResource(DefaultWireframeMaterialPtr, EngineDefault);
            }


            CreateDefaultResources();
        }

        protected abstract Material ConstructDefaultMaterial();
        protected abstract Material ConstructDefaultWireframeMaterial();

        protected virtual ITexture ConstructDefaultTexture()
        {
            Color Pink = Color.FromArgb(255, 0, 255);
            Color Black = Color.Black;


            Vector2 TextureSize = new Vector2(64, 64);
            Color[,] Colors = new Color[(int)TextureSize.X, (int)TextureSize.Y];
            for(int x = 0; x < TextureSize.X; x++)
            {
                for(int y = 0; y < TextureSize.Y; y++)
                {                   
                    Colors[x, y] = (x <= TextureSize.X / 2 && y <= TextureSize.Y / 2
                    || x > TextureSize.X / 2 && y > TextureSize.Y / 2) ? 
                    Black :
                    Pink;
                }
            }

            return TextureFactory.Create(Colors.Cast<Color>().ToArray(), TextureSize);
        }

        protected virtual void CreateDefaultResources()
        {

        }


        public abstract void ExecuteRenderCommand(RenderingCommand Command);

        public abstract void AddRenderable(IRenderable Renderable);

        public abstract void RemoveRenderable(IRenderable Renderable);
               

        public virtual void EnqueueRenderCommand(RenderingCommand CommandThisFrame)
        {
            CommandQueue.Add(CommandThisFrame);
        }



        public virtual void RenderScene(float deltaTime)
        {
            TickManager.ExecuteSingleTick();
        }

        
    }
}
