using System;
using System.Collections.Generic;
using System.Linq;

namespace Disce.Utils
{
    public class AlgebraUtils
    {
        public static (List<int> numbers, int remainder) SplitUnevenly(int num, int splitcount)
        {
            List<int> Splits = new List<int>();

            for (int i = 1; i < splitcount + 1; i++)
            {
                Splits.Add((int)Math.Round((double)(2 * i * num / (splitcount * (splitcount + 1)))));
            }
            return (Splits, num - Splits.Sum());
        }
        public static float NearestRound(float x, float delX)
        {
            if (delX < 1)
            {
                float i = (float)Math.Floor(x);
                float x2 = i;
                while ((x2 += delX) < x)
                {
                    ;
                }

                float x1 = x2 - delX;
                return Math.Abs(x - x1) < Math.Abs(x - x2) ? x1 : x2;
            }
            else
            {
                return (float)Math.Round(x / delX, MidpointRounding.AwayFromZero) * delX;
            }
        }
    }
}
