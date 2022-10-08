using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Disce.Utils
{
    public class Helpers
    {
        private static readonly Random RNG = new Random();
        public static async Task Speak(string Text)
        {

            IEnumerable<Locale> locales = await TextToSpeech.GetLocalesAsync();

            SpeechOptions settings = new SpeechOptions()
            {
                Volume = 1f,
                Pitch = 0.8f,
                Locale = locales.Where(x => x.Language == "en").FirstOrDefault()
            };

            await TextToSpeech.SpeakAsync(Text, settings);

        }
        public static List<int> GenHues(int count)
        {
            List<int> Hues = new List<int>();
            int TotalSpacing = 270;
            int Spacing = TotalSpacing / count;
            Hues.Add(RNG.Next(0, 360 / count));
            for (int i = 1; i != count; i++)
            {
                Hues.Add(RNG.Next(Hues.Last() + Spacing, (i + 1) * (360 / count)));
            }
            return Hues.OrderBy(a => Guid.NewGuid()).ToList();

        }
        public static SKColor RotateHue(SKColor Color, float Rotation)
        {
            Color FormsColor = Color.ToFormsColor();
            float NewHue = Color.Hue + Rotation;
            if (NewHue > 360.0f)
            {
                NewHue -= 360.0f;
            }
            return SKColor.FromHsl(NewHue, (float)FormsColor.Saturation * 100, (float)FormsColor.Luminosity * 100);
        }
        public static int RandExclude(int start, int end, int[] exclude, int? BonusNumber = null, float BonusNumberChance = 0.2f)
        {
            List<int> AllowedNumbers = Enumerable.Range(start, end).Where(i => !exclude.Contains(i)).ToList();

            if (BonusNumber.HasValue && AllowedNumbers.Contains(BonusNumber.Value))
            {
                // (1+x) / (y+x) = z
                // x = (yz - 1)/(-z+1)
                int NumberOfBonusToAdd = (int)Math.Round(((AllowedNumbers.Count * BonusNumberChance) - 1) / (-BonusNumberChance + 1));
                AllowedNumbers.AddRange(Enumerable.Repeat(BonusNumber.Value, NumberOfBonusToAdd));
            }
            return AllowedNumbers[RNG.Next(AllowedNumbers.Count)];
        }
    }
}
