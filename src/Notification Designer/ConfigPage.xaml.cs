using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.Storage;
using Notification_Designer.Helpers;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Notification_Designer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConfigPage : Page
    {
        public ConfigPage()
        {
            InitializeComponent();

            var wnd = MainWindow.Instance;
            if (wnd == null) return;

            if (wnd.AppWindow.Presenter.Kind != AppWindowPresenterKind.FullScreen)
            {
                wnd.SetTitleBar(titleBar);
                wnd.AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
            }
        }

        private void TitleBarSetRegionRects()
        {
            var wnd = MainWindow.Instance;
            if (wnd == null) return;

            // Specify the interactive regions of the title bar.

            double scaleAdjustment = titleBar.XamlRoot.RasterizationScale;

            RightPaddingColumn.Width = new GridLength(wnd.AppWindow.TitleBar.RightInset / scaleAdjustment);
            LeftPaddingColumn.Width = new GridLength(wnd.AppWindow.TitleBar.LeftInset / scaleAdjustment);

            GeneralTransform transform = backButton.TransformToVisual(null);
            Rect bounds = transform.TransformBounds(new Rect(0, 0,
                                                             backButton.ActualWidth,
                                                             backButton.ActualHeight));
            RectInt32 appButtonRectRect = GetRect(bounds, scaleAdjustment);

            var rectArray = new RectInt32[] { appButtonRectRect };

            InputNonClientPointerSource nonClientInputSrc =
                InputNonClientPointerSource.GetForWindowId(wnd.AppWindow.Id);

            nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rectArray);
        }

        private RectInt32 GetRect(Rect bounds, double scale)
        {
            return new RectInt32(
                _X: (int)Math.Round(bounds.X * scale),
                _Y: (int)Math.Round(bounds.Y * scale),
                _Width: (int)Math.Round(bounds.Width * scale),
                _Height: (int)Math.Round(bounds.Height * scale)
            );
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            TitleBarSetRegionRects();

            var currentTheme = MainWindow.Instance?.mainFrame.RequestedTheme;
            switch (currentTheme)
            {
                case ElementTheme.Light:
                    themeMode.SelectedIndex = 0;
                    break;
                case ElementTheme.Dark:
                    themeMode.SelectedIndex = 1;
                    break;
                case ElementTheme.Default:
                    themeMode.SelectedIndex = 2;
                    break;
            }

            if (ElementSoundPlayer.State == ElementSoundPlayerState.On)
                soundToggle.IsOn = true;

            if (ElementSoundPlayer.SpatialAudioMode == ElementSpatialAudioMode.On)
                spatialSoundBox.IsOn = true;
        }

        private void titleBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TitleBarSetRegionRects();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            var wnd = MainWindow.Instance;
            if (wnd == null) return;

            if (wnd.AppWindow.Presenter.Kind != AppWindowPresenterKind.FullScreen)
                wnd.AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Standard;

            wnd.mainFrame.Navigate(typeof(RootPage));
        }

        private void themeMode_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var selectedTheme = ((ComboBoxItem)themeMode.SelectedItem)?.Tag?.ToString();
            var window = MainWindow.Instance;

            if (selectedTheme != null)
            {
                var theme = Enum.Parse<ElementTheme>(selectedTheme);

                window?.mainFrame.RequestedTheme = theme;
                window?.AppWindow.TitleBar.PreferredTheme = theme switch
                {
                    ElementTheme.Default => TitleBarTheme.UseDefaultAppMode,
                    ElementTheme.Light => TitleBarTheme.Light,
                    ElementTheme.Dark => TitleBarTheme.Dark,
                    _ => TitleBarTheme.UseDefaultAppMode
                };

                ConfigHelper.WriteValue("AppTheme", (int)theme);
            }
        }

        private void soundToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (soundToggle.IsOn)
            {
                SpatialAudioCard.IsEnabled = true;
                ElementSoundPlayer.State = ElementSoundPlayerState.On;

                ConfigHelper.WriteValue("SoundMode", true);
            }
            else
            {
                SpatialAudioCard.IsEnabled = false;
                spatialSoundBox.IsOn = false;

                ElementSoundPlayer.State = ElementSoundPlayerState.Off;
                ElementSoundPlayer.SpatialAudioMode = ElementSpatialAudioMode.Off;

                ConfigHelper.WriteValue("SoundMode", false);
            }
        }

        private void spatialSoundBox_Toggled(object sender, RoutedEventArgs e)
        {
            if (spatialSoundBox.IsOn)
            {
                ElementSoundPlayer.SpatialAudioMode = ElementSpatialAudioMode.On;

                ConfigHelper.WriteValue("SpatialSoundMode", true);
            }
            else
            {
                ElementSoundPlayer.SpatialAudioMode = ElementSpatialAudioMode.Off;

                ConfigHelper.WriteValue("SpatialSoundMode", false);
            }
        }

        private void toCloneRepoCard_Click(object sender, RoutedEventArgs e)
        {
            DataPackage package = new DataPackage();
            package.SetText(gitCloneTextBlock.Text);
            Clipboard.SetContent(package);
        }

        private async void bugRequestCard_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/FireBlade/Notification-Designer/issues/new/choose"));
        }
    }
}
