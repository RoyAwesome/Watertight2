using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Watertight.VulkanRenderer
{
    class VulkanGPUBuffer : IDisposable
    {
        private static VulkanRenderer Renderer
        {
            get => VulkanRenderer.Instance;
        }

        private static Vulkan.Device Device
        {
            get => Renderer.Device;
        }

        public Vulkan.Buffer Buffer;

        public ulong Size
        {
            get;
            private set;
        }

        public uint Offset
        {
            get => Memory?.MemoryOffset ?? 0;
        }

        public bool Allocated
        {
            get => Memory != null;
        }
               
        Vulkan.BufferUsageFlags BufferUsage;
        bool ReuseMemory = true;

        VulkanMemory Memory;

        public VulkanGPUBuffer(Vulkan.BufferUsageFlags BufferUsage)
        {
            this.BufferUsage = BufferUsage;
        }

        public void CreateBuffer(uint Size)
        {
            //If we already have memory or a buffer when we create this buffer, throw it away.
            Dispose();

            Vulkan.BufferCreateInfo vertexBufferCreateInfo = new Vulkan.BufferCreateInfo
            {
                Size = Size,
                Usage = BufferUsage,
                SharingMode = Vulkan.SharingMode.Exclusive,
            };

            Buffer = VulkanRenderer.Instance.Device.CreateBuffer(vertexBufferCreateInfo);
            this.Size = Size;
        }

        public void AllocateMemory(Vulkan.MemoryPropertyFlags MemoryFlags = Vulkan.MemoryPropertyFlags.HostVisible | Vulkan.MemoryPropertyFlags.HostCoherent)
        {
            //Clean up any previously allocated memory
            if(Memory != null)
            {
                Memory.Dispose();
            }

            Vulkan.MemoryRequirements memoryRequirements = VulkanRenderer.Instance.Device.GetBufferMemoryRequirements(Buffer);
            Memory = new VulkanMemory(MemoryFlags, memoryRequirements);
            Memory.Allocate();
                        
            VulkanRenderer.Instance.Device.BindBufferMemory(Buffer, Memory.DeviceMemory, Memory.MemoryOffset);
        }

        public void WriteData(byte[] Data)
        {
            if(Buffer == null || (ulong)Data.Length > Size)
            {
                CreateBuffer((uint)Data.Length);
            }
           
            if(!ReuseMemory || !Allocated)
            {
                AllocateMemory();
            }

            IntPtr memory = VulkanRenderer.Instance.Device.MapMemory(Memory.DeviceMemory, 0, Data.Length);
            Marshal.Copy(Data, 0, memory, Data.Length);
            VulkanRenderer.Instance.Device.UnmapMemory(Memory.DeviceMemory);

        }       

        public void Dispose()
        {
            if (Memory != null)
            {
                Memory.Dispose();
            }
            if(Buffer != null)
            {
                Device.DestroyBuffer(Buffer);
            }
        }
    }
}
