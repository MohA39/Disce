using Disce.Objects;
using Disce.Utils;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Disce
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public partial class GamePage : ContentPage
    {
        private enum Screens
        {
            GameScreen,
            CongratulationScreen,
            WheelOfFortuneScreen
        }

        private enum MathSigns
        {
            Addition,
            Subtraction,
            Multiplication,
            Division,
        }

        private enum SpinDirection
        {
            Left,
            Right
        }

        private class Equation
        {
            public int Num1;
            public int Num2;
            public int Answer;
            public MathSigns Sign;
            public int UnansweredID;

            public Equation(int FirstNumber, int SecondNumber, int EqAnswer, MathSigns EqSign, int EqUnansweredID)
            {
                Num1 = FirstNumber;
                Num2 = SecondNumber;
                Answer = EqAnswer;
                Sign = EqSign;
                UnansweredID = EqUnansweredID;
            }
        }

        private class Prize
        {
            public int Value = -1;
            public double Chance = -1;
            public bool IsJackpot = false;
        }
        private class WheelSlice
        {
            public Prize prize;
            public SKColor Color = new SKColor();
        }

        private class FallingCoin
        {
            public Coin Coin;
            public float X;
            public float Y;
            public float Weight;
        };

        // Game configuration
        private const int FPS = 30;

        // Universal
        private static readonly Random RNG = new Random();
        private Color BGColor = Color.FromHsla(RNG.NextDouble(), 0.55, 0.55);
        private SKColor SKBGColor = new SKColor();


        // States
        private bool StartRan = false;
        public Games Game = Games.Unset;
        private Screens CurrentScreen;
        private bool GameLoaded = false;

        // Coins
        private Coin AnimatedCoinIcon;
        private readonly int Coins = Preferences.Get("Coins", 0);
        private SKRect CoinPlacementRectangle;
        private const float MathSignSize = 175;
        private List<Dragable> dragables = new List<Dragable>();
        private int CurrentDragIndex = -1;
        private float SymbolRotation = 0.0f;

        // Transition circle
        private SKColor OverlayColor = SKColor.FromHsl(RNG.Next(0, 360), 75, 50);
        private readonly int TransitionTime = 3000;
        private float CurrentRadius = -1.0f;
        private float RadiusIncrease = -1.0f;
        private bool ShowCircle = false;
        private bool IncreaseOrDecrease = true;
        private bool GrowthStopped = false;
        private Screens TransitioningTo;
        private Action TransitionShrinkCallback = null;

        // Dragable fontsize
        public const float MaxStartFont = 150;
        public const float MaxEndFont = 200;
        public const float FontDivisior = 1.30f;

        // Congratulations
        private float CurrentSigma = 20f;
        private float SigmaEach = -1.0f;
        private SKColor ShadowColor = new SKColor();
        private readonly string[] CongratulationTexts = new string[7] { "Great job!", "Awesome!", "Well done!", "Superb!", "Wonderful!", "Perfect!", "Splendid!" };
        private string CongratsMsg = "-";

        private readonly int ConfettiTime = 10000;
        private readonly List<Confetto> Confetti = new List<Confetto>();
        private bool DisplayConfetti = false;

        // Information
        private SKRect CanvasRect = new SKRect();
        private float CanvasDiagonal = -1;

        //  Touches
        private bool TouchVisible = true;
        private bool IsMovingCube = false;
        private bool IsWheelHeld = false;
        private SKPoint LastTouchPos = new SKPoint();
        private Timer HoldTimer = new Timer();
        private readonly int HoldTime = 1250;

        // Words
        private readonly string Alphabet_Lower = "abcdefghiklmnopqrstvxyz";
        private SKTypeface EnglishFont;
        private static List<List<int>> WordLevelChances = new List<List<int>>();
        private static List<string[]> English_Words = new List<string[]>();
        private readonly List<List<int>> WordIndexes = new List<List<int>>();
        private string CurrentWord = "";
        private readonly List<int> WordHues = Helpers.GenHues(6);
        private readonly List<Tuple<int, char>> CharactersPlaced = new List<Tuple<int, char>>();
        private int WordOpacity = 0xFF;
        private static readonly int MaxWordLevel = English_Words.Count;

        // Count the items
        private Train train;
        private readonly List<TrainCar> trainCars = new List<TrainCar>();
        private const int MaxCountPerLevel = 10;
        private float TrainCarSpacing = 0.0f;
        private int ItemCount = -1;
        private bool IsCountLevelTransitioning = false;
        private const float TrainDefaultSpeed = 6;
        private float TrainSpeed = 6;

        // Shared objects
        private Objects.ProgressBar progressBar;
        private Cube cube;
        private List<Item> Items = new List<Item>();

        // Equations
        private readonly List<Equation> Equations = new List<Equation>();
        private List<int> NumberHues = new List<int>();
        private readonly List<Point> SignLocations = new List<Point>();
        private int CurrentEquation = 0;
        private const int NUMBER_COUNT = 3;
        private int NumberOpacity = 0xFF;
        
        // Addition and subtraction
        private const int AddSubDefaultMinNum = 2;
        private const int AddSubDefaultMaxNum = 12;

        // Multiplication and Division
        private const int MultiDivDefaultMinNum = 2;
        private const int MultiDivDefaultMaxNum = 4;

        // Placement rects
        private readonly List<SKRoundRect> LetterPlacementRect = new List<SKRoundRect>();
        private readonly List<SKRoundRect> NumberPlacementRect = new List<SKRoundRect>();
        private int PRectXOffset = 0;
        private int PRectYOffset = 0;

        // Wheel of fortune
        private static readonly List<Prize> Prizes = new List<Prize>()
        {

            new Prize()
            {
                Value = 10,
                Chance = 2.5
            },
            new Prize()
            {
                Value = 500,
                Chance = 7.5,
                IsJackpot = true
            },
            new Prize()
            {
                Value = 10,
                Chance = 2.5
            },

            new Prize()
            {
                Value = 100,
                Chance = 12.5
            },
            new Prize()
            {
                Value = 50,
                Chance = 12.5
            },
            new Prize()
            {
                Value = 75,
                Chance = 12.5
            },
            new Prize()
            {
                Value = 250,
                Chance = 12.5
            },
            new Prize()
            {
                Value = 75,
                Chance = 12.5
            },
            new Prize()
            {
                Value = 50,
                Chance = 12.5
            },
            new Prize()
            {
                Value = 100,
                Chance = 12.5
            }
        };
        private readonly List<WheelSlice> WheelSlices = new List<WheelSlice>()
        {

            new WheelSlice()
            {
                prize = Prizes[0],
                Color = SKColor.Parse("#333333") // Light black
            },
            // Jackpot
            new WheelSlice()
            {
                prize = Prizes[1],
                Color = SKColors.LightSkyBlue
            },
            new WheelSlice()
            {
                prize = Prizes[2],
                Color = SKColor.Parse("#333333") // Light black
            },
            new WheelSlice()
            {
                prize = Prizes[3],
                Color = SKColor.FromHsl(0, 70, 50)
            },
            new WheelSlice()
            {
                prize = Prizes[4],
                Color = SKColor.FromHsl(50, 70, 50)
            },
            new WheelSlice()
            {
                prize = Prizes[5],
                Color = SKColor.FromHsl(100, 70, 50)
            },
            new WheelSlice()
            {
                prize = Prizes[6],
                Color = SKColor.FromHsl(150, 70, 50)
            },
            new WheelSlice()
            {
                prize = Prizes[7],
                Color = SKColor.FromHsl(200, 70, 50)
            },
            new WheelSlice()
            {
                prize = Prizes[8],
                Color = SKColor.FromHsl(250, 70, 50)
            },
            new WheelSlice()
            {
                prize = Prizes[9],
                Color = SKColor.FromHsl(300, 70, 50)
            }

        };
        private SKColor[] JackpotColors;
        private float CurrentWheelRotation = RNG.Next(0, 360);
        private bool IsWheelSpinning = false;
        private bool CanSpinWheel = true;
        private float WheelDiameter;
        private float WheelRadius;
        private SKPoint WheelLocation;
        private long WheelSpinStart = -1;
        private int WheelSpinDuration = -1;
        private SpinDirection WheelSpinDirection = SpinDirection.Right;
        private Prize PrizeWon = new Prize();

        // Display Wheel Of Fortune winnings
        private Coin WinningsCoin;
        private readonly float WinningsCoinScale = 0.30f;

        // Falling coins animation
        private List<FallingCoin> FallingCoins = new List<FallingCoin>();
        private const int FallingAnimTime = 6000;
        private long FallingCoinsStartUNIX = -1;
        private bool FallingCoinsEnabled = false;
        private bool FallingCoinsStopping = false;

        // Winnings animation
        private bool WinningsAnimEnabled = false;
        private double CurrentWinningsNumber = 0.0f;
        private static readonly int WinningsAnimTime = (int)(FallingAnimTime * .70);
        private double WinningsNumberIncrease = -1;

        // Black overlay
        private bool BlackOverlayEnabled = false;
        private float BlackOverlayOpacity = 0;

        public GamePage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            OnPageLoad();

        }
        protected override bool OnBackButtonPressed()
        {
            OnDisappear();
            return true;
        }
        private async void OnDisappear()
        {
            Navigation.InsertPageBefore(new TransititonPage(), this);
            await Navigation.PopAsync().ConfigureAwait(false);
        }

        private void OnPageLoad()
        {

            Task.Run(() =>
            {
                SKBGColor = BGColor.ToSKColor();

                canvasView.PaintSurface += OnCanvasViewPaintSurface;
                Timer timer = new Timer(1000.0 / FPS);

                timer.Elapsed += (_, __) => Device.BeginInvokeOnMainThread(canvasView.InvalidateSurface);
                timer.Start();
                CurrentScreen = Screens.GameScreen;

                // Load fonts
                using (Stream S = GetType().GetTypeInfo().Assembly.GetManifestResourceStream("Disce.Poppins.ttf"))
                {
                    EnglishFont = SKTypeface.FromStream(S);
                }

                WinningsCoin = new Coin(false, WinningsCoinScale);

                if (Game == Games.Words)
                {
                    English_Words = GenerateEnglishWordLevels();
                    WordLevelChances = GenerateWordLevelChances().ToList();
                    WordIndexes.Add(Enumerable.Range(0, English_Words[0].Length - 1).ToList());
                }

                GameLoaded = true;
                
            });

        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;
            canvas.Clear(SKBGColor);

            if (!GameLoaded)
            {
                return;
            }
            // Start sequence (repeats every level)
            if (!StartRan)
            {

                List<int> Hues = Helpers.GenHues(3);

                JackpotColors = new SKColor[] { SKColor.FromHsl(Hues[0], 100, 50), SKColor.FromHsl(Hues[1], 100, 50), SKColor.FromHsl(Hues[2], 100, 50) };
                CanvasRect = SKRect.Create(info.Width, info.Height);
                AnimatedCoinIcon = new Coin();
                CoinPlacementRectangle = new SKRect(0, info.Height - Coin.MaxSize.Height, Coin.MaxSize.Width, info.Height);

                CanvasDiagonal = (float)Math.Sqrt(Math.Pow(info.Width, 2.0) + Math.Pow(info.Height, 2.0));
                int Area = info.Width * info.Height;
                int AreaPercent = (int)(Area * 0.07); // 7 percent
                cube = new Cube((int)Math.Sqrt(AreaPercent), ref dragables, RNG.Next(-180, 180), RNG.Next(-180, 180), Helpers.GenHues(6));

                WheelDiameter = CanvasRect.Width * 0.9f;
                WheelRadius = WheelDiameter / 2;

                WheelLocation = new SKPoint(CanvasRect.Width / 2, CanvasRect.Height / 2);
                List<string> Symbols = new List<string>();

                progressBar = new Objects.ProgressBar(Game, CanvasRect.Width, CanvasRect.Height * 0.015f);
                if (Game == Games.Words)
                {

                    List<int> WordLevels = GetWordLevels();
                    if (WordIndexes.Count < WordLevels.Count)
                    {

                        for (int i = WordIndexes.Count; i < WordLevelChances[SaveManager.WordsLevel - 1].Count; i++)
                        {
                            WordIndexes.Add(Enumerable.Range(0, English_Words[i].Length).ToList());
                        }
                    }

                    for (int i = 0; i != WordLevels.Count; i++)
                    {
                        if (WordIndexes[i].Count == 0)
                        {
                            WordIndexes[i] = Enumerable.Range(0, English_Words[WordLevels[i] - 1].Length).ToList();
                        }
                    }

                    GenerateWord();

                    for (int i = 0; i != CurrentWord.Length; i++)
                    {
                        CharactersPlaced.Add(new Tuple<int, char>(-1, 'x'));
                    }

                    Symbols.AddRange(CurrentWord.ToLower().ToCharArray().Select(c => c.ToString()).ToArray());

                    while (Symbols.Count != 6)
                    {
                        string RandomCharacter = Alphabet_Lower[RNG.Next(0, Alphabet_Lower.Length)].ToString();
                        while (Symbols.Contains(RandomCharacter))
                        {
                            RandomCharacter = Alphabet_Lower[RNG.Next(0, Alphabet_Lower.Length)].ToString();
                        }
                        Symbols.Add(RandomCharacter);
                    }
                    Symbols[0] = Symbols[0].ToUpper()[0].ToString();
                    Symbols = Symbols.OrderBy(a => Guid.NewGuid()).ToList();

                }

                if (Game == Games.AddSubNumbers)
                {
                    while (Symbols.Count != 6)
                    {
                        MathSigns Sign = RNG.Next(0, 2) == 1 ? MathSigns.Addition : MathSigns.Subtraction;
                        int Num1 = -1;
                        int Num2 = -1;
                        int Answer = -1;
                        if (Sign == MathSigns.Addition)
                        {
                            Num1 = RNG.Next((int)Math.Round((double)(AddSubDefaultMinNum * (1 + ((SaveManager.AddSubLevel - 1) / 2)))), (int)Math.Round((double)(AddSubDefaultMaxNum * (1 + ((SaveManager.AddSubLevel - 1) / 2)))));
                            Num2 = RNG.Next((int)Math.Round((double)(AddSubDefaultMinNum * (1 + ((SaveManager.AddSubLevel - 1) / 2)))), (int)Math.Round((double)(AddSubDefaultMaxNum * (1 + ((SaveManager.AddSubLevel - 1) / 2)))));
                            Answer = Num1 + Num2;
                        }
                        if (Sign == MathSigns.Subtraction)
                        {
                            int RandomNum1 = RNG.Next((int)Math.Round((double)(AddSubDefaultMinNum * (1 + ((SaveManager.AddSubLevel - 1) / 2)))), (int)Math.Round((double)(AddSubDefaultMaxNum * (1 + ((SaveManager.AddSubLevel - 1) / 2)))));
                            int RandomNum2 = RNG.Next((int)Math.Round((double)(AddSubDefaultMinNum * (1 + ((SaveManager.AddSubLevel - 1) / 2)))), (int)Math.Round((double)(AddSubDefaultMaxNum * (1 + ((SaveManager.AddSubLevel - 1) / 2)))));
                            Num1 = Math.Max(RandomNum1, RandomNum2);
                            Num2 = Math.Min(RandomNum1, RandomNum2);
                            Answer = Num1 - Num2;

                        }
                        Equation equation = new Equation(Num1, Num2, Answer, Sign, RNG.Next(0, 3));
                        Equations.Add(equation);
                        Symbols.Add(GetUnspecifiedNumberFromEquation(equation).ToString());
                        Symbols = Symbols.OrderBy(a => Guid.NewGuid()).ToList();
                    }
                }
                if (Game == Games.MultiDivideNumbers)
                {

                    while (Symbols.Count != 6)
                    {
                        MathSigns Sign = RNG.Next(0, 2) == 1 ? MathSigns.Multiplication : MathSigns.Division;
                        int Num1 = -1;
                        int Num2 = -1;
                        int Answer = -1;
                        if (Sign == MathSigns.Multiplication)
                        {
                            Num1 = RNG.Next((int)Math.Round((double)(MultiDivDefaultMinNum * (1 + ((SaveManager.MultiDivLevel - 1) / 2)))), (int)Math.Round((double)(MultiDivDefaultMaxNum * (1 + ((SaveManager.MultiDivLevel - 1) / 2)))));
                            Num2 = RNG.Next((int)Math.Round((double)(MultiDivDefaultMinNum * (1 + ((SaveManager.MultiDivLevel - 1) / 2)))), (int)Math.Round((double)(MultiDivDefaultMaxNum * (1 + ((SaveManager.MultiDivLevel - 1) / 2)))));
                            Answer = Num1 * Num2;
                        }
                        if (Sign == MathSigns.Division)
                        {

                            Num2 = RNG.Next((int)Math.Round((double)(MultiDivDefaultMinNum * (1 + ((SaveManager.MultiDivLevel - 1) / 2)))), (int)Math.Round((double)(MultiDivDefaultMaxNum * (1 + ((SaveManager.MultiDivLevel - 1) / 2)))));
                            Answer = RNG.Next((int)Math.Round((double)(MultiDivDefaultMinNum * (1 + ((SaveManager.MultiDivLevel - 1) / 2)))), (int)Math.Round((double)(MultiDivDefaultMaxNum * (1 + ((SaveManager.MultiDivLevel - 1) / 2)))));
                            Num1 = Num2 * Answer;
                        }
                        Equation Number = new Equation(Num1, Num2, Answer, Sign, RNG.Next(0, 3));
                        Equations.Add(Number);
                        Symbols.Add(GetUnspecifiedNumberFromEquation(Number).ToString());
                        Symbols = Symbols.OrderBy(a => Guid.NewGuid()).ToList();
                    }
                }

                if (Game == Games.Words || Game == Games.AddSubNumbers || Game == Games.MultiDivideNumbers)
                {
                    for (int i = 0; i < 6; i++)
                    {

                        Dragable D = new Dragable(FPS, EnglishFont)
                        {
                            X = -1,
                            Y = -1,
                            CurrentCubeFace = (short)i,
                            Symbol = Symbols[i],

                        };
                        dragables.Add(D);
                    }
                }
                if (Game == Games.Words)
                {

                    int Spacing = (int)(CanvasRect.Width * 0.04);
                    int SpacingTotal = Spacing * (CurrentWord.Length - 1);
                    int LeftRightSpacing = (int)(CanvasRect.Width * 0.06); // 6% of the width 
                    int LineWidth = ((int)CanvasRect.Width - (LeftRightSpacing * 2) - SpacingTotal) / CurrentWord.Length;
                    int CurrentX = LeftRightSpacing;

                    int YPos = cube.Size + (int)(CanvasRect.Width * 0.05);

                    if (LetterPlacementRect.Count != CurrentWord.Length)
                    {
                        LetterPlacementRect.Clear();
                        for (int i = 0; i < CurrentWord.Length; i++)
                        {

                            SKRect Rect = SKRect.Create(CurrentX, YPos, LineWidth, LineWidth);
                            SKRoundRect RoundRect = new SKRoundRect(Rect, LineWidth / 10, LineWidth / 10);
                            LetterPlacementRect.Add(RoundRect);
                            CurrentX += LineWidth + Spacing;
                        }
                    }

                    SKRect ItemImageSpawnRect = new SKRect(CanvasRect.Left, LetterPlacementRect[0].Rect.Bottom, CanvasRect.Right, CoinPlacementRectangle.Top);
                    Enum.TryParse(CurrentWord, true, out ItemNames itemname);
                    Items.Clear();
                    Items.Add(new Item(itemname, ItemImageSpawnRect.MidX, ItemImageSpawnRect.MidY, 1.0f, 0f));
                    Helpers.Speak(CurrentWord);
                }

                if (Game == Games.AddSubNumbers || Game == Games.MultiDivideNumbers)
                {

                    NumberHues = Helpers.GenHues(NUMBER_COUNT);
                    RecalculateNumRects(NUMBER_COUNT);
                }

                if (Game == Games.CountTheItems)
                {

                    // Create train
                    float TrainScale = (CanvasDiagonal / 2240) - 0.10f;

                    train = new Train(TrainScale);
                    float TrainWidth = train.Bounds.Width;
                    float TrainHeight = train.Bounds.Height;
                    train.X = CanvasRect.Right;
                    
                    train.Y = progressBar.Size.Height + (CanvasRect.Height * 0.05f);

                    SKSize TrainCarSize = TrainCar.GetSize();

                    float CarScale = (CanvasDiagonal / 2240) - 0.10f;

                    int CarFitCount = (int)Math.Ceiling(CanvasRect.Width / TrainCarSize.Width) + 1;
                    TrainCarSpacing = CanvasRect.Width * 0.02f;
                    float CurrentX = CanvasRect.Right + TrainWidth + TrainCarSpacing;

                    for (int i = 0; i < CarFitCount; i++)
                    {
                        trainCars.Add(new TrainCar(i * 30, CarScale, RNG.Next(1, SaveManager.CountLevel * MaxCountPerLevel), CurrentX, progressBar.Size.Height + (TrainHeight - TrainCarSize.Height) + (CanvasRect.Height * 0.05f)));
                        CurrentX += TrainCarSize.Width + TrainCarSpacing;
                    }

                    ItemCount = RNG.Next(1, SaveManager.CountLevel * MaxCountPerLevel);
                    Array ItemNamesValues = Enum.GetValues(typeof(ItemNames));
                    Items = GenerateItems((ItemNames)ItemNamesValues.GetValue(RNG.Next(ItemNamesValues.Length)), ItemCount).ToList();

                }

                StartRan = true;

            }

            // Generate Confetti
            if (Confetti.Count == 0)
            {
                int MinWeight = info.Height / ((ConfettiTime - 500) / 1000 * FPS);
                for (int i = 0; i != 200; i++)
                {
                    int Width = info.Width / 50;
                    int Height = Width * 2;
                    Confetto C = new Confetto(new SKPoint(RNG.Next(0, info.Width), 0), new SKSize(Width, Height), RNG.Next(-40, 40), RNG.Next(MinWeight, MinWeight * 2), SKColor.FromHsl(RNG.Next(0, 360), 100, 50));
                    Confetti.Add(C);
                }
                DisplayConfetti = false;

            }

            if (CurrentScreen == Screens.GameScreen)
            {
                canvas.DrawDrawable(progressBar, new SKPoint (0, 0));

                if (Game == Games.Words)
                {

                    for (int i = 0; i != LetterPlacementRect.Count; i++)
                    {
                        SKPaint RectPaint = new SKPaint
                        {
                            Color = SKColor.FromHsl(WordHues[i], 45, 60)
                        };
                        canvas.DrawRoundRect(LetterPlacementRect[i], RectPaint);

                    }

                    canvas.DrawDrawable(Items.First(), new SKPoint(0, 0));
                }
                if (Game == Games.AddSubNumbers || Game == Games.MultiDivideNumbers)
                {
                    for (int i = 0; i < NumberPlacementRect.Count; i++)
                    {
                        SKPaint RectPaint = new SKPaint
                        {
                            Style = SKPaintStyle.Stroke,
                            StrokeWidth = 20,
                            Color = SKColor.FromHsl(NumberHues[i], 45, 60)

                        };

                        SKRect MathNumberRect = new SKRect();
                        switch (i)
                        {

                            case 0:
                                if (Equations.Count != 0)
                                {
                                    if (Equations[CurrentEquation].UnansweredID != 0)
                                    {
                                        int Num = Equations[CurrentEquation].Num1;
                                        SKPaint NumberPaint = new SKPaint
                                        {
                                            TextSize = (float)Math.Round(MaxEndFont / Math.Max(Num.ToString().Length / FontDivisior, 1)),
                                            Color = SKColors.White.WithAlpha((byte)NumberOpacity)
                                        };
                                        NumberPaint.MeasureText(Num.ToString(), ref MathNumberRect);
                                        canvas.DrawText(Num.ToString(), NumberPlacementRect[i].Rect.MidX - MathNumberRect.MidX, NumberPlacementRect[i].Rect.MidY - MathNumberRect.MidY, NumberPaint);
                                    }
                                }
                                break;
                            case 1:
                                if (Equations.Count != 0)
                                {
                                    if (Equations[CurrentEquation].UnansweredID != 1)
                                    {
                                        int Num2 = Equations[CurrentEquation].Num2;
                                        SKPaint NumberPaint = new SKPaint
                                        {
                                            TextSize = (float)Math.Round(MaxEndFont / Math.Max(Num2.ToString().Length / FontDivisior, 1)),
                                            Color = SKColors.White.WithAlpha((byte)NumberOpacity)
                                        };
                                        NumberPaint.MeasureText(Num2.ToString(), ref MathNumberRect);
                                        canvas.DrawText(Num2.ToString(), NumberPlacementRect[i].Rect.MidX - MathNumberRect.MidX, NumberPlacementRect[i].Rect.MidY - MathNumberRect.MidY, NumberPaint);
                                    }
                                }
                                break;
                            case 2:
                                if (Equations.Count != 0)
                                {
                                    if (Equations[CurrentEquation].UnansweredID != 2)
                                    {
                                        int Answer = Equations[CurrentEquation].Answer;
                                        SKPaint NumberPaint = new SKPaint
                                        {
                                            TextSize = (float)Math.Round(MaxEndFont / Math.Max(Answer.ToString().Length / FontDivisior, 1)),
                                            Color = SKColors.White.WithAlpha((byte)NumberOpacity)
                                        };
                                        NumberPaint.MeasureText(Answer.ToString(), ref MathNumberRect);
                                        canvas.DrawText(Answer.ToString(), NumberPlacementRect[i].Rect.MidX - MathNumberRect.MidX, NumberPlacementRect[i].Rect.MidY - MathNumberRect.MidY, NumberPaint);
                                    }
                                }
                                break;

                        }
                        if (Equations[CurrentEquation].UnansweredID != i)
                        {
                            canvas.DrawRoundRect(NumberPlacementRect[i], RectPaint);
                        }
                        else
                        {
                            SKRoundRect SKRR = new SKRoundRect(NumberPlacementRect[i]);
                            SKRR.Offset(PRectXOffset, PRectYOffset);
                            canvas.Save();
                            canvas.RotateDegrees(SymbolRotation);
                            canvas.DrawRoundRect(SKRR, RectPaint);
                            canvas.Restore();
                        }
                    }
                    SKPaint MathSignPaint = new SKPaint
                    {
                        Color = SKColors.White,
                        TextSize = MathSignSize

                    };
                    canvas.DrawText(MathSignToChar(Equations[CurrentEquation].Sign).ToString(), (float)SignLocations[0].X, (float)SignLocations[0].Y, MathSignPaint);
                    canvas.DrawText("=", (float)SignLocations[1].X, (float)SignLocations[1].Y, MathSignPaint);
                }

                // Cube
                if (Game == Games.Words || Game == Games.AddSubNumbers || Game == Games.MultiDivideNumbers)
                {
                    canvas.DrawDrawable(cube, new SKPoint(0, progressBar.Size.Height));
                }

                canvas.DrawDrawable(AnimatedCoinIcon, new SKPoint(CoinPlacementRectangle.MidX, CoinPlacementRectangle.MidY));

                // Draw amount of coins
                SKPaint CoinPaint = new SKPaint
                {
                    Color = SKColors.White,
                    TextSize = 100
                };

                SKRect TextMeasurement = new SKRect();
                CoinPaint.MeasureText(Coins.ToString(), ref TextMeasurement);
                canvas.DrawText(Coins.ToString(), new SKPoint(CoinPlacementRectangle.Width + TextMeasurement.Width, CanvasRect.Height - (TextMeasurement.Height / 2)), CoinPaint);

                if (Game == Games.CountTheItems)
                {
                    canvas.DrawDrawable(train, 0, 0);
                    train.X -= TrainSpeed;

                    for (int i = 0; i < trainCars.Count; i++)
                    {
                        TrainCar TC = trainCars[i];
                        TC.X -= TrainSpeed;

                        float NextHue = trainCars.Max(car => car.Hue) + 30;
                        if (TC.Bounds.Right <= CanvasRect.Left && !IsCountLevelTransitioning)
                        {
                            TC.X = trainCars.Max(car => car.Bounds.Location.X) + TC.Bounds.Width + TrainCarSpacing;
                            TC.Number = Helpers.RandExclude(1, SaveManager.CountLevel * MaxCountPerLevel, trainCars.Select(x => x.Number).ToArray(), ItemCount, 0.20f);
                            TC.Hue = NextHue;
                            TC.AnsweredIncorrectly = false;
                        }

                        canvas.DrawDrawable(TC, 0, 0);

                    }
                    foreach (Item item in Items)
                    {

                        canvas.DrawDrawable(item, new SKPoint(0, 0));
                    }

                }

                // Manage dragables
                for (int i = 0; i != dragables.Count; i++)
                {

                    if (dragables[i].CurrentCubeFace == -1 && !dragables[i].Hidden)
                    {
                        canvas.DrawDrawable(dragables[i], 0, 0);

                        if (dragables[i].IsHeldDown)
                        {

                            SKColor color = Helpers.RotateHue(dragables[i].Color, 2f);
                            if (Game == Games.Words)
                            {

                                SKRect LowerRect = new SKRect();
                                string LowerSymbol = dragables[i].Symbol.ToLower();
                                SKRect UpperRect = new SKRect();
                                string UpperSymbol = dragables[i].Symbol.ToUpper();
                                float Margin = (float)(Math.Sqrt(info.Width * info.Height) * 0.025f);

                                SKPaint LowerPaint = new SKPaint
                                {
                                    TextSize = 175,
                                    Color = color,
                                    Typeface = EnglishFont
                                };

                                LowerPaint.MeasureText(LowerSymbol, ref LowerRect);

                                SKPaint UpperPaint = new SKPaint
                                {
                                    TextSize = 225,
                                    Color = color,
                                    Typeface = EnglishFont
                                };
                                UpperPaint.MeasureText(UpperSymbol, ref UpperRect);

                                float LowerX = info.Width - Margin - LowerRect.Width;
                                float LowerY = Margin + LowerRect.Height + (UpperRect.Height - LowerRect.Height); 
                                canvas.DrawText(LowerSymbol, LowerX, LowerY, LowerPaint);

                                float UpperX = LowerX - Margin - UpperRect.Width;
                                float UpperY = Margin + UpperRect.Height;
                                canvas.DrawText(UpperSymbol, UpperX, UpperY, UpperPaint);
                            }
                            dragables[i].Color = color;
                        }
                    }
                }

            }

            // Wheel of fortune
            if (CurrentScreen == Screens.WheelOfFortuneScreen)
            {

                SKPoint[] PointerTriangle = new SKPoint[]
                {
                    new SKPoint(-CanvasRect.Width * 0.05f, -CanvasRect.Height * 0.025f),
                    new SKPoint(CanvasRect.Width * 0.05f, -CanvasRect.Height * 0.025f),
                    new SKPoint(0, CanvasRect.Height * 0.025f)
                }; // Offset from centroid

                SKPoint PointerCentroid = new SKPoint(WheelLocation.X, WheelLocation.Y - ((PointerTriangle[0].Y + PointerTriangle[1].Y + PointerTriangle[2].Y) / 3) - WheelRadius);

                double CurrentRotation = 0;
                int CurrentTriangleChosen = -1;

                for (int i = 0; i != WheelSlices.Count; i++)
                {
                    canvas.Save();
                    canvas.RotateDegrees(CurrentWheelRotation, WheelLocation.X, WheelLocation.Y);

                    SKColor color = WheelSlices[i].Color;
                    SKPoint[] TriangleVertices = CalcWheelTriangle(CurrentRotation, 360 * (WheelSlices[i].prize.Chance / 100), WheelRadius);

                    if (GeometryUtils.InTriangle(TranslateSKPoint(PointerCentroid, PointerTriangle[2]), canvas.TotalMatrix.MapPoints(TriangleVertices)))
                    {
                        CurrentTriangleChosen = i;
                    }

                    if (!WheelSlices[i].prize.IsJackpot)
                    {
                        canvas.DrawVertices(SKVertexMode.Triangles, TriangleVertices, new SKColor[] { color, color, color }, new SKPaint { Color = color });
                    }
                    else
                    {
                        canvas.DrawVertices(SKVertexMode.Triangles, TriangleVertices, new SKColor[] { JackpotColors[0], JackpotColors[1], JackpotColors[2] }, new SKPaint { });
                        JackpotColors = new SKColor[] { 
                            Helpers.RotateHue(JackpotColors[0], (float)((RNG.NextDouble() * 1.0) + 0.0)), 
                            Helpers.RotateHue(JackpotColors[1], (float)((RNG.NextDouble() * 1.5) + 0.7)), 
                            Helpers.RotateHue(JackpotColors[2], (float)((RNG.NextDouble() * 2.0) + 1.0)) };
                    }

                    canvas.RotateDegrees((float)CurrentRotation, WheelLocation.X, WheelLocation.Y);
                    canvas.RotateDegrees((float)(360 * (WheelSlices[i].prize.Chance / 100)) / 2, WheelLocation.X, WheelLocation.Y);
                    DrawWheelText(canvas, WheelSlices[i].prize.Value.ToString(), WheelRadius);
                    CurrentRotation += 360 * (WheelSlices[i].prize.Chance / 100); 
                    canvas.Restore();
                }

                if (IsWheelSpinning)
                {
                    long TimeSpun = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - WheelSpinStart;
                    if (TimeSpun < WheelSpinDuration)
                    {
                        if (WheelSpinDirection == SpinDirection.Left)
                        {
                            RotateWheel(-10);
                        }
                        if (WheelSpinDirection == SpinDirection.Right)
                        {
                            RotateWheel(10);
                        }
                    }

                    if (WheelSpinDuration - TimeSpun <= 0)
                    {
                        WheelSpinStart = -1;
                        WheelSpinDuration = -1;

                        // Black overlay enable
                        FadeInBlackOverlay(ToggleFallingCoins);

                        WinningsAnimEnabled = true;
                        PrizeWon = WheelSlices[CurrentTriangleChosen].prize;
                        WinningsNumberIncrease = PrizeWon.Value / (double)(WinningsAnimTime / 1000 * FPS);
                        CanSpinWheel = false;
                        IsWheelSpinning = false;
                    }

                }
                canvas.DrawVertices(SKVertexMode.Triangles, new SKPoint[] { 
                    TranslateSKPoint(PointerCentroid, PointerTriangle[0]),
                    TranslateSKPoint(PointerCentroid, PointerTriangle[1]),
                    TranslateSKPoint(PointerCentroid, PointerTriangle[2]) }, 
                new SKColor[] { SKColors.Silver, SKColors.Silver, SKColors.Silver }, new SKPaint { Color = SKColors.Silver });
            }

            if (CurrentScreen == Screens.CongratulationScreen)
            {
                // Congratulations text
                SKRect ShadowSize = new SKRect();

                SKPaint ShadowPaint = new SKPaint
                {
                    Color = ShadowColor,
                    MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Outer, CurrentSigma),
                    FilterQuality = SKFilterQuality.Low,
                    TextSize = 200
                };
                ShadowPaint.MeasureText(CongratsMsg, ref ShadowSize);

                SKRect TextSize = new SKRect();
                SKPaint TextPaint = new SKPaint
                {
                    Color = SKColors.White,
                    TextSize = ShadowPaint.TextSize
                };
                TextPaint.MeasureText(CongratsMsg, ref TextSize);

                canvas.DrawText(CongratsMsg, (info.Width / 2) - ShadowSize.MidX, (info.Height / 2) - ShadowSize.MidY, ShadowPaint);
                canvas.DrawText(CongratsMsg, (info.Width / 2) - ShadowSize.MidX, (info.Height / 2) - ShadowSize.MidY, TextPaint);
                ShadowColor = Helpers.RotateHue(ShadowColor, 10f);
                CurrentSigma += SigmaEach;

                // Confetti

                if (DisplayConfetti)
                {

                    List<int> ToRemove = new List<int>();
                    for (int i = 0; i != Confetti.Count; i++)
                    {
                        if (Confetti[i].Y > (CanvasRect.Bottom * 1.1) + Height)
                        {
                            ToRemove.Add(i);
                        }
                        canvas.DrawDrawable(Confetti[i], 0, 0);
                    }

                    for (int i = ToRemove.Count; i-- > 0;)
                    {
                        Confetti.RemoveAt(ToRemove[i]);

                    }

                }

            }

            if (ShowCircle)
            {
                // Transition Circle
                SKPaint CirclePaint = new SKPaint
                {
                    Color = OverlayColor
                };
                canvas.DrawCircle(0, info.Height, CurrentRadius, CirclePaint);
                if (IncreaseOrDecrease && !GrowthStopped)
                {

                    if (CurrentRadius < CanvasDiagonal && CurrentRadius < CanvasDiagonal)
                    {
                        CurrentRadius += RadiusIncrease;
                    }
                    else
                    {

                        if (Game == Games.Words)
                        {
                            ClearGameWords();
                        }
                        if (Game == Games.AddSubNumbers || Game == Games.MultiDivideNumbers)
                        {

                            NumberOpacity = 0xFF;
                            BGColor = Color.FromHsla(RNG.NextDouble(), 0.55, 0.55);
                            SKBGColor = BGColor.ToSKColor();

                        }
                        GrowthStopped = true;
                        IncreaseOrDecrease = false;
                        if (TransitionShrinkCallback != null)
                        {
                            TransitionShrinkCallback();
                        }

                        CurrentScreen = TransitioningTo;
                    }
                }
                if (!IncreaseOrDecrease)
                {
                    if (CurrentRadius > 0)
                    {
                        CurrentRadius -= RadiusIncrease;
                    }
                    else
                    {

                        ShowCircle = false;

                        TouchVisible = true;
                    }
                }

            }

            // Black overlay
            if (BlackOverlayEnabled)
            {
                canvas.DrawRect(0, 0, CanvasRect.Width, CanvasRect.Height, new SKPaint { Color = SKColors.Black.WithAlpha((byte)(255 * BlackOverlayOpacity / 100)) });
            }

            // Falling coin animation & Winnings text
            if (FallingCoinsEnabled)
            {
                // Draw winnings
                SKPaint WinningsTextPaint = new SKPaint
                {
                    Color = SKColors.White,
                    TextSize = 125
                };

                SKRect WinningTextRect = new SKRect();
                WinningsTextPaint.MeasureText(Coins.ToString(), ref WinningTextRect);

                canvas.DrawText(Math.Floor(CurrentWinningsNumber).ToString(), new SKPoint(CanvasRect.MidX + WinningTextRect.MidX, CanvasRect.MidY - WinningTextRect.MidY), WinningsTextPaint);
                canvas.DrawDrawable(WinningsCoin, CanvasRect.MidX - WinningTextRect.Width, CanvasRect.MidY);
                int OffscreenCount = 0;
                foreach (FallingCoin C in FallingCoins)
                {
                    canvas.DrawDrawable(C.Coin, C.X, C.Y);
                    C.Y += C.Weight;
                    if (C.Y > CanvasRect.Height + (C.Coin.Bounds.Height * C.Coin.Scale))
                    {
                        if (!FallingCoinsStopping)
                        {
                            C.Y = -C.Coin.Bounds.Height * 2;
                        }
                        else
                        {
                            OffscreenCount++;
                        }
                    }

                }

                if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - FallingCoinsStartUNIX > FallingAnimTime - 300 && !FallingCoinsStopping)
                {
                    FallingCoinsStopping = true;
                }
                if (OffscreenCount == FallingCoins.Count && FallingCoinsStopping)
                {
                    FadeOutBlackOverlay();
                    ToggleFallingCoins();
                    TransitionToScreen(Screens.GameScreen);
                }
            }

            if (WinningsAnimEnabled)
            {
                if (CurrentWinningsNumber < PrizeWon.Value)
                {
                    CurrentWinningsNumber += WinningsNumberIncrease;
                }
                else
                {
                    WinningsNumberIncrease = 0;
                    WinningsAnimEnabled = false;
                    PrizeWon = new Prize();
                    
                }
            }

        }
        private void canvasView_Touch(object sender, SKTouchEventArgs e)
        {
            if (TouchVisible)
            {
                SKPoint point = e.Location;
                SKRect CubeRect = new SKRect(0, 0, cube.Size, cube.Size);

                switch (e.ActionType)
                {
                    case SKTouchAction.Pressed:
                        if (CurrentScreen == Screens.GameScreen)
                        {
                            if (Game == Games.MultiDivideNumbers || Game == Games.AddSubNumbers || Game == Games.Words)
                            {
                                if (CubeRect.Contains(point))
                                {
                                    IsMovingCube = true;
                                }
                                for (int i = 0; i != dragables.Count; i++)
                                {
                                    Dragable D = dragables[i];
                                    if (D.IsVisible)
                                    {

                                        if (D.SymbolRect.Contains(point) && !D.TouchHidden)
                                        {
                                            CurrentDragIndex = i;
                                            dragables[i].IsTouched = true;

                                            long TIMECOUNT = DateTime.Now.Ticks;
                                            HoldTimer = new Timer(HoldTime); // Hold time
                                            HoldTimer.Elapsed += (_, __) =>
                                            {
                                                if (dragables[CurrentDragIndex].IsTouched)
                                                {
                                                    dragables[CurrentDragIndex].CurrentCubeFace = -1;
                                                    dragables[CurrentDragIndex].IsTouched = false;
                                                    dragables[CurrentDragIndex].IsHeldDown = true;

                                                    dragables[CurrentDragIndex].X = point.X;
                                                    dragables[CurrentDragIndex].Y = point.Y;

                                                    dragables[CurrentDragIndex].Color = SKColor.FromHsl(RNG.Next(0, 360), 50, 35);

                                                    for (int i2 = 0; i != CharactersPlaced.Count; i2++)
                                                    {
                                                        if (CharactersPlaced[i2].Item1 == CurrentDragIndex)
                                                        {
                                                            CharactersPlaced[i2] = new Tuple<int, char>(-1, 'x');
                                                        }
                                                    }
                                                    Helpers.Speak(dragables[CurrentDragIndex].Symbol);
                                                }
                                            };
                                            HoldTimer.AutoReset = false;
                                            HoldTimer.Start();
                                        }
                                    }
                                }
                            }

                        }

                        if (CurrentScreen == Screens.WheelOfFortuneScreen)
                        {

                            if (GeometryUtils.InCircle(WheelLocation.X, WheelLocation.Y, WheelRadius, point.X, point.Y) && CanSpinWheel)
                            {
                                IsWheelHeld = true;
                            }
                        }
                        break;

                    case SKTouchAction.Moved:
                        if (IsMovingCube && (CurrentDragIndex == -1 ? true : !dragables[CurrentDragIndex].IsHeldDown))
                        {

                            if (CubeRect.Contains(point) && CurrentScreen == Screens.GameScreen)
                            {
                                int NewXRotation = cube.XRotation + (int)(LastTouchPos.Y - point.Y);
                                if (NewXRotation > 180)
                                {
                                    NewXRotation = -Math.Abs(180 - (NewXRotation - 180));
                                }
                                if (NewXRotation < -180)
                                {
                                    NewXRotation = 180 - Math.Abs(NewXRotation) + 180;
                                }

                                int NewYRotation = -1;

                                if (NewXRotation <= 90 && NewXRotation >= -90)
                                {
                                    NewYRotation = cube.YRotation - (int)(LastTouchPos.X - point.X);
                                }
                                else
                                {
                                    NewYRotation = cube.YRotation + (int)(LastTouchPos.X - point.X);
                                }
                                if (NewYRotation > 180)
                                {
                                    NewYRotation = -Math.Abs(180 - (NewYRotation - 180));
                                }
                                if (NewYRotation < -180)
                                {
                                    NewYRotation = 180 - Math.Abs(NewYRotation) + 180;
                                }
                                cube.XRotation = NewXRotation;
                                cube.YRotation = NewYRotation;
                            }
                        }
                        if (CurrentDragIndex != -1)
                        {
                            if (dragables[CurrentDragIndex].IsHeldDown)
                            {

                                dragables[CurrentDragIndex].X = point.X;
                                dragables[CurrentDragIndex].Y = point.Y;

                                SKRect TextSizeRect = new SKRect();
                                SKPaint MeasurePaint = new SKPaint
                                {
                                    TextSize = dragables[CurrentDragIndex].FontSize
                                };
                                MeasurePaint.MeasureText(dragables[CurrentDragIndex].Symbol, ref TextSizeRect);
                                dragables[CurrentDragIndex].SymbolRect = SKRect.Create(point.X - (TextSizeRect.Width / 2), point.Y - (TextSizeRect.Height / 2), TextSizeRect.Width, TextSizeRect.Height);
                            }
                            else
                            {
                                dragables[CurrentDragIndex].FontSize = (int)Math.Round(Dragable.MaxStartFont / Math.Max(dragables[CurrentDragIndex].Symbol.Length / Dragable.FontDivisior, 1));
                                dragables[CurrentDragIndex].CurrentCubeFace = (short)CurrentDragIndex;
                                dragables[CurrentDragIndex].IsTouched = false;
                                dragables[CurrentDragIndex].IsHeldDown = false;
                            }
                        }
                        if (IsWheelHeld && !IsWheelSpinning)
                        {
                            double OldAngle = GeometryUtils.LineAngle(LastTouchPos, WheelLocation);
                            double NewAngle = GeometryUtils.LineAngle(point, WheelLocation);
                            float Rotation = (float)(NewAngle - OldAngle);
                            if (Rotation < 0)
                            {
                                WheelSpinDirection = SpinDirection.Left;
                            }
                            else
                            {
                                WheelSpinDirection = SpinDirection.Right;
                            }
                            RotateWheel(Rotation);

                        }
                        break;
                    case SKTouchAction.Released:
                        if (CurrentScreen == Screens.CongratulationScreen && !ShowCircle)
                        {
                            TransitionToScreen(Screens.WheelOfFortuneScreen);
                        }
                        if (Game == Games.CountTheItems)
                        {
                            foreach (TrainCar tc in trainCars)
                            {
                                if (tc.Bounds.Contains(point) && !tc.AnsweredIncorrectly && !IsCountLevelTransitioning)
                                {
                                    if (tc.Number == ItemCount) // Correct Answer
                                    {
                                        Task.Run(async () =>
                                        {
                                            IsCountLevelTransitioning = true;

                                            await Task.Delay(500);
                                            TrainSpeed = TrainDefaultSpeed * 4;
                                            int FadeOutTime = 750;
                                            int Steps = 255; // 0.3% each

                                            double OldHue = BGColor.Hue;
                                            Color NewBGColor = Color.FromHsla(RNG.NextDouble(), 0.55, 0.55);
                                            double HueIncreaseEach = (NewBGColor.Hue - OldHue) / (Steps * 2);

                                            for (int i = 0; i != Steps; i++)
                                            {
                                                BGColor = Color.FromHsla(BGColor.Hue + HueIncreaseEach, 0.55, 0.55);
                                                SKBGColor = BGColor.ToSKColor();

                                                SaveManager.CountProgress += 1.0f / 12.0f / Steps;
                                                
                                                await Task.Delay(FadeOutTime / Steps);
                                            }
                                            Preferences.Set("SaveManager.CountProgress", SaveManager.CountProgress);
                                            CongratulateIfApplicable(Game);

                                            while (trainCars.Max(x => x.Bounds.Right) > CanvasRect.Left)
                                            {
                                                await Task.Delay(100);
                                            }
                                            TrainSpeed = TrainDefaultSpeed;
                                            trainCars.Clear();
                                            StartRan = false;

                                            IsCountLevelTransitioning = false;
                                        });

                                    }
                                    else
                                    {
                                        Task.Run(async () =>
                                        {
                                            int Steps = 25;
                                            int Time = 1000;
                                            for (int i = 0; i != Steps; i++)
                                            {
                                                SaveManager.CountProgress = Math.Max(SaveManager.CountProgress - (1.0f / 12.0f / 3.0f / Steps), 0.0f);
                                                await Task.Delay(Time / Steps);
                                            }

                                            Preferences.Set("SaveManager.CountProgress", SaveManager.CountProgress);

                                        });
                                        tc.AnsweredIncorrectly = true;
                                    }
                                }

                            }
                        }
                        if (CurrentDragIndex != -1 && Game != Games.CountTheItems)
                        {
                            int LastDragIndex = CurrentDragIndex;
                            if (CubeRect.Contains(point))
                            {
                                dragables[CurrentDragIndex].FontSize = (int)Math.Round(Dragable.MaxStartFont / Math.Max(dragables[CurrentDragIndex].Symbol.Length / Dragable.FontDivisior, 1));
                                dragables[CurrentDragIndex].CurrentCubeFace = (short)CurrentDragIndex;
                            }
                            if (Game == Games.Words)
                            {
                                int IntersectingWithSpaceIndex = BoxIDIntersect(dragables[CurrentDragIndex].SymbolRect);
                                if (IntersectingWithSpaceIndex != -1)
                                {
                                    SKPaint tp = new SKPaint
                                    {
                                        TextSize = dragables[CurrentDragIndex].FontSize
                                    };
                                    SKRect TextSize = new SKRect();
                                    tp.MeasureText(dragables[CurrentDragIndex].Symbol, ref TextSize);
                                    if (CharactersPlaced[IntersectingWithSpaceIndex].Item1 != -1)
                                    {
                                        int index = CharactersPlaced[IntersectingWithSpaceIndex].Item1;
                                        dragables[index].CurrentCubeFace = (short)index;
                                        dragables[index].FontSize = (int)Math.Round(Dragable.MaxStartFont / Math.Max(dragables[CurrentDragIndex].Symbol.Length / Dragable.FontDivisior, 1));
                                    }
                                    SKRect rect = LetterPlacementRect[IntersectingWithSpaceIndex].Rect;
                                    dragables[CurrentDragIndex].X = rect.MidX;
                                    dragables[CurrentDragIndex].Y = rect.MidY;
                                    dragables[CurrentDragIndex].SymbolRect = SKRect.Create(rect.MidX - (TextSize.Width / 2), rect.MidY - (TextSize.Height / 2), TextSize.Width, TextSize.Height);
                                    CharactersPlaced[IntersectingWithSpaceIndex] = new Tuple<int, char>(CurrentDragIndex, dragables[CurrentDragIndex].Symbol[0]);
                                    if (IsPlacedCorrectly())
                                    {
                                        Task.Run(async () =>
                                        {
                                            await Task.Delay(1500);
                                            int FadeOutTime = 750;
                                            int Steps = 255; // 0.3% each

                                            double OldHue = BGColor.Hue;
                                            Color NewBGColor = Color.FromHsla(RNG.NextDouble(), 0.55, 0.55);
                                            double HueIncreaseEach = (NewBGColor.Hue - OldHue) / (Steps * 2);

                                            for (int i = 0; i != Steps; i++)
                                            {
                                                BGColor = Color.FromHsla(BGColor.Hue + HueIncreaseEach, 0.55, 0.55);
                                                SKBGColor = BGColor.ToSKColor();
                                                WordOpacity -= 0xFF / Steps;
                                                SetAllDragableOpacity(WordOpacity);
                                                SaveManager.WordsProgress += 1.0f / 12.0f / Steps;
                                                Preferences.Set("SaveManager.WordsProgress", SaveManager.WordsProgress);
                                                await Task.Delay(FadeOutTime / Steps);
                                            }
                                            ClearGameWords();
                                            CongratulateIfApplicable(Game);
                                            for (int i = 0; i != Steps; i++)
                                            {
                                                WordOpacity += 0xFF / Steps;
                                                SetAllDragableOpacity(WordOpacity);
                                                await Task.Delay(FadeOutTime / Steps);
                                            }
                                        });
                                    }

                                }
                            }
                            if (Game == Games.AddSubNumbers || Game == Games.MultiDivideNumbers)
                            {
                                int index = Equations[CurrentEquation].UnansweredID;
                                if (NumberPlacementRect[index].Rect.IntersectsWith(dragables[CurrentDragIndex].SymbolRect))
                                {

                                    SKPaint tp = new SKPaint
                                    {
                                        TextSize = dragables[CurrentDragIndex].FontSize
                                    };
                                    SKRect TextSize = new SKRect();
                                    tp.MeasureText(dragables[CurrentDragIndex].Symbol, ref TextSize);

                                    SKRect rect = NumberPlacementRect[index].Rect;
                                    dragables[CurrentDragIndex].X = rect.MidX;
                                    dragables[CurrentDragIndex].Y = rect.MidY;
                                    dragables[CurrentDragIndex].SymbolRect = SKRect.Create(rect.MidX - (TextSize.Width / 2), rect.MidY - (TextSize.Height / 2), TextSize.Width, TextSize.Height);

                                    if (dragables[CurrentDragIndex].Symbol == GetUnspecifiedNumberFromEquation(Equations[CurrentEquation]).ToString())
                                    {

                                        dragables[LastDragIndex].SymbolRect = new SKRect();
                                        Task.Run(async () =>
                                        {
                                            await Task.Delay(1500);
                                            int FadeOutTime = 750;
                                            int Steps = 255; // 0.3% each
                                            float StartProgress = Game == Games.AddSubNumbers ? SaveManager.AddSubProgress : SaveManager.MultiDivProgress;
                                            float EndProgress = Game == Games.AddSubNumbers ? SaveManager.AddSubProgress + (1.0f / 12.0f) : SaveManager.MultiDivProgress + (1.0f / 12.0f);

                                            double OldHue = BGColor.Hue;
                                            Color NewBGColor = Color.FromHsla(RNG.NextDouble(), 0.55, 0.55);
                                            double HueIncreaseEach = (NewBGColor.Hue - OldHue) / (Steps * 2);

                                            for (int i = 0; i != Steps; i++)
                                            {
                                                NumberOpacity -= 0xFF / Steps;
                                                BGColor = Color.FromHsla(BGColor.Hue + HueIncreaseEach, 0.55, 0.55);
                                                SKBGColor = BGColor.ToSKColor();
                                                switch (Game)
                                                {
                                                    case Games.AddSubNumbers:
                                                        SaveManager.AddSubProgress += (EndProgress - StartProgress) / Steps;

                                                        break;
                                                    case Games.MultiDivideNumbers:
                                                        SaveManager.MultiDivProgress += (EndProgress - StartProgress) / Steps;

                                                        break;
                                                }

                                                await Task.Delay(FadeOutTime / Steps);
                                            }
                                            switch (Game)
                                            {
                                                case Games.AddSubNumbers:
                                                    Preferences.Set("SaveManager.AddSubProgress", SaveManager.AddSubProgress);
                                                    break;
                                                case Games.MultiDivideNumbers:
                                                    Preferences.Set("SaveManager.MultiDivProgress", SaveManager.MultiDivProgress);
                                                    break;
                                            }
                                            CongratulateIfApplicable(Game);
                                            if (CurrentEquation + 1 == 6 && EndProgress < 1.0f)
                                            {
                                                SetAllDragableOpacity(0x00);
                                            }
                                            dragables[LastDragIndex].Hidden = true;
                                            CurrentEquation++;

                                            if (CurrentEquation == 6)
                                            {

                                                ClearGameNumbers();
                                                CurrentEquation = 0;
                                            }

                                            if (Equations.Count != 0)
                                            {
                                                RecalculateNumRects(NUMBER_COUNT);
                                            }

                                            for (int i = 0; i != dragables.Count; i++)
                                            {
                                                dragables[i].Color = SKColors.Black;
                                                dragables[i].TouchHidden = false;
                                            }
                                            for (int i = 0; i != Steps; i++)
                                            {

                                                if (WordOpacity < 255)
                                                {
                                                    WordOpacity += 0xFF / Steps;

                                                    SetAllDragableOpacity(WordOpacity);
                                                }

                                                NumberOpacity += 0xFF / Steps;
                                                await Task.Delay(FadeOutTime / Steps);
                                            }

                                        });
                                    }
                                    else
                                    {
                                        Task.Run(async () =>
                                        {
                                            int Steps = 25;
                                            int Time = 1000;
                                            for (int i = 0; i != Steps; i++)
                                            {
                                                switch (Game)
                                                {
                                                    case Games.AddSubNumbers:
                                                        SaveManager.AddSubProgress = Math.Max(SaveManager.AddSubProgress - (1.0f / 12.0f / 3.0f / Steps), 0.0f);
                                                        break;
                                                    case Games.MultiDivideNumbers:
                                                        SaveManager.MultiDivProgress = Math.Max(SaveManager.MultiDivProgress - (1.0f / 12.0f / 3.0f / Steps), 0.0f);
                                                        break;
                                                }

                                                await Task.Delay(Time / Steps);
                                            }
                                            switch (Game)
                                            {
                                                case Games.AddSubNumbers:
                                                    Preferences.Set("SaveManager.AddSubProgress", SaveManager.AddSubProgress);
                                                    break;
                                                case Games.MultiDivideNumbers:
                                                    Preferences.Set("SaveManager.MultiDivProgress", SaveManager.MultiDivProgress);
                                                    break;
                                            }

                                        });
                                        Task.Run(async () =>
                                        {
                                            long StartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                                            dragables[LastDragIndex].Color = SKColors.Red;
                                            List<(int TranslationX, int TranslationY, float Rotation)> Transformations = new List<(int TranslationX, int TranslationY, float Rotation)>
                                            {
                                            (2, 1, 0),
                                            (-1, -2, -1),
                                            (-3, 0, 1),
                                            (0, 2, 0),
                                            (1, -1, 1),
                                            (-1, 2, -1),
                                            (-3, 1, 0),
                                            (2, 1, -1),
                                            (-1, -1, 1),
                                            (2, 2, 0),
                                            (1, -2, -1)
                                            };
                                            int i = 0;
                                            while (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - StartTime < 1000)
                                            {
                                                if (i == Transformations.Count)
                                                {
                                                    i = 0;
                                                }
                                                dragables[LastDragIndex].XOffset = Transformations[i].TranslationX;
                                                dragables[LastDragIndex].YOffset = Transformations[i].TranslationY;
                                                dragables[LastDragIndex].Rotation = Transformations[i].Rotation;
                                                PRectXOffset = Transformations[i].TranslationX;
                                                PRectYOffset = Transformations[i].TranslationY;
                                                SymbolRotation = Transformations[i].Rotation;
                                                i++;
                                                await Task.Delay(1000 / Transformations.Count);

                                            }
                                            dragables[LastDragIndex].CurrentCubeFace = (short)LastDragIndex;
                                            dragables[LastDragIndex].FontSize = (float)Math.Round(Dragable.MaxStartFont / Math.Max(dragables[LastDragIndex].Symbol.Length / Dragable.FontDivisior, 1));
                                            dragables[LastDragIndex].TouchHidden = true;
                                            dragables[LastDragIndex].XOffset = 0;
                                            dragables[LastDragIndex].YOffset = 0;
                                            dragables[LastDragIndex].Rotation = 0;
                                            PRectXOffset = 0;
                                            PRectYOffset = 0;
                                            SymbolRotation = 0;
                                        });
                                    }
                                }
                            }

                            dragables[CurrentDragIndex].IsTouched = false;
                            dragables[CurrentDragIndex].IsHeldDown = false;
                            CurrentDragIndex = -1;
                            HoldTimer.Stop();
                        }
                        if (CurrentScreen == Screens.WheelOfFortuneScreen)
                        {
                            if (IsWheelHeld && !IsWheelSpinning)
                            {
                                WheelSpinStart = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                                WheelSpinDuration = RNG.Next(3000, 5000);
                                IsWheelSpinning = true;
                            }
                        }

                        IsWheelHeld = false;
                        break;
                }
                LastTouchPos = point;
            }
            e.Handled = true;
        }

        private List<FallingCoin> GenFallingCoins(int Count = 35, int PhaseCount = 5)
        {
            List<FallingCoin> Result = new List<FallingCoin>();

            for (int i = 0; i != Count; i++)
            {
                Coin coin = new Coin(false, RNG.Next(20, 40) / 100.0f);
                SKRect CoinPosRect = new SKRect(0, -coin.Bounds.Height * 2, CanvasRect.Width, 0);
                int MinWeight = ((int)CoinPosRect.Height + (int)CanvasRect.Height) / (FallingAnimTime / PhaseCount / 1000 * FPS);

                Result.Add(new FallingCoin
                {
                    Coin = coin,
                    Weight = RNG.Next(MinWeight, MinWeight * 2),
                    X = RNG.Next((int)CoinPosRect.Left, (int)CoinPosRect.Right),
                    Y = RNG.Next((int)CoinPosRect.Top, (int)CoinPosRect.Bottom)
                });
            }

            return Result;
        }

        private void ToggleFallingCoins()
        {
            if (FallingCoinsEnabled)
            {
                FallingCoinsEnabled = false;
                FallingCoinsStartUNIX = -1;
                FallingCoins.Clear();
            }
            else
            {
                FallingCoins = GenFallingCoins();
                FallingCoinsStartUNIX = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                FallingCoinsEnabled = true;
                FallingCoinsStopping = false;
            }
        }

        private void FadeInBlackOverlay(Action Callback = null)
        {
            BlackOverlayEnabled = true;
            Task.Run(async () =>
            {
                int Smoothness = 50;
                int ResultOpacity = 50;
                int Time = 1500;
                for (int i = 0; i != Smoothness; i++)
                {
                    BlackOverlayOpacity += ResultOpacity / Smoothness;
                    await Task.Delay(Time / Smoothness);
                }
                if (Callback != null)
                {
                    Callback.Invoke();
                }
            });
        }
        private void FadeOutBlackOverlay(Action Callback = null)
        {
            Task.Run(async () =>
            {
                int Smoothness = 50;
                float StartOpacity = BlackOverlayOpacity;
                int ResultOpacity = 0;
                int Time = 1500;
                for (int i = 0; i != Smoothness; i++)
                {
                    BlackOverlayOpacity -= (StartOpacity - ResultOpacity) / Smoothness;
                    await Task.Delay(Time / Smoothness);
                }
                if (Callback != null)
                {
                    Callback.Invoke();
                }
                BlackOverlayOpacity = 0;
                BlackOverlayEnabled = false;
            });
        }
        private SKPoint[] CalcWheelTriangle(double Rotation, double WidthDegrees, float Height)
        {
            SKPoint Center = new SKPoint(WheelLocation.X, WheelLocation.Y);
            SKPoint Left = new SKPoint(Center.X, Center.Y - Height);
            SKPoint[] Vertices = GeometryUtils.RotateTriangle(new SKPoint[]
            {
                Center, // bottom
                Left, // left
                GeometryUtils.RotatePoint(Left, Center, WidthDegrees) // right
            }, new SKPoint(WheelLocation.X, WheelLocation.Y), Rotation);

            return Vertices;

        }
        private void DrawWheelText(SKCanvas canvas, string text, float Height)
        {
            SKPaint TextPaint = new SKPaint
            {
                TextSize = 45,
                Color = SKColors.White,
                Typeface = SKTypeface.FromFamilyName(
    "Arial",
    SKFontStyleWeight.Bold,
    SKFontStyleWidth.Normal,
    SKFontStyleSlant.Upright),
                IsLinearText = false
            };
            SKRect TextSize = new SKRect();

            TextPaint.MeasureText(text, ref TextSize);
            SKPoint TextPosition = new SKPoint(WheelLocation.X + TextSize.MidY, WheelLocation.Y - (Height * 0.9f) + (TextSize.Right / 2));
            canvas.Save();
            canvas.RotateDegrees(90, TextPosition.X, TextPosition.Y);
            canvas.DrawText(text, new SKPoint(TextPosition.X, TextPosition.Y), TextPaint);
            canvas.Restore();
        }

        private void SetAllDragableOpacity(int Opacity)
        {
            foreach (Dragable d in dragables)
            {
                d.Opacity = Opacity;
            }
        }
        private SKPoint TranslateSKPoint(SKPoint P, SKPoint Offset)
        {
            return new SKPoint(P.X + Offset.X, P.Y + Offset.Y);
        }

        private void CongratulatePlayer()
        {
            TransitionToScreen(Screens.CongratulationScreen, () => { DisplayConfetti = true; });
            CongratsMsg = CongratulationTexts[RNG.Next(0, CongratulationTexts.Length - 1)];
            ShadowColor = SKColor.FromHsl(RNG.Next(0, 360), 75, 30);
            SigmaEach = 0.0f;
            TouchVisible = false;
        }

        private void TransitionToScreen(Screens screen, Action transitionShrinkCallback = null)
        {
            TransitioningTo = screen;
            IncreaseOrDecrease = true;
            GrowthStopped = false;
            OverlayColor = SKColor.FromHsl(RNG.Next(0, 360), 100, 50);
            CurrentRadius = (float)Math.Sqrt(CanvasRect.Width * CanvasRect.Height * 0.01);
            RadiusIncrease = Math.Max(CanvasRect.Width, CanvasRect.Height) / (TransitionTime / 1000 * FPS);
            TransitionShrinkCallback = transitionShrinkCallback;
            ShowCircle = true;
        }

        private void RotateWheel(float Rotation)
        {
            float NewRotation = CurrentWheelRotation + Rotation;
            if (NewRotation > 360.0f)
            {
                NewRotation -= 360.0f;
            }
            if (NewRotation < 0.0f)
            {
                NewRotation += 360.0f;
            }
            CurrentWheelRotation = NewRotation;
        }

        private void ClearGameWords()
        {
            dragables.Clear();

            CharactersPlaced.Clear();
            StartRan = false;
        }


        private void ClearGameNumbers()
        {
            dragables.Clear();
            Equations.Clear();
            StartRan = false;
        }

        private int BoxIDIntersect(SKRect rect)
        {
            for (int i = 0; i != LetterPlacementRect.Count; i++)
            {
                if (rect.IntersectsWith(LetterPlacementRect[i].Rect))
                {
                    return i;
                }
            }
            return -1;
        }
        private int GetUnspecifiedNumberFromEquation(Equation equation)
        {
            switch (equation.UnansweredID)
            {
                case 0:
                    return equation.Num1;

                case 1:
                    return equation.Num2;

                case 2:
                    return equation.Answer;

            }
            throw new Exception();
        }
        private void RecalculateNumRects(int NUMBER_COUNT)
        {
            NumberPlacementRect.Clear();
            SignLocations.Clear();
            int Spacing = (int)(CanvasRect.Width * 0.03);
            int SpacingTotal = Spacing * (NUMBER_COUNT - 1);
            int LeftRightSpacing = (int)(CanvasRect.Width * 0.08); // 8% of the width 

            int CurrentX = LeftRightSpacing;
            int YPos = cube.Size + (int)(CanvasRect.Width * 0.05);
            int SW = (int)(CanvasRect.Height * 0.1);

            SKPaint MathSignMeasurePaint = new SKPaint
            {
                TextSize = MathSignSize
            };

            SKRect Sign1Rect = new SKRect();
            MathSignMeasurePaint.MeasureText(MathSignToChar(Equations[CurrentEquation].Sign).ToString(), ref Sign1Rect);

            SKRect Sign2Rect = new SKRect();
            MathSignMeasurePaint.MeasureText("=", ref Sign2Rect);
            int[] ExtraSymbolSpacing = new int[] { (int)Sign1Rect.Width + Spacing, (int)Sign2Rect.Width + Spacing, 0 };

            int LineWidth = ((int)CanvasRect.Width - (LeftRightSpacing * 2) - SpacingTotal - ExtraSymbolSpacing.Sum()) / NUMBER_COUNT;

            SKRect[] SignRects = new SKRect[] { Sign1Rect, Sign2Rect };

            List<SKRect> RectsDrawn = new List<SKRect>();
            for (int i = 0; i < NUMBER_COUNT; i++)
            {

                SKRect Rect = SKRect.Create(CurrentX, YPos, LineWidth, SW);
                SKRoundRect RoundRect = new SKRoundRect(Rect, LineWidth / 10, SW / 10);
                NumberPlacementRect.Add(RoundRect);
                CurrentX += LineWidth + Spacing;
                if (i < 2)
                {
                    CurrentX += ExtraSymbolSpacing[i];
                }
                RectsDrawn.Add(Rect);
            }

            for (int i = 0; i != SignRects.Length; i++)
            {
                SKRect MiddleRect = new SKRect(RectsDrawn[i].Right, RectsDrawn[i].Top, RectsDrawn[i + 1].Left, RectsDrawn[i + 1].Bottom);
                SignLocations.Add(new Point(MiddleRect.MidX - SignRects[i].MidX, MiddleRect.MidY - SignRects[i].MidY));
            }
        }

        private bool CongratulateIfApplicable(Games Game)
        {
            switch (Game)
            {
                case Games.Words:
                    if (SaveManager.WordsProgress >= 1)
                    {
                        SaveManager.WordsProgress -= 1;
                        Preferences.Set("SaveManager.WordsProgress", SaveManager.WordsProgress);
                        if (MaxWordLevel != SaveManager.WordsLevel)
                        {
                            SaveManager.WordsLevel++;
                            Preferences.Set("SaveManager.WordsLevel", SaveManager.WordsLevel);
                        }
                        CongratulatePlayer();
                        return true;
                    }
                    break;
                case Games.AddSubNumbers:
                    if (SaveManager.AddSubProgress >= 1)
                    {
                        SaveManager.AddSubProgress -= 1;
                        Preferences.Set("SaveManager.AddSubProgress", SaveManager.AddSubProgress);
                        SaveManager.AddSubLevel++;
                        Preferences.Set("SaveManager.AddSubLevel", SaveManager.AddSubLevel);
                        CongratulatePlayer();
                        return true;
                    }
                    break;
                case Games.MultiDivideNumbers:
                    if (SaveManager.MultiDivProgress >= 1)
                    {
                        SaveManager.MultiDivProgress -= 1;
                        Preferences.Set("SaveManager.MultiDivProgress", SaveManager.MultiDivProgress);
                        SaveManager.MultiDivLevel++;
                        Preferences.Set("SaveManager.MultiDivLevel", SaveManager.MultiDivLevel);
                        CongratulatePlayer();
                        return true;
                    }
                    break;
                case Games.CountTheItems:
                    if (SaveManager.CountProgress >= 1)
                    {
                        SaveManager.CountProgress -= 1;
                        Preferences.Set("SaveManager.CountProgress", SaveManager.CountProgress);
                        SaveManager.CountLevel++;
                        Preferences.Set("SaveManager.CountLevel", SaveManager.CountLevel);
                        CongratulatePlayer();
                        return true;
                    }
                    break;
            }
            return false;
        }

        private bool IsPlacedCorrectly()
        {
            for (int i = 0; i != CurrentWord.Length; i++)
            {
                if (CurrentWord[i] != CharactersPlaced[i].Item2)
                {
                    return false;
                }
            }

            return true;
        }

        private char MathSignToChar(MathSigns Sign)
        {
            switch (Sign)
            {
                case MathSigns.Addition:
                    return '+';
                case MathSigns.Subtraction:
                    return '-';
                case MathSigns.Multiplication:
                    return 'x';
                case MathSigns.Division:
                    return '÷';
            }
            throw new Exception();
        }
        private List<int> GetWordLevels()
        {
            List<int> Chances = WordLevelChances[SaveManager.WordsLevel - 1];
            int IndexOfZero = Chances.IndexOf(0);
            int StartIndex = IndexOfZero != -1 ? IndexOfZero : 1;
            return Enumerable.Range(StartIndex, Chances.Count - StartIndex + 1).ToList();
        }
        private void GenerateWord()
        {
            List<int> Chances = WordLevelChances[SaveManager.WordsLevel - 1];
            int RandomNumber = RNG.Next(0, 100);
            int FromNumber = 0;
            for (int i = 0; i != Chances.Count; i++)
            {

                if (RandomNumber >= FromNumber && RandomNumber < FromNumber + Chances[i])
                {
                    string[] Words = English_Words[i];
                    int WordIndex = RNG.Next(0, WordIndexes[SaveManager.WordsLevel - 1].Count - 1);
                    CurrentWord = Words[WordIndexes[SaveManager.WordsLevel - 1][WordIndex]];
                    WordIndexes[SaveManager.WordsLevel - 1].RemoveAt(WordIndex);
                    return;
                }
                FromNumber += Chances[i];
            }

            return;
        }

        public List<string[]> GenerateEnglishWordLevels()
        {
            List<string[]> wordlevels = new List<string[]>();
            const int MinWordCount = 7; // How many words the first level of each word length has 
            IEnumerable<IGrouping<int, string>> SortedByLength = Enum.GetNames(typeof(ItemNames)).Select(x => char.ToUpper(x[0]) + x.Substring(1)).GroupBy(x => x.Length);
            foreach (IGrouping<int, string> LengthGroup in SortedByLength)
            {
                List<string> Level = new List<string>();
                List<string> LengthGroupList = LengthGroup.ToList();
                int GroupSize = MinWordCount;
                int PossibleGroups = (int)Math.Round(LengthGroupList.Count / GroupSize * 0.80); // Remove 10% of the levels to be unevenly distributed
                int RemainderWords = LengthGroupList.Count - (PossibleGroups * GroupSize);

                int WordsAdded = 0;
                (List<int> numbers, int remainder) UnevenSplits = AlgebraUtils.SplitUnevenly(RemainderWords, PossibleGroups);

                for (int i = 0; i < PossibleGroups - 1; i++)
                {
                    int NumberToTake = GroupSize + UnevenSplits.numbers[i];
                    wordlevels.Add(LengthGroup.Skip(WordsAdded).Take(NumberToTake).ToArray());
                    WordsAdded += NumberToTake;
                }
                wordlevels.Add(LengthGroup.Skip(WordsAdded).ToArray());
            }
            return wordlevels;
        }

        private IEnumerable<Item> GenerateItems(ItemNames ItemName, int Count)
        {
            for (int i = 0; i < Count; i++)
            {

                SKBitmap image = Item.GetImage(ItemName);
                float Min = Math.Min(1.0f * (4.0f / Count), 0.1f);
                float Max = Math.Min(1.0f * (6.6f / Count), 0.8f);
                float scale = (float)((RNG.NextDouble() * (Max - Min)) + Min);
                int Rotation = RNG.Next(0, 360);
                SKSize RotatedSize = GeometryUtils.RotateSize(new SKSize(image.Width * scale, image.Height * scale), Rotation);
                SKRect SpawnRect = new SKRect(CanvasRect.Left, train.Y + (train.Bounds.Height * train.Scale) + RotatedSize.Height, CanvasRect.Right - RotatedSize.Width, CoinPlacementRectangle.Top - RotatedSize.Height);

                yield return new Item(ItemName, RNG.Next((int)SpawnRect.Left, (int)SpawnRect.Right), RNG.Next((int)SpawnRect.Top, (int)SpawnRect.Bottom), scale, Rotation);
            }
        }

        private IEnumerable<List<int>> GenerateWordLevelChances()
        {
            for (int i = 1; i < English_Words.Count + 1; i++)
            {

                (List<int> numbers, int remainder) UnevenSplits = AlgebraUtils.SplitUnevenly(100, i);
                List<int> NoRemainder = UnevenSplits.numbers;
                NoRemainder[NoRemainder.Count - 1] += UnevenSplits.remainder;
                yield return NoRemainder;
            }

        }

    }
}