using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ARK.App
{
    public partial class ChartbookManagerWindow : Window
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public ChartbookManagerWindow()
        {
            InitializeComponent();
            SourceInitialized += ChartbookManagerWindow_SourceInitialized;
        }

        private void ChartbookManagerWindow_SourceInitialized(object? sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int useDarkMode = 1;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));
        }
    }
}