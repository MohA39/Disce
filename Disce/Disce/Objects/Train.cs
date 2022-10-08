using SkiaSharp;
using System;
using System.IO;
using System.Reflection;

namespace Disce.Objects
{
    public class Train : SKDrawable
    {
        private static readonly Random RNG = new Random();
        private static SKBitmap TrainImage = null;

        private float X_Offset;
        private float Y_Offset;

        public float Scale { get; private set; }
        public float Y { get; set; }
        public float X { get; set; }

        public Train(float scale = 0.25f)
        {

            if (TrainImage == null)
            {
                // Load train image
                using (Stream S = GetType().GetTypeInfo().Assembly.GetManifestResourceStream("Disce.Train.png"))
                {
                    TrainImage = SKBitmap.Decode(S);
                }
            }

            X_Offset = 0;
            Y_Offset = 0;

            Scale = scale;
        }

        protected override void OnDraw(SKCanvas canvas)
        {
            canvas.DrawBitmap(TrainImage, SKRect.Create(X + X_Offset, Y + Y_Offset, TrainImage.Width * Scale, TrainImage.Height * Scale));
            X_Offset = RNG.Next(-3, 3);
            Y_Offset = RNG.Next(-3, 3);

            base.OnDraw(canvas);
        }

        protected override SKRect OnGetBounds()
        {
            SKRect bounds = base.OnGetBounds();

            return new SKRect(bounds.Location.X, bounds.Location.Y, TrainImage.Width * Scale, TrainImage.Height * Scale);
        }
    }
}
