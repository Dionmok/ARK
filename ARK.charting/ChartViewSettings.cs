namespace ARK.Charting
{
    public class ChartViewSettings
    {
        public double PriceAxisWidth { get; set; } = 60;
        public double RightPaddingPixels { get; set; } = 20;
        public bool ZoomToMouseCursor { get; set; } = true;
        public double AxisDragSensitivity { get; set; } = 0.003;

        public bool LockPanning { get; set; }
        public bool LockFillSpace { get; set; }
        public double LockedRightEdgeBars { get; set; }
    }
}