using EmberaEngine.Engine.Core;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Utilities
{
    public class Helper
    {
        public static System.Numerics.Vector2 ToNumerics2(OpenTK.Mathematics.Vector2 value)
        {
            return Unsafe.As<OpenTK.Mathematics.Vector2, System.Numerics.Vector2>(ref value);
        }

        public static OpenTK.Mathematics.Vector2 ToVector2(System.Numerics.Vector2 value)
        {
            return new OpenTK.Mathematics.Vector2(value.X, value.Y);
        }

        public static OpenTK.Mathematics.Vector4 ToVector4(OpenTK.Mathematics.Color4 value)
        {
            return new OpenTK.Mathematics.Vector4(value.R, value.G, value.B, value.A);
        }

        public static OpenTK.Mathematics.Vector3 ToVector3(System.Numerics.Vector3 value)
        {
            return new(value.X, value.Y, value.Z);
        }

        public static System.Numerics.Vector3 ToNumerics3(Vector3 value)
        {
            return Unsafe.As<Vector3, System.Numerics.Vector3>(ref value);
        }

        public static System.Numerics.Quaternion ToQuaternion(System.Numerics.Vector3 Euler)
        {
            double cy = Math.Cos(Euler.Z * 0.5);
            double sy = Math.Sin(Euler.Z * 0.5);
            double cp = Math.Cos(Euler.Y * 0.5);
            double sp = Math.Sin(Euler.Y * 0.5);
            double cr = Math.Cos(Euler.X * 0.5);
            double sr = Math.Sin(Euler.X * 0.5);

            System.Numerics.Quaternion q = new System.Numerics.Quaternion();
            q.W = (float)(cr * cp * cy + sr * sp * sy);
            q.X = (float)(sr * cp * cy - cr * sp * sy);
            q.Y = (float)(cr * sp * cy + sr * cp * sy);
            q.Z = (float)(cr * cp * sy - sr * sp * cy);

            return q;
        }

        public static OpenTK.Mathematics.Quaternion ToOpenTKQuaternion(System.Numerics.Quaternion q)
        {
            return new OpenTK.Mathematics.Quaternion(q.X, q.Y, q.Z, q.W);
        }

        public static Vector3 ToEulerAngles(System.Numerics.Quaternion q)
        {
            Vector3 angles;

            // roll (x-axis rotation)
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
                angles.Y = (float)(Math.PI / 2 * Math.Sign(sinp)); // use 90 degrees if out of range
            else
                angles.Y = (float)Math.Asin(sinp);

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }

        public static Vector3 ToEulerAngles(Quaternion q)
        {
            Vector3 angles;

            // roll (x-axis rotation)
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
                angles.Y = (float)(Math.PI / 2 * Math.Sign(sinp)); // use 90 degrees if out of range
            else
                angles.Y = (float)Math.Asin(sinp);

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }

        public static Vector3 ToDegrees(Vector3 radians)
        {
            return new Vector3(MathHelper.RadiansToDegrees(radians.X), MathHelper.RadiansToDegrees(radians.Y), MathHelper.RadiansToDegrees(radians.Z));
        }

        public static Vector3 ToRadians(Vector3 degrees)
        {
            return new Vector3(MathHelper.DegreesToRadians(degrees.X), MathHelper.DegreesToRadians(degrees.Y), MathHelper.DegreesToRadians(degrees.Z));
        }

        public static System.Numerics.Vector3 ToRadians(System.Numerics.Vector3 degrees)
        {
            return new System.Numerics.Vector3(MathHelper.DegreesToRadians(degrees.X), MathHelper.DegreesToRadians(degrees.Y), MathHelper.DegreesToRadians(degrees.Z));
        }


        public static Vector3 FromNumerics3(System.Numerics.Vector3 vec) =>
    new Vector3(vec.X, vec.Y, vec.Z);

        public static Quaternion FromNumericsQuat(System.Numerics.Quaternion quat) =>
            new Quaternion(quat.X, quat.Y, quat.Z, quat.W);


        public static Texture loadImageAsTex(string file, TextureMinFilter tminf = TextureMinFilter.Linear, TextureMagFilter tmagf = TextureMagFilter.Linear)
        {
            Image image = new EmberaEngine.Engine.Utilities.Image();
            image.LoadPNG(file);

            Texture texture = new Texture(EmberaEngine.Engine.Core.TextureTarget2d.Texture2D);
            texture.SetFilter(tminf, tmagf);
            texture.SetWrapMode(EmberaEngine.Engine.Core.TextureWrapMode.ClampToEdge, EmberaEngine.Engine.Core.TextureWrapMode.ClampToEdge);
            texture.TexImage2D<byte>(image.Width, image.Height, EmberaEngine.Engine.Core.PixelInternalFormat.Rgba16f, EmberaEngine.Engine.Core.PixelFormat.Rgba, EmberaEngine.Engine.Core.PixelType.UnsignedByte, image.Pixels);
            texture.GenerateMipmap();

            image.Pixels = [];

            return texture;
        }

        public static Texture loadHDRIAsTex(string file, TextureMinFilter tminf = TextureMinFilter.Linear, TextureMagFilter tmagf = TextureMagFilter.Linear)
        {
            Image image = new EmberaEngine.Engine.Utilities.Image();
            image.LoadHDRI(file);

            Texture texture = new Texture(EmberaEngine.Engine.Core.TextureTarget2d.Texture2D);
            texture.SetFilter(tminf, tmagf);
            texture.SetWrapMode(EmberaEngine.Engine.Core.TextureWrapMode.ClampToEdge, EmberaEngine.Engine.Core.TextureWrapMode.ClampToEdge);
            texture.TexImage2D(image.Width, image.Height, EmberaEngine.Engine.Core.PixelInternalFormat.Rgb32f, EmberaEngine.Engine.Core.PixelFormat.Rgb, EmberaEngine.Engine.Core.PixelType.Float, image.PixelHP);
            texture.GenerateMipmap();

            image.Pixels = [];

            return texture;
        }

        public static Vector3[] GenerateNoise(int size)
        {
            List<Vector3> vector3s = new List<Vector3>();

            for (int i = 0; i < size; i++)
            {
                vector3s.Add(new Vector3(UtilRandom.GetFloat() * 2 - 1, UtilRandom.GetFloat() * 2 - 1, 0.0f));
            }

            return vector3s.ToArray();
        }

    }
}
