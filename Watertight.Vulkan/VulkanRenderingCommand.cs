using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Watertight.Math;
using Watertight.Rendering;
using Watertight.Rendering.Interfaces;
using Watertight.Util;

namespace Watertight.VulkanRenderer
{
    class VulkanRenderingCommand : RenderingCommand, IDisposable
    {
        public Vulkan.CommandBuffer[] CommandBuffers
        {
            get;
            private set;
        }

        public static Vulkan.Device Device
        {
            get
            {
                return VulkanRenderer.Instance.Device;
            }
        }

        public static VulkanRenderer Renderer
        {
            get => VulkanRenderer.Instance;
        }

        public static Vulkan.SurfaceCapabilitiesKhr SurfaceCapabilities
        {
            get => Renderer.SurfaceCapabilities;
        }



        public Vulkan.Semaphore Semaphore
        {
            get
            {
                if (_Seamphore == null)
                {
                    _Seamphore = Device.CreateSemaphore(new Vulkan.SemaphoreCreateInfo());
                }
                return _Seamphore;
            }
        }
        private Vulkan.Semaphore _Seamphore;

        public Vulkan.Queue SubmitQueue
        {
            get;
        }

        Vulkan.CommandPool CommandPool
        {
            get;
        }

        public override bool Dirty 
        { 
            get
            {
                if (CommandBuffers == null) return true;
                for(int i = 0; i < CommandBuffers.Length; i++)
                {
                    if (CommandBuffers[i] == null) return true;
                }
                return base.Dirty;
            }
            protected set => base.Dirty = value; 
        }


        public VulkanRenderingCommand(Vulkan.Queue Queue)
        {

            this.SubmitQueue = Queue;

            Vulkan.CommandPoolCreateInfo cpci = new Vulkan.CommandPoolCreateInfo
            {
                Flags = Vulkan.CommandPoolCreateFlags.ResetCommandBuffer
            };

            CommandPool = Device.CreateCommandPool(cpci);
        }

        ~VulkanRenderingCommand()
        {
            Dispose();
        }



        private Vulkan.Pipeline Pipeline;
        private void BindPipeline()
        {
            if (Pipeline != null)
            {
                VulkanRenderer.Instance.Device.DestroyPipeline(Pipeline);
                Pipeline = null;
            }

            VulkanMaterial vkmat = Material as VulkanMaterial;

            Vulkan.GraphicsPipelineCreateInfo pipelinecreate = vkmat.PipelineCreateInfo;

            if (VertexBuffer != null)
            {
                VulkanVertexBuffer vkVB = VertexBuffer as VulkanVertexBuffer;
                pipelinecreate.VertexInputState = VulkanVertexBuffer.VertexInputStateCreateInfo;
            }


            Pipeline = Device.CreateGraphicsPipelines(null, new Vulkan.GraphicsPipelineCreateInfo[] { pipelinecreate })[0];
        }

        public void WriteUniforms(int SwapIndex)
        {
            VulkanMaterial vkMat = Material as VulkanMaterial;

            if (vkMat != null && vkMat.HasUniformBuffers)
            {
                ICamera Camera = this.Camera ?? Renderer.MainCamera;

                byte[] Model = Transform.ToBytes();
                byte[] View = Camera?.View.ToBytes() ?? Matrix4x4.Identity.ToBytes();

                Matrix4x4 Proj = Camera?.Projection ?? Matrix4x4.Identity;
                //Proj.M22 *= -1;
                byte[] Projection = Proj.ToBytes();

                byte[] MVP = MathConvert.CombineByteArrays(Model, View, Projection);

                vkMat.UniformBuffers[SwapIndex].WriteData(MVP);
            }
        }



