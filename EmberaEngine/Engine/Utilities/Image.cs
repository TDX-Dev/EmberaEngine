using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Utilities
{
    public class Image
    {
        public int Width, Height;
        public byte[] Pixels;

        public Image()
        {

        }

        public void LoadPNG(string path, bool directPath = false)
        {
            SixLabors.ImageSharp.Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(path);
            this.Width = image.Width;
            this.Height = image.Height;
            List<byte> pixels = new List<byte>(4 * image.Width * image.Height);
            Pixels = new byte[4 * image.Width * image.Height];
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);

                    for (int x = 0; x < image.Width; x++)
                    {
                        pixels.Add(row[x].R);
                        pixels.Add(row[x].G);
                        pixels.Add(row[x].B);
                        pixels.Add(row[x].A);
                    }
                }
            });

            Pixels = pixels.ToArray();
        }


    }
}
