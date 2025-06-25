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

        ImGuiKey ConvertToImGuiKey(OpenTK.Windowing.GraphicsLibraryFramework.Keys key)
        {
            return key switch
            {
                Keys.Tab => ImGuiKey.Tab,
                Keys.Left => ImGuiKey.LeftArrow,
                Keys.Right => ImGuiKey.RightArrow,
                Keys.Up => ImGuiKey.UpArrow,
                Keys.Down => ImGuiKey.DownArrow,
                Keys.PageUp => ImGuiKey.PageUp,
                Keys.PageDown => ImGuiKey.PageDown,
                Keys.Home => ImGuiKey.Home,
                Keys.End => ImGuiKey.End,
                Keys.Insert => ImGuiKey.Insert,
                Keys.Delete => ImGuiKey.Delete,
                Keys.Backspace => ImGuiKey.Backspace,
                Keys.Space => ImGuiKey.Space,
                Keys.Enter => ImGuiKey.Enter,
                Keys.Escape => ImGuiKey.Escape,

                // Number keys
                Keys.Number0 => ImGuiKey._0,
                Keys.Number1 => ImGuiKey._1,
                Keys.Number2 => ImGuiKey._2,
                Keys.Number3 => ImGuiKey._3,
                Keys.Number4 => ImGuiKey._4,
                Keys.Number5 => ImGuiKey._5,
                Keys.Number6 => ImGuiKey._6,
                Keys.Number7 => ImGuiKey._7,
                Keys.Number8 => ImGuiKey._8,
                Keys.Number9 => ImGuiKey._9,

                // Letters
                Keys.A => ImGuiKey.A,
                Keys.B => ImGuiKey.B,
                Keys.C => ImGuiKey.C,
                Keys.D => ImGuiKey.D,
                Keys.E => ImGuiKey.E,
                Keys.F => ImGuiKey.F,
                Keys.G => ImGuiKey.G,
                Keys.H => ImGuiKey.H,
                Keys.I => ImGuiKey.I,
                Keys.J => ImGuiKey.J,
                Keys.K => ImGuiKey.K,
                Keys.L => ImGuiKey.L,
                Keys.M => ImGuiKey.M,
                Keys.N => ImGuiKey.N,
                Keys.O => ImGuiKey.O,
                Keys.P => ImGuiKey.P,
                Keys.Q => ImGuiKey.Q,
                Keys.R => ImGuiKey.R,
                Keys.S => ImGuiKey.S,
                Keys.T => ImGuiKey.T,
                Keys.U => ImGuiKey.U,
                Keys.V => ImGuiKey.V,
                Keys.W => ImGuiKey.W,
                Keys.X => ImGuiKey.X,
                Keys.Y => ImGuiKey.Y,
                Keys.Z => ImGuiKey.Z,

                // Function keys
                Keys.F1 => ImGuiKey.F1,
                Keys.F2 => ImGuiKey.F2,
                Keys.F3 => ImGuiKey.F3,
                Keys.F4 => ImGuiKey.F4,
                Keys.F5 => ImGuiKey.F5,
                Keys.F6 => ImGuiKey.F6,
                Keys.F7 => ImGuiKey.F7,
                Keys.F8 => ImGuiKey.F8,
                Keys.F9 => ImGuiKey.F9,
                Keys.F10 => ImGuiKey.F10,
                Keys.F11 => ImGuiKey.F11,
                Keys.F12 => ImGuiKey.F12,

                // Modifier keys
                Keys.LeftShift => ImGuiKey.LeftShift,
                Keys.RightShift => ImGuiKey.RightShift,
                Keys.LeftControl => ImGuiKey.LeftCtrl,
                Keys.RightControl => ImGuiKey.RightCtrl,
                Keys.LeftAlt => ImGuiKey.LeftAlt,
                Keys.RightAlt => ImGuiKey.RightAlt,
                Keys.LeftSuper => ImGuiKey.LeftSuper,
                Keys.RightSuper => ImGuiKey.RightSuper,

                // Punctuation/symbols
                Keys.Comma => ImGuiKey.Comma,
                Keys.Period => ImGuiKey.Period,
                Keys.Slash => ImGuiKey.Slash,
                Keys.Semicolon => ImGuiKey.Semicolon,
                Keys.Equal => ImGuiKey.Equal,
                Keys.Minus => ImGuiKey.Minus,
                Keys.LeftBracket => ImGuiKey.LeftBracket,
                Keys.RightBracket => ImGuiKey.RightBracket,
                Keys.Backslash => ImGuiKey.Backslash,
                Keys.Apostrophe => ImGuiKey.Apostrophe,
                Keys.GraveAccent => ImGuiKey.GraveAccent,

                // Numpad
                Keys.Keypad0 => ImGuiKey.Keypad0,
                Keys.Keypad1 => ImGuiKey.Keypad1,
                Keys.Keypad2 => ImGuiKey.Keypad2,
                Keys.Keypad3 => ImGuiKey.Keypad3,
                Keys.Keypad4 => ImGuiKey.Keypad4,
                Keys.Keypad5 => ImGuiKey.Keypad5,
                Keys.Keypad6 => ImGuiKey.Keypad6,
                Keys.Keypad7 => ImGuiKey.Keypad7,
                Keys.Keypad8 => ImGuiKey.Keypad8,
                Keys.Keypad9 => ImGuiKey.Keypad9,
                Keys.KeypadAdd => ImGuiKey.KeypadAdd,
                Keys.KeypadSubtract => ImGuiKey.KeypadSubtract,
                Keys.KeypadMultiply => ImGuiKey.KeypadMultiply,
                Keys.KeypadDivide => ImGuiKey.KeypadDivide,
                Keys.KeypadDecimal => ImGuiKey.KeypadDecimal,
                Keys.KeypadEnter => ImGuiKey.KeypadEnter,

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
