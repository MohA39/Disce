using SkiaSharp;
using System;
using System.IO;
using System.Reflection;

namespace Disce.Objects
{
    public class ProgressBar : SKDrawable
    {
        private Games Game;
        private float MaxWidth;
        public SKSize Size { get; private set; }
        public ProgressBar(Games game, float maxWidth, float height)
        {
            Game = game;
            MaxWidth = maxWidth;
            Size = new SKSize(-1, height);
            
        }
        protected override void OnDraw(SKCanvas canvas)
        {
            // Progress bar

            SKPaint ProgressBarPaint = new SKPaint();
            //float Size.Height = CanvasRect.Height * 0.015f;
            float StartLighting = 30f;
            float EndLighting = 70f;
            switch (Game)
            {
                case Games.Words:
                    float WordsBarWidth = MaxWidth * SaveManager.WordsProgress;

                    SKRect WordsProgressBarRect = SKRect.Create(0, 0, WordsBarWidth, Size.Height);
                    float WordsBarHue = GetProgressBarHue(SaveManager.WordsLevel);
                    ProgressBarPaint.Shader = SKShader.CreateLinearGradient(
                            new SKPoint(WordsProgressBarRect.Left, WordsProgressBarRect.MidY),
                            new SKPoint(WordsProgressBarRect.Right, WordsProgressBarRect.MidY),
                            new SKColor[] { SKColor.FromHsl(WordsBarHue, 100, StartLighting), SKColor.FromHsl(WordsBarHue, 100, StartLighting + ((EndLighting - StartLighting) * SaveManager.WordsProgress)) },
                            new float[] { 0, 1 },
                            SKShaderTileMode.Repeat);
                    canvas.DrawRect(WordsProgressBarRect, ProgressBarPaint);
                    break;
                case Games.AddSubNumbers:
                    float AddSubBarWidth = MaxWidth * SaveManager.AddSubProgress;
                    SKRect AddSubProgressBarRect = SKRect.Create(0, 0, AddSubBarWidth, Size.Height);
                    float AddSubBarHue = GetProgressBarHue(SaveManager.AddSubLevel);
                    ProgressBarPaint.Shader = SKShader.CreateLinearGradient(
                            new SKPoint(AddSubProgressBarRect.Left, AddSubProgressBarRect.MidY),
                            new SKPoint(AddSubProgressBarRect.Right, AddSubProgressBarRect.MidY),
                            new SKColor[] { SKColor.FromHsl(AddSubBarHue, 100, StartLighting), SKColor.FromHsl(AddSubBarHue, 100, StartLighting + ((EndLighting - StartLighting) * SaveManager.AddSubProgress)) },
                            new float[] { 0, 1 },
                            SKShaderTileMode.Repeat);
                    canvas.DrawRect(AddSubProgressBarRect, ProgressBarPaint);
                    break;
                case Games.MultiDivideNumbers:
                    float MultiDivideBarWidth = MaxWidth * SaveManager.MultiDivProgress;
                    SKRect MultiDivideProgressBarRect = SKRect.Create(0, 0, MultiDivideBarWidth, Size.Height);
                    float MultiDivideBarHue = GetProgressBarHue(SaveManager.MultiDivLevel);
                    ProgressBarPaint.Shader = SKShader.CreateLinearGradient(
                            new SKPoint(MultiDivideProgressBarRect.Left, MultiDivideProgressBarRect.MidY),
                            new SKPoint(MultiDivideProgressBarRect.Right, MultiDivideProgressBarRect.MidY),
                            new SKColor[] { SKColor.FromHsl(MultiDivideBarHue, 100, StartLighting), SKColor.FromHsl(MultiDivideBarHue, 100, StartLighting + ((EndLighting - StartLighting) * SaveManager.MultiDivProgress)) },
                            new float[] { 0, 1 },
                            SKShaderTileMode.Repeat);
                    canvas.DrawRect(MultiDivideProgressBarRect, ProgressBarPaint);
                    break;

                case Games.CountTheItems:
                    float CountBarWidth = MaxWidth * SaveManager.CountProgress;
                    SKRect CountProgressBarRect = SKRect.Create(0, 0, CountBarWidth, Size.Height);
                    float CountBarHue = GetProgressBarHue(SaveManager.CountLevel);
                    ProgressBarPaint.Shader = SKShader.CreateLinearGradient(
                            new SKPoint(CountProgressBarRect.Left, CountProgressBarRect.MidY),
                            new SKPoint(CountProgressBarRect.Right, CountProgressBarRect.MidY),
                            new SKColor[] { SKColor.FromHsl(CountBarHue, 100, StartLighting), SKColor.FromHsl(CountBarHue, 100, StartLighting + ((EndLighting - StartLighting) * SaveManager.WordsProgress)) },
                            new float[] { 0, 1 },
                            SKShaderTileMode.Repeat);
                    canvas.DrawRect(CountProgressBarRect, ProgressBarPaint);
                    break;
            }
            base.OnDraw(canvas);
        }

        private float GetProgressBarHue(int Level)
        {
            float HueChangePerLevel = 20.0f;
            float LevelsInCycle = 360.0f / HueChangePerLevel;
            return (Level * HueChangePerLevel) - (360.0f * (float)Math.Floor((Level / LevelsInCycle) - 0.01));
        }
    }
}
