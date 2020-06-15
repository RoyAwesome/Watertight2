using System;
using Watertight.Framework.Components;
using Watertight.Interfaces;
using Watertight.Rendering;
using Watertight.Rendering.Interfaces;
using SDL2;
using Watertight.Tickable;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Watertight.VulkanRenderer.ResourceFactories;
using System.Drawing;
using Watertight.Util;
using Watertight.Rendering.Materials;
using System.Numerics;
using System.Reflection;
using Watertight.Filesystem;

namespace Watertight.VulkanRenderer
{
    public static class Vk
    {
        public const int WHOLE_SIZE = -1;
    }

    public class VulkanRenderer : Renderer, IRendererResourceFactory
    {
        static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static VulkanRenderer Instance
        {
            get
            {
                return IEngine.Instance.Renderer as VulkanRenderer;
            }
        }

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

        public override ITextureFactory TextureFactory
        {
            get;
        } = new VulkanTextureFactory();

        public override IMaterialFactory MaterialFactory
        {
            get;
        } = new VulkanMaterialFactory();

        public override IRendererResourceFactory RendererResourceFactory => this;

        private List<IRenderable> Renderables = new List<IRenderable>();


        public override void AddRenderable(IRenderable Renderable)
        {
            Renderables.Add(Renderable);
        }

        public override void RemoveRenderable(IRenderable Renderable)
        {
            Renderables.Remove(Renderable);
        }

        public RenderingCommand CreateRenderCommand()
        {
            return new VulkanRenderingCommand(Queue);

        }

        public override void CreateRenderer()
        {
            TickFunction WindowTick = new TickFunction
            {
                TickFunc = UpdateInput,
                TickPriority = TickFunction.HighPriority,
                CanTick = true,
            };

            IEngine.Instance.GameThreadTickManager.AddTick(WindowTick);

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

            SDL.SDL_SetHint(SDL.SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING, "1");
            SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);
        }

      

        public ICamera CreateCamera(CameraComponent Owner)
        {
            return new VulkanCamera(Owner);
        }


        IntPtr Window = IntPtr.Zero;
        Vulkan.Instance vkInstance;
        Vulkan.SurfaceKhr Surface;
        public Vulkan.PhysicalDevice PrimaryDevice;
        Vulkan.Instance.DebugReportCallback DebugCallback;

