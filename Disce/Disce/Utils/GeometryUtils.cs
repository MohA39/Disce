using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Disce.Utils
{
    public class GeometryUtils
    {
        public static SKSize RotateSize(SKSize size, float angle)
        {
            double AngleRadians = angle * Math.PI / 180;
            double Sine = Math.Abs(Math.Sin(AngleRadians));
            double Cosine = Math.Abs(Math.Cos(AngleRadians));
            return new SKSize((float)((Cosine * size.Width) + (Sine * size.Height)), (float)((Cosine * size.Height) + (Sine * size.Width)));
        }

        public static bool InCircle(float CX, float CY, float Radius, float X, float Y)
        {
            float dx = Math.Abs(X - CX);
            float dy = Math.Abs(Y - CY);
            return (dx * dx) + (dy * dy) <= Radius * Radius;

        }
        public static float sign(SKPoint p1, SKPoint p2, SKPoint p3)
        {
            return ((p1.X - p3.X) * (p2.Y - p3.Y)) - ((p2.X - p3.X) * (p1.Y - p3.Y));
        }

        public static double LineAngle(SKPoint Point1, SKPoint Point2)
        {
            return Math.Atan2(Point1.Y - Point2.Y, Point1.X - Point2.X) * (180 / Math.PI);
        }
        public static bool InTriangle(SKPoint pt, SKPoint v1, SKPoint v2, SKPoint v3)
        {
            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = sign(pt, v1, v2);
            d2 = sign(pt, v2, v3);
            d3 = sign(pt, v3, v1);

            has_neg = d1 < 0 || d2 < 0 || d3 < 0;
            has_pos = d1 > 0 || d2 > 0 || d3 > 0;

            return !(has_neg && has_pos);
        }
        public static bool InTriangle(SKPoint pt, SKPoint[] Triangle)
        {
            return InTriangle(pt, Triangle[0], Triangle[1], Triangle[2]);
        }
        public static SKPoint[] RotateTriangle(SKPoint[] Triangle, SKPoint Center, double Rotation)
        {
            List<SKPoint> PointList = new List<SKPoint>();
            //SKPoint Center = new SKPoint((Triangle[0].X + Triangle[1].X + Triangle[2].X) / 3, (Triangle[0].Y + Triangle[1].Y + Triangle[2].Y) / 3);
            foreach (SKPoint P in Triangle)
            {
                PointList.Add(RotatePoint(P, Center, Rotation));
            }
            return PointList.ToArray();
        }

        private SKPoint RotatePoint(SKPoint pointToRotate, SKPoint centerPoint, double[] angleInDegrees)
        {
            SKPoint CurrentP = pointToRotate;
            foreach (double angle in angleInDegrees)
            {
                CurrentP = RotatePoint(CurrentP, centerPoint, angle);
            }
            return CurrentP;
        }

        public static SKPoint RotatePoint(SKPoint pointToRotate, SKPoint centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new SKPoint
            {
                X =
                    (float)
                    ((cosTheta * (pointToRotate.X - centerPoint.X)) -
                    (sinTheta * (pointToRotate.Y - centerPoint.Y)) + centerPoint.X),
                Y =
                    (float)
                    ((sinTheta * (pointToRotate.X - centerPoint.X)) +
                    (cosTheta * (pointToRotate.Y - centerPoint.Y)) + centerPoint.Y)
            };
        }
        public static SKColorFilter GenerateHueFilter(float Hue)
        {
            // Snippet source: https://blog.gskinner.com/archives/2007/12/colormatrix_cla.html
            float lr = 0.213f;
            float lg = 0.715f;
            float lb = 0.072f;
            float a = 0.143f;
            float b = 0.140f;
            float c = -0.283f;

            Hue = Hue % 360;

            float hueangle = Hue * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(hueangle);
            float sin = (float)Math.Sin(hueangle);

            return SKColorFilter.CreateColorMatrix(new float[] {
            lr + (cos * (1 - lr)) + (sin * -lr), lg + (cos * -lg) + (sin * -lg), lb + (cos * -lb) + (sin * (1 - lb)), 0, 0,
            lr + (cos * -lr) + (sin * a), lg + (cos * (1 - lg)) + (sin * b), lb + (cos * -lb) + (sin * c), 0, 0,
            lr + (cos * -lr) + (sin * -(1 - lr)), lg + (cos * -lg) + (sin * lg), lb + (cos * (1 - lb)) + (sin * lb), 0, 0,
            0, 0, 0, 1, 0,
            });
        }
    }
}
