using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace ARK.App
{
    public static class ThemeManager
    {
        private static readonly Dictionary<string, Color> DefaultColors = new()
        {
            { "Bg0Color", (Color)ColorConverter.ConvertFromString("#181818") },
            { "Bg1Color", (Color)ColorConverter.ConvertFromString("#1E1E1E") },
            { "Bg2Color", (Color)ColorConverter.ConvertFromString("#2A2A2A") },
            { "ButtonBgColor", (Color)ColorConverter.ConvertFromString("#2A2A2A") },
            { "TextPrimaryColor", (Color)ColorConverter.ConvertFromString("#E0E0E0") },
            { "TextSecondaryColor", (Color)ColorConverter.ConvertFromString("#A0A0A0") },
        };

        private static readonly string DefaultFont = "Segoe UI";

        private static readonly string ThemeFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes", "Theme.xaml");

        public static void SetColor(string resourceKey, Color color)
        {
            Application.Current.Resources[resourceKey] = new SolidColorBrush(color);
        }

        public static Color GetColor(string resourceKey)
        {
            if (Application.Current.Resources[resourceKey] is SolidColorBrush brush)
            {
                return brush.Color;
            }
            return Colors.Black;
        }

        public static void SetFont(string resourceKey, FontFamily font)
        {
            Application.Current.Resources[resourceKey] = font;
        }

        public static FontFamily GetFont(string resourceKey)
        {
            if (Application.Current.Resources[resourceKey] is FontFamily font)
            {
                return font;
            }
            return new FontFamily("Segoe UI");
        }

        public static void ResetToDefaults()
        {
            foreach (var kvp in DefaultColors)
            {
                var colorKey = kvp.Key;
                var brushKey = colorKey.Replace("Color", "Brush");
                SetColor(colorKey, kvp.Value);
                SetColor(brushKey, kvp.Value);
            }

            SetFont("UiFont", new FontFamily(DefaultFont));
        }

        public static void SaveToFile()
        {
            var lines = new List<string>
            {
                "<ResourceDictionary xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"",
                "                     xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">",
                "",
                                "    <!-- Background colors -->",
                $"    <Color x:Key=\"Bg0Color\">{ColorToHex(GetColor("Bg0Color"))}</Color>",
                $"    <Color x:Key=\"Bg1Color\">{ColorToHex(GetColor("Bg1Color"))}</Color>",
                $"    <Color x:Key=\"Bg2Color\">{ColorToHex(GetColor("Bg2Color"))}</Color>",
                "",
                "    <SolidColorBrush x:Key=\"Bg0Brush\" Color=\"{StaticResource Bg0Color}\"/>",
                "    <SolidColorBrush x:Key=\"Bg1Brush\" Color=\"{StaticResource Bg1Color}\"/>",
                "    <SolidColorBrush x:Key=\"Bg2Brush\" Color=\"{StaticResource Bg2Color}\"/>",
                "",
                "    <!-- Button color -->",
                $"    <Color x:Key=\"ButtonBgColor\">{ColorToHex(GetColor("ButtonBgColor"))}</Color>",
                "    <SolidColorBrush x:Key=\"ButtonBgBrush\" Color=\"{StaticResource ButtonBgColor}\"/>",
                "",
                "    <!-- Text colors -->",
                $"    <Color x:Key=\"TextPrimaryColor\">{ColorToHex(GetColor("TextPrimaryColor"))}</Color>",
                $"    <Color x:Key=\"TextSecondaryColor\">{ColorToHex(GetColor("TextSecondaryColor"))}</Color>",
                "",
                "    <SolidColorBrush x:Key=\"TextPrimaryBrush\" Color=\"{StaticResource TextPrimaryColor}\"/>",
                "    <SolidColorBrush x:Key=\"TextSecondaryBrush\" Color=\"{StaticResource TextSecondaryColor}\"/>",
                "",
                "    <!-- Candle colors -->",
                $"    <Color x:Key=\"BullColor\">{ColorToHex(GetColor("BullColor"))}</Color>",
                $"    <Color x:Key=\"BearColor\">{ColorToHex(GetColor("BearColor"))}</Color>",
                "",
                "    <SolidColorBrush x:Key=\"BullBrush\" Color=\"{StaticResource BullColor}\"/>",
                "    <SolidColorBrush x:Key=\"BearBrush\" Color=\"{StaticResource BearColor}\"/>",
                "",
                "    <!-- Fonts -->",
                $"    <FontFamily x:Key=\"UiFont\">{GetFont("UiFont").Source}</FontFamily>",
                "",
                "</ResourceDictionary>"
            };

            var directory = Path.GetDirectoryName(ThemeFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllLines(ThemeFilePath, lines);
        }

        private static string ColorToHex(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}