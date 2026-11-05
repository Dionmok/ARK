using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace ARK.App.SettingsPages
{
    public partial class AppearanceSettingsPage : UserControl
    {
        private bool _isLoadingFonts = true;

        public AppearanceSettingsPage()
        {
            InitializeComponent();
            LoadCurrentValues();
        }

        private void LoadCurrentValues()
        {
            _isLoadingFonts = true;

            Bg0Swatch.Background = new SolidColorBrush(ThemeManager.GetColor("Bg0Color"));
            Bg1Swatch.Background = new SolidColorBrush(ThemeManager.GetColor("Bg1Color"));
            Bg2Swatch.Background = new SolidColorBrush(ThemeManager.GetColor("Bg2Color"));
            ButtonBgSwatch.Background = new SolidColorBrush(ThemeManager.GetColor("ButtonBgColor"));
            TextPrimarySwatch.Background = new SolidColorBrush(ThemeManager.GetColor("TextPrimaryColor"));
            TextSecondarySwatch.Background = new SolidColorBrush(ThemeManager.GetColor("TextSecondaryColor"));

            if (FontComboBox.Items.Count == 0)
            {
                foreach (var family in Fonts.SystemFontFamilies)
                {
                    var item = new ComboBoxItem
                    {
                        Content = family.Source,
                        FontFamily = family,
                        Tag = family.Source
                    };
                    FontComboBox.Items.Add(item);
                }
            }

            var currentFont = ThemeManager.GetFont("UiFont");
            foreach (ComboBoxItem item in FontComboBox.Items)
            {
                if ((string)item.Tag == currentFont.Source)
                {
                    FontComboBox.SelectedItem = item;
                    break;
                }
            }
            _isLoadingFonts = false;
        }   

        private void PickColorFor(string colorKey, string brushKey, Border swatch)
        {
            var currentColor = ThemeManager.GetColor(colorKey);

            var dialog = new System.Windows.Forms.ColorDialog
            {
                Color = System.Drawing.Color.FromArgb(currentColor.R, currentColor.G, currentColor.B)
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var newColor = Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B);
                ThemeManager.SetColor(colorKey, newColor);
                ThemeManager.SetColor(brushKey, newColor);
                swatch.Background = new SolidColorBrush(newColor);
            }
        }

        private void Bg0Swatch_MouseLeftButtonUp(object sender, RoutedEventArgs e)
            => PickColorFor("Bg0Color", "Bg0Brush", Bg0Swatch);

        private void Bg1Swatch_MouseLeftButtonUp(object sender, RoutedEventArgs e)
            => PickColorFor("Bg1Color", "Bg1Brush", Bg1Swatch);

        private void Bg2Swatch_MouseLeftButtonUp(object sender, RoutedEventArgs e)
    => PickColorFor("Bg2Color", "Bg2Brush", Bg2Swatch);

        private void ButtonBgSwatch_MouseLeftButtonUp(object sender, RoutedEventArgs e)
            => PickColorFor("ButtonBgColor", "ButtonBgBrush", ButtonBgSwatch);

        private void TextPrimarySwatch_MouseLeftButtonUp(object sender, RoutedEventArgs e)
            => PickColorFor("TextPrimaryColor", "TextPrimaryBrush", TextPrimarySwatch);

        private void TextSecondarySwatch_MouseLeftButtonUp(object sender, RoutedEventArgs e)
            => PickColorFor("TextSecondaryColor", "TextSecondaryBrush", TextSecondarySwatch);

        private void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontComboBox.SelectedItem is ComboBoxItem item && item.Tag is string fontName)
            {
                FontComboBox.FontFamily = new FontFamily(fontName);
            }

            if (_isLoadingFonts) return;
            if (FontComboBox.SelectedItem is ComboBoxItem item2 && item2.Tag is string fontName2)
            {
                ThemeManager.SetFont("UiFont", new FontFamily(fontName2));
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.SaveToFile();
            MessageBox.Show("Appearance settings saved.", "ARK", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.ResetToDefaults();
            LoadCurrentValues();
        }
    }
}
