using Disce.Utils;
using SkiaSharp;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
namespace Disce.Objects
{
    public enum ItemNames
    {
        air,
        ant,
        apple,
        arm,
        art,
        axe,
        bag,
        banana,
        bank,
        bat,
        bed,
        bee,
        bell,
        bill,
        bird,
        black,
        blue,
        book,
        boot,
        boy,
        brain,
        bread,
        brown,
        burger,
        bus,
        cab,
        cake,
        camel,
        camera,
        can,
        cap,
        car,
        carrot,
        cash,
        cat,
        clock,
        cloud,
        cow,
        crowd,
        dance,
        deer,
        dig,
        doctor,
        dog,
        duck,
        ear,
        earth,
        egg,
        elf,
        fan,
        fish,
        flower,
        fork,
        fox,
        fries,
        galaxy,
        game,
        garlic,
        gem,
        girl,
        goat,
        grass,
        green,
        horse,
        house,
        hut,
        ink,
        jacket,
        jam,
        kite,
        lips,
        magic,
        man,
        map,
        milk,
        monkey,
        mouth,
        nest,
        nurse,
        nut,
        orange,
        ore,
        owl,
        parrot,
        pawn,
        pen,
        pet,
        phone,
        pill,
        pin,
        pink,
        plane,
        pond,
        pool,
        purple,
        queen,
        quiz,
        rain,
        rat,
        red,
        robot,
        rocket,
        saw,
        ship,
        soda,
        sofa,
        soup,
        star,
        stone,
        tablet,
        tea,
        tent,
        tree,
        vase,
        watch,
        white,
        woman,
        worm,
        yellow,
        zebra
    }
    public class Item : SKDrawable
    {

        private static readonly Dictionary<string, SKBitmap> ItemImages = new Dictionary<string, SKBitmap>();

        public SKBitmap ItemImage { get; private set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Scale { get; set; }
        public float Rotation { get; set; }

        public Item(ItemNames Item, float x, float y, float scale, float rotation)
        {
            ItemImage = GetImage(Item);
            X = x;
            Y = y;
            Scale = scale;
            Rotation = rotation;
        }

        protected override void OnDraw(SKCanvas canvas)
        {
            float ActualWidth = ItemImage.Width * Scale;
            float ActualHeight = ItemImage.Height * Scale;

            SKSize RotatedSize = GeometryUtils.RotateSize(new SKSize(ActualWidth, ActualHeight), Rotation);
            SKRect RotatedItemRect = SKRect.Create(new SKPoint(X - (ActualWidth / 2), Y - (ActualHeight / 2)), RotatedSize);

            RotatedItemRect.Inflate(Scale, Scale);
            canvas.Save();
            canvas.RotateDegrees(Rotation, RotatedItemRect.MidX, RotatedItemRect.MidY);
            canvas.DrawBitmap(ItemImage, RotatedItemRect);
            canvas.Restore();
            base.OnDraw(canvas);
        }
        public static SKBitmap GetImage(ItemNames Item)
        {
            string ItemName = Item.ToString();
            if (!ItemImages.ContainsKey(ItemName))
            {
                using (Stream S = typeof(Item).GetTypeInfo().Assembly.GetManifestResourceStream("Disce.Items." + ItemName + ".png"))
                {
                    ItemImages.Add(ItemName, SKBitmap.Decode(S));
                }
            }
            return ItemImages[ItemName];
        }
    }
}
