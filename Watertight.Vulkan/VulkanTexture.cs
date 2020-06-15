using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Watertight.Rendering.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Advanced;
using System.Reflection.Metadata.Ecma335;

namespace Watertight.VulkanRenderer
{
    class VulkanTexture : ITexture
    {
        private static VulkanRenderer Renderer
        {
            get => VulkanRenderer.Instance;
        }

        private static Vulkan.Device Device
        {
            get => Renderer.Device;
        }


        Image<Rgba32> InternalTexture;

        VulkanGPUBuffer GPUBuffer;

        public Vector2 Size
        {
            get
            {
                return new Vector2(InternalTexture.Width, InternalTexture.Height);
            }
        }

        public virtual Vulkan.ImageSubresourceRange SubresourceRange
        {
            get => new Vulkan.ImageSubresourceRange
            {
                AspectMask = Vulkan.ImageAspectFlags.Color,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1,
            };
        }

        public virtual Vulkan.ImageSubresourceLayers ImageSubresourceLayers
        {
            get => new Vulkan.ImageSubresourceLayers
            {
                AspectMask = Vulkan.ImageAspectFlags.Color,
                MipLevel = 0,
                BaseArrayLayer = 0,
                LayerCount = 1,
            };
        }

        public Vulkan.Extent3D vkExtent
        {
            get => new Vulkan.Extent3D
            {
                Width = (uint)Size.X,
                Height = (uint)Size.Y,
                Depth = 1,
            };
        }
           

    public VulkanTexture(Vector2 Size)
        {
            InternalTexture = new Image<Rgba32>((int)Size.X, (int)Size.Y);
            PostImageLoad();
        }

        public VulkanTexture(Vector2 Size, byte[] Data)
        {
            InternalTexture = Image.LoadPixelData<Rgba32>(Data, (int)Size.X, (int)Size.Y);
            PostImageLoad();
        }

        public VulkanTexture(byte[] Raw)
        {
            InternalTexture = Image.Load<Rgba32>(Raw);
            PostImageLoad();
        }

        public VulkanTexture(Vulkan.Image vkImage, Vector2 Size)
        {
            this.vkImage = vkImage;
            //TODO: Initialize everything properly from this vkImage, so we can use this VulkanTexture wrapper around Swapchain Images
        }
        
        ~VulkanTexture()
        {
            InternalTexture?.Dispose();
            GPUBuffer?.Dispose();
            if (vkImage != null)
            {
                Device.DestroyImage(vkImage);
            }

            if(vkImageView != null)
            {
                Device.DestroyImageView(vkImageView);
            }
        }

        private void PostImageLoad()
        {
            if (GPUBuffer != null)
            {
                GPUBuffer.Dispose();
                GPUBuffer = null;
            }

            GPUBuffer = new VulkanGPUBuffer(Vulkan.BufferUsageFlags.TransferSrc);
            GPUBuffer.WriteData(GetRGBABytes());

            CreateVulkanImage();
        }
          
        public void SetData(byte[] PixelData)
        {
            InternalTexture = Image.Load<Rgba32>(PixelData);
            PostImageLoad();
        }

        public byte[] GetRGBABytes()
        {
            return MemoryMarshal.AsBytes(InternalTexture.GetPixelSpan<Rgba32>()).ToArray();
        }

        #region VulkanBinding
        public Vulkan.Image vkImage;
        VulkanMemory Memory;
        public Vulkan.ImageView vkImageView;
        

        Vulkan.ImageCreateInfo ImageCreateInfo
        {
            get => new Vulkan.ImageCreateInfo
            {
                ImageType = Vulkan.ImageType.Image2D,
                Extent = vkExtent,
                MipLevels = 1,
                ArrayLayers = 1,
                Format = Vulkan.Format.R8G8B8A8Srgb,
                Tiling = Vulkan.ImageTiling.Optimal,
                InitialLayout = Vulkan.ImageLayout.Undefined,
                Usage = Vulkan.ImageUsageFlags.TransferDst | Vulkan.ImageUsageFlags.Sampled,
                SharingMode = Vulkan.SharingMode.Exclusive,
                Samples = Vulkan.SampleCountFlags.Count1
            };
        }

        public void CreateVulkanImage()
        {
            vkImage = Device.CreateImage(ImageCreateInfo);

            Vulkan.MemoryRequirements memoryRequirements = Device.GetImageMemoryRequirements(vkImage);
            Memory = new VulkanMemory(Vulkan.MemoryPropertyFlags.DeviceLocal, memoryRequirements);
            Memory.Allocate();

            Device.BindImageMemory(vkImage, Memory.DeviceMemory, Memory.MemoryOffset);

            VulkanTexture.TransformImageToLayout(this, Vulkan.ImageLayout.Undefined, Vulkan.ImageLayout.TransferDstOptimal);
            VulkanTexture.CopyBufferToTexture(GPUBuffer, this);
            VulkanTexture.TransformImageToLayout(this, Vulkan.ImageLayout.TransferDstOptimal, Vulkan.ImageLayout.ShaderReadOnlyOptimal);


            vkImageView = VulkanTexture.CreateImageView(this, Vulkan.Format.R8G8B8A8Srgb);

            //TODO: Move the sampler into it's own object.  We only need a small number of these.  Maybe make them resources?
            if (vkSampler == null)
            {
                CreateSampler();
            }
           
        }


