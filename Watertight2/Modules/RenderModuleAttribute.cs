using System;
using System.Collections.Generic;
using System.Text;

namespace Watertight.Modules
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RenderModuleAttribute : Attribute
    {
        [Flags]
        public enum RendererCapabilities : byte
        {
            None                = 0,
            Textures            = 0x01,
            Materials           = 0x02,
            VertexBuffers       = 0x04,

            //Helpers
            Supports_2D         = Textures | Materials | VertexBuffers,
            Supports_3D         = Textures | Materials | VertexBuffers,

            All = Byte.MaxValue,
        }

        internal RendererCapabilities Capabilities
        {
            get;
            private set;
        }

        internal SubclassOf<Watertight.Rendering.Renderer> RendererType
        {
            get;
            private set;
        }


        public RenderModuleAttribute(RendererCapabilities Capabilities, System.Type RendererType)
        {
            this.Capabilities = Capabilities;
            this.RendererType = RendererType;
        }
    }
}