        public void CreateMainPassCommandBuffers(uint SwapChainIndex)
        {
            //Clean up this object            

            if (Material != null && Pipeline == null && VertexBuffer != null)
            {
                BindPipeline();
            }
            VulkanMaterial vkMaterial = Material as VulkanMaterial;


            VulkanVertexBuffer vkVertexBuffer = VertexBuffer as VulkanVertexBuffer;
            if (vkVertexBuffer != null && !vkVertexBuffer.Bound)
            {
                vkVertexBuffer.Bind();
            }

            if(CommandBuffers == null)
            {
                CommandBuffers = new Vulkan.CommandBuffer[Renderer.SwapChainImages.Length];
            }            

            if (CommandBuffers[SwapChainIndex] != null)
            {
                DisposeCommandBuffer(CommandBuffers[SwapChainIndex]);
            }

            CommandBuffers[SwapChainIndex] = CreateCommandBuffer(1)[0];

            var Buffer = CommandBuffers[SwapChainIndex];
            {
                Vulkan.CommandBufferBeginInfo beginInfo = new Vulkan.CommandBufferBeginInfo
                {
                    Flags = Vulkan.CommandBufferUsageFlags.SimultaneousUse,
                };


                Buffer.Begin(beginInfo);

                if (ClearColor.HasValue)
                {
                    Vulkan.ImageSubresourceRange subresourceRange = new Vulkan.ImageSubresourceRange
                    {
                        AspectMask = Vulkan.ImageAspectFlags.Color,
                        LevelCount = 1,
                        LayerCount = 1,
                    };
                    Vulkan.ImageMemoryBarrier presentToClearBarrier = new Vulkan.ImageMemoryBarrier
                    {
                        SrcAccessMask = Vulkan.AccessFlags.MemoryRead,
                        DstAccessMask = Vulkan.AccessFlags.TransferWrite,
                        OldLayout = Vulkan.ImageLayout.Undefined,
                        NewLayout = Vulkan.ImageLayout.TransferDstOptimal,
                        SrcQueueFamilyIndex = Renderer.presentQueueFamily,
                        DstQueueFamilyIndex = Renderer.presentQueueFamily,
                        Image = Renderer.SwapChainImages[SwapChainIndex],
                        SubresourceRange = subresourceRange,
                    };
                    Vulkan.ImageMemoryBarrier clearToPresentBarrier = new Vulkan.ImageMemoryBarrier
                    {
                        SrcAccessMask = Vulkan.AccessFlags.MemoryWrite,
                        DstAccessMask = Vulkan.AccessFlags.MemoryRead,
                        OldLayout = Vulkan.ImageLayout.TransferDstOptimal,
                        NewLayout = Vulkan.ImageLayout.PresentSrcKhr,
                        SrcQueueFamilyIndex = Renderer.presentQueueFamily,
                        DstQueueFamilyIndex = Renderer.presentQueueFamily,
                        Image = Renderer.SwapChainImages[SwapChainIndex],
                        SubresourceRange = subresourceRange,
                    };
                    Buffer.CmdPipelineBarrier(Vulkan.PipelineStageFlags.Transfer, Vulkan.PipelineStageFlags.Transfer, (Vulkan.DependencyFlags)0, null, null, presentToClearBarrier);
                    Buffer.CmdClearColorImage(Renderer.SwapChainImages[SwapChainIndex], Vulkan.ImageLayout.TransferDstOptimal, new Vulkan.ClearColorValue(ClearColor.Value.GetFloats()), subresourceRange);
                    Buffer.CmdPipelineBarrier(Vulkan.PipelineStageFlags.Transfer, Vulkan.PipelineStageFlags.BottomOfPipe, (Vulkan.DependencyFlags)0, null, null, clearToPresentBarrier);
                }

                if (vkVertexBuffer != null)
                {
                    Vulkan.RenderPassBeginInfo rpbi = new Vulkan.RenderPassBeginInfo
                    {
                        Framebuffer = Renderer.Framebuffers[SwapChainIndex],
                        RenderPass = vkMaterial.RenderPass.vkRenderPass,
                        RenderArea = new Vulkan.Rect2D { Extent = SurfaceCapabilities.CurrentExtent },
                    };

                    Buffer.CmdBeginRenderPass(rpbi, Vulkan.SubpassContents.Inline);
                    if (Pipeline != null)
                    {
                        if (vkMaterial.HasUniformBuffers)
                        {
                            Buffer.CmdBindDescriptorSet(Vulkan.PipelineBindPoint.Graphics, vkMaterial.PipelineLayout, 0, vkMaterial.descriptorSet[SwapChainIndex], null);
                        }

                        Buffer.CmdBindPipeline(Vulkan.PipelineBindPoint.Graphics, Pipeline);
                      
                        Buffer.CmdBindVertexBuffer(0, vkVertexBuffer.VertexBuffer, 0);
                        Buffer.CmdBindIndexBuffer(vkVertexBuffer.IndexBuffer, 0, Vulkan.IndexType.Uint16);
                        
                        Buffer.CmdDrawIndexed((uint)NumIndicies, 1, 0, 0, 0);
                    }
                    Buffer.CmdEndRenderPass();
                }


                Buffer.End();
            }

            EndRecord();
        }

        protected Vulkan.CommandBuffer[] CreateCommandBuffer(uint NumBuffers)
        {

            Vulkan.CommandBufferAllocateInfo cbai = new Vulkan.CommandBufferAllocateInfo
            {
                Level = Vulkan.CommandBufferLevel.Primary,
                CommandPool = CommandPool,
                CommandBufferCount = NumBuffers,
            };


            return Device.AllocateCommandBuffers(cbai);
        }

        public void BeginRecord(uint NumBuffers)
        {
            //Clean up this object if we have extra data
            Dispose();

            CommandBuffers = CreateCommandBuffer(NumBuffers);
        }

        public void EndRecord()
        {
            Dirty = false;
        }

        public void Submit()
        {
            //Can't submit a dirty command
            if (Dirty)
            {
                return;
            }
            Renderer.ExecuteManualRenderCommand(this);
        }

        public void Dispose()
        {
            if (CommandBuffers != null && CommandPool != null)
            {
                for(int i = 0; i < CommandBuffers.Length; i++)
                {
                    DisposeCommandBuffer(CommandBuffers[i]);
                }
                CommandBuffers = null;
            }

            Dirty = true;
        }

        private void DisposeCommandBuffer(Vulkan.CommandBuffer commandBuffer)
        {
            if(commandBuffer == null)
            {
                return;
            }
            Device.FreeCommandBuffer(CommandPool, commandBuffer);
        }

        #region Utilities
        public static void CopyBuffers(VulkanGPUBuffer src, VulkanGPUBuffer dst)
        {
            VulkanRenderingCommand cmd = Renderer.CreateRenderCommand() as VulkanRenderingCommand;

            cmd.BeginRecord(1);
            Vulkan.BufferCopy bufferCopy = new Vulkan.BufferCopy
            {
                Size = src.Size,
                SrcOffset = src.Offset,
                DstOffset = dst.Offset,
            };

            cmd.CommandBuffers[0].CmdCopyBuffer(src.Buffer, dst.Buffer, bufferCopy);

            cmd.EndRecord();
            cmd.Submit();
        }

        #endregion
    }
}
