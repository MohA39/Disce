using Disce.Utils;
using SkiaSharp;
using System.Collections.Generic;

namespace Disce.Objects
{
    public class Cube : SKDrawable
    {
        // Needs rewriting.
        public List<Dragable> Dragables { get; private set; }
        public int Size { get; private set; }
        public int XRotation { get; set; }
        public int YRotation { get; set; }

        private readonly List<int> Hues = new List<int>();

        private SKRect TextRect = new SKRect();

        public Cube(int size, ref List<Dragable> dragables, int Xrotation = -180, int Yrotation = -180, List<int> FaceHues = null)
        {
            Size = size;
            XRotation = Xrotation;
            YRotation = Yrotation;
            Hues = FaceHues != null ? FaceHues : Utils.Helpers.GenHues(6);
            Dragables = dragables;

        }

        protected override void OnDraw(SKCanvas canvas)
        {
            int WidthHeight = Size;

            SKImageInfo CubeInfo = new SKImageInfo(WidthHeight, WidthHeight);
            using (SKSurface CubeSurface = SKSurface.Create(CubeInfo))
            {
                SKCanvas CubeCanvas = CubeSurface.Canvas;
                CubeCanvas.Clear();

                CubeCanvas.Translate(WidthHeight / 2, WidthHeight / 2);
                SK3dView view = new SK3dView();

                view.RotateXDegrees(XRotation);
                view.RotateYDegrees(YRotation);

                view.Translate(-WidthHeight / 4, WidthHeight / 4, WidthHeight / 4);
                SKRect face = SKRect.Create(0, 0, WidthHeight / 2, WidthHeight / 2);
                // front
                if ((YRotation < 90 && YRotation > -90 && XRotation < 90 && XRotation > -90) ||
                    (((YRotation >= -180 && YRotation < -90) || (YRotation <= 180 && YRotation > 90)) && (XRotation > 90 || XRotation < -90))) // -90 - 90
                {
                    using (new SKAutoCanvasRestore(CubeCanvas, true))
                    {

                        view.Save();
                        view.TranslateZ(-face.Width);
                        view.ApplyToCanvas(CubeCanvas);
                        view.Restore();

                        SKPaint frontFace = new SKPaint
                        {
                            Color = SKColor.FromHsl(Hues[0], 75f, 50f),
                            IsAntialias = true
                        };
                        CubeCanvas.DrawRect(face, frontFace);
                        int DragableIndex = GetDragable(0);
                        if (DragableIndex != -1)
                        {

                            string Symbol = Dragables[DragableIndex].Symbol;

                            SKPaint tp = new SKPaint
                            {
                                Color = GetCubeDragableColor(DragableIndex),
                                TextSize = Dragables[DragableIndex].FontSize,
                                Typeface = Dragables[DragableIndex].EnglishFont
                            };
                            SKRect TextSize = new SKRect();
                            tp.MeasureText(Symbol, ref TextSize);

                            float x = face.MidX - (TextSize.Width / 2);
                            float y = face.MidY - TextSize.MidY;

                            TextRect = SKRect.Create(x, face.MidY - (TextSize.Height / 2), TextSize.Width, TextSize.Height);

                            TextRect = CubeCanvas.TotalMatrix.MapRect(TextRect);
                            Dragables[DragableIndex].SymbolRect = TextRect;
                            Dragables[DragableIndex].X = x;
                            Dragables[DragableIndex].Y = y;
                            Dragables[DragableIndex].Width = TextSize.Width;
                            Dragables[DragableIndex].Height = TextSize.Height;

                            Dragables[DragableIndex].IsVisible = true;
                            if (((YRotation >= -180 && YRotation < -90) || (YRotation <= 180 && YRotation > 90)) && (XRotation > 90 || XRotation < -90))
                            {
                                CubeCanvas.Save();
                                CubeCanvas.Scale(-1, -1, x + (TextSize.Width / 2), y + TextSize.MidY);
                                CubeCanvas.DrawDrawable(Dragables[DragableIndex], 0, 0);
                                CubeCanvas.Restore();
                            }
                            else
                            {
                                CubeCanvas.DrawDrawable(Dragables[DragableIndex], 0, 0);
                            }
                        }

                    }
                }
                else
                {
                    int DragableIndex = GetDragable(0);

                    if (DragableIndex != -1)
                    {
                        Dragables[DragableIndex].IsVisible = false;
                    }
                }

                // Backside
                if ((((YRotation >= -180 && YRotation < -90) || (YRotation <= 180 && YRotation > 90)) && XRotation < 90 && XRotation > -90) ||
                    (YRotation < 90 && YRotation > -90 && (XRotation > 90 || XRotation < -90))) // -180 - 180 / -90 - 90
                {
                    using (new SKAutoCanvasRestore(CubeCanvas, true))
                    {
                        view.Save();
                        view.ApplyToCanvas(CubeCanvas);
                        view.Restore();
                        // draw the face
                        SKPaint backFace = new SKPaint
                        {
                            Color = SKColor.FromHsl(Hues[1], 75f, 50f),
                            IsAntialias = true
                        };
                        CubeCanvas.DrawRect(face, backFace);

                        int DragableIndex = GetDragable(1);
                        if (DragableIndex != -1)
                        {
                            string Symbol = Dragables[DragableIndex].Symbol;
                            SKPaint tp = new SKPaint
                            {
                                Color = GetCubeDragableColor(DragableIndex),
                                TextSize = Dragables[DragableIndex].FontSize,
                                Typeface = Dragables[DragableIndex].EnglishFont
                            };

                            SKRect TextSize = new SKRect();
                            tp.MeasureText(Symbol, ref TextSize);

                            float x = face.MidX - (TextSize.Width / 2);
                            float y = face.MidY - TextSize.MidY;

                            TextRect = SKRect.Create(x, face.MidY - (TextSize.Height / 2), TextSize.Width, TextSize.Height);
                            TextRect = CubeCanvas.TotalMatrix.MapRect(TextRect);
                            Dragables[DragableIndex].SymbolRect = TextRect;

                            Dragables[DragableIndex].X = x;
                            Dragables[DragableIndex].Y = y;

                            Dragables[DragableIndex].Width = TextSize.Width;
                            Dragables[DragableIndex].Height = TextSize.Height;

                            Dragables[DragableIndex].IsVisible = true;

                            if (YRotation < 90 && YRotation > -90 && (XRotation > 90 || XRotation < -90))
                            {

                                CubeCanvas.Save();
                                CubeCanvas.Scale(1, -1, x + (TextSize.Width / 2), y + TextSize.MidY);
                                CubeCanvas.DrawDrawable(Dragables[DragableIndex], 0, 0);
                                CubeCanvas.Restore();
                            }
                            else
                            {
                                CubeCanvas.Save();
                                CubeCanvas.Scale(-1, 1, x + (TextSize.Width / 2), y + TextSize.MidY);
                                CubeCanvas.DrawDrawable(Dragables[DragableIndex], 0, 0);
                                CubeCanvas.Restore();
                            }
                        }

                    }
                }
                else
                {
                    int DragableIndex = GetDragable(1);

                    if (DragableIndex != -1)
                    {
                        Dragables[DragableIndex].IsVisible = false;
                    }
                }

                int LeftRightOffset = 10;
                // LEFT
                if ((YRotation <= 180 - LeftRightOffset && YRotation > 0 + LeftRightOffset && XRotation < 90 && XRotation > -90) ||
                    (YRotation >= -180 + LeftRightOffset && YRotation < 0 - LeftRightOffset && (XRotation > 90 || XRotation < -90)))
                {
                    using (new SKAutoCanvasRestore(CubeCanvas, true))
                    {

                        view.Save();
                        view.RotateYDegrees(-90);
                        view.ApplyToCanvas(CubeCanvas);
                        view.Restore();

                        SKPaint leftFace = new SKPaint
                        {
                            Color = SKColor.FromHsl(Hues[2], 75f, 50f),
                            IsAntialias = true
                        };
                        CubeCanvas.DrawRect(face, leftFace);
                        int DragableIndex = GetDragable(2);
                        if (DragableIndex != -1)
                        {
                            string Symbol = Dragables[DragableIndex].Symbol;
                            SKPaint tp = new SKPaint
                            {
                                Color = GetCubeDragableColor(DragableIndex),
                                TextSize = Dragables[DragableIndex].FontSize,
                                Typeface = Dragables[DragableIndex].EnglishFont
                            };
                            SKRect TextSize = new SKRect();
                            tp.MeasureText(Symbol, ref TextSize);

                            float x = face.MidX - (TextSize.Width / 2);
                            float y = face.MidY - TextSize.MidY;

                            TextRect = SKRect.Create(x, face.MidY - (TextSize.Height / 2), TextSize.Width, TextSize.Height);
                            TextRect = CubeCanvas.TotalMatrix.MapRect(TextRect);
                            Dragables[DragableIndex].SymbolRect = TextRect;

                            Dragables[DragableIndex].X = x;
                            Dragables[DragableIndex].Y = y;

                            Dragables[DragableIndex].Width = TextSize.Width;
                            Dragables[DragableIndex].Height = TextSize.Height;

                            Dragables[DragableIndex].IsVisible = true;
                            if (YRotation >= -180 && YRotation < 0 && (XRotation > 90 || XRotation < -90))
                            {
                                CubeCanvas.Save();
                                CubeCanvas.Scale(-1, -1, x + (TextSize.Width / 2), y + TextSize.MidY);

                                CubeCanvas.DrawDrawable(Dragables[DragableIndex], 0, 0);
                                CubeCanvas.Restore();
                            }
                            else
                            {
                                CubeCanvas.DrawDrawable(Dragables[DragableIndex], 0, 0);
                            }
                        }

                    }
                }
                else
                {
                    int DragableIndex = GetDragable(2);

                    if (DragableIndex != -1)
                    {
                        Dragables[DragableIndex].IsVisible = false;
                    }
                }
                // Right
                if ((YRotation >= -180 + LeftRightOffset && YRotation < 0 - LeftRightOffset && XRotation < 90 && XRotation > -90) ||
                   (YRotation <= 180 - LeftRightOffset && YRotation > 0 + LeftRightOffset && (XRotation > 90 || XRotation < -90)))
                {
                    using (new SKAutoCanvasRestore(CubeCanvas, true))
                    {

                        view.Save();
                        view.RotateYDegrees(-90);
                        view.TranslateZ(face.Height);
                        view.ApplyToCanvas(CubeCanvas);
                        view.Restore();

                        SKPaint rightFace = new SKPaint
                        {
                            Color = SKColor.FromHsl(Hues[3], 75f, 50f),
                            IsAntialias = true
                        };

                        CubeCanvas.DrawRect(face, rightFace);

                        int DragableIndex = GetDragable(3);
                        if (DragableIndex != -1)
                        {
                            string Symbol = Dragables[DragableIndex].Symbol;
                            SKPaint tp = new SKPaint
                            {
                                Color = GetCubeDragableColor(DragableIndex),
                                TextSize = Dragables[DragableIndex].FontSize,
                                Typeface = Dragables[DragableIndex].EnglishFont
                            };

                            SKRect TextSize = new SKRect();
                            tp.MeasureText(Symbol, ref TextSize);

                            float x = face.MidX - (TextSize.Width / 2);
                            float y = face.MidY - TextSize.MidY;

                            TextRect = SKRect.Create(x, face.MidY - (TextSize.Height / 2), TextSize.Width, TextSize.Height);
                            TextRect = CubeCanvas.TotalMatrix.MapRect(TextRect);
                            Dragables[DragableIndex].SymbolRect = TextRect;

                            Dragables[DragableIndex].X = x;
                            Dragables[DragableIndex].Y = y;

                            Dragables[DragableIndex].Width = TextSize.Width;
                            Dragables[DragableIndex].Height = TextSize.Height;

                            Dragables[DragableIndex].IsVisible = true;

                            if (YRotation <= 180 && YRotation > 0 && (XRotation > 90 || XRotation < -90))
                            {
                                CubeCanvas.Save();
                                CubeCanvas.Scale(1, -1, x + (TextSize.Width / 2), y + TextSize.MidY);
                                CubeCanvas.DrawDrawable(Dragables[DragableIndex], 0, 0);
                                CubeCanvas.Restore();
                            }
                            else
                            {
                                CubeCanvas.Save();
                                CubeCanvas.Scale(-1, 1, x + (TextSize.Width / 2), y + TextSize.MidY);
                                CubeCanvas.DrawDrawable(Dragables[DragableIndex], 0, 0);
                                CubeCanvas.Restore();
                            }
                        }

                    }
                }
                else
                {
                    int DragableIndex = GetDragable(3);

                    if (DragableIndex != -1)
                    {
                        Dragables[DragableIndex].IsVisible = false;
                    }
                }

                int Nearest180 = (int)AlgebraUtils.NearestRound(XRotation, 180);
                // Top
                if ((Nearest180 == 0 ? Nearest180 + XRotation : Nearest180 + (XRotation * -1)) < -10)
                {
                    using (new SKAutoCanvasRestore(CubeCanvas, true))
                    {
                        CubeCanvas.Save();
                        view.Save();
                        view.RotateXDegrees(90);
                        view.ApplyToCanvas(CubeCanvas);
                        view.Restore();

                        SKPaint topFace = new SKPaint
                        {
                            Color = SKColor.FromHsl(Hues[4], 75f, 50f),
                            IsAntialias = true
                        };
                        CubeCanvas.DrawRect(face, topFace);

                        int DragableIndex = GetDragable(4);
                        if (DragableIndex != -1)
                        {
                            string Symbol = Dragables[DragableIndex].Symbol;
                            SKPaint tp = new SKPaint
                            {
                                Color = GetCubeDragableColor(DragableIndex),
                                TextSize = Dragables[DragableIndex].FontSize,
                                Typeface = Dragables[DragableIndex].EnglishFont
                            };
                            SKRect TextSize = new SKRect();
                            tp.MeasureText(Symbol, ref TextSize);

                            float x = face.MidX - (TextSize.Width / 2);
                            float y = face.MidY - TextSize.MidY;

                            TextRect = SKRect.Create(x, face.MidY - (TextSize.Height / 2), TextSize.Width, TextSize.Height);
                            TextRect = CubeCanvas.TotalMatrix.MapRect(TextRect);

                            Dragables[DragableIndex].SymbolRect = TextRect;

                            Dragables[DragableIndex].X = x;
                            Dragables[DragableIndex].Y = y;

                            Dragables[DragableIndex].Width = TextSize.Width;
                            Dragables[DragableIndex].Height = TextSize.Height;

                            Dragables[DragableIndex].IsVisible = true;

                            CubeCanvas.RotateDegrees(AlgebraUtils.NearestRound(YRotation, 45), x + (TextSize.Width / 2), y + TextSize.MidY);
                            CubeCanvas.DrawDrawable(Dragables[DragableIndex], 0, 0);

                        }
                        CubeCanvas.Restore();
                    }
                }
                else
                {
                    int DragableIndex = GetDragable(4);

                    if (DragableIndex != -1)
                    {
                        Dragables[DragableIndex].IsVisible = false;
                    }
                }

                // Bottom
                if ((Nearest180 == 0 ? Nearest180 + XRotation : Nearest180 + (XRotation * -1)) > 10)
                {
                    using (new SKAutoCanvasRestore(CubeCanvas, true))
                    {
                        view.Save();
                        view.RotateXDegrees(90);
                        view.TranslateZ(face.Height);
                        view.ApplyToCanvas(CubeCanvas);
                        view.Restore();

                        SKPaint bottomFace = new SKPaint
                        {
                            Color = SKColor.FromHsl(Hues[5], 75f, 50f),
                            IsAntialias = true
                        };

                        CubeCanvas.DrawRect(face, bottomFace);
                        int DragableIndex = GetDragable(5);
                        if (DragableIndex != -1)
                        {
                            string Symbol = Dragables[DragableIndex].Symbol;
                            SKPaint tp = new SKPaint
                            {
                                Color = GetCubeDragableColor(DragableIndex),
                                TextSize = Dragables[DragableIndex].FontSize,
                                Typeface = Dragables[DragableIndex].EnglishFont
                            };
                            SKRect TextSize = new SKRect();
                            tp.MeasureText(Symbol, ref TextSize);

                            float x = face.MidX - (TextSize.Width / 2);
                            float y = face.MidY - TextSize.MidY;

                            TextRect = SKRect.Create(x, face.MidY - (TextSize.Height / 2), TextSize.Width, TextSize.Height);
                            TextRect = CubeCanvas.TotalMatrix.MapRect(TextRect);
                            Dragables[DragableIndex].SymbolRect = TextRect;

                            Dragables[DragableIndex].X = x;
                            Dragables[DragableIndex].Y = y;

                            Dragables[DragableIndex].Width = TextSize.Width;
                            Dragables[DragableIndex].Height = TextSize.Height;

                            Dragables[DragableIndex].IsVisible = true;

                            CubeCanvas.Save();
                            CubeCanvas.Scale(1, -1, x + (TextSize.Width / 2), y + TextSize.MidY);
                            CubeCanvas.RotateDegrees(AlgebraUtils.NearestRound(YRotation, 45) * -1, x + (TextSize.Width / 2), y + TextSize.MidY);
                            CubeCanvas.DrawDrawable(Dragables[DragableIndex], 0, 0);
                            CubeCanvas.Restore();

                        }
                    }
                }
                else
                {
                    int DragableIndex = GetDragable(5);

                    if (DragableIndex != -1)
                    {
                        Dragables[DragableIndex].IsVisible = false;
                    }
                }

                canvas.DrawSurface(CubeSurface, new SKPoint(0, 0));
            }
            base.OnDraw(canvas);
        }

        private int GetDragable(int Face)
        {
            return Dragables.FindIndex(x => x.CurrentCubeFace == Face);
        }

        private SKColor GetCubeDragableColor(int i)
        {
            return Dragables[i].Color.WithAlpha((byte)Dragables[i].Opacity);
        }
    }
}
