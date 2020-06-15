using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Watertight.Filesystem;
using Watertight.Interfaces;

namespace Watertight.Rendering.Materials
{   
    public abstract class Shader : IIsResource
    {
        //Spir-V's magic number.  The first 4 bytes should be this, and if they are, we've loaded a spirv module.
        public const uint SPIRVMagic = 0x07230203;
        public ResourcePtr ResourcePtr 
        { 
            get; 
            set; 
        }

        //The source data for this Shader
        public byte[] Data
        {
            get;
            set;
        }

        public enum Stage
        {
            Fragment,
            Vertex,
            TessellationControl,
            TessellationEvaluation,
            Geometry,
        }        

        public Stage ShaderStage
        {
            get;
            private set;
        }


        public const string GLSL = "glsl";
        public const string HLSL = "hlsl";
        public const string SPIRV = "spirv";
        public const string CG = "cg";

        public virtual string ShaderFormat
        {
            get;
            set;
        }

        public Shader(Stage Stage)
        {
            this.ShaderStage = Stage;
        }
        

        public bool HasSpirVHeader()
        {
            return Data.Length > 4
                   && Data[0] == 0x03
                   && Data[1] == 0x02
                   && Data[2] == 0x23
                   && Data[3] == 0x07;

        }
    }
}
