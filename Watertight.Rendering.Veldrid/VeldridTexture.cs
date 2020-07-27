using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Watertight.Rendering.Interfaces;
using SixLabors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Advanced;
using Veldrid;

namespace Watertight.Rendering.VeldridRendering
{
    class VeldridTexture : ITexture
    {
        static VeldridRenderer Renderer => IEngine.Instance.Renderer as VeldridRenderer;


        Veldrid.Texture Texture;

        Image<Rgba32> InternalTexture;

        public Vector2 Size
        {
            get;
            set;
        }

        public VeldridTexture(Vector2 Size)
        {
            InternalTexture = new Image<Rgba32>((int)Size.X, (int)Size.Y);
            this.Size = Size;
            PostImageLoad();
        }

        public VeldridTexture(Vector2 Size, byte[] Data)
        {
            InternalTexture = Image.LoadPixelData<Rgba32>(Data, (int)Size.X, (int)Size.Y);
            this.Size = Size;
            PostImageLoad();
        }

        public VeldridTexture(byte[] Raw)
        {
            InternalTexture = Image.Load<Rgba32>(Raw);
            this.Size = new Vector2(InternalTexture.Size().Width, InternalTexture.Size().Height);
            PostImageLoad();
        }

        private void PostImageLoad()
        {
            uint tx = (uint)Size.X;
            uint ty = (uint)Size.Y;

            Texture = Renderer.VeldridFactory.CreateTexture(new Veldrid.TextureDescription(
                tx, ty, 1, 
                1, 1,
                Veldrid.PixelFormat.R8_G8_B8_A8_UNorm, Veldrid.TextureUsage.Sampled, Veldrid.TextureType.Texture2D));

            Veldrid.Texture StagingTexture = Renderer.VeldridFactory.CreateTexture(new Veldrid.TextureDescription(
                tx, ty, 1, 
                1, 1,
                Veldrid.PixelFormat.R8_G8_B8_A8_UNorm, Veldrid.TextureUsage.Staging, Veldrid.TextureType.Texture2D));

            Span<Rgba32> Pixels;
            InternalTexture.TryGetSinglePixelSpan(out Pixels);

            Span<byte> Bytes = MemoryMarshal.AsBytes(Pixels);

         
            //TODO: Fix this copy (Bytes.ToArray()) and use the memory directly.  UpdateTexture takes an intptr so get that
            Renderer.GraphicsDevice.UpdateTexture(StagingTexture, Bytes.ToArray(),
                0, 0, 0,
                tx, ty, 1,
                0, 0);

            using(Veldrid.CommandList cl = Renderer.VeldridFactory.CreateCommandList())
            {
                cl.Begin();
                cl.CopyTexture(StagingTexture, Texture);
                cl.End();
                Renderer.GraphicsDevice.SubmitCommands(cl);
                Renderer.GraphicsDevice.WaitForIdle();
            }

        }

        public void SetData(byte[] PixelData)
        {
            InternalTexture?.Dispose();
            InternalTexture = Image.LoadPixelData<Rgba32>(PixelData, (int)Size.X, (int)Size.Y);
            PostImageLoad();
        }

        public ResourceSet GetTextureResourceSet()
        {
            if (_ResourceSet == null)
            {
                if (_surfaceTextureView == null)
                {
                    _surfaceTextureView = Renderer.VeldridFactory.CreateTextureView(Texture);
                }

                _ResourceSet = Renderer.VeldridFactory.CreateResourceSet(new ResourceSetDescription(
                    ResourceLayout,
                    _surfaceTextureView,
                    Renderer.GraphicsDevice.Aniso4xSampler));
            }

            return _ResourceSet;
        }
        ResourceSet _ResourceSet;
        private TextureView _surfaceTextureView;

        public static ResourceLayoutDescription TextureResourceLayoutDescription
        {
            get => new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment));
        }

        public static ResourceLayout ResourceLayout
        { 
            get
            {
                if(_ResourceLayout == null)
                {
                    _ResourceLayout = Renderer.VeldridFactory.CreateResourceLayout(TextureResourceLayoutDescription);
                }
                return _ResourceLayout;
            }
        }
        static ResourceLayout _ResourceLayout;

    }
}
