using System;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Disce
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TransititonPage : ContentPage
    {
        public ImageSource WaveSource = null;

        public TransititonPage()
        {
            InitializeComponent();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            OnPageAppearing();
        }
        private void OnPageAppearing()
        {

            Letters.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => OnContinueClicked(Games.Words)),
            });
            Numbers.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => OnContinueClicked(Games.AddSubNumbers)),
            });
            DivMultiNumbers.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => OnContinueClicked(Games.MultiDivideNumbers)),
            });
            Count.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => OnContinueClicked(Games.CountTheItems)),
            });

            if (WaveSource != null)
            {
                WaveReverse.Source = WaveSource;
            }
            else
            {
                WaveContainter.IsVisible = false;
            }
            Task AppearedEvent = Task.Run(async () =>
            {
                await Task.Delay(250);
                await Device.InvokeOnMainThreadAsync(() =>
                {
                    OnAppeared();
                });
            });
            WavesRectangle.WidthRequest = WaveReverse.WidthRequest;
            WavesRectangle.HeightRequest = Application.Current.MainPage.Height * 1.2;

            Letters.HeightRequest = Application.Current.MainPage.Height / 3;
            Numbers.HeightRequest = Application.Current.MainPage.Height / 3;
            Count.HeightRequest = Application.Current.MainPage.Height / 3;

            DivMultiNumbers.WidthRequest = Application.Current.MainPage.Width / 2;
            Numbers.WidthRequest = Application.Current.MainPage.Width / 2;
        }
        public async void OnAppeared()
        {
            if (WaveSource != null)
            {
                await WaveContainter.TranslateTo(0, Application.Current.MainPage.Height, 1800, Easing.CubicIn);
                WaveContainter.InputTransparent = true;
            }
        }

        private async void OnContinueClicked(Games Game)
        {
            GamePage GP = new GamePage { Game = Game };

            Navigation.InsertPageBefore(GP, this);
            await Navigation.PopAsync().ConfigureAwait(false);
        }

    }
}