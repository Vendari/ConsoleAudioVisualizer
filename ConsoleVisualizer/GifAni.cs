using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ConsoleVisualizer
{
    class GifAni
    {
        public static Image ResizeImage(Image source, int width, int height)
        {
            Image dest = new Bitmap(width, height);
            using (Graphics gr = Graphics.FromImage(dest))
            {
                gr.FillRectangle(Brushes.White, 0, 0, width, height);
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                float srcW = source.Width;
                float srcH = source.Height;
                float dstW = width;
                float dstH = height;

                if (srcW <= dstW && srcH <= dstH)
                {
                    int left = (width - source.Width) / 2;
                    int top = (height - source.Height) / 2;
                    gr.DrawImage(source, left, top, source.Width, source.Height);
                }
                else if (srcW / srcH > dstW / dstH)
                {
                    float cy = srcH / srcW * dstW;
                    float top = ((float)dstH - cy) / 2.0f;
                    if (top < 1.0f) top = 0;
                    gr.DrawImage(source, 0, top, dstW, cy);
                }
                return dest;
            }
        }
    }
}
