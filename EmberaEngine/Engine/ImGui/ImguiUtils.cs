using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Imgui
{
    class ImguiUtils
    {
        public static void CreateTexture(TextureTarget target, string Name, out int Texture)
        {
            Texture = GL.GenTexture();
            GL.BindTexture(target, Texture);
            GL.BindTexture(target, 0);
            //LabelObject(ObjectLabelIdentifier.Texture, Texture, $"Texture: {Name}");
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateProgram(string Name, out int Program)
        {
            Program = GL.CreateProgram();
            //LabelObject(ObjectLabelIdentifier.Program, Program, $"Program: {Name}");
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateShader(ShaderType type, string Name, out int Shader)
        {
            Shader = GL.CreateShader(type);
            //LabelObject(ObjectLabelIdentifier.Shader, Shader, $"Shader: {type}: {Name}");
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateBuffer(string Name, out int Buffer)
        {
            Buffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            //LabelObject(ObjectLabelIdentifier.Buffer, Buffer, $"Buffer: {Name}");
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateVertexBuffer(string Name, out int Buffer) => CreateBuffer($"VBO: {Name}", out Buffer);

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateElementBuffer(string Name, out int Buffer) => CreateBuffer($"EBO: {Name}", out Buffer);

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateVertexArray(string Name, out int VAO)
        {
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);
            GL.BindVertexArray(0);
            //LabelObject(ObjectLabelIdentifier.VertexArray, VAO, $"VAO: {Name}");
        }

        public static ImGuiKey ConvertToImGuiKey(OpenTK.Windowing.GraphicsLibraryFramework.Keys key)
        {
            return key switch
            {
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Tab => ImGuiKey.Tab,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Left => ImGuiKey.LeftArrow,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Right => ImGuiKey.RightArrow,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Up => ImGuiKey.UpArrow,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Down => ImGuiKey.DownArrow,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.PageUp => ImGuiKey.PageUp,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.PageDown => ImGuiKey.PageDown,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Home => ImGuiKey.Home,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.End => ImGuiKey.End,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Insert => ImGuiKey.Insert,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Delete => ImGuiKey.Delete,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Backspace => ImGuiKey.Backspace,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Space => ImGuiKey.Space,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Enter => ImGuiKey.Enter,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape => ImGuiKey.Escape,

                // Number keys
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.D0 => ImGuiKey._0,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.D1 => ImGuiKey._1,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.D2 => ImGuiKey._2,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.D3 => ImGuiKey._3,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.D4 => ImGuiKey._4,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.D5 => ImGuiKey._5,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.D6 => ImGuiKey._6,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.D7 => ImGuiKey._7,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.D8 => ImGuiKey._8,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.D9 => ImGuiKey._9,

                // Letters
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.A => ImGuiKey.A,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.B => ImGuiKey.B,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.C => ImGuiKey.C,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.D => ImGuiKey.D,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.E => ImGuiKey.E,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.F => ImGuiKey.F,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.G => ImGuiKey.G,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.H => ImGuiKey.H,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.I => ImGuiKey.I,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.J => ImGuiKey.J,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.K => ImGuiKey.K,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.L => ImGuiKey.L,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.M => ImGuiKey.M,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.N => ImGuiKey.N,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.O => ImGuiKey.O,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.P => ImGuiKey.P,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Q => ImGuiKey.Q,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.R => ImGuiKey.R,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.S => ImGuiKey.S,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.T => ImGuiKey.T,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.U => ImGuiKey.U,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.V => ImGuiKey.V,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.W => ImGuiKey.W,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.X => ImGuiKey.X,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Y => ImGuiKey.Y,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Z => ImGuiKey.Z,

                // Function keys
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.F1 => ImGuiKey.F1,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.F2 => ImGuiKey.F2,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.F3 => ImGuiKey.F3,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.F4 => ImGuiKey.F4,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.F5 => ImGuiKey.F5,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.F6 => ImGuiKey.F6,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.F7 => ImGuiKey.F7,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.F8 => ImGuiKey.F8,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.F9 => ImGuiKey.F9,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.F10 => ImGuiKey.F10,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.F11 => ImGuiKey.F11,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.F12 => ImGuiKey.F12,

                // Modifier keys
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftShift => ImGuiKey.LeftShift,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightShift => ImGuiKey.RightShift,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftControl => ImGuiKey.LeftCtrl,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightControl => ImGuiKey.RightCtrl,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftAlt => ImGuiKey.LeftAlt,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightAlt => ImGuiKey.RightAlt,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftSuper => ImGuiKey.LeftSuper,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightSuper => ImGuiKey.RightSuper,

                // Punctuation/symbols
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Comma => ImGuiKey.Comma,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Period => ImGuiKey.Period,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Slash => ImGuiKey.Slash,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Semicolon => ImGuiKey.Semicolon,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Equal => ImGuiKey.Equal,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Minus => ImGuiKey.Minus,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftBracket => ImGuiKey.LeftBracket,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightBracket => ImGuiKey.RightBracket,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Backslash => ImGuiKey.Backslash,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.Apostrophe => ImGuiKey.Apostrophe,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.GraveAccent => ImGuiKey.GraveAccent,

                // Numpad
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPad0 => ImGuiKey.Keypad0,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPad1 => ImGuiKey.Keypad1,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPad2 => ImGuiKey.Keypad2,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPad3 => ImGuiKey.Keypad3,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPad4 => ImGuiKey.Keypad4,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPad5 => ImGuiKey.Keypad5,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPad6 => ImGuiKey.Keypad6,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPad7 => ImGuiKey.Keypad7,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPad8 => ImGuiKey.Keypad8,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPad9 => ImGuiKey.Keypad9,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadAdd => ImGuiKey.KeypadAdd,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadSubtract => ImGuiKey.KeypadSubtract,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadMultiply => ImGuiKey.KeypadMultiply,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadDivide => ImGuiKey.KeypadDivide,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadDecimal => ImGuiKey.KeypadDecimal,
                OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadEnter => ImGuiKey.KeypadEnter,

                _ => ImGuiKey.None
            };
        }


    }


    public enum TextureCoordinate
    {
        S = TextureParameterName.TextureWrapS,
        T = TextureParameterName.TextureWrapT,
        R = TextureParameterName.TextureWrapR
    }

    class Texture : IDisposable
    {
        public const SizedInternalFormat Srgb8Alpha8 = (SizedInternalFormat)All.Srgb8Alpha8;
        public const SizedInternalFormat RGB32F = (SizedInternalFormat)All.Rgb32f;

        public const GetPName MAX_TEXTURE_MAX_ANISOTROPY = (GetPName)0x84FF;

        public static readonly float MaxAniso;

        static Texture()
        {
            MaxAniso = GL.GetFloat(MAX_TEXTURE_MAX_ANISOTROPY);
        }

        public readonly string Name;
        public readonly int GLTexture;
        public readonly int Width, Height;
        public readonly int MipmapLevels;
        public readonly SizedInternalFormat InternalFormat;

        public Texture(string name, Bitmap image, bool generateMipmaps, bool srgb)
        {
            Name = name;
            Width = image.Width;
            Height = image.Height;
            InternalFormat = srgb ? Srgb8Alpha8 : SizedInternalFormat.Rgba8;

            if (generateMipmaps)
            {
                // Calculate how many levels to generate for this texture
                MipmapLevels = (int)Math.Floor(Math.Log(Math.Max(Width, Height), 2));
            }
            else
            {
                // There is only one level
                MipmapLevels = 1;
            }

            //Util.CheckGLError("Clear");

            ImguiUtils.CreateTexture(TextureTarget.Texture2D, Name, out GLTexture);

            GL.BindTexture(TextureTarget.Texture2D, GLTexture);
            GL.TexStorage2D(TextureTarget2d.Texture2D, MipmapLevels, InternalFormat, Width, Height);
            //Util.CheckGLError("Storage2d");

            BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, Width, Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, Width, Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            //Util.CheckGLError("SubImage");

            image.UnlockBits(data);
            image.Dispose();

            if (generateMipmaps) GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, MipmapLevels - 1);

            SetWrap(TextureCoordinate.S, TextureWrapMode.Repeat);
            SetWrap(TextureCoordinate.T, TextureWrapMode.Repeat);

            SetMinFilter(generateMipmaps ? TextureMinFilter.Linear : TextureMinFilter.LinearMipmapLinear);
            SetMagFilter(TextureMagFilter.Linear);
        }

        public Texture(string name, int GLTex, int width, int height, int mipmaplevels, SizedInternalFormat internalFormat)
        {
            Name = name;
            GLTexture = GLTex;
            Width = width;
            Height = height;
            MipmapLevels = mipmaplevels;
            InternalFormat = internalFormat;
        }

        public Texture(string name, int width, int height, IntPtr data, bool generateMipmaps = false, bool srgb = false)
        {
            Name = name;
            Width = width;
            Height = height;
            InternalFormat = srgb ? Srgb8Alpha8 : SizedInternalFormat.Rgba8;
            MipmapLevels = generateMipmaps == false ? 1 : (int)Math.Floor(Math.Log(Math.Max(Width, Height), 2));

            ImguiUtils.CreateTexture(TextureTarget.Texture2D, Name, out GLTexture);
            GL.BindTexture(TextureTarget.Texture2D, GLTexture);
            GL.TexStorage2D(TextureTarget2d.Texture2D, MipmapLevels, InternalFormat, Width, Height);

            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, Width, Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data);

            if (generateMipmaps) GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, MipmapLevels - 1);

            SetWrap(TextureCoordinate.S, TextureWrapMode.Repeat);
            SetWrap(TextureCoordinate.T, TextureWrapMode.Repeat);
        }

        public void SetMinFilter(TextureMinFilter filter)
        {
            GL.BindTexture(TextureTarget.Texture2D, GLTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)filter);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void SetMagFilter(TextureMagFilter filter)
        {
            GL.BindTexture(TextureTarget.Texture2D, GLTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)filter);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void SetAnisotropy(float level)
        {
            const TextureParameterName TEXTURE_MAX_ANISOTROPY = (TextureParameterName)0x84FE;

            GL.BindTexture(TextureTarget.Texture2D, GLTexture);
            GL.TexParameter(TextureTarget.Texture2D, TEXTURE_MAX_ANISOTROPY, Math.Clamp(level, 1, MaxAniso));
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void SetLod(int @base, int min, int max)
        {
            GL.BindTexture(TextureTarget.Texture2D, GLTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, @base);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinLod, min);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLod, max);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void SetWrap(TextureCoordinate coord, TextureWrapMode mode)
        {
            GL.BindTexture(TextureTarget.Texture2D, GLTexture);
            GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)coord, (int)mode);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Dispose()
        {
            GL.DeleteTexture(GLTexture);
        }
    }
    }
