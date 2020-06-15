using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Watertight.Math;
using Watertight.Rendering.Interfaces;
using Watertight.Util;

namespace Watertight.VulkanRenderer
{
   

    class VulkanVertexBuffer : IVertexBuffer, IDisposable
    {
        public static Vulkan.VertexInputBindingDescription InputBindingDescription
        {
            get
            {
                if (_InputBindingDescription == null)
                {
                    _InputBindingDescription = new Vulkan.VertexInputBindingDescription
                    {
                        Binding = 0,
                        Stride = Vertex.Stride,
                        InputRate = Vulkan.VertexInputRate.Vertex
                    };
                }

                return _InputBindingDescription.Value;
            }
        }
        private static Vulkan.VertexInputBindingDescription? _InputBindingDescription;

        public static Vulkan.VertexInputAttributeDescription[] VertexInputAttributeDescriptions
        {
            get
            {
                if (_VertexInputAttributeDescriptions == null)
                {
                    _VertexInputAttributeDescriptions = new Vulkan.VertexInputAttributeDescription[]
                    {
                        new Vulkan.VertexInputAttributeDescription
                        {
                            Binding = 0,
                            Location = 0,
                            Format = Vulkan.Format.R32G32B32Sfloat,
                            Offset = Vertex.Offset_Location,
                        },
                        new Vulkan.VertexInputAttributeDescription
                        {
                            Binding = 0,
                            Location = 1,
                            Format = Vulkan.Format.R32G32Sfloat,
                            Offset = Vertex.Offset_UV,
                        },
                        new Vulkan.VertexInputAttributeDescription
                        {
                            Binding = 0,
                            Location = 2,
                            Format = Vulkan.Format.R32G32B32A32Sfloat,
                            Offset = Vertex.Offset_Color,
                        },
                    };
                }

                return _VertexInputAttributeDescriptions;
            }

        }
        private static Vulkan.VertexInputAttributeDescription[] _VertexInputAttributeDescriptions;

        public static Vulkan.PipelineVertexInputStateCreateInfo VertexInputStateCreateInfo
        {
            get
            {
                if (_VertexInputStateCreateInfo == null)
                {
                    _VertexInputStateCreateInfo = new Vulkan.PipelineVertexInputStateCreateInfo
                    {
                        VertexAttributeDescriptions = VertexInputAttributeDescriptions,
                        VertexBindingDescriptions = new Vulkan.VertexInputBindingDescription[] { InputBindingDescription },
                    };
                }

                return _VertexInputStateCreateInfo;
            }
        }
        private static Vulkan.PipelineVertexInputStateCreateInfo _VertexInputStateCreateInfo;

        public Vulkan.Buffer VertexBuffer
        {
            get
            {
                return _VertexBuffer?.Buffer;
            }
        }
        private VulkanGPUBuffer _VertexBuffer;

        public Vulkan.Buffer IndexBuffer
        {
            get
            {
                return _IndexBuffer?.Buffer;
            }
        }
        private VulkanGPUBuffer _IndexBuffer;

      
        public bool Bound
        {
            get;
            private set;
        } = false;

      

        public void Bind()
        {           
            _VertexBuffer = new VulkanGPUBuffer(Vulkan.BufferUsageFlags.VertexBuffer);
            _VertexBuffer.WriteData(GetVertexData());

            _IndexBuffer = new VulkanGPUBuffer(Vulkan.BufferUsageFlags.IndexBuffer);
            _IndexBuffer.WriteData(GetIndexData());

            Bound = true;
        }

       

        public int NumIndicies
        {
            get;
            set;
        }

        public int NumVerticies
        {
            get;
            set;
        }
              
        byte[] VertexCache = new byte[] { };
        byte[] IndexCache = new byte[] { };

        public byte[] GetVertexData()
        {
            return VertexCache;
        }

        public void Dispose()
        {
            
        }

        public void SetVertexData(Vertex[] vertices, ushort[] indicies)
        {
            this.NumVerticies = vertices.Length;
            this.NumIndicies = indicies.Length;            
            VertexCache = Utils.ConvertVertexArrayToByteBuffer(vertices);
            IndexCache = Utils.ConvertIndexArrayToByteBuffer(indicies);
            Bound = false;
        }

        public byte[] GetIndexData()
        {
            return IndexCache;
        }
    }

   
}
