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
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Notification_Designer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RootPage : Page
    {
        private ObservableCollection<NotificationElement> Elements { get; set; } = [];
        private AdvancedCollectionView TextElements { get; set; }
        private ObservableCollection<EditorToastComboBox> comboBoxes = [];
        private XmlDocument? doc;

        public RootPage()
        {
            InitializeComponent();

            TimestampOverrideDatePicker.Date = DateTimeOffset.Now;
            TimestampOverrideTimePicker.Time = DateTime.Now.TimeOfDay;

            TextElements = new(Elements, true)
            {
                Filter = item => item is TextNotificationElement
            };
        }

        private void TitleBarSetRegionRects()
        {
            var wnd = MainWindow.Instance;
            if (wnd == null) return;

            // Specify the interactive regions of the title bar.

            double scaleAdjustment = titleBarGrid.XamlRoot.RasterizationScale;

            RightPaddingColumn.Width = new GridLength(wnd.AppWindow.TitleBar.RightInset / scaleAdjustment);
            LeftPaddingColumn.Width = new GridLength(wnd.AppWindow.TitleBar.LeftInset / scaleAdjustment);

            GeneralTransform transform = fallthroughButton.TransformToVisual(null);
            Rect bounds = transform.TransformBounds(new Rect(0, 0,
                                                             fallthroughButton.ActualWidth,
                                                             fallthroughButton.ActualHeight));
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

        private void titleBarGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TitleBarSetRegionRects();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            TitleBarSetRegionRects();

            settingsAccel.Key = (VirtualKey)0xBC;

            var wnd = MainWindow.Instance;
            if (wnd == null) return;

            wnd.ExtendsContentIntoTitleBar = true;
            wnd.SetTitleBar(titleBarGrid);
        }

        private void AddTextElementCard_Click(object sender, RoutedEventArgs e)
        {
            Elements.Add(new TextNotificationElement());

            AddTextElementCard.IsEnabled = Elements.OfType<TextNotificationElement>().Count() < 3;
            MaxTextElementBlock.Text = $"{Elements.OfType<TextNotificationElement>().Count()} / 3";
            if (Elements.OfType<TextNotificationElement>().Count() >= 3)
            {
                MaxTextElementBlock.Foreground = (Brush)Application.Current.Resources["SystemFillColorCriticalBrush"];
            }
            else
            {
                MaxTextElementBlock.Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"];
            }
        }

        /// <summary>
        /// Counts the amount of valid text elements.
        /// </summary>
        /// <param name="count">The amount of valid text elements.</param>
        /// <returns><see langword="true"/> if the operation was successful; otherwise, <see langword="false"/>.</returns>
        private bool CountTextElements(out int count)
        {
            count = 0;

            try
            {
                foreach (var element in Elements)
                {
                    if (element is TextNotificationElement text)
                    {
                        Debug.WriteLine("Text: " + text.Text);
                        if (!string.IsNullOrEmpty(text.Text))
                        {
                            count++;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if there's enough valid text elements for the notification.
        /// </summary>
        /// <returns><see langword="true"/> if enough valid text elements exist; otherwise, <see langword="false"/>.</returns>
        private bool CountTextElements()
        {
            foreach (var element in Elements)
            {
                if (element is TextNotificationElement text)
                {
                    if (!string.IsNullOrEmpty(text.Text))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void TextElementCardDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is AppBarButton button)
            {
                if (button.DataContext is TextNotificationElement element)
                {
                    Elements.Remove(element);

                    AddTextElementCard.IsEnabled = Elements.OfType<TextNotificationElement>().Count() < 3;
                    MaxTextElementBlock.Text = $"{Elements.OfType<TextNotificationElement>().Count()} / 3";
                    if (Elements.OfType<TextNotificationElement>().Count() >= 3)
                    {
                        MaxTextElementBlock.Foreground = (Brush)Application.Current.Resources["SystemFillColorCriticalBrush"];
                    }
                    else
                    {
                        MaxTextElementBlock.Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"];
                    }
                }
            }
        }

        private void TextElementBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AddTextElementCard.IsEnabled = Elements.OfType<TextNotificationElement>().Count() < 3;
            MaxTextElementBlock.Text = $"{Elements.OfType<TextNotificationElement>().Count()} / 3";
            if (Elements.OfType<TextNotificationElement>().Count() >= 3)
            {
                MaxTextElementBlock.Foreground = (Brush)Application.Current.Resources["SystemFillColorCriticalBrush"];
            }
            else
            {
                MaxTextElementBlock.Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"];
            }
        }

        private void TextElementBox_LostFocus(object sender, RoutedEventArgs e)
        {
            AddTextElementCard.IsEnabled = Elements.OfType<TextNotificationElement>().Count() < 3;
            MaxTextElementBlock.Text = $"{Elements.OfType<TextNotificationElement>().Count()} / 3";
            if (Elements.OfType<TextNotificationElement>().Count() >= 3)
            {
                MaxTextElementBlock.Foreground = (Brush)Application.Current.Resources["SystemFillColorCriticalBrush"];
            }
            else
            {
                MaxTextElementBlock.Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"];
            }
        }

        private XmlDocument BuildNotification()
        {
            var toast = new ToastContent();

            var actions = new ToastActionsCustom();
            toast.Actions = actions;

            var visual = new ToastVisual();
            toast.Visual = visual;

            var bindingGeneric = new ToastBindingGeneric();
            visual.BindingGeneric = bindingGeneric;

            #region Elements
            foreach (var element in Elements)
            {
                switch (element)
                {
                    case TextNotificationElement text:
                        bindingGeneric.Children.Add(new AdaptiveText
                        {
                            Text = text.Text,
                        });
                        break;
                    case ImageNotificationElement img:
                        switch (img.Type)
                        {
                            case ImageNotificationElementType.InlineImage:
                                AdaptiveImageCrop inCrop = AdaptiveImageCrop.Default;
                                if (InlineImageCropModeButtons.SelectedItem != null)
                                {
                                    if ((string)((RadioButton)InlineImageCropModeButtons.SelectedItem).Tag == "circle")
                                    {
                                        inCrop = AdaptiveImageCrop.Circle;
                                    }
                                }

                                bindingGeneric.Children.Add(new AdaptiveImage
                                {
                                    HintCrop = inCrop,
                                    Source = img.Uri
                                });
                                break;
                            case ImageNotificationElementType.HeroImage:
                                bindingGeneric.HeroImage = new ToastGenericHeroImage
                                {
                                    Source = img.Uri
                                };
                                break;
                            case ImageNotificationElementType.AppLogoOverride:
                                ToastGenericAppLogoCrop lCrop = ToastGenericAppLogoCrop.Default;
                                if (AppLogoOverrideCropModeButtons.SelectedItem != null)
                                {
                                    if ((string)((RadioButton)AppLogoOverrideCropModeButtons.SelectedItem).Tag == "circle")
                                    {
                                        lCrop = ToastGenericAppLogoCrop.Circle;
                                    }
                                }
                                bindingGeneric.AppLogoOverride = new ToastGenericAppLogo
                                {
                                    Source = img.Uri,
                                    HintCrop = lCrop
                                };
                                break;
                        }
                        break;
                }
            }
            #endregion

            if (ScenarioCombo.SelectedItem is ComboBoxItem scenarioItem)
            {
                var s = Enum.Parse<ToastScenario>((string)scenarioItem.Tag);
                Debug.WriteLine("Setting scenario: " + s.ToString());
                toast.Scenario = s;
            }

            if (SoundEventCombo.SelectedItem is ComboBoxItem soundEventItem)
            {
                if (soundEventItem.Tag.ToString() != "None")
                {
                    toast.Audio = new ToastAudio
                    {
                        Src = GetSoundUri(Enum.Parse<AppNotificationSoundEvent>((string)soundEventItem.Tag)),
                        Loop = SoundLoopToggle.IsOn
                    };
                }
            }

            if (!string.IsNullOrEmpty(AttributionBox.Text))
            {
                if (!string.IsNullOrEmpty(AttribLanguageBox.Text))
                {
                    bindingGeneric.Attribution = new ToastGenericAttributionText
                    {
                        Text = AttributionBox.Text,
                        Language = AttribLanguageBox.Text
                    };
                }
                else
                {
                    bindingGeneric.Attribution = new ToastGenericAttributionText
                    {
                        Text = AttributionBox.Text
                    };
                }
            }

            if (TimestampOverrideToggle.IsOn)
            {
                var date = TimestampOverrideDatePicker.Date ?? DateTimeOffset.Now;
                var newDate = date.Add(TimestampOverrideTimePicker.Time);

                toast.DisplayTimestamp = newDate;
            }

            if (ProgressBarToggle.IsOn)
            {
                string status = ProgressBarStatusBox.Text;

                double value = ProgressBarPercentageSlider.Value / 100;

                string? valueStringOverride = null;
                string? title = null;

                if (!string.IsNullOrEmpty(ProgressBarValueOverrideBox.Text))
                {
                    valueStringOverride = ProgressBarValueOverrideBox.Text;
                }

                if (!string.IsNullOrEmpty(ProgressBarTitleBox.Text))
                {
                    title = ProgressBarTitleBox.Text;
                }

                Debug.WriteLine("Value: " + value.ToString());

                bindingGeneric.Children.Add(new AdaptiveProgressBar
                {
                    Title = title,
                    Status = status,
                    Value = value,
                    ValueStringOverride = valueStringOverride
                });
            }

            #region Buttons


            if (!string.IsNullOrEmpty(Button1TextBox.Text))
            {
                switch ((string)((ComboBoxItem)Button1TypeCombo.SelectedItem).Tag)
                {
                    case "regular":
                        var button = new ToastButton(Button1TextBox.Text, string.Empty);
                        if (!string.IsNullOrEmpty(Button1IconUriBox.Text))
                        {
                            button.ImageUri = Button1IconUriBox.Text;
                        }
                        actions.Buttons.Add(button);
                        break;
                    case "context":
                        actions.ContextMenuItems.Add(new ToastContextMenuItem(Button1TextBox.Text, string.Empty));
                        break;
                }
            }

            if ((string)((ComboBoxItem)Button1TypeCombo.SelectedItem).Tag == "snooze")
            {
                if (!string.IsNullOrEmpty(Button1TextBox.Text))
                {
                    actions.Buttons.Add(new ToastButtonSnooze(Button1TextBox.Text));
                }
                else
                {
                    actions.Buttons.Add(new ToastButtonSnooze());
                }
            }
            else if ((string)((ComboBoxItem)Button1TypeCombo.SelectedItem).Tag == "dismiss")
            {
                if (!string.IsNullOrEmpty(Button1TextBox.Text))
                {
                    actions.Buttons.Add(new ToastButtonDismiss(Button1TextBox.Text));
                }
                else
                {
                    actions.Buttons.Add(new ToastButtonDismiss());
                }
            }

            if (!string.IsNullOrEmpty(Button2TextBox.Text))
            {
                switch ((string)((ComboBoxItem)Button2TypeCombo.SelectedItem).Tag)
                {
                    case "regular":
                        var button = new ToastButton(Button2TextBox.Text, string.Empty);
                        if (!string.IsNullOrEmpty(Button2IconUriBox.Text))
                        {
                            button.ImageUri = Button2IconUriBox.Text;
                        }
                        actions.Buttons.Add(button);
                        break;
                    case "context":
                        actions.ContextMenuItems.Add(new ToastContextMenuItem(Button2TextBox.Text, string.Empty));
                        break;
                }
            }

            if ((string)((ComboBoxItem)Button2TypeCombo.SelectedItem).Tag == "snooze")
            {
                if (!string.IsNullOrEmpty(Button2TextBox.Text))
                {
                    actions.Buttons.Add(new ToastButtonSnooze(Button2TextBox.Text));
                }
                else
                {
                    actions.Buttons.Add(new ToastButtonSnooze());
                }
            }
            else if ((string)((ComboBoxItem)Button2TypeCombo.SelectedItem).Tag == "dismiss")
            {
                if (!string.IsNullOrEmpty(Button2TextBox.Text))
                {
                    actions.Buttons.Add(new ToastButtonDismiss(Button2TextBox.Text));
                }
                else
                {
                    actions.Buttons.Add(new ToastButtonDismiss());
                }
            }

            if (!string.IsNullOrEmpty(Button3TextBox.Text))
            {
                switch ((string)((ComboBoxItem)Button3TypeCombo.SelectedItem).Tag)
                {
                    case "regular":
                        var button = new ToastButton(Button3TextBox.Text, string.Empty);
                        if (!string.IsNullOrEmpty(Button3IconUriBox.Text))
                        {
                            button.ImageUri = Button3IconUriBox.Text;
                        }
                        actions.Buttons.Add(button);
                        break;
                    case "context":
                        actions.ContextMenuItems.Add(new ToastContextMenuItem(Button3TextBox.Text, string.Empty));
                        break;
                }
            }

            if ((string)((ComboBoxItem)Button3TypeCombo.SelectedItem).Tag == "snooze")
            {
                if (!string.IsNullOrEmpty(Button3TextBox.Text))
                {
                    actions.Buttons.Add(new ToastButtonSnooze(Button3TextBox.Text));
                }
                else
                {
                    actions.Buttons.Add(new ToastButtonSnooze());
                }
            }
            else if ((string)((ComboBoxItem)Button3TypeCombo.SelectedItem).Tag == "dismiss")
            {
                if (!string.IsNullOrEmpty(Button3TextBox.Text))
                {
                    actions.Buttons.Add(new ToastButtonDismiss(Button3TextBox.Text));
                }
                else
                {
                    actions.Buttons.Add(new ToastButtonDismiss());
                }
            }

            if (!string.IsNullOrEmpty(Button4TextBox.Text))
            {
                switch ((string)((ComboBoxItem)Button4TypeCombo.SelectedItem).Tag)
                {
                    case "regular":
                        var button = new ToastButton(Button4TextBox.Text, string.Empty);
                        if (!string.IsNullOrEmpty(Button4IconUriBox.Text))
                        {
                            button.ImageUri = Button4IconUriBox.Text;
                        }
                        actions.Buttons.Add(button);
                        break;
                    case "context":
                        actions.ContextMenuItems.Add(new ToastContextMenuItem(Button4TextBox.Text, string.Empty));
                        break;
                }
            }

            if ((string)((ComboBoxItem)Button4TypeCombo.SelectedItem).Tag == "snooze")
            {
                if (!string.IsNullOrEmpty(Button4TextBox.Text))
                {
                    actions.Buttons.Add(new ToastButtonSnooze(Button4TextBox.Text));
                }
                else
                {
                    actions.Buttons.Add(new ToastButtonSnooze());
                }
            }
            else if ((string)((ComboBoxItem)Button4TypeCombo.SelectedItem).Tag == "dismiss")
            {
                if (!string.IsNullOrEmpty(Button4TextBox.Text))
                {
                    actions.Buttons.Add(new ToastButtonDismiss(Button4TextBox.Text));
                }
                else
                {
                    actions.Buttons.Add(new ToastButtonDismiss());
                }
            }

            if (!string.IsNullOrEmpty(Button5TextBox.Text))
            {
                switch ((string)((ComboBoxItem)Button5TypeCombo.SelectedItem).Tag)
                {
                    case "regular":
                        var button = new ToastButton(Button5TextBox.Text, string.Empty);
                        if (!string.IsNullOrEmpty(Button5IconUriBox.Text))
                        {
                            button.ImageUri = Button5IconUriBox.Text;
                        }
                        actions.Buttons.Add(button);
                        break;
                    case "context":
                        actions.ContextMenuItems.Add(new ToastContextMenuItem(Button5TextBox.Text, string.Empty));
                        break;
                }
            }

            if ((string)((ComboBoxItem)Button5TypeCombo.SelectedItem).Tag == "snooze")
            {
                if (!string.IsNullOrEmpty(Button5TextBox.Text))
                {
                    actions.Buttons.Add(new ToastButtonSnooze(Button5TextBox.Text));
                }
                else
                {
                    actions.Buttons.Add(new ToastButtonSnooze());
                }
            }
            else if ((string)((ComboBoxItem)Button5TypeCombo.SelectedItem).Tag == "dismiss")
            {
                if (!string.IsNullOrEmpty(Button5TextBox.Text))
                {
                    actions.Buttons.Add(new ToastButtonDismiss(Button5TextBox.Text));
                }
                else
                {
                    actions.Buttons.Add(new ToastButtonDismiss());
                }
            }
            #endregion

            #region Input Boxes

            if (InputBox1Toggle.IsOn && !string.IsNullOrEmpty(Input1IdBox.Text))
            {
                actions.Inputs.Add(new ToastTextBox(Input1IdBox.Text)
                {
                    DefaultInput = Input1DefaultBox.Text,
                    Title = Input1TitleBox.Text,
                    PlaceholderContent = Input1PlaceholderBox.Text
                });
            }

            if (InputBox2Toggle.IsOn && !string.IsNullOrEmpty(Input2IdBox.Text))
            {
                actions.Inputs.Add(new ToastTextBox(Input2IdBox.Text)
                {
                    DefaultInput = Input2DefaultBox.Text,
                    Title = Input2TitleBox.Text,
                    PlaceholderContent = Input2PlaceholderBox.Text
                });
            }

            if (InputBox3Toggle.IsOn && !string.IsNullOrEmpty(Input3IdBox.Text))
            {
                actions.Inputs.Add(new ToastTextBox(Input3IdBox.Text)
                {
                    DefaultInput = Input3DefaultBox.Text,
                    Title = Input3TitleBox.Text,
                    PlaceholderContent = Input3PlaceholderBox.Text
                });
            }

            if (InputBox4Toggle.IsOn && !string.IsNullOrEmpty(Input4IdBox.Text))
            {
                actions.Inputs.Add(new ToastTextBox(Input4IdBox.Text)
                {
                    DefaultInput = Input4DefaultBox.Text,
                    Title = Input4TitleBox.Text,
                    PlaceholderContent = Input4PlaceholderBox.Text
                });
            }

            if (InputBox5Toggle.IsOn && !string.IsNullOrEmpty(Input5IdBox.Text))
            {
                actions.Inputs.Add(new ToastTextBox(Input5IdBox.Text)
                {
                    DefaultInput = Input5DefaultBox.Text,
                    Title = Input5TitleBox.Text,
                    PlaceholderContent = Input5PlaceholderBox.Text
                });
            }
            #endregion

            foreach (EditorToastComboBox combo in comboBoxes)
            {

                if (actions.Inputs.Count >= 5)
                {
                    break;
                }

                var selectBox = new ToastSelectionBox(combo.Id)
                {
                    DefaultSelectionBoxItemId = combo.DefaultItemId,
                    Title = combo.Name
                };

                selectBox.Items.Clear();

                foreach (EditorToastComboBoxItem item in combo.Items)
                {
                    selectBox.Items.Add(new ToastSelectionBoxItem(item.Id, item.Content));
                }

                actions.Inputs.Add(selectBox);
            }

            if (HeaderEnabledSwitch.IsOn)
            {
                toast.Header = new ToastHeader(HeaderIdBox.Text, HeaderTitleBox.Text, HeaderArgsBox.Text);
            }

            toast.Launch = ArgsBox.Text;

            var doc = toast.GetXml();

            if (AutoSnoozeDismissToggle.IsOn)
            {
                // The user has enabled Auto Snooze/Dismiss, override buttons
                // Enabling Auto Snooze/Dismiss automatically prevents adding
                // custom inputs in UI
                toast.Actions = new ToastActionsSnoozeAndDismiss();
                doc = toast.GetXml();
            }
            else
            {
                // Workaround to button colors/tooltips not being supported in ToastActionsCustom
                // This code modifies the notification XML itself to set the colors & tooltips

                // Get all <action> elements inside <actions>
                var xmlActions = doc.GetElementsByTagName("action");
                doc.DocumentElement.SetAttribute("useButtonStyle", "true");

                int i = 1; // Buttons are 1-indexed

                var xmlActionsCopy = xmlActions.Cast<Windows.Data.Xml.Dom.IXmlNode>().ToList();

                foreach (var node in xmlActionsCopy)
                {
                    var actionNode = node as Windows.Data.Xml.Dom.XmlElement;

                    if (actionNode != null)
                    {
                        switch (i)
                        {
                            case 1:
                                switch ((Button1StyleCombo.SelectedItem as ComboBoxItem)!.Tag.ToString())
                                {
                                    case "success":
                                        actionNode.SetAttribute("hint-buttonStyle", "Success");
                                        break;
                                    case "crit":
                                        actionNode.SetAttribute("hint-buttonStyle", "Critical");
                                        break;
                                }

                                if (!string.IsNullOrEmpty(Button1ToolTipBox.Text))
                                {
                                    actionNode.SetAttribute("hint-toolTip", Button1ToolTipBox.Text);
                                }
                                break;
                            case 2:
                                switch ((Button2StyleCombo.SelectedItem as ComboBoxItem)!.Tag.ToString())
                                {
                                    case "success":
                                        actionNode.SetAttribute("hint-buttonStyle", "Success");
                                        break;
                                    case "crit":
                                        actionNode.SetAttribute("hint-buttonStyle", "Critical");
                                        break;
                                }

                                if (!string.IsNullOrEmpty(Button2ToolTipBox.Text))
                                {
                                    actionNode.SetAttribute("hint-toolTip", Button2ToolTipBox.Text);
                                }
                                break;
                            case 3:
                                switch ((Button3StyleCombo.SelectedItem as ComboBoxItem)!.Tag.ToString())
                                {
                                    case "success":
                                        actionNode.SetAttribute("hint-buttonStyle", "Success");
                                        break;
                                    case "crit":
                                        actionNode.SetAttribute("hint-buttonStyle", "Critical");
                                        break;
                                }

                                if (!string.IsNullOrEmpty(Button3ToolTipBox.Text))
                                {
                                    actionNode.SetAttribute("hint-toolTip", Button3ToolTipBox.Text);
                                }
                                break;
                            case 4:
                                switch ((Button4StyleCombo.SelectedItem as ComboBoxItem)!.Tag.ToString())
                                {
                                    case "success":
                                        actionNode.SetAttribute("hint-buttonStyle", "Success");
                                        break;
                                    case "crit":
                                        actionNode.SetAttribute("hint-buttonStyle", "Critical");
                                        break;
                                }

                                if (!string.IsNullOrEmpty(Button4ToolTipBox.Text))
                                {
                                    actionNode.SetAttribute("hint-toolTip", Button4ToolTipBox.Text);
                                }
                                break;
                            case 5:
                                switch ((Button5StyleCombo.SelectedItem as ComboBoxItem)!.Tag.ToString())
                                {
                                    case "success":
                                        actionNode.SetAttribute("hint-buttonStyle", "Success");
                                        break;
                                    case "crit":
                                        actionNode.SetAttribute("hint-buttonStyle", "Critical");
                                        break;
                                }

                                if (!string.IsNullOrEmpty(Button5ToolTipBox.Text))
                                {
                                    actionNode.SetAttribute("hint-toolTip", Button5ToolTipBox.Text);
                                }
                                break;
                        }

                        i++;
                    }
                }
            }

            return doc;
        }

        private async void BuildShowNotifyCard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var inputBoxCount = 0;

                if (InputBox1Toggle.IsOn) inputBoxCount++;
                if (InputBox2Toggle.IsOn) inputBoxCount++;
                if (InputBox3Toggle.IsOn) inputBoxCount++;
                if (InputBox4Toggle.IsOn) inputBoxCount++;
                if (InputBox5Toggle.IsOn) inputBoxCount++;

                if ((comboBoxes.Count + inputBoxCount) > 5)
                {
                    var result = await new ContentDialog
                    {
                        XamlRoot = RootGrid.XamlRoot,
                        Title = "Input Limits",
                        Content = "Notifications have a maximum limit of 5 inputs (input and combo boxes), so some of your combo boxes won't be displayed. This is due to a Windows limitation. Do you want to continue?",
                        PrimaryButtonText = "Continue anyway",
                        SecondaryButtonText = "Cancel",
                        DefaultButton = ContentDialogButton.Primary
                    }.ShowAsync();

                    if (result != ContentDialogResult.Primary)
                    {
                        return;
                    }
                }

                doc = BuildNotification();

                ShowNotifyCard.IsEnabled = true;
                //ExportXmlCard.IsEnabled = true;

                // Workaround because SetText throws an exception if the box is read only
                XmlPresenter.IsReadOnly = false;
                XmlPresenter.Document.SetText(TextSetOptions.None, doc.GetPrettyXml());
                var f = new XamlTextFormatter(XmlPresenter); // XAML and XML are similar enough
                f.ApplyColors();
                XmlPresenter.IsReadOnly = true;
            }
            catch (Exception ex)
            {
                var msg = "An error occured when building the notification: " + ex.Message;

#if DEBUG
                msg += "\n\nSTACK TRACE:\n" + ex.StackTrace;
#endif

                _ = new ContentDialog
                {
                    XamlRoot = RootGrid.XamlRoot,
                    Title = "Notification Builder",
                    Content = msg,
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close
                }.ShowAsync();
            }
        }

        private Uri GetSoundUri(AppNotificationSoundEvent soundEvent)
        {
            string soundName = soundEvent switch
            {
                AppNotificationSoundEvent.Default => "ms-winsoundevent:Notification.Default",
                AppNotificationSoundEvent.IM => "ms-winsoundevent:Notification.IM",
                AppNotificationSoundEvent.Mail => "ms-winsoundevent:Notification.Mail",
                AppNotificationSoundEvent.Reminder => "ms-winsoundevent:Notification.Reminder",
                AppNotificationSoundEvent.SMS => "ms-winsoundevent:Notification.SMS",
                AppNotificationSoundEvent.Alarm => "ms-winsoundevent:Notification.Looping.Alarm",
                AppNotificationSoundEvent.Alarm2 => "ms-winsoundevent:Notification.Looping.Alarm2",
                AppNotificationSoundEvent.Alarm3 => "ms-winsoundevent:Notification.Looping.Alarm3",
                AppNotificationSoundEvent.Alarm4 => "ms-winsoundevent:Notification.Looping.Alarm4",
                AppNotificationSoundEvent.Alarm5 => "ms-winsoundevent:Notification.Looping.Alarm5",
                AppNotificationSoundEvent.Alarm6 => "ms-winsoundevent:Notification.Looping.Alarm6",
                AppNotificationSoundEvent.Alarm7 => "ms-winsoundevent:Notification.Looping.Alarm7",
                AppNotificationSoundEvent.Alarm8 => "ms-winsoundevent:Notification.Looping.Alarm8",
                AppNotificationSoundEvent.Alarm9 => "ms-winsoundevent:Notification.Looping.Alarm9",
                AppNotificationSoundEvent.Alarm10 => "ms-winsoundevent:Notification.Looping.Alarm10",
                AppNotificationSoundEvent.Call => "ms-winsoundevent:Notification.Looping.Call",
                AppNotificationSoundEvent.Call2 => "ms-winsoundevent:Notification.Looping.Call2",
                AppNotificationSoundEvent.Call3 => "ms-winsoundevent:Notification.Looping.Call3",
                AppNotificationSoundEvent.Call4 => "ms-winsoundevent:Notification.Looping.Call4",
                AppNotificationSoundEvent.Call5 => "ms-winsoundevent:Notification.Looping.Call5",
                AppNotificationSoundEvent.Call6 => "ms-winsoundevent:Notification.Looping.Call6",
                AppNotificationSoundEvent.Call7 => "ms-winsoundevent:Notification.Looping.Call7",
                AppNotificationSoundEvent.Call8 => "ms-winsoundevent:Notification.Looping.Call8",
                AppNotificationSoundEvent.Call9 => "ms-winsoundevent:Notification.Looping.Call9",
                AppNotificationSoundEvent.Call10 => "ms-winsoundevent:Notification.Looping.Call10",
                _ => "ms-winsoundevent:Notification.Default"
            };

            return new Uri(soundName);
        }

        private void HeroURIBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ImageNotificationElement? old = Elements.FirstOrDefault(x =>
            {
                if (x is ImageNotificationElement img)
                {
                    if (img.Type == ImageNotificationElementType.HeroImage)
                    {
                        return true;
                    }
                }

                return false;
            }) as ImageNotificationElement;

            if (old != null)
            {
                if (!string.IsNullOrEmpty(HeroURIBox.Text))
                {
                    old.Uri = HeroURIBox.Text;
                }
                else
                {
                    Elements.Remove(old);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(HeroURIBox.Text))
                {
                    var element = new ImageNotificationElement();
                    element.Type = ImageNotificationElementType.HeroImage;
                    element.Uri = HeroURIBox.Text;
                    Elements.Add(element);
                }
            }
        }

        private void InlineImgURIBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ImageNotificationElement? old = Elements.FirstOrDefault(x =>
            {
                if (x is ImageNotificationElement img)
                {
                    if (img.Type == ImageNotificationElementType.InlineImage)
                    {
                        return true;
                    }
                }

                return false;
            }) as ImageNotificationElement;

            if (old != null)
            {
                if (!string.IsNullOrEmpty(InlineImgURIBox.Text))
                {
                    old.Uri = InlineImgURIBox.Text;
                }
                else
                {
                    Elements.Remove(old);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(InlineImgURIBox.Text))
                {
                    var element = new ImageNotificationElement();
                    element.Type = ImageNotificationElementType.InlineImage;
                    element.Uri = InlineImgURIBox.Text;
                    Elements.Add(element);
                }
            }
        }

        private void AppLogoOverrideURIBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ImageNotificationElement? old = Elements.FirstOrDefault(x =>
            {
                if (x is ImageNotificationElement img)
                {
                    if (img.Type == ImageNotificationElementType.AppLogoOverride)
                    {
                        return true;
                    }
                }

                return false;
            }) as ImageNotificationElement;

            if (old != null)
            {
                if (!string.IsNullOrEmpty(AppLogoOverrideURIBox.Text))
                {
                    old.Uri = AppLogoOverrideURIBox.Text;
                }
                else
                {
                    Elements.Remove(old);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(AppLogoOverrideURIBox.Text))
                {
                    var element = new ImageNotificationElement();
                    element.Type = ImageNotificationElementType.AppLogoOverride;
                    element.Uri = AppLogoOverrideURIBox.Text;
                    Elements.Add(element);
                }
            }
        }

        private async void AppLogoOverrideBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            await BrowseForImage(AppLogoOverrideBrowseButton, AppLogoOverrideURIBox);
        }

        private async Task BrowseForImage(Button browseButton, TextBox box)
        {
            var wnd = MainWindow.Instance;
            if (wnd == null) return;

            browseButton.IsEnabled = false;
            // Create a file picker
            var openPicker = new FileOpenPicker(wnd.AppWindow.Id);

            // Set options for the file picker
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".gif");
            openPicker.FileTypeFilter.Add(".bmp");

            // Open the picker for the user to pick a file
            var result = await openPicker.PickSingleFileAsync();
            if (result != null)
            {
                var file = await StorageFile.GetFileFromPathAsync(result.Path);

                if (file.FileType.Equals(".gif", StringComparison.InvariantCultureIgnoreCase))
                {
                    bool animated = await IsGifAnimatedAsync(file);

                    if (animated)
                    {
                        _ = new ContentDialog
                        {
                            Title = "Error",
                            Content = "Notifications don't support animated GIFs. Please try with a non-animated GIF or with a different file type.",
                            CloseButtonText = "OK",
                            DefaultButton = ContentDialogButton.Close,
                            XamlRoot = RootGrid.XamlRoot
                        }.ShowAsync();

                        browseButton.IsEnabled = true;

                        return;
                    }
                }

                var newPath = "file:///" + file.Path.Replace('\\', '/');

                box.Text = newPath;
            }

            browseButton.IsEnabled = true;
        }

        private async Task<bool> IsGifAnimatedAsync(StorageFile gifFile)
        {
            using var stream = await gifFile.OpenAsync(FileAccessMode.Read);
            var decoder = await BitmapDecoder.CreateAsync(stream);
            return decoder.FrameCount > 1;
        }

        private async void InlineImageBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            await BrowseForImage(InlineImageBrowseButton, InlineImgURIBox);
        }

        private async void HeroImageBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            await BrowseForImage(HeroImageBrowseButton, HeroURIBox);
        }

        private void ComboBoxCardItemAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button)
            {
                if (button.DataContext is EditorToastComboBox combo)
                {
                    combo.Items.Add(new EditorToastComboBoxItem());
                }
            }
        }

        private void ComboBoxCardItemDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is AppBarButton button)
            {
                if (button.DataContext is EditorToastComboBox edit)
                {
                    if (button.Tag is EditorToastComboBoxItem item)
                    {
                        edit.Items.Remove(item);
                    }
                }
            }
        }

        private void AddComboBoxButton_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxes.Count < 5)
            {
                comboBoxes.Add(new EditorToastComboBox());

                AddComboBoxButton.Label = $"Add\n{comboBoxes.Count} / 5";
                AddComboBoxButton.IsEnabled = comboBoxes.Count < 5;
            }
        }

        private async void ComboBoxCardDeleteButton(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button)
            {
                if (button.DataContext is EditorToastComboBox edit)
                {
                    var dlg = new ContentDialog
                    {
                        XamlRoot = RootGrid.XamlRoot,
                        Title = "Delete Combo Box",
                        Content = "Are you sure you want to delete this combo box?",
                        PrimaryButtonText = "Delete",
                        SecondaryButtonText = "Cancel",
                        DefaultButton = ContentDialogButton.Primary
                    };

                    var result = await dlg.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        comboBoxes.Remove(edit);

                        AddComboBoxButton.Label = $"Add\n{comboBoxes.Count} / 5";
                        AddComboBoxButton.IsEnabled = comboBoxes.Count < 5;
                    }
                }
            }
        }

        private async void Button1IconBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            await BrowseForImage(Button1IconBrowseButton, Button1IconUriBox);
        }

        private async void Button2IconBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            await BrowseForImage(Button2IconBrowseButton, Button2IconUriBox);
        }

        private async void Button3IconBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            await BrowseForImage(Button3IconBrowseButton, Button3IconUriBox);
        }

        private async void Button4IconBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            await BrowseForImage(Button4IconBrowseButton, Button4IconUriBox);
        }

        private async void Button5IconBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            await BrowseForImage(Button5IconBrowseButton, Button5IconUriBox);
        }

        private void ShowNotifyCard_Click(object sender, RoutedEventArgs e)
        {
            if (doc != null)
            {
                var t = new AppNotification(doc.GetXml());

                AppNotificationManager.Default.Show(t);
            }
        }

        private async void ImportXmlCard_Click(object sender, RoutedEventArgs e)
        {
            var wnd = MainWindow.Instance;
            if (wnd == null) return;

            // INCOMPLETE XML READ CODE; may be finished in future versions

            // Create a file picker
            var picker = new FileOpenPicker(wnd.AppWindow.Id);

            picker.FileTypeFilter.Add(".xml");
            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            var result = await picker.PickSingleFileAsync();

            if (result != null)
            {
                using (Stream stream = (await (await StorageFile.GetFileFromPathAsync(result.Path)).OpenReadAsync()).AsStreamForRead())
                {
                    using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(stream))
                    {
                        reader.ReadStartElement("toast");
                        var launch = reader.GetAttribute("launch");
                        if (launch != null)
                        {
                            ArgsBox.Text = launch;
                        }

                        reader.ReadStartElement("visual");
                        reader.ReadStartElement("binding");
                        var bindingTemp = reader.GetAttribute("template");
                        if (bindingTemp == "toastGeneric")
                        {
                            while (reader.Read())
                            {
                                if (reader.NodeType != System.Xml.XmlNodeType.Element
                                    && reader.NodeType != System.Xml.XmlNodeType.EndElement
                                    && reader.Name != "text"
                                    && reader.Name != "image") break;

                                if (reader.NodeType == System.Xml.XmlNodeType.Element)
                                {
                                    if (reader.Name == "text") // Text
                                    {
                                        Elements.Add(new TextNotificationElement
                                        {
                                            Text = reader.ReadContentAsString()
                                        });
                                    }
                                    else if (reader.Name == "image")
                                    {
                                        var src = reader.GetAttribute("src");
                                        if (src != null)
                                        {
                                            var placement = reader.GetAttribute("placement");
                                            if (placement != null)
                                            {
                                                switch (placement)
                                                {
                                                    case "hero":
                                                        HeroURIBox.Text = src;
                                                        break;
                                                    case "appLogoOverride":
                                                        AppLogoOverrideURIBox.Text = src;
                                                        break;
                                                }
                                                ;
                                            }
                                            else
                                            {
                                                InlineImgURIBox.Text = src;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        reader.ReadEndElement();
                        reader.ReadEndElement();

                        reader.ReadStartElement("actions");
                        if (reader.GetAttribute("hint-systemCommands") == "SnoozeAndDismiss")
                        {
                            AutoSnoozeDismissToggle.IsOn = true;
                        }
                        else
                        {
                            // Buttons
                            for (int i = 0; i < 5; i++)
                            {
                                try
                                {
                                    reader.ReadStartElement("action");

                                    var content = reader.GetAttribute("content");
                                    var imageUri = reader.GetAttribute("imageUri");
                                    var hint_buttonStyle = reader.GetAttribute("hint-buttonStyle");
                                    var hint_toolTip = reader.GetAttribute("hint-toolTip");
                                    var args = reader.GetAttribute("arguments");
                                    var activateType = reader.GetAttribute("activationType");
                                    var isSnoozeDismiss = activateType == "system";
                                    var placement = reader.GetAttribute("placement");

                                    switch (i)
                                    {
                                        case 0:
                                            if (content != null)
                                                Button1TextBox.Text = content;

                                            if (imageUri != null)
                                                Button1IconUriBox.Text = imageUri;

                                            if (hint_buttonStyle != null)
                                                Button1StyleCombo.SelectedItem = Button1StyleCombo.Items.FirstOrDefault(x =>
                                                {
                                                    if (x is ComboBoxItem item)
                                                    {
                                                        if (item.Tag.ToString() == "success" && hint_buttonStyle == "Success")
                                                        {
                                                            return true;
                                                        }
                                                        else if (item.Tag.ToString() == "crit" && hint_buttonStyle == "Critical")
                                                        {
                                                            return true;
                                                        }
                                                    }

                                                    return false;
                                                });
                                            else
                                                Button1StyleCombo.SelectedItem = Button1StyleCombo.Items.FirstOrDefault(x =>
                                                {
                                                    if (x is ComboBoxItem item)
                                                    {
                                                        if (item.Tag.ToString() == "regular")
                                                        {
                                                            return true;
                                                        }
                                                    }

                                                    return false;
                                                });

                                            if (hint_toolTip != null)
                                                Button1ToolTipBox.Text = hint_toolTip;

                                            if (isSnoozeDismiss)
                                                Button1TypeCombo.SelectedItem = Button1TypeCombo.Items.FirstOrDefault(x =>
                                                {
                                                    if (x is ComboBoxItem item)
                                                    {
                                                        if (item.Tag.ToString() == args) // Should be the same, as the proper notification values are "snooze" or "dismiss" and combo box items with that same tag exist
                                                        {
                                                            return true;
                                                        }
                                                    }
                                                    return false;
                                                });
                                            else if (placement == "contextMenu")
                                                Button1TypeCombo.SelectedItem = Button1TypeCombo.Items.FirstOrDefault(x =>
                                                {
                                                    if (x is ComboBoxItem item)
                                                    {
                                                        if (item.Tag.ToString() == "context")
                                                        {
                                                            return true;
                                                        }
                                                    }
                                                    return false;
                                                });

                                            break;
                                        case 1:
                                            if (content != null)
                                                Button2TextBox.Text = content;

                                            if (imageUri != null)
                                                Button2IconUriBox.Text = imageUri;

                                            if (hint_buttonStyle != null)
                                                Button2StyleCombo.SelectedItem = Button2StyleCombo.Items.FirstOrDefault(x =>
                                                {
                                                    if (x is ComboBoxItem item)
                                                    {
                                                        if (item.Tag.ToString() == "success" && hint_buttonStyle == "Success")
                                                        {
                                                            return true;
                                                        }
                                                        else if (item.Tag.ToString() == "crit" && hint_buttonStyle == "Critical")
                                                        {
                                                            return true;
                                                        }
                                                    }

                                                    return false;
                                                });
                                            else
                                                Button2StyleCombo.SelectedItem = Button2StyleCombo.Items.FirstOrDefault(x =>
                                                {
                                                    if (x is ComboBoxItem item)
                                                    {
                                                        if (item.Tag.ToString() == "regular")
                                                        {
                                                            return true;
                                                        }
                                                    }

                                                    return false;
                                                });

                                            if (hint_toolTip != null)
                                                Button2ToolTipBox.Text = hint_toolTip;

                                            if (isSnoozeDismiss)
                                                Button2TypeCombo.SelectedItem = Button2TypeCombo.Items.FirstOrDefault(x =>
                                                {
                                                    if (x is ComboBoxItem item)
                                                    {
                                                        if (item.Tag.ToString() == args) // Should be the same, as the proper notification values are "snooze" or "dismiss" and combo box items with that same tag exist
                                                        {
                                                            return true;
                                                        }
                                                    }
                                                    return false;
                                                });
                                            else if (placement == "contextMenu")
                                                Button2TypeCombo.SelectedItem = Button2TypeCombo.Items.FirstOrDefault(x =>
                                                {
                                                    if (x is ComboBoxItem item)
                                                    {
                                                        if (item.Tag.ToString() == "context")
                                                        {
                                                            return true;
                                                        }
                                                    }
                                                    return false;
                                                });

                                            break;
                                        case 2:
                                            if (content != null)
                                                Button3TextBox.Text = content;

                                            if (imageUri != null)
                                                Button3IconUriBox.Text = imageUri;

                                            if (hint_buttonStyle != null)
                                                Button3StyleCombo.SelectedItem = Button3StyleCombo.Items.FirstOrDefault(x =>
                                                {
                                                    if (x is ComboBoxItem item)
                                                    {
                                                        if (item.Tag.ToString() == "success" && hint_buttonStyle == "Success")
                                                        {
                                                            return true;
                                                        }
                                                        else if (item.Tag.ToString() == "crit" && hint_buttonStyle == "Critical")
                                                        {
                                                            return true;
                                                        }
                                                    }

                                                    return false;
                                                });
                                            else
                                                Button3StyleCombo.SelectedItem = Button3StyleCombo.Items.FirstOrDefault(x =>
                                                {
                                                    if (x is ComboBoxItem item)
                                                    {
                                                        if (item.Tag.ToString() == "regular")
                                                        {
                                                            return true;
                                                        }
                                                    }

                                                    return false;
                                                });

                                            if (hint_toolTip != null)
                                                Button3ToolTipBox.Text = hint_toolTip;

                                            if (isSnoozeDismiss)
                                                Button3TypeCombo.SelectedItem = Button3TypeCombo.Items.FirstOrDefault(x =>
                                                {
                                                    if (x is ComboBoxItem item)
                                                    {
                                                        if (item.Tag.ToString() == args) // Should be the same, as the proper notification values are "snooze" or "dismiss" and combo box items with that same tag exist
                                                        {
                                                            return true;
                                                        }
                                                    }
                                                    return false;
                                                });
                                            else if (placement == "contextMenu")
                                                Button3TypeCombo.SelectedItem = Button3TypeCombo.Items.FirstOrDefault(x =>
                                                {
                                                    if (x is ComboBoxItem item)
                                                    {
                                                        if (item.Tag.ToString() == "context")
                                                        {
                                                            return true;
                                                        }
                                                    }
                                                    return false;
                                                });

                                            break;

                                        case 3:
                                            if (content != null)
                                                Button4TextBox.Text = content;

                                            if (imageUri != null)
                                                Button4IconUriBox.Text = imageUri;

                                            if (hint_buttonStyle != null)
                                                Button4StyleCombo.SelectedItem = Button4StyleCombo.Items.FirstOrDefault(x =>
                                                {
                                                    if (x is ComboBoxItem item)
                                                    {
                                                        if (item.Tag.ToString() == "success" && hint_buttonStyle == "Success")
                                                        {
                                                            return true;
                                                        }
                                                        else if (item.Tag.ToString() == "crit" && hint_buttonStyle == "Critical")
                                                        {
                                                            return true;
                                                        }
                                                    }

                                                    return false;
                                                });
                                            else
                                                Button4StyleCombo.SelectedItem = Button4StyleCombo.Items.FirstOrDefault(x =>
                                                {
                                                    if (x is ComboBoxItem item)
                                                    {
                                                        if (item.Tag.ToString() == "regular")
                                                        {
                                                            return true;
                                                        }
                                                    }

                                                    return false;
                                                });

                                            if (hint_toolTip != null)
                                                Button4ToolTipBox.Text = hint_toolTip;

                                            if (isSnoozeDismiss)
                                                Button4TypeCombo.SelectedItem = Button4TypeCombo.Items.FirstOrDefault(x =>
                                                {
                                                    if (x is ComboBoxItem item)
                                                    {
                                                        if (item.Tag.ToString() == args) // Should be the same, as the proper notification values are "snooze" or "dismiss" and combo box items with that same tag exist
                                                        {
                                                            return true;
                                                        }
                                                    }
                                                    return false;
                                                });
                                            else if (placement == "contextMenu")
                                                Button4TypeCombo.SelectedItem = Button4TypeCombo.Items.FirstOrDefault(x =>
                                                {
                                                    if (x is ComboBoxItem item)
                                                    {
                                                        if (item.Tag.ToString() == "context")
                                                        {
                                                            return true;
                                                        }
                                                    }
                                                    return false;
                                                });

                                            break;
                                        case 4:
                                            if (content != null)
                                                Button5TextBox.Text = content;

                                            if (imageUri != null)
                                                Button5IconUriBox.Text = imageUri;

                                            if (hint_buttonStyle != null)
                                                Button5StyleCombo.SelectedItem = Button5StyleCombo.Items.FirstOrDefault(x =>
                                                {
                                                    if (x is ComboBoxItem item)
                                                    {
                                                        if (item.Tag.ToString() == "success" && hint_buttonStyle == "Success")
                                                        {
                                                            return true;
                                                        }
                                                        else if (item.Tag.ToString() == "crit" && hint_buttonStyle == "Critical")
                                                        {
                                                            return true;
                                                        }
                                                    }

                                                    return false;
                                                });
                                            else
                                                Button5StyleCombo.SelectedItem = Button5StyleCombo.Items.FirstOrDefault(x =>
                                                {
                                                    if (x is ComboBoxItem item)
                                                    {
                                                        if (item.Tag.ToString() == "regular")
                                                        {
                                                            return true;
                                                        }
                                                    }

                                                    return false;
                                                });

                                            if (hint_toolTip != null)
                                                Button5ToolTipBox.Text = hint_toolTip;

                                            if (isSnoozeDismiss)
                                                Button5TypeCombo.SelectedItem = Button5TypeCombo.Items.FirstOrDefault(x =>
                                                {
                                                    if (x is ComboBoxItem item)
                                                    {
                                                        if (item.Tag.ToString() == args) // Should be the same, as the proper notification values are "snooze" or "dismiss" and combo box items with that same tag exist
                                                        {
                                                            return true;
                                                        }
                                                    }
                                                    return false;
                                                });
                                            else if (placement == "contextMenu")
                                                Button5TypeCombo.SelectedItem = Button5TypeCombo.Items.FirstOrDefault(x =>
                                                {
                                                    if (x is ComboBoxItem item)
                                                    {
                                                        if (item.Tag.ToString() == "context")
                                                        {
                                                            return true;
                                                        }
                                                    }
                                                    return false;
                                                });

                                            break;
                                    }

                                    reader.ReadEndElement();
                                }
                                catch (System.Xml.XmlException)
                                {
                                    break;
                                }
                            }

                            // Inputs
                            for (int i = 0; i < 5; i++)
                            {
                                try
                                {
                                    reader.ReadStartElement("input");

                                    var type = reader.GetAttribute("type");
                                    var id = reader.GetAttribute("id");
                                    var title = reader.GetAttribute("title");
                                    var defaultInput = reader.GetAttribute("defaultInput");

                                    if (type == "text") // Input box
                                    {
                                        var placeHolderContent = reader.GetAttribute("placeHolderContent");

                                        (RootGrid.FindName($"InputBox{i + 1}Toggle") as ToggleSwitch)!.IsOn = true;

                                        if (title != null)
                                            (RootGrid.FindName($"Input{i + 1}TitleBox") as TextBox)!.Text = title;

                                        if (id != null)
                                            (RootGrid.FindName($"Input{i + 1}IdBox") as TextBox)!.Text = id;

                                        if (defaultInput != null)
                                            (RootGrid.FindName($"Input{i + 1}DefaultBox") as TextBox)!.Text = defaultInput;

                                        if (placeHolderContent != null)
                                            (RootGrid.FindName($"Input{i + 1}PlaceholderBox") as TextBox)!.Text = placeHolderContent;
                                    }
                                    else if (type == "selection") // Combo box
                                    {
                                        var combo = new EditorToastComboBox();

                                        if (defaultInput != null)
                                            combo.DefaultItemId = defaultInput;

                                        if (id != null)
                                            combo.Id = id;

                                        if (title != null)
                                            combo.Name = title;

                                        while (reader.Read())
                                        {
                                            if (reader.NodeType != System.Xml.XmlNodeType.Element
                                                || reader.NodeType != System.Xml.XmlNodeType.EndElement
                                                || reader.Name != "selection") break;

                                            var iid = reader.GetAttribute("id");
                                            var content = reader.GetAttribute("content");

                                            var item = new EditorToastComboBoxItem();

                                            if (iid != null)
                                                item.Id = iid;

                                            if (content != null)
                                                item.Content = content;

                                            combo.Items.Add(item);
                                        }
                                    }

                                    reader.ReadEndElement();
                                }
                                catch (System.Xml.XmlException)
                                {
                                    break;
                                }
                            }
                        }
                        reader.ReadEndElement();

                        reader.ReadEndElement();
                    }
                }

            }
        }

        private async void ExportXmlCard_Click(object sender, RoutedEventArgs e)
        {
            if (doc != null)
            {
                var wnd = MainWindow.Instance;
                if (wnd == null) return;

                // Create a file picker
                var picker = new FileSavePicker(wnd.AppWindow.Id);

                // Set options for the file picker
                picker.FileTypeChoices.Add("XML Notification Data", [".xml"]);

                // Open the picker for the user to pick a file
                var result = await picker.PickSaveFileAsync();

                if (result != null)
                {
                    var file = await StorageFile.GetFileFromPathAsync(result.Path);

                    CachedFileManager.DeferUpdates(file);

                    var xml = doc.GetPrettyXml();
                    await FileIO.WriteTextAsync(file, xml);

                    var r = await CachedFileManager.CompleteUpdatesAsync(file);

                    var text = r switch
                    {
                        FileUpdateStatus.Complete => "The XML data was exported successfully!",
                        FileUpdateStatus.CompleteAndRenamed => "The XML data was exported successfully, but the file has been renamed.",
                        FileUpdateStatus.CurrentlyUnavailable => "The XML data couldn't be exported because the file is currently unavailable.",
                        FileUpdateStatus.Incomplete => "The export operation was interrupted and the XML file is incomplete.",
                        FileUpdateStatus.Failed => "The export operation failed.",
                        FileUpdateStatus.UserInputNeeded => "User input is required to export the XML file.",
                        _ => $"A message wasn't found for this status code: FileUpdateStatus.{result}."
                    };

                    var title = r switch
                    {
                        FileUpdateStatus.Complete or FileUpdateStatus.CompleteAndRenamed => "File Export Success",
                        FileUpdateStatus.Incomplete => "File Export Incomplete",
                        FileUpdateStatus.CurrentlyUnavailable => "File Unavailable",
                        FileUpdateStatus.UserInputNeeded => "User Input Required",
                        FileUpdateStatus.Failed => "File Export Failed",
                        _ => "File Export"
                    };

                    var success = r switch
                    {
                        FileUpdateStatus.Complete or FileUpdateStatus.CompleteAndRenamed or FileUpdateStatus.UserInputNeeded => true,
                        _ => false
                    };

                    var dlg = new ContentDialog
                    {
                        XamlRoot = RootGrid.XamlRoot,
                        Title = title,
                        Content = text,
                        CloseButtonText = "OK",
                        DefaultButton = ContentDialogButton.Close
                    };

                    if (success)
                    {
                        dlg.PrimaryButtonText = "Open file location";
                    }

                    var dlgResult = await dlg.ShowAsync();

                    if (dlgResult == ContentDialogResult.Primary)
                    {
                        //var psi = new ProcessStartInfo
                        //{
                        //    FileName = "explorer.exe",
                        //    Arguments = $"/select,{file.Path}",
                        //    ErrorDialog = true,
                        //    UseShellExecute = true
                        //};
                        //Process.Start(psi);

                        _ = Launcher.LaunchFolderAsync(await file.GetParentAsync(), new FolderLauncherOptions
                        {
                            ItemsToSelect =
                            {
                                file
                            }
                        });
                    }
                }
            }
        }

        private void fullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            var wnd = MainWindow.Instance;
            if (wnd == null) return;

            if (wnd.AppWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen)
            {
                wnd.AppWindow.SetPresenter(AppWindowPresenterKind.Default);
                fullScreenButton.Text = "Full-screen";

                titleBarGrid.Visibility = Visibility.Visible;
                fullscreenMenuButton.Visibility = Visibility.Collapsed;

                wnd.ExtendsContentIntoTitleBar = true;
                wnd.SetTitleBar(titleBarGrid);

                RootGrid.RowDefinitions.Clear();
            }
            else
            {
                wnd.AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
                fullScreenButton.Text = "Exit full-screen";

                titleBarGrid.Visibility = Visibility.Collapsed;
                fullscreenMenuButton.Visibility = Visibility.Visible;

                wnd.ExtendsContentIntoTitleBar = false;
                wnd.SetTitleBar(null);

                RootGrid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(0, GridUnitType.Auto)
                });

                RootGrid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(1, GridUnitType.Star)
                });
            }
                        
        }

        private void settingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var wnd = MainWindow.Instance;
            if (wnd == null) return;

            wnd.mainFrame.Navigate(typeof(ConfigPage));
        }
    }

    /// <summary>
    /// The base class for elements in an <see cref="AppNotification"/>.
    /// </summary>
    public abstract class NotificationElement
    {

    }

    /// <summary>
    /// A <see cref="NotificationElement"/> for text on the notification. Up to 3 <see cref="TextNotificationElement"/>s can exist per notification.
    /// </summary>
    public class TextNotificationElement : NotificationElement
    {
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>
    /// A <see cref="NotificationElement"/> that displays images on the notification. Up to 3 <see cref="ImageNotificationElement"/>s can exist per notification, and only 1 can exist per type.
    /// </summary>
    public class ImageNotificationElement : NotificationElement
    {
        public string Uri { get; set; } = string.Empty;
        public ImageNotificationElementType Type { get; set; } = ImageNotificationElementType.InlineImage;
    }

    public enum ImageNotificationElementType
    {
        InlineImage,
        HeroImage,
        AppLogoOverride
    }

    public sealed partial class FrameworkElementTagToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is FrameworkElement element)
            {
                return element.Tag == parameter;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException("Converting a boolean to a FrameworkElement is impossible, so this conversion is not implemented. Avoid using TwoWay bindings with this converter.");
        }
    }

    public class EditorToastComboBox : DependencyObject
    {
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
            "Name",
            typeof(string),
            typeof(EditorToastComboBox),
            new PropertyMetadata(string.Empty));

        public string Name
        {
            get => (string)GetValue(NameProperty);
            set => SetValue(NameProperty, value);
        }
        public string Id { get; set; } = string.Empty;
        public string DefaultItemId { get; set; } = string.Empty;
        public ObservableCollection<EditorToastComboBoxItem> Items { get; set; } = [];

        public override string ToString()
        {
            return Name;
        }
    }

    public class EditorToastComboBoxItem
    {
        public string Content { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;

        public override string ToString()
        {
            return Content + " (" + Id + ")";
        }
    }
}
