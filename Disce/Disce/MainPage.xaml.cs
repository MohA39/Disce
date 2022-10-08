using MediaManager;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Disce
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private static readonly ImageSource IS = ImageSource.FromResource("Disce.Waves.png");
        private static readonly Random RNG = new Random();
        private static bool IsStarted = false;
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            OnPageLoad();
        }

        private void OnPageLoad()
        {
            if (!IsStarted)
            {

                Task Background = new Task(async () =>
                {
                    Application.Current.MainPage = this;
                    double StartHue = RNG.NextDouble();
                    while (true)
                    {
                        for (double i = StartHue; i < 1; i += 1.0 / 360.0)
                        {
                            MainCP.BackgroundColor = Color.FromHsla(i, 0.5, 0.5);
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                TitleShadow.TextColor = Color.FromHsla(i, 0.7, 0.35);
                                Title.TextColor = Color.FromHsla(i, 0.4, 0.7);
                            });
                            StartHue = 0;
                            await Task.Delay(33);
                        }
                    }
                });
                Background.Start();

                CrossMediaManager.Current.RepeatMode = MediaManager.Playback.RepeatMode.All;
                CrossMediaManager.Current.ToggleRepeat();
                Task.Run(async () =>
                {
                    await Task.Delay(500);
                    using (Stream S = GetType().GetTypeInfo().Assembly.GetManifestResourceStream("Disce.MenuMusic.mp3"))
                    {
                        await CrossMediaManager.Current.Play(S, "MenuMusic.mp3");
                        CrossMediaManager.Current.MediaItemFinished += OnMediaFinished;
                    }
                });

                IsStarted = true;
            }
        }

        private void OptionsButton_Clicked(object sender, EventArgs e)
        {
            OptionsPage OP = new OptionsPage
            {
                BackgroundColor = Color.FromHsla(RNG.NextDouble(), 0.5, 0.5)
            };
            Navigation.PushModalAsync(OP, false);
        }

        private void OnMediaFinished(object sender, MediaManager.Media.MediaItemEventArgs e)
        {
            CrossMediaManager.Current.PlayPreviousOrSeekToStart();
        }
        private async void PlayButton_Clicked(object sender, EventArgs e)
        {
            WavesRectangle.WidthRequest = Waves.WidthRequest;
            WavesRectangle.HeightRequest = Application.Current.MainPage.Height * 1.2;

            WaveContainter.InputTransparent = false;
            Waves.Source = IS;

            double StartX = 0;
            double StartY = Application.Current.MainPage.Height - 25;

            await WaveContainter.TranslateTo(StartX, StartY, 0);

            double EndX = -1000;
            double EndY = -70;

            await WaveContainter.TranslateTo(EndX, EndY, 2000, Easing.CubicOut);

            await CrossMediaManager.Current.Stop();
            CrossMediaManager.Current.Queue = null;
            CrossMediaManager.Current.MediaItemFinished -= OnMediaFinished;

            TransititonPage TP = new TransititonPage
            {
                WaveSource = IS,
                BackgroundColor = Color.FromHsla(RNG.NextDouble(), 0.45, 0.55)
            };
            await Navigation.PushModalAsync(new NavigationPage(TP), false);

            WaveContainter.TranslationX = StartX;
            WaveContainter.TranslationY = StartY + 1000;
            WaveContainter.InputTransparent = true;
        }

    }
}
