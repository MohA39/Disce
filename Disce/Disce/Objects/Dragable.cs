using SkiaSharp;
using System;

namespace Disce.Objects
{
    public class Dragable : SKDrawable
    {
        public const float MaxStartFont = 150;
        public const float MaxEndFont = 200;
        public const float FontDivisior = 1.30f;
        public const int FontIncreaseTime = 1250;

        public string Symbol = "";
        public SKColor Color = SKColors.Black;
        public short CurrentCubeFace = -1;
        public float X = -1;
        public float Y = -1;
        public float XOffset = 0;
        public float YOffset = 0;
        public float Rotation = 0;
        public float Width = -1;
        public float Height = -1;
        public float FontSize = -1;
        public int Opacity = 0xFF;
        public bool IsVisible = false;
        public bool Hidden = false;
        public bool TouchHidden = false;
        public bool IsTouched = false;
        public bool IsHeldDown = false;
        public SKRect SymbolRect = new SKRect();

        private readonly int _FPS;
        public SKTypeface EnglishFont { get; private set; }

        public Dragable(int FPS, SKTypeface englishFont)
        {
            _FPS = FPS;
            EnglishFont = englishFont;

            FontSize = (float)Math.Round(MaxStartFont / Math.Max(Symbol.Length / FontDivisior, 1));
        }

        protected override void OnDraw(SKCanvas canvas)
        {
            float StartFont = (float)Math.Round(MaxStartFont / Math.Max(Symbol.Length / FontDivisior, 1));
            float EndFont = (float)Math.Round(MaxEndFont / Math.Max(Symbol.Length / FontDivisior, 1));

            SKPaint tp = new SKPaint
            {
                Color = Color.WithAlpha((byte)Opacity),
                TextSize = FontSize,
                Typeface = EnglishFont
            };

            if (IsTouched)
            {

                if (FontSize < EndFont)
                {
                    FontSize += (EndFont - StartFont) / (FontIncreaseTime / 1000 * _FPS);
                }
            }
            if (CurrentCubeFace == -1 && !Hidden)
            {

                SKRect TextSize = new SKRect();
                tp.MeasureText(Symbol, ref TextSize);

                Width = TextSize.Width;
                Height = TextSize.Height;

                IsVisible = true;

                canvas.Save();
                canvas.RotateDegrees(Rotation);

                canvas.DrawText(Symbol, X - TextSize.MidX + XOffset, Y - TextSize.MidY + YOffset, tp);
                canvas.Restore();
            }

            if (CurrentCubeFace != -1 && !Hidden)
            {
                canvas.DrawText(Symbol, X, Y, tp);
            }
            base.OnDraw(canvas);
        }
    }
}
