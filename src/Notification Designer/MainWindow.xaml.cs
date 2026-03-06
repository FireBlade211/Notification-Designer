using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Input;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Microsoft.Windows.Storage.Pickers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Provider;
using CommunityToolkit.WinUI.Collections;
using Windows.System;
using Notification_Designer.Helpers;
using Microsoft.UI.Windowing;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Notification_Designer
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public static MainWindow? Instance;

        public MainWindow()
        {
            InitializeComponent();

            Instance = this;

            mainFrame.Navigate(typeof(RootPage));

            // load settings
            if (ConfigHelper.ReadValue("AppTheme") is int nTheme)
            {
                var theme = (ElementTheme)nTheme;

                mainFrame.RequestedTheme = theme;
                AppWindow.TitleBar.PreferredTheme = theme switch
                {
                    ElementTheme.Default => TitleBarTheme.UseDefaultAppMode,
                    ElementTheme.Light => TitleBarTheme.Light,
                    ElementTheme.Dark => TitleBarTheme.Dark,
                    _ => TitleBarTheme.UseDefaultAppMode
                };
            }

            if (ConfigHelper.ReadValue("SoundMode") is bool sm)
                ElementSoundPlayer.State = sm ? ElementSoundPlayerState.On : ElementSoundPlayerState.Off;

            if (ConfigHelper.ReadValue("SpatialSoundMode") is bool ssm)
                ElementSoundPlayer.SpatialAudioMode = ssm ? ElementSpatialAudioMode.On : ElementSpatialAudioMode.Off;
        }
    }
}
