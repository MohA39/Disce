using Disce.Utils;
using SkiaSharp;
using System;
using System.IO;
using System.Reflection;

namespace Disce.Objects
{
    public class TrainCar : SKDrawable
    {
        private static readonly Random RNG = new Random();
        private static SKBitmap TrainCarImage;
        public float Scale;
        public float X_Offset;
        public float Y_Offset;
        public float Hue;
        public int Number;
        public bool AnsweredIncorrectly = false;

        public float Y;
        public float X;

        public TrainCar(float hue, float scale, int number, float x, float y)
        {

            LoadImage();
            Hue = hue;
            Scale = scale;
            Number = number;
            Y = y;
            X = x;
        }

        private static void LoadImage()
        {
            if (TrainCarImage == null)
            {
                using (Stream S = typeof(TrainCar).GetTypeInfo().Assembly.GetManifestResourceStream("Disce.TrainCar.png"))
                {
                    TrainCarImage = SKBitmap.Decode(S);
                }
            }
        }
        public static SKSize GetSize()
        {
            LoadImage();
            return new SKSize(TrainCarImage.Width, TrainCarImage.Height);
        }
        protected override SKRect OnGetBounds()
        {
            SKRect bounds = base.OnGetBounds();

            return SKRect.Create(X + X_Offset, Y + Y_Offset, TrainCarImage.Width * Scale, TrainCarImage.Height * Scale);
        }
        protected override void OnDraw(SKCanvas canvas)
        {
            using (SKPaint paint = new SKPaint())
            {
                paint.ColorFilter = GeometryUtils.GenerateHueFilter(Hue);
                canvas.DrawBitmap(TrainCarImage, Bounds, paint);

                //GenerateHueFilter
                using (SKPaint TrainTextPaint = new SKPaint())
                {
                    TrainTextPaint.TextSize = 100;
                    TrainTextPaint.Color = AnsweredIncorrectly ? SKColors.Red : SKColors.White;
                    SKRect TrainTextMeasurement = new SKRect();

                    TrainTextPaint.MeasureText(Number.ToString(), ref TrainTextMeasurement);
                    canvas.DrawText(Number.ToString(), Bounds.MidX - TrainTextMeasurement.MidX, Bounds.MidY - TrainTextMeasurement.MidY - (50 * Scale), TrainTextPaint);
                }

                X_Offset = RNG.Next(-3, 3);
                Y_Offset = RNG.Next(-3, 3);

            }
        }
    }
}
