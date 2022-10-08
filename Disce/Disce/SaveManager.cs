using Xamarin.Essentials;

namespace Disce
{
    public class SaveManager
    {
        public static int WordsLevel = Preferences.Get("WordsLevel", 1);
        public static float WordsProgress = Preferences.Get("WordsProgress", 0.0f);

        public static int AddSubLevel = Preferences.Get("AddSubLevel", 1);
        public static float AddSubProgress = Preferences.Get("AddSubProgress", 0.0f);

        public static int MultiDivLevel = Preferences.Get("MultiDivLevel", 1);
        public static float MultiDivProgress = Preferences.Get("MultiDivProgress", 0.0f);

        public static int CountLevel = Preferences.Get("CountLevel", 1);
        public static float CountProgress = Preferences.Get("CountProgress", 0.0f);
        // To be properly implemented
    }
}