        public static Vulkan.Sampler vkSampler;

        public static void CreateSampler()
        {
            Vulkan.SamplerCreateInfo SamplerCreateInfo = new Vulkan.SamplerCreateInfo
            {
                MagFilter = Vulkan.Filter.Linear,
                MinFilter = Vulkan.Filter.Linear,
                AddressModeU = Vulkan.SamplerAddressMode.Repeat,
                AddressModeV = Vulkan.SamplerAddressMode.Repeat,
                AddressModeW = Vulkan.SamplerAddressMode.Repeat,
                AnisotropyEnable = true,
                MaxAnisotropy = 16,
                BorderColor = Vulkan.BorderColor.FloatTransparentBlack,
                UnnormalizedCoordinates = false,
                CompareEnable = false,
                CompareOp = Vulkan.CompareOp.Always,
                MipmapMode = Vulkan.SamplerMipmapMode.Linear,
                MipLodBias = 0.0f,
                MinLod = 0.0f,
                MaxLod = 0.0f,
            };
            vkSampler = Device.CreateSampler(SamplerCreateInfo);
        }

        #endregion

        public static Vulkan.ImageView CreateImageView(VulkanTexture Texture, Vulkan.Format Format)
        {
            Vulkan.ImageViewCreateInfo imageViewCreateInfo = new Vulkan.ImageViewCreateInfo
            {
                Image = Texture.vkImage,
                ViewType = Vulkan.ImageViewType.View2D,
                Format = Format,
                SubresourceRange = Texture.SubresourceRange,
            };
            return Device.CreateImageView(imageViewCreateInfo);
        }

        public static void TransformImageToLayout(VulkanTexture Texture, Vulkan.ImageLayout OldLayout, Vulkan.ImageLayout NewLayout)
        {
            VulkanRenderingCommand TransferCommand = Renderer.RendererResourceFactory.CreateRenderCommand() as VulkanRenderingCommand;
            TransferCommand.BeginRecord(1);
            var Buffer = TransferCommand.CommandBuffers[0];

            Buffer.Begin(new Vulkan.CommandBufferBeginInfo
            {
                Flags = Vulkan.CommandBufferUsageFlags.OneTimeSubmit,
            });

            Vulkan.ImageMemoryBarrier Barrier = new Vulkan.ImageMemoryBarrier
            {
                OldLayout = OldLayout,
                NewLayout = NewLayout,
                DstQueueFamilyIndex = 0,
                SrcQueueFamilyIndex = 0,
                Image = Texture.vkImage,
                SubresourceRange = Texture.SubresourceRange,
                SrcAccessMask = 0,
                DstAccessMask = 0,
            };

            Vulkan.PipelineStageFlags sourceFlags = 0;
            Vulkan.PipelineStageFlags dstFlags = 0;

            if(OldLayout == Vulkan.ImageLayout.Undefined && NewLayout == Vulkan.ImageLayout.TransferDstOptimal)
            {
                Barrier.SrcAccessMask = 0;
                Barrier.DstAccessMask = Vulkan.AccessFlags.TransferWrite;

                sourceFlags = Vulkan.PipelineStageFlags.TopOfPipe;
                dstFlags = Vulkan.PipelineStageFlags.Transfer;
            }
            else if(OldLayout == Vulkan.ImageLayout.TransferDstOptimal && NewLayout == Vulkan.ImageLayout.ShaderReadOnlyOptimal)
            {
                Barrier.SrcAccessMask = Vulkan.AccessFlags.TransferWrite;
                Barrier.DstAccessMask = Vulkan.AccessFlags.ShaderRead;

                sourceFlags = Vulkan.PipelineStageFlags.Transfer;
                dstFlags = Vulkan.PipelineStageFlags.FragmentShader;
            }

            Buffer.CmdPipelineBarrier(sourceFlags,
                dstFlags,
                0,
                null,
                null,
                Barrier);

            Buffer.End();

            TransferCommand.EndRecord();
            TransferCommand.Submit();

            TransferCommand.Dispose();
        }

        public static void CopyBufferToTexture(VulkanGPUBuffer GPUBuffer, VulkanTexture Texture)
        {
            VulkanRenderingCommand TransferCommand = Renderer.RendererResourceFactory.CreateRenderCommand() as VulkanRenderingCommand;
            TransferCommand.BeginRecord(1);

            var Buffer = TransferCommand.CommandBuffers[0];
            
            Buffer.Begin(new Vulkan.CommandBufferBeginInfo
            {
                Flags = Vulkan.CommandBufferUsageFlags.OneTimeSubmit,
            });

            Vulkan.BufferImageCopy ImageCopyRegion = new Vulkan.BufferImageCopy
            {
                BufferOffset = GPUBuffer.Offset,
                BufferImageHeight = 0,
                BufferRowLength = 0,

                ImageSubresource = Texture.ImageSubresourceLayers,
                ImageOffset = new Vulkan.Offset3D(),
                ImageExtent = Texture.vkExtent,
            };

            Buffer.CmdCopyBufferToImage(GPUBuffer.Buffer, Texture.vkImage, Vulkan.ImageLayout.TransferDstOptimal, ImageCopyRegion);

            Buffer.End();

            TransferCommand.EndRecord();
            TransferCommand.Submit();

            TransferCommand.Dispose();
        }


    }
}
