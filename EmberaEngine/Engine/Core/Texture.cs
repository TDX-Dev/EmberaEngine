using System;
using System.Collections.Generic;
using EmberaEngine.Engine.Utilities;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp.PixelFormats;

namespace EmberaEngine.Engine.Core
{
    public class Texture : IDisposable
    {

        private bool _disposed = false;

        public Guid Id = Guid.NewGuid();

        private int handle;
        private TextureTarget target;
        private TextureDimension textureDimension;
        private SizedInternalFormat SizedInternalFormat;

        public int Height { get; private set; } = 1;
        public int Width { get; private set; } = 1;
        public int Depth { get; private set; } = 1;

        public TextureWrapMode WrapS = TextureWrapMode.ClampToEdge;
        public TextureWrapMode WrapT = TextureWrapMode.ClampToEdge;
        public TextureWrapMode WrapR = TextureWrapMode.ClampToEdge;


        public static Texture White2DTex;
        public static Texture Black2DTex;

        static Texture()
        {
            White2DTex = new Texture(EmberaEngine.Engine.Core.TextureTarget2d.Texture2D);
            White2DTex.SetFilter(EmberaEngine.Engine.Core.TextureMinFilter.Linear, EmberaEngine.Engine.Core.TextureMagFilter.Nearest);
            White2DTex.SetWrapMode(EmberaEngine.Engine.Core.TextureWrapMode.ClampToEdge, EmberaEngine.Engine.Core.TextureWrapMode.ClampToEdge);
            White2DTex.TexImage2D<byte>(1, 1, EmberaEngine.Engine.Core.PixelInternalFormat.Rgba, EmberaEngine.Engine.Core.PixelFormat.Rgba, EmberaEngine.Engine.Core.PixelType.UnsignedByte, new byte[] { 255, 255, 255, 255 });
            White2DTex.GenerateMipmap();

            Black2DTex = new Texture(TextureTarget2d.Texture2D);
            Black2DTex.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            Black2DTex.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            Black2DTex.TexImage2D(1, 1, PixelInternalFormat.Rgba, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

        }

        public static Texture GetWhite2D()
        {
            return White2DTex;
        }

        public static Texture GetBlack2D()
        {
            return Black2DTex;
        }

        public static Texture ConfigureDefault(TextureTarget target, int width, int height, byte[] pixels)
        {
            Texture Tex = new Texture(EmberaEngine.Engine.Core.TextureTarget2d.Texture2D);
            Tex.SetFilter(EmberaEngine.Engine.Core.TextureMinFilter.Linear, EmberaEngine.Engine.Core.TextureMagFilter.Nearest);
            Tex.SetWrapMode(EmberaEngine.Engine.Core.TextureWrapMode.ClampToEdge, EmberaEngine.Engine.Core.TextureWrapMode.ClampToEdge);
            Tex.TexImage2D<byte>(width, height, EmberaEngine.Engine.Core.PixelInternalFormat.Rgba16f, EmberaEngine.Engine.Core.PixelFormat.Rgba, EmberaEngine.Engine.Core.PixelType.UnsignedByte, pixels);
            Tex.GenerateMipmap();

            return Tex; 
        }

        public static Texture ConfigureDefault(TextureTarget target, int width, int height, nint pixels)
        {
            Texture Tex = new Texture(EmberaEngine.Engine.Core.TextureTarget2d.Texture2D);
            Tex.SetFilter(EmberaEngine.Engine.Core.TextureMinFilter.Linear, EmberaEngine.Engine.Core.TextureMagFilter.Nearest);
            Tex.SetWrapMode(EmberaEngine.Engine.Core.TextureWrapMode.ClampToEdge, EmberaEngine.Engine.Core.TextureWrapMode.ClampToEdge);
            Tex.TexImage2D(width, height, EmberaEngine.Engine.Core.PixelInternalFormat.Rgba16f, EmberaEngine.Engine.Core.PixelFormat.Rgba, EmberaEngine.Engine.Core.PixelType.UnsignedByte, pixels);
            Tex.GenerateMipmap();

            return Tex;
        }

        // Creates a 2D texture by default
        public Texture()
        {
            target = (TextureTarget.Texture2D);
            textureDimension = TextureDimension.Two;

            GL.CreateTextures(target, 1, out handle);
        }

        public Texture(TextureTargetCube textureTarget)
        {
            target = (TextureTarget)textureTarget;
            textureDimension = TextureDimension.Three;
            GL.CreateTextures(target, 1, out handle);
        }


        public Texture(TextureTargetd textureTarget)
        {

            target = (TextureTarget)textureTarget;
            textureDimension = TextureDimension.Undefined;

            GL.CreateTextures(target, 1, out handle);
        }


        public Texture(TextureTarget1d textureTarget)
        {

            target = (TextureTarget)textureTarget;
            textureDimension = TextureDimension.One;

            GL.CreateTextures(target, 1, out  handle);
        }

        public Texture(TextureTarget2d textureTarget)
        {
            target = (TextureTarget)textureTarget;
            textureDimension = TextureDimension.Two;

            GL.CreateTextures(target, 1, out handle);
        }

        public Texture(TextureTarget3d textureTarget)
        {
            target = (TextureTarget)textureTarget;
            textureDimension = TextureDimension.Three;

            GL.CreateTextures(target, 1, out handle);
        }

        public void SetFilter(TextureMinFilter minFilter, TextureMagFilter magFilter)
        {
            GL.TextureParameter(handle, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TextureParameter(handle, TextureParameterName.TextureMagFilter, (int)magFilter);
        }

        public void SetWrapMode(TextureWrapMode wrapS, TextureWrapMode wrapT)
        {
            this.WrapS = wrapS;
            this.WrapT = wrapT;
            GL.TextureParameter(handle, TextureParameterName.TextureWrapS, (int)wrapS);
            GL.TextureParameter(handle, TextureParameterName.TextureWrapT, (int)wrapT);
        }

        public void SetWrapMode(TextureWrapMode wrapS, TextureWrapMode wrapT, TextureWrapMode wrapR)
        {
            this.WrapS = wrapS;
            this.WrapT = wrapT;
            this.WrapR = wrapR;

            GL.TextureParameter(handle, TextureParameterName.TextureWrapS, (int)wrapS);
            GL.TextureParameter(handle, TextureParameterName.TextureWrapT, (int)wrapT);
            GL.TextureParameter(handle, TextureParameterName.TextureWrapR, (int)wrapR);
        }

        public unsafe void SubTexture2D<T>(int width, int height, PixelFormat pixelFormat, PixelType pixelType, T[] pixels, int level = 0, int xOffset = 0, int yOffset = 0) where T : unmanaged
        {
            fixed (T* p = pixels)
            {
                IntPtr ptr = (IntPtr)p;
                SubTexture2D(width, height, pixelFormat, pixelType, ptr, level, xOffset, yOffset);
            }
        }

        public unsafe void SubTexture2DOffset(int width, int height, PixelFormat pixelFormat, PixelType pixelType, nint pixels, int level = 0, int xOffset = 0, int yOffset = 0)
        {
            SubTexture2D(width, height, pixelFormat, pixelType, pixels, level, xOffset, yOffset);
        }

        public void TexStorage2D(int width, int height, SizedInternalFormat pixelInternalFormat)
        {
            GL.BindTexture(target, handle);
            GL.TexStorage2D(OpenTK.Graphics.OpenGL.TextureTarget2d.Texture2D, 0, (OpenTK.Graphics.OpenGL.SizedInternalFormat)pixelInternalFormat, width, height);
            GL.BindTexture(target, 0);

            this.Width = width;
            this.Height = height;
        }

        public void TexImage2D<T>(int width, int height, PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, PixelType pixelType, T[] pixels) where T : unmanaged
        {
            GL.BindTexture(target, handle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, (OpenTK.Graphics.OpenGL.PixelInternalFormat)pixelInternalFormat, width, height, 0, (OpenTK.Graphics.OpenGL.PixelFormat)pixelFormat, (OpenTK.Graphics.OpenGL.PixelType)pixelType, pixels);
            GL.BindTexture(target, 0);

            this.Width = width;
            this.Height = height;
        }

        public void TexImage2D(int width, int height, PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, PixelType pixelType, IntPtr pixels)
        {
            GL.BindTexture(target, handle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, (OpenTK.Graphics.OpenGL.PixelInternalFormat)pixelInternalFormat, width, height, 0, (OpenTK.Graphics.OpenGL.PixelFormat)pixelFormat, (OpenTK.Graphics.OpenGL.PixelType)pixelType, pixels);
            GL.BindTexture(target, 0);

            this.Width = width;
            this.Height = height;
        }

        public void TexImageMultisample2D(int width, int height, int samples, PixelInternalFormat pixelInternalFormat, IntPtr pixels)
        {
            GL.BindTexture(target, handle);
            GL.TexImage2DMultisample((TextureTargetMultisample)target, samples, (OpenTK.Graphics.OpenGL.PixelInternalFormat)pixelInternalFormat, width, height, true);
            GL.BindTexture(target, 0);

            this.Width = width;
            this.Height = height;
        }

        public void TexImage2D(int width, int height, TextureTarget texTarget,  PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, PixelType pixelType, IntPtr pixels)
        {
            GL.BindTexture(target, handle);
            GL.TexImage2D(texTarget, 0, (OpenTK.Graphics.OpenGL.PixelInternalFormat)pixelInternalFormat, width, height, 0, (OpenTK.Graphics.OpenGL.PixelFormat)pixelFormat, (OpenTK.Graphics.OpenGL.PixelType)pixelType, pixels);
            GL.BindTexture(target, 0);

            this.Width = width;
            this.Height = height;
        }

        public void TexImage2D<T>(int width, int height, TextureTarget texTarget, PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, PixelType pixelType, T[] pixels) where T : unmanaged
        {
            GL.BindTexture(target, handle);
            GL.TexImage2D(texTarget, 0, (OpenTK.Graphics.OpenGL.PixelInternalFormat)pixelInternalFormat, width, height, 0, (OpenTK.Graphics.OpenGL.PixelFormat)pixelFormat, (OpenTK.Graphics.OpenGL.PixelType)pixelType, pixels);
            GL.BindTexture(target, 0);

            this.Width = width;
            this.Height = height;
        }

        public void SubTexture2D(int width, int height, PixelFormat pixelFormat, PixelType pixelType, IntPtr pixels, int level = 0, int xOffset = 0, int yOffset = 0)
        {
            GL.TextureSubImage2D(handle, level, xOffset, yOffset, width, height, (OpenTK.Graphics.OpenGL.PixelFormat)pixelFormat, (OpenTK.Graphics.OpenGL.PixelType)pixelType, pixels);
        }

        public void SubTexture2DB(int width, int height, PixelFormat pixelFormat, PixelType pixelType, nint pixels, int level = 0, int xOffset = 0, int yOffset = 0)
        {
            GL.BindTexture(target, handle);
            GL.TexSubImage2D(target, level, xOffset, yOffset, width, height, (OpenTK.Graphics.OpenGL.PixelFormat)pixelFormat, (OpenTK.Graphics.OpenGL.PixelType)pixelType, pixels);
            GL.BindTexture(target, 0);
        }

        public unsafe void SubTexture2DB<T>(int width, int height, PixelFormat pixelFormat, PixelType pixelType, T[] pixels, int level = 0, int xOffset = 0, int yOffset = 0) where T : unmanaged
        {
            GL.BindTexture(target, handle);
            GL.TexSubImage2D(target, level, xOffset, yOffset, width, height, (OpenTK.Graphics.OpenGL.PixelFormat)pixelFormat, (OpenTK.Graphics.OpenGL.PixelType)pixelType, pixels);
            GL.BindTexture(target, 0);
        }

        public void TexImage3D(int width, int height, int depth, PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, PixelType pixelType, IntPtr pixels)
        {
            GL.BindTexture(target, handle);
            GL.TexImage3D(target, 0, (OpenTK.Graphics.OpenGL.PixelInternalFormat)pixelInternalFormat, width, height, depth, 0, (OpenTK.Graphics.OpenGL.PixelFormat)pixelFormat, (OpenTK.Graphics.OpenGL.PixelType)pixelType, pixels);
            GL.BindTexture(target, 0);

            this.Width = width;
            this.Height = height;
            this.Depth = depth;
        }

        public void SetAnisotropy(float value)
        {
            GL.TextureParameter(handle, TextureParameterName.TextureMaxAnisotropy, value);
        }

        public void GenerateMipmap()
        {
            GL.GenerateTextureMipmap(handle);
        }

        ~Texture()
        {
            //GraphicsObjectCollector.AddTexToDispose(this.handle);
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (handle != 0)
            {
                int handleToDelete = handle;
                MainThreadDispatcher.Queue(() =>
                {
                    GL.DeleteTexture(handleToDelete);
                });
                handle = 0;
            }

            _disposed = true;
        }

        public void SetActiveUnit(TextureUnit unit)
        {
            GL.ActiveTexture((OpenTK.Graphics.OpenGL.TextureUnit)unit);
        }

        public void Bind()
        {
            GL.BindTexture(target, handle);
        }

        public void Bind(TextureTarget target)
        {
            GL.BindTexture(target, handle);
        }

        public void BindImageTexture(int unit, TextureAccess access, SizedInternalFormat format)
        {
            GL.BindImageTexture(unit, handle, 0, true, 0, access, (OpenTK.Graphics.OpenGL.SizedInternalFormat)format);
        }

        public int GetRendererID()
        {
            return handle;
        }

        public int GetTextureTargetID()
        {
            return (int)target;
        }
    }

    public enum TextureTargetCube
    {
        TextureCubeMap = TextureTarget.TextureCubeMap
    }

    public enum SizedInternalFormat
    {
        //
        // Summary:
        //     [requires: v1.1] Original was GL_R3_G3_B2 = 0x2A10
        R3G3B2 = 10768,
        //
        // Summary:
        //     [requires: v1.1] Original was GL_RGB4 = 0x804F
        Rgb4 = 32847,
        //
        // Summary:
        //     [requires: v1.1] Original was GL_RGB5 = 0x8050
        Rgb5 = 32848,
        //
        // Summary:
        //     [requires: v1.1] Original was GL_RGB8 = 0x8051
        Rgb8 = 32849,
        //
        // Summary:
        //     [requires: v1.1] Original was GL_RGB10 = 0x8052
        Rgb10 = 32850,
        //
        // Summary:
        //     [requires: v1.1] Original was GL_RGB12 = 0x8053
        Rgb12 = 32851,
        //
        // Summary:
        //     [requires: v1.1] Original was GL_RGBA2 = 0x8055
        Rgba2 = 32853,
        //
        // Summary:
        //     [requires: v1.1] Original was GL_RGBA4 = 0x8056
        Rgba4 = 32854,
        //
        // Summary:
        //     [requires: v1.1] Original was GL_RGB5_A1 = 0x8057
        Rgb5A1 = 32855,
        //
        // Summary:
        //     [requires: v1.1] Original was GL_RGBA8 = 0x8058
        Rgba8 = 32856,
        //
        // Summary:
        //     [requires: v1.1] Original was GL_RGB10_A2 = 0x8059
        Rgb10A2 = 32857,
        //
        // Summary:
        //     [requires: v1.1] Original was GL_RGBA12 = 0x805A
        Rgba12 = 32858,
        //
        // Summary:
        //     [requires: v1.1] Original was GL_RGBA16 = 0x805B
        Rgba16 = 32859,
        //
        // Summary:
        //     [requires: v3.0 or ARB_texture_rg] Original was GL_R8 = 0x8229
        R8 = 33321,
        //
        // Summary:
        //     [requires: v3.0 or ARB_texture_rg] Original was GL_R16 = 0x822A
        R16 = 33322,
        //
        // Summary:
        //     [requires: v3.0 or ARB_texture_rg] Original was GL_RG8 = 0x822B
        Rg8 = 33323,
        //
        // Summary:
        //     [requires: v3.0 or ARB_texture_rg] Original was GL_RG16 = 0x822C
        Rg16 = 33324,
        //
        // Summary:
        //     [requires: v3.0 or ARB_texture_rg] Original was GL_R16F = 0x822D
        R16f = 33325,
        //
        // Summary:
        //     [requires: v3.0 or ARB_texture_rg] Original was GL_R32F = 0x822E
        R32f = 33326,
        //
        // Summary:
        //     [requires: v3.0 or ARB_texture_rg] Original was GL_RG16F = 0x822F
        Rg16f = 33327,
        //
        // Summary:
        //     [requires: v3.0 or ARB_texture_rg] Original was GL_RG32F = 0x8230
        Rg32f = 33328,
        //
        // Summary:
        //     [requires: v3.0 or ARB_texture_rg] Original was GL_R8I = 0x8231
        R8i = 33329,
        //
        // Summary:
        //     [requires: v3.0 or ARB_texture_rg] Original was GL_R8UI = 0x8232
        R8ui = 33330,
        //
        // Summary:
        //     [requires: v3.0 or ARB_texture_rg] Original was GL_R16I = 0x8233
        R16i = 33331,
        //
        // Summary:
        //     [requires: v3.0 or ARB_texture_rg] Original was GL_R16UI = 0x8234
        R16ui = 33332,
        //
        // Summary:
        //     [requires: v3.0 or ARB_texture_rg] Original was GL_R32I = 0x8235
        R32i = 33333,
        //
        // Summary:
        //     [requires: v3.0 or ARB_texture_rg] Original was GL_R32UI = 0x8236
        R32ui = 33334,
        //
        // Summary:
        //     [requires: v3.0 or ARB_texture_rg] Original was GL_RG8I = 0x8237
        Rg8i = 33335,
        //
        // Summary:
        //     [requires: v3.0 or AMD_interleaved_elements, ARB_texture_rg] Original was GL_RG8UI
        //     = 0x8238
        Rg8ui = 33336,
        //
        // Summary:
        //     [requires: v3.0 or ARB_texture_rg] Original was GL_RG16I = 0x8239
        Rg16i = 33337,
        //
        // Summary:
        //     [requires: v3.0 or AMD_interleaved_elements, ARB_texture_rg] Original was GL_RG16UI
        //     = 0x823A
        Rg16ui = 33338,
        //
        // Summary:
        //     [requires: v3.0 or ARB_texture_rg] Original was GL_RG32I = 0x823B
        Rg32i = 33339,
        //
        // Summary:
        //     [requires: v3.0 or ARB_texture_rg] Original was GL_RG32UI = 0x823C
        Rg32ui = 33340,
        //
        // Summary:
        //     [requires: v3.0] Original was GL_RGBA32F = 0x8814
        Rgba32f = 34836,
        //
        // Summary:
        //     [requires: v3.0 or ARB_texture_buffer_object_rgb32] Original was GL_RGB32F =
        //     0x8815
        Rgb32f = 34837,
        //
        // Summary:
        //     [requires: v3.0] Original was GL_RGBA16F = 0x881A
        Rgba16f = 34842,
        //
        // Summary:
        //     [requires: v3.0] Original was GL_RGB16F = 0x881B
        Rgb16f = 34843,
        //
        // Summary:
        //     [requires: v3.0] Original was GL_R11F_G11F_B10F = 0x8C3A
        R11fG11fB10f = 35898,
        //
        // Summary:
        //     [requires: v3.0] Original was GL_RGB9_E5 = 0x8C3D
        Rgb9E5 = 35901,
        //
        // Summary:
        //     [requires: v2.1] Original was GL_SRGB8 = 0x8C41
        Srgb8 = 35905,
        //
        // Summary:
        //     [requires: v2.1] Original was GL_SRGB8_ALPHA8 = 0x8C43
        Srgb8Alpha8 = 35907,
        //
        // Summary:
        //     [requires: v3.0] Original was GL_RGBA32UI = 0x8D70
        Rgba32ui = 36208,
        //
        // Summary:
        //     [requires: v3.0 or ARB_texture_buffer_object_rgb32] Original was GL_RGB32UI =
        //     0x8D71
        Rgb32ui = 36209,
        //
        // Summary:
        //     [requires: v3.0] Original was GL_RGBA16UI = 0x8D76
        Rgba16ui = 36214,
        //
        // Summary:
        //     [requires: v3.0] Original was GL_RGB16UI = 0x8D77
        Rgb16ui = 36215,
        //
        // Summary:
        //     [requires: v3.0 or AMD_interleaved_elements] Original was GL_RGBA8UI = 0x8D7C
        Rgba8ui = 36220,
        //
        // Summary:
        //     [requires: v3.0] Original was GL_RGB8UI = 0x8D7D
        Rgb8ui = 36221,
        //
        // Summary:
        //     [requires: v3.0] Original was GL_RGBA32I = 0x8D82
        Rgba32i = 36226,
        //
        // Summary:
        //     [requires: v3.0 or ARB_texture_buffer_object_rgb32, ARB_vertex_attrib_64bit]
        //     Original was GL_RGB32I = 0x8D83
        Rgb32i = 36227,
        //
        // Summary:
        //     [requires: v3.0] Original was GL_RGBA16I = 0x8D88
        Rgba16i = 36232,
        //
        // Summary:
        //     [requires: v3.0] Original was GL_RGB16I = 0x8D89
        Rgb16i = 36233,
        //
        // Summary:
        //     [requires: v3.0] Original was GL_RGBA8I = 0x8D8E
        Rgba8i = 36238,
        //
        // Summary:
        //     [requires: v3.0] Original was GL_RGB8I = 0x8D8F
        Rgb8i = 36239,
        //
        // Summary:
        //     [requires: v3.1 or EXT_texture_snorm] Original was GL_R8_SNORM = 0x8F94
        R8Snorm = 36756,
        //
        // Summary:
        //     [requires: v3.1 or EXT_texture_snorm] Original was GL_RG8_SNORM = 0x8F95
        Rg8Snorm = 36757,
        //
        // Summary:
        //     [requires: v3.1 or EXT_texture_snorm] Original was GL_RGB8_SNORM = 0x8F96
        Rgb8Snorm = 36758,
        //
        // Summary:
        //     [requires: v3.1 or EXT_texture_snorm] Original was GL_RGBA8_SNORM = 0x8F97
        Rgba8Snorm = 36759,
        //
        // Summary:
        //     [requires: v3.1 or EXT_texture_snorm] Original was GL_R16_SNORM = 0x8F98
        R16Snorm = 36760,
        //
        // Summary:
        //     [requires: v3.1 or EXT_texture_snorm] Original was GL_RG16_SNORM = 0x8F99
        Rg16Snorm = 36761,
        //
        // Summary:
        //     [requires: v3.1 or EXT_texture_snorm] Original was GL_RGB16_SNORM = 0x8F9A
        Rgb16Snorm = 36762,
        //
        // Summary:
        //     [requires: v3.3 or ARB_texture_rgb10_a2ui] Original was GL_RGB10_A2UI = 0x906F
        Rgb10A2ui = 36975
    }

    public enum TextureUnit
    {
        Texture0 = 33984,
        Texture1,
        Texture2,
        Texture3,
        Texture4,
        Texture5,
        Texture6,
        Texture7,
        Texture8,
        Texture9,
        Texture10,
        Texture11,
        Texture12,
        Texture13,
        Texture14,
        Texture15,
        Texture16,
        Texture17,
        Texture18,
        Texture19,
        Texture20,
        Texture21,
        Texture22,
        Texture23,
        Texture24,
        Texture25,
        Texture26,
        Texture27,
        Texture28,
        Texture29,
        Texture30,
        Texture31
    }

    public enum PixelInternalFormat
    {

        DepthComponent = 6402,

        Alpha = 6406,

        Rgb = 6407,

        Rgba = 6408,

        Luminance = 6409,

        LuminanceAlpha = 6410,

        R3G3B2 = 10768,

        Alpha4 = 32827,

        Alpha8 = 32828,

        Alpha12 = 32829,

        Alpha16 = 32830,

        Luminance4 = 32831,

        Luminance8 = 32832,

        Luminance12 = 32833,

        Luminance16 = 32834,

        Luminance4Alpha4 = 32835,

        Luminance6Alpha2 = 32836,

        Luminance8Alpha8 = 32837,

        Luminance12Alpha4 = 32838,

        Luminance12Alpha12 = 32839,

        Luminance16Alpha16 = 32840,

        Intensity = 32841,

        Intensity4 = 32842,

        Intensity8 = 32843,

        Intensity12 = 32844,

        Intensity16 = 32845,

        Rgb2Ext = 32846,

        Rgb4 = 32847,

        Rgb5 = 32848,

        Rgb8 = 32849,

        Rgb10 = 32850,

        Rgb12 = 32851,

        Rgb16 = 32852,

        Rgba2 = 32853,

        Rgba4 = 32854,

        Rgb5A1 = 32855,

        Rgba8 = 32856,

        Rgb10A2 = 32857,

        Rgba12 = 32858,

        Rgba16 = 32859,

        DualAlpha4Sgis = 33040,

        DualAlpha8Sgis = 33041,

        DualAlpha12Sgis = 33042,

        DualAlpha16Sgis = 33043,

        DualLuminance4Sgis = 33044,

        DualLuminance8Sgis = 33045,

        DualLuminance12Sgis = 33046,

        DualLuminance16Sgis = 33047,

        DualIntensity4Sgis = 33048,

        DualIntensity8Sgis = 33049,

        DualIntensity12Sgis = 33050,

        DualIntensity16Sgis = 33051,

        DualLuminanceAlpha4Sgis = 33052,

        DualLuminanceAlpha8Sgis = 33053,

        QuadAlpha4Sgis = 33054,

        QuadAlpha8Sgis = 33055,

        QuadLuminance4Sgis = 33056,

        QuadLuminance8Sgis = 33057,

        QuadIntensity4Sgis = 33058,

        QuadIntensity8Sgis = 33059,

        DepthComponent16 = 33189,

        DepthComponent16Sgix = 33189,

        DepthComponent24 = 33190,

        DepthComponent24Sgix = 33190,

        DepthComponent32 = 33191,

        DepthComponent32Sgix = 33191,

        CompressedRed = 33317,

        CompressedRg = 33318,

        R8 = 33321,

        R16 = 33322,

        Rg8 = 33323,

        Rg16 = 33324,

        R16f = 33325,

        R32f = 33326,

        Rg16f = 33327,

        Rg32f = 33328,

        R8i = 33329,

        R8ui = 33330,

        R16i = 33331,

        R16ui = 33332,

        R32i = 33333,

        R32ui = 33334,

        Rg8i = 33335,

        Rg8ui = 33336,

        Rg16i = 33337,

        Rg16ui = 33338,

        Rg32i = 33339,

        Rg32ui = 33340,

        CompressedRgbS3tcDxt1Ext = 33776,

        CompressedRgbaS3tcDxt1Ext = 33777,

        CompressedRgbaS3tcDxt3Ext = 33778,

        CompressedRgbaS3tcDxt5Ext = 33779,

        RgbIccSgix = 33888,

        RgbaIccSgix = 33889,

        AlphaIccSgix = 33890,

        LuminanceIccSgix = 33891,

        IntensityIccSgix = 33892,

        LuminanceAlphaIccSgix = 33893,

        R5G6B5IccSgix = 33894,

        R5G6B5A8IccSgix = 33895,

        Alpha16IccSgix = 33896,

        Luminance16IccSgix = 33897,

        Intensity16IccSgix = 33898,

        Luminance16Alpha8IccSgix = 33899,

        CompressedAlpha = 34025,

        CompressedLuminance = 34026,

        CompressedLuminanceAlpha = 34027,

        CompressedIntensity = 34028,

        CompressedRgb = 34029,

        CompressedRgba = 34030,

        DepthStencil = 34041,

        Rgba32f = 34836,

        Rgb32f = 34837,

        Rgba16f = 34842,

        Rgb16f = 34843,

        Depth24Stencil8 = 35056,

        R11fG11fB10f = 35898,

        Rgb9E5 = 35901,

        Srgb = 35904,

        Srgb8 = 35905,

        SrgbAlpha = 35906,

        Srgb8Alpha8 = 35907,

        SluminanceAlpha = 35908,

        Sluminance8Alpha8 = 35909,

        Sluminance = 35910,

        Sluminance8 = 35911,

        CompressedSrgb = 35912,

        CompressedSrgbAlpha = 35913,

        CompressedSluminance = 35914,

        CompressedSluminanceAlpha = 35915,

        CompressedSrgbS3tcDxt1Ext = 35916,

        CompressedSrgbAlphaS3tcDxt1Ext = 35917,

        CompressedSrgbAlphaS3tcDxt3Ext = 35918,

        CompressedSrgbAlphaS3tcDxt5Ext = 35919,

        DepthComponent32f = 36012,

        Depth32fStencil8 = 36013,

        Rgba32ui = 36208,

        Rgb32ui = 36209,

        Rgba16ui = 36214,

        Rgb16ui = 36215,

        Rgba8ui = 36220,

        Rgb8ui = 36221,

        Rgba32i = 36226,

        Rgb32i = 36227,

        Rgba16i = 36232,

        Rgb16i = 36233,

        Rgba8i = 36238,

        Rgb8i = 36239,

        Float32UnsignedInt248Rev = 36269,

        CompressedRedRgtc1 = 36283,

        CompressedSignedRedRgtc1 = 36284,

        CompressedRgRgtc2 = 36285,

        CompressedSignedRgRgtc2 = 36286,

        CompressedRgbaBptcUnorm = 36492,

        CompressedSrgbAlphaBptcUnorm = 36493,

        CompressedRgbBptcSignedFloat = 36494,

        CompressedRgbBptcUnsignedFloat = 36495,

        R8Snorm = 36756,

        Rg8Snorm = 36757,

        Rgb8Snorm = 36758,

        Rgba8Snorm = 36759,

        R16Snorm = 36760,

        Rg16Snorm = 36761,

        Rgb16Snorm = 36762,

        Rgba16Snorm = 36763,

        Rgb10A2ui = 36975,

        One = 1,

        Two = 2,

        Three = 3,

        Four = 4
    }

    public enum PixelType
    {
        Byte = 5120,
        UnsignedByte = 5121,
        Short = 5122,
        UnsignedShort = 5123,
        Int = 5124,
        UnsignedInt = 5125,
        Float = 5126,
        HalfFloat = 5131,
        Bitmap = 6656,
        UnsignedByte332 = 32818,
        UnsignedByte332Ext = 32818,
        UnsignedShort4444 = 32819,
        UnsignedShort4444Ext = 32819,
        UnsignedShort5551 = 32820,
        UnsignedShort5551Ext = 32820,
        UnsignedInt8888 = 32821,
        UnsignedInt8888Ext = 32821,
        UnsignedInt1010102 = 32822,
        UnsignedInt1010102Ext = 32822,
        UnsignedByte233Reversed = 33634,
        UnsignedShort565 = 33635,
        UnsignedShort565Reversed = 33636,
        UnsignedShort4444Reversed = 33637,
        UnsignedShort1555Reversed = 33638,
        UnsignedInt8888Reversed = 33639,
        UnsignedInt2101010Reversed = 33640,
        UnsignedInt248 = 34042,
        UnsignedInt10F11F11FRev = 35899,
        UnsignedInt5999Rev = 35902,
        Float32UnsignedInt248Rev = 36269
    }

    public enum PixelFormat
    {
        UnsignedShort = 5123,
        UnsignedInt = 5125,
        ColorIndex = 6400,
        StencilIndex = 6401,
        DepthComponent = 6402,
        Red = 6403,
        RedExt = 6403,
        Green = 6404,
        Blue = 6405,
        Alpha = 6406,
        Rgb = 6407,
        Rgba = 6408,
        Luminance = 6409,
        LuminanceAlpha = 6410,
        AbgrExt = 0x8000,
        CmykExt = 32780,
        CmykaExt = 32781,
        Bgr = 32992,
        Bgra = 32993,
        Ycrcb422Sgix = 33211,
        Ycrcb444Sgix = 33212,
        Rg = 33319,
        RgInteger = 33320,
        R5G6B5IccSgix = 33894,
        R5G6B5A8IccSgix = 33895,
        Alpha16IccSgix = 33896,
        Luminance16IccSgix = 33897,
        Luminance16Alpha8IccSgix = 33899,
        DepthStencil = 34041,
        RedInteger = 36244,
        GreenInteger = 36245,
        BlueInteger = 36246,
        AlphaInteger = 36247,
        RgbInteger = 36248,
        RgbaInteger = 36249,
        BgrInteger = 36250,
        BgraInteger = 36251
    }

    public enum TextureWrapMode
    {
        Clamp = 10496,
        Repeat = 10497,
        ClampToBorder = 33069,
        ClampToBorderArb = 33069,
        ClampToBorderNv = 33069,
        ClampToBorderSgis = 33069,
        ClampToEdge = 33071,
        ClampToEdgeSgis = 33071,
        MirroredRepeat = 33648
    }

    public enum TextureMagFilter
    {
        Nearest = 9728,
        Linear = 9729,
        LinearDetailSgis = 32919,
        LinearDetailAlphaSgis = 32920,
        LinearDetailColorSgis = 32921,
        LinearSharpenSgis = 32941,
        LinearSharpenAlphaSgis = 32942,
        LinearSharpenColorSgis = 32943,
        Filter4Sgis = 33094,
        PixelTexGenQCeilingSgix = 33156,
        PixelTexGenQRoundSgix = 33157,

        PixelTexGenQFloorSgix = 33158
    }

    public enum TextureMinFilter
    {
        Nearest = 9728,
        Linear = 9729,
        NearestMipmapNearest = 9984,
        LinearMipmapNearest = 9985,
        NearestMipmapLinear = 9986,
        LinearMipmapLinear = 9987,
        Filter4Sgis = 33094,
        LinearClipmapLinearSgix = 33136,
        PixelTexGenQCeilingSgix = 33156,
        PixelTexGenQRoundSgix = 33157,
        PixelTexGenQFloorSgix = 33158,
        NearestClipmapNearestSgix = 33869,
        NearestClipmapLinearSgix = 33870,
        LinearClipmapNearestSgix = 33871
    }

    public enum TextureDimension : int
    {
        Undefined,
        One,
        Two,
        Three,
    }

    public enum TextureTarget3d
    {
        Texture3D = 32879,
        ProxyTexture3D = 32880,
        TextureCubeMap = 34067,
        ProxyTextureCubeMap = 34075,
        Texture2DArray = 35866,
        ProxyTexture2DArray = 35867,
        TextureCubeMapArray = 36873,
        ProxyTextureCubeMapArray = 36875
    }

    public enum TextureTarget2d
    {
        Texture2D = 3553,
        ProxyTexture2D = 32868,
        TextureRectangle = 34037,
        ProxyTextureRectangle = 34039,
        TextureCubeMap = 34067,
        ProxyTextureCubeMap = 34075,
        Texture1DArray = 35864,
        ProxyTexture1DArray = 35865
    }

    public enum TextureTarget1d
    {
        Texture1D = 3552,
        ProxyTexture1D = 32867
    }

    public enum TextureTargetd
    {
        //
        // Summary:
        //     [requires: v1.0 or ARB_internalformat_query2] Original was GL_TEXTURE_1D = 0x0DE0
        Texture1D = 3552,
        //
        // Summary:
        //     [requires: v1.0 or ARB_internalformat_query2] Original was GL_TEXTURE_2D = 0x0DE1
        Texture2D = 3553,
        //
        // Summary:
        //     [requires: v1.1] Original was GL_PROXY_TEXTURE_1D = 0x8063
        ProxyTexture1D = 32867,
        //
        // Summary:
        //     [requires: EXT_texture] Original was GL_PROXY_TEXTURE_1D_EXT = 0x8063
        ProxyTexture1DExt = 32867,
        //
        // Summary:
        //     [requires: v1.1] Original was GL_PROXY_TEXTURE_2D = 0x8064
        ProxyTexture2D = 32868,
        //
        // Summary:
        //     [requires: EXT_texture] Original was GL_PROXY_TEXTURE_2D_EXT = 0x8064
        ProxyTexture2DExt = 32868,
        //
        // Summary:
        //     [requires: v1.2 or ARB_internalformat_query2] Original was GL_TEXTURE_3D = 0x806F
        Texture3D = 32879,
        //
        // Summary:
        //     [requires: EXT_texture3D] Original was GL_TEXTURE_3D_EXT = 0x806F
        Texture3DExt = 32879,
        //
        // Summary:
        //     Original was GL_TEXTURE_3D_OES = 0x806F
        Texture3DOes = 32879,
        //
        // Summary:
        //     [requires: v1.2] Original was GL_PROXY_TEXTURE_3D = 0x8070
        ProxyTexture3D = 32880,
        //
        // Summary:
        //     [requires: EXT_texture3D] Original was GL_PROXY_TEXTURE_3D_EXT = 0x8070
        ProxyTexture3DExt = 32880,
        //
        // Summary:
        //     [requires: SGIS_detail_texture] Original was GL_DETAIL_TEXTURE_2D_SGIS = 0x8095
        DetailTexture2DSgis = 32917,
        //
        // Summary:
        //     [requires: SGIS_texture4D] Original was GL_TEXTURE_4D_SGIS = 0x8134
        Texture4DSgis = 33076,
        //
        // Summary:
        //     [requires: SGIS_texture4D] Original was GL_PROXY_TEXTURE_4D_SGIS = 0x8135
        ProxyTexture4DSgis = 33077,
        //
        // Summary:
        //     [requires: v3.1 or ARB_internalformat_query2] Original was GL_TEXTURE_RECTANGLE
        //     = 0x84F5
        TextureRectangle = 34037,
        //
        // Summary:
        //     [requires: ARB_texture_rectangle] Original was GL_TEXTURE_RECTANGLE_ARB = 0x84F5
        TextureRectangleArb = 34037,
        //
        // Summary:
        //     [requires: NV_texture_rectangle] Original was GL_TEXTURE_RECTANGLE_NV = 0x84F5
        TextureRectangleNv = 34037,
        //
        // Summary:
        //     [requires: v3.1] Original was GL_PROXY_TEXTURE_RECTANGLE = 0x84F7
        ProxyTextureRectangle = 34039,
        //
        // Summary:
        //     [requires: ARB_texture_rectangle] Original was GL_PROXY_TEXTURE_RECTANGLE_ARB
        //     = 0x84F7
        ProxyTextureRectangleArb = 34039,
        //
        // Summary:
        //     [requires: NV_texture_rectangle] Original was GL_PROXY_TEXTURE_RECTANGLE_NV =
        //     0x84F7
        ProxyTextureRectangleNv = 34039,
        //
        // Summary:
        //     [requires: v1.3 or ARB_internalformat_query2] Original was GL_TEXTURE_CUBE_MAP
        //     = 0x8513
        TextureCubeMap = 34067,
        //
        // Summary:
        //     [requires: v1.3 or ARB_direct_state_access] Original was GL_TEXTURE_BINDING_CUBE_MAP
        //     = 0x8514
        TextureBindingCubeMap = 34068,
        //
        // Summary:
        //     [requires: v1.3] Original was GL_TEXTURE_CUBE_MAP_POSITIVE_X = 0x8515
        TextureCubeMapPositiveX = 34069,
        //
        // Summary:
        //     [requires: v1.3] Original was GL_TEXTURE_CUBE_MAP_NEGATIVE_X = 0x8516
        TextureCubeMapNegativeX = 34070,
        //
        // Summary:
        //     [requires: v1.3] Original was GL_TEXTURE_CUBE_MAP_POSITIVE_Y = 0x8517
        TextureCubeMapPositiveY = 34071,
        //
        // Summary:
        //     [requires: v1.3] Original was GL_TEXTURE_CUBE_MAP_NEGATIVE_Y = 0x8518
        TextureCubeMapNegativeY = 34072,
        //
        // Summary:
        //     [requires: v1.3] Original was GL_TEXTURE_CUBE_MAP_POSITIVE_Z = 0x8519
        TextureCubeMapPositiveZ = 34073,
        //
        // Summary:
        //     [requires: v1.3] Original was GL_TEXTURE_CUBE_MAP_NEGATIVE_Z = 0x851A
        TextureCubeMapNegativeZ = 34074,
        //
        // Summary:
        //     [requires: v1.3] Original was GL_PROXY_TEXTURE_CUBE_MAP = 0x851B
        ProxyTextureCubeMap = 34075,
        //
        // Summary:
        //     [requires: ARB_texture_cube_map] Original was GL_PROXY_TEXTURE_CUBE_MAP_ARB =
        //     0x851B
        ProxyTextureCubeMapArb = 34075,
        //
        // Summary:
        //     [requires: EXT_texture_cube_map] Original was GL_PROXY_TEXTURE_CUBE_MAP_EXT =
        //     0x851B
        ProxyTextureCubeMapExt = 34075,
        //
        // Summary:
        //     [requires: v3.0 or ARB_internalformat_query2] Original was GL_TEXTURE_1D_ARRAY
        //     = 0x8C18
        Texture1DArray = 35864,
        //
        // Summary:
        //     [requires: v3.0] Original was GL_PROXY_TEXTURE_1D_ARRAY = 0x8C19
        ProxyTexture1DArray = 35865,
        //
        // Summary:
        //     [requires: EXT_texture_array] Original was GL_PROXY_TEXTURE_1D_ARRAY_EXT = 0x8C19
        ProxyTexture1DArrayExt = 35865,
        //
        // Summary:
        //     [requires: v3.0 or ARB_internalformat_query2] Original was GL_TEXTURE_2D_ARRAY
        //     = 0x8C1A
        Texture2DArray = 35866,
        //
        // Summary:
        //     [requires: v3.0] Original was GL_PROXY_TEXTURE_2D_ARRAY = 0x8C1B
        ProxyTexture2DArray = 35867,
        //
        // Summary:
        //     [requires: EXT_texture_array] Original was GL_PROXY_TEXTURE_2D_ARRAY_EXT = 0x8C1B
        ProxyTexture2DArrayExt = 35867,
        //
        // Summary:
        //     [requires: v3.1 or ARB_internalformat_query2] Original was GL_TEXTURE_BUFFER
        //     = 0x8C2A
        TextureBuffer = 35882,
        //
        // Summary:
        //     [requires: v4.0 or ARB_internalformat_query2] Original was GL_TEXTURE_CUBE_MAP_ARRAY
        //     = 0x9009
        TextureCubeMapArray = 36873,
        //
        // Summary:
        //     [requires: ARB_texture_cube_map_array] Original was GL_TEXTURE_CUBE_MAP_ARRAY_ARB
        //     = 0x9009
        TextureCubeMapArrayArb = 36873,
        //
        // Summary:
        //     Original was GL_TEXTURE_CUBE_MAP_ARRAY_EXT = 0x9009
        TextureCubeMapArrayExt = 36873,
        //
        // Summary:
        //     Original was GL_TEXTURE_CUBE_MAP_ARRAY_OES = 0x9009
        TextureCubeMapArrayOes = 36873,
        //
        // Summary:
        //     [requires: v4.0] Original was GL_PROXY_TEXTURE_CUBE_MAP_ARRAY = 0x900B
        ProxyTextureCubeMapArray = 36875,
        //
        // Summary:
        //     [requires: ARB_texture_cube_map_array] Original was GL_PROXY_TEXTURE_CUBE_MAP_ARRAY_ARB
        //     = 0x900B
        ProxyTextureCubeMapArrayArb = 36875,
        //
        // Summary:
        //     [requires: v3.2 or ARB_internalformat_query2, ARB_texture_multisample, NV_internalformat_sample_query]
        //     Original was GL_TEXTURE_2D_MULTISAMPLE = 0x9100
        Texture2DMultisample = 37120,
        //
        // Summary:
        //     [requires: v3.2 or ARB_texture_multisample] Original was GL_PROXY_TEXTURE_2D_MULTISAMPLE
        //     = 0x9101
        ProxyTexture2DMultisample = 37121,
        //
        // Summary:
        //     [requires: v3.2 or ARB_internalformat_query2, ARB_texture_multisample, NV_internalformat_sample_query]
        //     Original was GL_TEXTURE_2D_MULTISAMPLE_ARRAY = 0x9102
        Texture2DMultisampleArray = 37122,
        //
        // Summary:
        //     [requires: v3.2 or ARB_texture_multisample] Original was GL_PROXY_TEXTURE_2D_MULTISAMPLE_ARRAY
        //     = 0x9103
        ProxyTexture2DMultisampleArray = 37123
    }

}
