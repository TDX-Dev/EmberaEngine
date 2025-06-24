using EmberaEngine.Core;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.AssetHandling
{
    public class TextureLoader : IAssetLoader<Texture>
    {
        public IEnumerable<string> SupportedExtensions = [
            "png", "jpg", "jpeg", "tga", "exr"
        ];

        IEnumerable<string> IAssetLoader.SupportedExtensions => SupportedExtensions;

        public Texture LoadSync(string virtualPath)
        {
            byte[] file = VirtualFileSystem.Open(virtualPath);
            var img = new Image();
            img.Load(file);

            var texture = new Texture(TextureTarget2d.Texture2D);
            texture.TexImage2D(img.Width, img.Height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.UnsignedByte, img.Pixels);
            texture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            texture.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            texture.GenerateMipmap();

            texture.Id = AssetLookup.GetFileGuidByPath(virtualPath);

            img.Pixels = [];

            return texture;
        }

        IAssetReference<Texture> IAssetLoader<Texture>.Load(string virtualPath)
        {
            var textureReference = new TextureReference();

            (int, int, int) imageDimensions = Image.GetImageDimensions(VirtualFileSystem.OpenStream(virtualPath));
            imageDimensions.Item3 = 4; // hardcoded value since im always returning with alpha added anyway
            uint imageSize = (uint)(imageDimensions.Item1 * imageDimensions.Item2 * imageDimensions.Item3);
            BufferObject<byte> stagingBuffer = new BufferObject<byte>(OpenTK.Graphics.OpenGL.BufferStorageTarget.PixelUnpackBuffer, imageSize, OpenTK.Graphics.OpenGL.BufferStorageFlags.MapPersistentBit | OpenTK.Graphics.OpenGL.BufferStorageFlags.MapWriteBit);

            unsafe
            {
                void* bufferMemory = stagingBuffer.GetMappedBufferRange(0, imageSize, OpenTK.Graphics.OpenGL.BufferAccessMask.MapPersistentBit | OpenTK.Graphics.OpenGL.BufferAccessMask.MapWriteBit);
                Task.Run(() =>
                {

                    byte[] file = VirtualFileSystem.Open(virtualPath);
                    var img = new Image();
                    img.Load(file);

                    unsafe
                    {
                        fixed (byte* sourcePtr = img.Pixels)
                        {
                            NativeMemory.Copy(sourcePtr, bufferMemory, imageSize);
                        }
                    }

                    MainThreadDispatcher.Queue(async () =>
                    {
                        var texture = new Texture(TextureTarget2d.Texture2D);
                        texture.TexImage2D(imageDimensions.Item1, imageDimensions.Item2, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.UnsignedByte, nint.Zero);
                        stagingBuffer.Bind(OpenTK.Graphics.OpenGL.BufferTarget.PixelUnpackBuffer);
                        texture.SubTexture2D(imageDimensions.Item1, imageDimensions.Item2, PixelFormat.Rgba, PixelType.UnsignedByte, nint.Zero);
                        texture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
                        texture.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
                        texture.GenerateMipmap();

                        AssetLookup.RegisterFile(texture.Id, virtualPath);
                        texture.Id = AssetLookup.GetFileGuidByPath(virtualPath);

                        textureReference.SetValue(texture); // calls OnLoad internally

                        img.Pixels = [];
                        stagingBuffer.DeleteBuffer();
                    });
                });

            }

            //Task.Run(async () =>
            //{
            //    await Task.Delay(UtilRandom.Next(10) * 1000);
            //    byte[] file = VirtualFileSystem.Open(virtualPath);
            //    var img = new Image();
            //    img.Load(file);

            //    MainThreadDispatcher.Queue(async () =>
            //    {
            //        var texture = new Texture(TextureTarget2d.Texture2D);
            //        texture.TexImage2D(imageDimensions.Item1, imageDimensions.Item2, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.UnsignedByte, img.Pixels);
            //        texture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            //        texture.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            //        texture.GenerateMipmap();

            //        textureReference.SetValue(texture); // calls OnLoad internally

            //        img.Pixels = [];
            //    });
            //});


            return textureReference;
        }

    }
}
