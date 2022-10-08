using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Disce.Objects
{
    internal class Coin : SKDrawable
    {
        private static readonly List<SKBitmap> CoinAnimation = new List<SKBitmap>();
        private float _CurrentCoin = 0.0f;
        public float Scale { get; private set; }
        public static SKSize MaxSize { get; private set; }
        private readonly bool _IsAnimated = false;

        public Coin(bool animate = true, float scale = 0.25f)
        {

            if (CoinAnimation.Count == 0)
            {
                MaxSize = new SKSize(0, 0);

                int CoinImageCount = 10;
                for (int i = 1; i != CoinImageCount + 1; i++)
                {
                    using (Stream S = GetType().GetTypeInfo().Assembly.GetManifestResourceStream("Disce.Gold_" + i.ToString() + ".png"))
                    {
                        SKBitmap s = SKBitmap.Decode(S);
                        MaxSize = new SKSize(Math.Max(MaxSize.Width, s.Width * scale), Math.Max(MaxSize.Height, s.Height * scale));
                        CoinAnimation.Add(s);
                    }
                }
            }

            Scale = scale;
            _IsAnimated = animate;
        }

        protected override void OnDraw(SKCanvas canvas)
        {
            // Draw rotating coin
            if (_CurrentCoin >= 10)
            {
                _CurrentCoin = 0;
            }
            SKBitmap coinimage = CoinAnimation[(int)Math.Floor(_CurrentCoin)];
            canvas.Save();
            canvas.Scale(Scale);
            canvas.DrawBitmap(coinimage, new SKPoint(-coinimage.Width / 2, -coinimage.Height / 2));
            canvas.Restore();

            if (_IsAnimated)
            {
                _CurrentCoin += 0.40f;
            }

            base.OnDraw(canvas);
        }

        protected override SKRect OnGetBounds()
        {
            SKRect bounds = base.OnGetBounds();
            SKBitmap coinimage = CoinAnimation[(int)Math.Floor(_CurrentCoin)];
            return new SKRect(bounds.Location.X, bounds.Location.Y, coinimage.Width, coinimage.Height);
        }
    }
}