        protected override void CreateWindow_Internal()
        {
            Logger.Info("Starting Up Vulkan Renderer");
            Window = SDL.SDL_CreateWindow("Test", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, (int)ScreenSize.X, (int)ScreenSize.Y, SDL.SDL_WindowFlags.SDL_WINDOW_VULKAN | SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
            if (Window == IntPtr.Zero)
            {
                Logger.Error("Failed to create SDL window");
                IEngine.Instance.Shutdown();
            }

            SDL.SDL_SetWindowBordered(Window, SDL.SDL_bool.SDL_TRUE);

            uint ExtensionCount;
            SDL.SDL_Vulkan_GetInstanceExtensions(Window, out ExtensionCount, null);
            IntPtr[] ExtensionNames = new IntPtr[(int)ExtensionCount];
            SDL.SDL_Vulkan_GetInstanceExtensions(Window, out ExtensionCount, ExtensionNames);


            List<string> Extensions = new List<string>();
            for (int i = 0; i < ExtensionNames.Length; i++)
            {
                IntPtr Name = ExtensionNames[i];
                string? Ext = Marshal.PtrToStringUTF8(Name);
                if (Ext != null)
                {
                    Extensions.Add(Ext);
                    Logger.Info("Enabling Vulkan Extension {0}", Ext);
                }
            }
            Extensions.Add("VK_EXT_debug_report");

            List<string> Layers = new List<string>
            {
               "VK_LAYER_KHRONOS_validation",
                /*
                "VK_LAYER_GOOGLE_threading",
                "VK_LAYER_LUNARG_parameter_validation",
                "VK_LAYER_LUNARG_device_limits",
                "VK_LAYER_LUNARG_object_tracker",
                "VK_LAYER_LUNARG_image",
                "VK_LAYER_LUNARG_core_validation",
                "VK_LAYER_LUNARG_swapchain",
                "VK_LAYER_GOOGLE_unique_objects"
                */
            };

            Vulkan.ApplicationInfo applicationInfo = new Vulkan.ApplicationInfo
            {
                EngineName = "Watertight",
                EngineVersion = 1,
                ApplicationName = IEngine.Instance.Name,
                ApplicationVersion = 1,
            };



            Vulkan.InstanceCreateInfo createInfo = new Vulkan.InstanceCreateInfo
            {
                ApplicationInfo = applicationInfo,
                EnabledExtensionCount = (uint)Extensions.Count,
                EnabledExtensionNames = Extensions.ToArray(),
                EnabledLayerCount = (uint)Layers.Count,
                EnabledLayerNames = Layers.ToArray(),
            };

            vkInstance = new Vulkan.Instance(createInfo);
            DebugCallback = DebugReportCallback;
            vkInstance.EnableDebug(DebugCallback);
           
            {
                //HACKHACK: SDL and Vulkan do not play nice together, so we have to manually set some internal data.

                //Get the Marshalled ptr from the Instance
                Vulkan.IMarshalling MarshalledInstace = vkInstance;
                ulong SurfaceId = 0;
                SDL.SDL_Vulkan_CreateSurface(Window, MarshalledInstace.Handle, out SurfaceId);

                //Two problems.  1) the constructor for SurfaceKhr is internal, so we need to construct it with Activator
                Vulkan.SurfaceKhr surfaceKhr = Activator.CreateInstance(typeof(Vulkan.SurfaceKhr), true) as Vulkan.SurfaceKhr;
                //The internal marshalled pointer here is a private field named "m".  We got a ulong from SDL above, so we need to set that by using Reflection
                var pointerField = typeof(Vulkan.SurfaceKhr).GetField("m", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                pointerField?.SetValue(surfaceKhr, SurfaceId);

                Surface = surfaceKhr;
            }

                       
            InitializeVulkan(vkInstance.EnumeratePhysicalDevices()[0], Surface);

        }



        protected override void CreateDefaultResources()
        {
            base.CreateDefaultResources();
            
            ClearScreenCommand = CreateRenderCommand()
                .WithDebugName("ClearScreenCommand")
                .WithClearColor(Color.CornflowerBlue)
                as VulkanRenderingCommand;
        }

       

        static Vulkan.Bool32 DebugReportCallback(Vulkan.DebugReportFlagsExt flags, Vulkan.DebugReportObjectTypeExt objectType, ulong objectHash, IntPtr location, int messageCode, IntPtr layerPrefix, IntPtr message, IntPtr userData)
        {
            if(flags.HasFlag(Vulkan.DebugReportFlagsExt.Information))
            {
                return false;
            }
            string layerString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(layerPrefix);
            string messageString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message);

            Logger.Debug("Vulkan DebugReport layer ({0}) Message: {1}", layerString, messageString);
            return false;
        }

        VulkanRenderingCommand ClearScreenCommand;

        #region Vulkan Startup

        public Vulkan.Device Device;
        Vulkan.Queue Queue;
        public Vulkan.SurfaceCapabilitiesKhr SurfaceCapabilities;
        public Vulkan.SwapchainKhr SwapChain;
        Vulkan.Image[] Images;
        Vulkan.Framebuffer[] FrameBuffers;

        Vulkan.Fence[] SubmitFences;
        Vulkan.Semaphore ImageAvailableSemaphor;

        public VulkanRenderPass ClearScreenPass;
        public VulkanRenderPass MainRenderPass;

        public Vulkan.Image[] SwapChainImages
        {
            get => Images;
        }

        public Vulkan.Framebuffer[] Framebuffers
        {
            get => FrameBuffers;
        }

        public int SwapChainImageCount
        {
            get
            {
                return Images.Length;
            }
        }


        public Vulkan.SurfaceFormatKhr SwapChainSurfaceFormat;

        public uint presentQueueFamily = 0;

        public void InitializeVulkan(Vulkan.PhysicalDevice physicalDevice, Vulkan.SurfaceKhr OutputSurface)
        {
            Logger.Info("Initialzing Vulkan with {0}", physicalDevice.GetProperties().DeviceName);
            PrimaryDevice = physicalDevice;
            

            var queueFamilyProperties = PrimaryDevice.GetQueueFamilyProperties();

            uint qfpIndex = 0;
            for(; qfpIndex < queueFamilyProperties.Length; qfpIndex++)
            {
                if(!PrimaryDevice.GetSurfaceSupportKHR(qfpIndex, OutputSurface))
                {
                    continue;
                }
                if(queueFamilyProperties[qfpIndex].QueueFlags.HasFlag(Vulkan.QueueFlags.Graphics))
                {
                    break;
                }
            }

            Vulkan.DeviceQueueCreateInfo qinfo = new Vulkan.DeviceQueueCreateInfo
            {
                QueuePriorities = new float[] { 1.0f },
                QueueFamilyIndex = qfpIndex,
                
            };

            Vulkan.DeviceCreateInfo dcInfo = new Vulkan.DeviceCreateInfo
            {
                EnabledExtensionNames = new string[] { "VK_KHR_swapchain" },
                QueueCreateInfos = new Vulkan.DeviceQueueCreateInfo[] { qinfo },      
                EnabledFeatures = new Vulkan.PhysicalDeviceFeatures
                {
                    SamplerAnisotropy = true,
                }
            };
            presentQueueFamily = qfpIndex;
            Device = physicalDevice.CreateDevice(dcInfo);
            Queue = Device.GetQueue(0, 0);
            SurfaceCapabilities = physicalDevice.GetSurfaceCapabilitiesKHR(OutputSurface);
            SwapChainSurfaceFormat = SelectSurfaceFormat(physicalDevice, OutputSurface);
            SwapChain = CreateSwapchain(Surface, SwapChainSurfaceFormat);
            Images = Device.GetSwapchainImagesKHR(SwapChain);

            ClearScreenPass = new VulkanRenderPass(this, VulkanRenderPass.ClearColorAttachmentDescription);
            MainRenderPass = new VulkanRenderPass(this, VulkanRenderPass.MainColorAttachmentDescription);

            FrameBuffers = CreateFramebuffers(Images, SwapChainSurfaceFormat);

            SubmitFences = new Vulkan.Fence[Images.Length];
            for(int i = 0; i < SubmitFences.Length; i++)
            {
                SubmitFences[i] = Device.CreateFence(new Vulkan.FenceCreateInfo
                {
                    Flags = Vulkan.FenceCreateFlags.Signaled,
                });
            }
            
            ImageAvailableSemaphor = Device.CreateSemaphore(new Vulkan.SemaphoreCreateInfo());
        }

        Vulkan.SurfaceFormatKhr SelectSurfaceFormat(Vulkan.PhysicalDevice pd, Vulkan.SurfaceKhr surf)
        {
            foreach(var Format in pd.GetSurfaceFormatsKHR(surf))
            {
                if(Format.Format == Vulkan.Format.R8G8B8A8Unorm || Format.Format == Vulkan.Format.B8G8R8A8Unorm)
                {
                    return Format;
                }
            }

            throw new Exception("Failed to find a valid surface format we can use");
        }

        public Watertight.Math.Rectangle SwapChainImageRectangle;

        Vulkan.SwapchainKhr CreateSwapchain(Vulkan.SurfaceKhr surf, Vulkan.SurfaceFormatKhr surfaceFormat)
        {
            var compositeAlpha = SurfaceCapabilities.SupportedCompositeAlpha.HasFlag(Vulkan.CompositeAlphaFlagsKhr.Inherit)
                ? Vulkan.CompositeAlphaFlagsKhr.Inherit
                : Vulkan.CompositeAlphaFlagsKhr.Opaque;

            SwapChainImageRectangle = Watertight.Math.Rectangle.FromMinAndSize(Vector2.Zero, new Vector2(SurfaceCapabilities.CurrentExtent.Width, SurfaceCapabilities.CurrentExtent.Height));

            var swapchainInfo = new Vulkan.SwapchainCreateInfoKhr
            {
                Surface = surf,
                MinImageCount = SurfaceCapabilities.MinImageCount,
                ImageFormat = surfaceFormat.Format,
                ImageColorSpace = surfaceFormat.ColorSpace,
                ImageExtent = SurfaceCapabilities.CurrentExtent,
                ImageUsage = Vulkan.ImageUsageFlags.ColorAttachment | Vulkan.ImageUsageFlags.TransferDst,
                PreTransform = Vulkan.SurfaceTransformFlagsKhr.Identity,
                ImageArrayLayers = 1,
                ImageSharingMode = Vulkan.SharingMode.Exclusive,
                QueueFamilyIndices = new uint[] { 0 },
                PresentMode = Vulkan.PresentModeKhr.Immediate,
                CompositeAlpha = compositeAlpha,
            };

            return Device.CreateSwapchainKHR(swapchainInfo);
        }

        public Vulkan.PipelineViewportStateCreateInfo PipelineViewportStateCreateInfo
        {
            get => new Vulkan.PipelineViewportStateCreateInfo
            {
                ViewportCount = 1,
                Viewports = new Vulkan.Viewport[]
                {
                    new Vulkan.Viewport
                    {
                        X = 0,
                        Y = 0,
                        Width = SwapChainImageRectangle.Width,
                        Height = SwapChainImageRectangle.Height,
                        MinDepth = 0.0f,
                        MaxDepth = 1.0f,
                    }
                },
                ScissorCount = 1,
                Scissors = new Vulkan.Rect2D[]
                {
                    new Vulkan.Rect2D
                    {
                        Extent = new Vulkan.Extent2D
                        {
                            Width = (uint)SwapChainImageRectangle.Width,
                            Height = (uint)SwapChainImageRectangle.Height,
                        },
                        Offset = new Vulkan.Offset2D
                        {
                            X = 0,
                            Y = 0,
                        }
                    }
                }
            };
        }


        Vulkan.Framebuffer[] CreateFramebuffers(Vulkan.Image[] images, Vulkan.SurfaceFormatKhr surfaceFormat)
        {
            Vulkan.ImageView[] DisplayViews = new Vulkan.ImageView[images.Length];

            for(int i =0; i < images.Length; i++)
            {
                Vulkan.ImageViewCreateInfo ivci = new Vulkan.ImageViewCreateInfo
                {
                    Image = images[i],
                    ViewType = Vulkan.ImageViewType.View2D,
                    Format = surfaceFormat.Format,
                    Components = new Vulkan.ComponentMapping
                    {
                        R = Vulkan.ComponentSwizzle.R,
                        G = Vulkan.ComponentSwizzle.G,
                        B = Vulkan.ComponentSwizzle.B,
                        A = Vulkan.ComponentSwizzle.A,
                    },
                    SubresourceRange = new Vulkan.ImageSubresourceRange
                    {
                        AspectMask = Vulkan.ImageAspectFlags.Color,
                        LevelCount = 1,
                        LayerCount = 1,
                    },
                };
                DisplayViews[i] = Device.CreateImageView(ivci);
            }

            Vulkan.Framebuffer[] framebuffers = new Vulkan.Framebuffer[images.Length];
            for (int i = 0; i < images.Length; i++)
            {
                Vulkan.FramebufferCreateInfo fbci = new Vulkan.FramebufferCreateInfo
                {
                    Layers = 1,
                    RenderPass = MainRenderPass.vkRenderPass,
                    Attachments= new Vulkan.ImageView[] { DisplayViews[i]},
                    Width = SurfaceCapabilities.CurrentExtent.Width,
                    Height = SurfaceCapabilities.CurrentExtent.Height,
                };
                framebuffers[i] = Device.CreateFramebuffer(fbci);
            }

            return framebuffers;
        }
        #endregion
               
        #region Input

        private void UpdateInput(float DeltaTime)
        {
            SDL.SDL_PumpEvents();

           
            SDL.SDL_Event Event;
            while(SDL.SDL_PollEvent(out Event) > 0)
            {
                if(Event.type == SDL.SDL_EventType.SDL_QUIT)
                {
                    IEngine.Instance.Shutdown();
                }
            }
        }

        #endregion

        #region RenderTickPhases
        public void RenderStart(float DeltaTime)
        {
            if(Window != IntPtr.Zero)
            {
                SDL.SDL_SetWindowTitle(Window, string.Format("{0} - {2} - FPS: {1}", IEngine.Instance.Name, IEngine.Instance.FPS.ToString("0"), IEngine.Instance.Version));
               
            }

        }

        public void RenderEnd(float DeltaTime)
        {
            SDL.SDL_UpdateWindowSurface(Window);
        }

        public void SortRenderer(float DeltaTime)
        {
           // CommandQueue.Sort((x, y) => (int)x.Transform.Translation.Z - (int)y.Transform.Translation.Z);
        }

        public void PreRender(float DeltaTime)
        {
            foreach (IRenderable renderable in Renderables)
            {
                renderable?.PreRender(this);
            }            
        }

        private uint nextIndex;
        Vulkan.Semaphore PreviousCommandSemaphore = null;

        public void RenderAll(float DeltaTime)
        {
            nextIndex = Device.AcquireNextImageKHR(SwapChain, ulong.MaxValue, ImageAvailableSemaphor);
            PreviousCommandSemaphore = ImageAvailableSemaphor;


            ExecuteRenderCommand(ClearScreenCommand);         

            foreach (RenderingCommand command in CommandQueue)
            {
                ExecuteRenderCommand(command);
               
            }
            CommandQueue.Clear();

            foreach (IRenderable renderable in Renderables)
            {
                renderable?.Render(this);
            }


            Vulkan.PresentInfoKhr presentInfo = new Vulkan.PresentInfoKhr
            {
                Swapchains = new Vulkan.SwapchainKhr[] { SwapChain },
                ImageIndices = new uint[] { nextIndex },
                WaitSemaphores = new Vulkan.Semaphore[] { PreviousCommandSemaphore },
            };

            Queue.PresentKHR(presentInfo);
            Queue.WaitIdle();

            //Hold the commands from garbage collection until we are done
            foreach(GCHandle handle in PinnedCommandsThisFrame)
            {
                handle.Free();
            }
            PinnedCommandsThisFrame.Clear();
        }

        List<GCHandle> PinnedCommandsThisFrame = new List<GCHandle>();

        public override void ExecuteRenderCommand(RenderingCommand Command)
        {
            VulkanRenderingCommand vkRenderCommand = Command as VulkanRenderingCommand;           
            if (vkRenderCommand.Dirty)
            {
                vkRenderCommand.CreateMainPassCommandBuffers(nextIndex);
            }
            
            vkRenderCommand.WriteUniforms((int)nextIndex);
            
            Vulkan.SubmitInfo submitInfo = new Vulkan.SubmitInfo
            {
                WaitSemaphores = new Vulkan.Semaphore[] { PreviousCommandSemaphore },
                SignalSemaphores = new Vulkan.Semaphore[] { vkRenderCommand.Semaphore },
                WaitDstStageMask = new Vulkan.PipelineStageFlags[] { Vulkan.PipelineStageFlags.ColorAttachmentOutput },
                CommandBuffers = new Vulkan.CommandBuffer[] { vkRenderCommand.CommandBuffers[nextIndex] },                
            };

            Queue.Submit(submitInfo);      
            PreviousCommandSemaphore = vkRenderCommand.Semaphore;


            PinnedCommandsThisFrame.Add(GCHandle.Alloc(Command));
        }

        public void ExecuteManualRenderCommand(RenderingCommand Command)
        {
            VulkanRenderingCommand vkRenderCommand = Command as VulkanRenderingCommand;
            Vulkan.SubmitInfo submitInfo = new Vulkan.SubmitInfo
            {
                 CommandBuffers = vkRenderCommand.CommandBuffers,
            };

            Queue.Submit(submitInfo);
            Queue.WaitIdle();
        }

        protected override Material ConstructDefaultMaterial()
        {
            //Manually load the shaders
            {
                Shader frag = MaterialFactory.CreateShader(Shader.Stage.Fragment,
                 Assembly.GetExecutingAssembly().GetManifestResourceStream("Watertight.VulkanRenderer.Resources.Shaders.DefaultShader.frag.spv"));
                FileSystem.EmplaceInMemoryResource(VulkanMaterial.DefaultShader_Fragment, frag);
            }
            {
                Shader vert = MaterialFactory.CreateShader(Shader.Stage.Vertex,
                    Assembly.GetExecutingAssembly().GetManifestResourceStream("Watertight.VulkanRenderer.Resources.Shaders.DefaultShader.vert.spv"));
                FileSystem.EmplaceInMemoryResource(VulkanMaterial.DefaultShader_Vertex, vert);
            }

            VulkanMaterial m = MaterialFactory.Create() as VulkanMaterial;
            m.PostLoad();

            return m;
        }

        public IVertexBuffer CreateVertexBuffer()
        {
            return new VulkanVertexBuffer();
        }

        protected override Material ConstructDefaultWireframeMaterial()
        {
            Material m = DefaultMaterialPtr.Get<Material>().Clone() as Material;
            m.Topology = RenderTopology.Line_List;

            return m;
        }





        #endregion
    }
}
