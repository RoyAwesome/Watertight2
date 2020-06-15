using System;
using System.Collections.Generic;
using System.Text;

namespace Watertight.VulkanRenderer
{
    public class VulkanRenderPass
    {
        public Vulkan.RenderPass vkRenderPass;

        public static Vulkan.AttachmentDescription MainColorAttachmentDescription
        {
            get => new Vulkan.AttachmentDescription
            {
                Format = VulkanRenderer.Instance.SwapChainSurfaceFormat.Format,
                Samples = Vulkan.SampleCountFlags.Count1,
                LoadOp = Vulkan.AttachmentLoadOp.Load,
                StoreOp = Vulkan.AttachmentStoreOp.Store,
                StencilLoadOp = Vulkan.AttachmentLoadOp.DontCare,
                StencilStoreOp = Vulkan.AttachmentStoreOp.DontCare,

                InitialLayout = Vulkan.ImageLayout.PresentSrcKhr,
                FinalLayout = Vulkan.ImageLayout.PresentSrcKhr,
            };
        }



        public static Vulkan.AttachmentDescription ClearColorAttachmentDescription
        {
            get => new Vulkan.AttachmentDescription
            {
                Format = VulkanRenderer.Instance.SwapChainSurfaceFormat.Format,
                Samples = Vulkan.SampleCountFlags.Count1,
                LoadOp = Vulkan.AttachmentLoadOp.Clear,
                StoreOp = Vulkan.AttachmentStoreOp.Store,
                StencilLoadOp = Vulkan.AttachmentLoadOp.DontCare,
                StencilStoreOp = Vulkan.AttachmentStoreOp.DontCare,

                InitialLayout = Vulkan.ImageLayout.PresentSrcKhr,
                FinalLayout = Vulkan.ImageLayout.PresentSrcKhr,
            };
        }

        public VulkanRenderPass(VulkanRenderer Renderer, Vulkan.AttachmentDescription Attachment)
        {
            Vulkan.SubpassDescription subpassDescription = new Vulkan.SubpassDescription
            {
                PipelineBindPoint = Vulkan.PipelineBindPoint.Graphics,
                ColorAttachments = new Vulkan.AttachmentReference[] {
                    new Vulkan.AttachmentReference
                    {
                        Attachment = 0,
                        Layout = Vulkan.ImageLayout.ColorAttachmentOptimal,
                    }
                },
            };

            Vulkan.RenderPassCreateInfo prci = new Vulkan.RenderPassCreateInfo
            {
                Attachments = new Vulkan.AttachmentDescription[] { Attachment },
                Subpasses = new Vulkan.SubpassDescription[] { subpassDescription }
            };

            vkRenderPass = Renderer.Device.CreateRenderPass(prci);
        }

    }
}
