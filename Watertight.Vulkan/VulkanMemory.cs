using System;
using System.Collections.Generic;
using System.Text;

namespace Watertight.VulkanRenderer
{
    class VulkanMemory : IDisposable
    {
        private static VulkanRenderer Renderer
        {
            get => VulkanRenderer.Instance;
        }

        private static Vulkan.Device Device
        {
            get => Renderer.Device;
        }

        public Vulkan.DeviceMemory DeviceMemory
        {
            get;
            private set;
        }

        public uint MemoryOffset
        {
            get;
            private set;
        } = 0;

        public ulong Size
        {
            get => MemoryRequirements.Size;
        }


        public Vulkan.MemoryPropertyFlags MemoryFlags
        {
            get;
            private set;
        }

        Vulkan.MemoryRequirements MemoryRequirements;

        public VulkanMemory(Vulkan.MemoryPropertyFlags MemoryFlags, Vulkan.MemoryRequirements memoryRequirements)
        {
            this.MemoryFlags = MemoryFlags;
            this.MemoryRequirements = memoryRequirements;
        }

        ~VulkanMemory()
        {
            if(DeviceMemory != null)
            {
                VulkanRenderer.Instance.Device.FreeMemory(DeviceMemory);
            }            
        }

        public void Allocate()
        {
            if (DeviceMemory != null)
            {
                Dispose();
            }

            Vulkan.MemoryAllocateInfo allocInfo = new Vulkan.MemoryAllocateInfo
            {
                AllocationSize = MemoryRequirements.Size,
                MemoryTypeIndex = (uint)FindMemoryType(MemoryRequirements.MemoryTypeBits)
            };
            DeviceMemory = VulkanRenderer.Instance.Device.AllocateMemory(allocInfo);
        }

        public void Dispose()
        {
            VulkanRenderer.Instance.Device.FreeMemory(DeviceMemory);
        }

        private int FindMemoryType(uint typeFilter)
        {
            Vulkan.PhysicalDeviceMemoryProperties physicalDeviceMemoryProperties = VulkanRenderer.Instance.PrimaryDevice.GetMemoryProperties();
            for (int i = 0; i < physicalDeviceMemoryProperties.MemoryTypes.Length; i++)
            {
                if (((int)typeFilter & (1 << i)) > 0 && (physicalDeviceMemoryProperties.MemoryTypes[i].PropertyFlags.HasFlag(MemoryFlags)))
                {
                    return i;
                }
            }

            throw new Exception("Failed to find suitable memory type");
        }
    }
}
