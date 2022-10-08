using SkiaSharp;
using System;
using System.IO;
using System.Reflection;

namespace Disce.Objects
{
    public class Confetto : SKDrawable
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Rotation { get; private set; }
        public float Weight { get; private set; }
        public SKColor Color { get; private set; }
        public SKSize Size { get; private set; }
        public Confetto(SKPoint location, SKSize size, float rotation, float weight, SKColor color)
        {
            X = location.X;
            Y = location.Y;
            Size = size;
            Rotation = rotation;
            Weight = weight;
            Color = color;
        }

        protected override void OnDraw(SKCanvas canvas)
        {
            Y += Weight;
            using (SKPaint P = new SKPaint{ Color = Color })
            {
                canvas.Save();
                canvas.RotateDegrees(Rotation);
                canvas.DrawRect(X, Y, Size.Width, Size.Height, P);
                canvas.Restore();
                base.OnDraw(canvas);
            }

            
        }
    }
}
