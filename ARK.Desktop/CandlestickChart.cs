using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace ARK.Desktop;

internal sealed class CandlestickChart : Control
{
    private static readonly Pen GridLine = new(Brushes.DimGray);
    private static readonly Pen Wick = new(Brushes.LightGray);
    private static readonly IBrush Rising = new SolidColorBrush(Color.Parse("#3DA35D"));
    private static readonly IBrush Falling = new SolidColorBrush(Color.Parse("#C94F4F"));

    private Candle[] candles = [];

    public void SetSymbol(string symbol)
    {
        // Use a stable seed so both platforms show the same sample data.
        int seed = 0;
        foreach (char letter in symbol)
            seed = unchecked(seed * 31 + letter);

        var random = new Random(seed);
        double price = 100 + Math.Abs(seed % 400);
        candles = new Candle[60];
        for (int i = 0; i < candles.Length; i++)
        {
            double close = price + random.NextDouble() * 4 - 2;
            double high = Math.Max(price, close) + random.NextDouble() * 1.5;
            double low = Math.Min(price, close) - random.NextDouble() * 1.5;
            candles[i] = new Candle(price, high, low, close);
            price = close;
        }

        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var surface = new Rect(Bounds.Size);
        context.FillRectangle(Brushes.Black, surface);
        if (candles.Length == 0 || surface.Width < 72 || surface.Height < 48)
            return;

        var plot = new Rect(8, 12, surface.Width - 72, surface.Height - 36);
        double low = candles.Min(candle => candle.Low);
        double high = candles.Max(candle => candle.High);
        double range = Math.Max(high - low, 0.01);
        double PriceY(double price) => plot.Bottom - (price - low) / range * plot.Height;

        for (int i = 0; i <= 5; i++)
        {
            double price = low + range * i / 5;
            double y = PriceY(price);
            context.DrawLine(GridLine, new Point(plot.Left, y), new Point(plot.Right, y));
            DrawLabel(context, price.ToString("0.00", CultureInfo.CurrentCulture), new Point(plot.Right + 5, y - 6));
        }

        double slotWidth = plot.Width / candles.Length;
        double bodyWidth = slotWidth * 0.65;
        for (int i = 0; i < candles.Length; i++)
        {
            Candle candle = candles[i];
            double x = plot.Left + (i + 0.5) * slotWidth;
            context.DrawLine(Wick, new Point(x, PriceY(candle.High)), new Point(x, PriceY(candle.Low)));
            double openY = PriceY(candle.Open);
            double closeY = PriceY(candle.Close);
            var body = new Rect(x - bodyWidth / 2, Math.Min(openY, closeY), bodyWidth, Math.Max(1, Math.Abs(openY - closeY)));
            context.FillRectangle(candle.Close >= candle.Open ? Rising : Falling, body);
        }

        for (int i = 0; i < candles.Length; i += 15)
        {
            double x = plot.Left + i * slotWidth;
            DrawLabel(context, $"08:{i:00}", new Point(x, plot.Bottom + 5));
        }
    }

    private static void DrawLabel(DrawingContext context, string label, Point origin) =>
        context.DrawText(new FormattedText(label, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            Typeface.Default, 11, Brushes.LightGray), origin);

    private readonly record struct Candle(double Open, double High, double Low, double Close);
}
