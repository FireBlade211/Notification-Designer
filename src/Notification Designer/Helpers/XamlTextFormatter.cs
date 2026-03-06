using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Notification_Designer.Helpers
{
    public class XamlTextFormatter(RichEditBox richEditBox)
    {
        private RichEditBox m_richEditBox = richEditBox;

        enum ZoneType
        {
            Unknown,
            OpenTag,
            EndTag,
            TagName,
            PropertyName,
            PropertyValue,
            Whitespace,
            Comment,
            Content
        }

        public void ApplyColors()
        {
            var doc = m_richEditBox.Document;
            doc.BeginUndoGroup();

            string rebText;
            doc.GetText(TextGetOptions.None, out rebText);

            var startIndex = 0;
            var currentZoneType = ZoneType.Unknown;
            bool inATag = false;
            for (var currIndex = 0; currIndex < rebText.Length; currIndex++)
            {
                if (char.IsWhiteSpace(rebText[currIndex]))
                {
                    if (currentZoneType != ZoneType.Whitespace)
                    {
                        UpdateZone(startIndex, currIndex, currentZoneType);
                        startIndex = currIndex;
                        currentZoneType = ZoneType.Whitespace;
                    }
                    // else already whitespace, so continue
                }
                else if (rebText[currIndex] == '<')
                {
                    UpdateZone(startIndex, currIndex, currentZoneType);
                    startIndex = currIndex;
                    if (rebText.Substring(currIndex).StartsWith("<!--"))
                    {
                        currentZoneType = ZoneType.Comment;
                        currIndex += 3;

                        // Look for end of comment
                        var endIndex = rebText.IndexOf("-->", currIndex + 1);
                        if (endIndex >= 0)
                        {
                            currIndex = endIndex + 2;
                        }
                    }
                    else
                    {
                        currentZoneType = ZoneType.OpenTag;
                        inATag = true;
                    }
                }
                else if (rebText[currIndex] == '/')
                {
                    // This could either be "</" at the start a tag like "</StackPanel>", or "/>" at
                    // the end of a tag like "<Button/>".
                    if (currentZoneType != ZoneType.OpenTag && currentZoneType != ZoneType.EndTag)
                    {
                        UpdateZone(startIndex, currIndex, currentZoneType);
                        startIndex = currIndex;
                        currentZoneType = ZoneType.EndTag;
                        inATag = false;
                    }
                    // else already end tag, so continue
                }
                else if (rebText[currIndex] == '>')
                {
                    if (currentZoneType != ZoneType.EndTag)
                    {
                        UpdateZone(startIndex, currIndex, currentZoneType);
                        startIndex = currIndex;
                        currentZoneType = ZoneType.EndTag;
                        inATag = false;
                    }
                    // else already end tag, so continue
                }
                else
                {
                    if (currentZoneType == ZoneType.OpenTag)
                    {
                        UpdateZone(startIndex, currIndex, currentZoneType);
                        startIndex = currIndex;
                        currentZoneType = ZoneType.TagName;
                    }
                    else if (currentZoneType == ZoneType.PropertyName && rebText[currIndex] == '=')
                    {
                        UpdateZone(startIndex, currIndex, currentZoneType);
                        startIndex = currIndex;
                        currentZoneType = ZoneType.PropertyValue;

                        // Look for the end of the value
                        if (currIndex < rebText.Length - 2 && rebText[currIndex + 1] == '"')
                        {
                            var endIndex = rebText.IndexOf("\"", currIndex + 2);
                            if (endIndex >= 0)
                            {
                                currIndex = endIndex;
                            }
                        }
                    }
                    else if (currentZoneType != ZoneType.TagName && currentZoneType != ZoneType.PropertyName
                         && currentZoneType != ZoneType.PropertyValue && inATag)
                    {
                        UpdateZone(startIndex, currIndex, currentZoneType);
                        startIndex = currIndex;
                        currentZoneType = ZoneType.PropertyName;
                    }
                    else if (!inATag)
                    {
                        UpdateZone(startIndex, currIndex, currentZoneType);
                        startIndex = currIndex;
                        currentZoneType = ZoneType.Content;
                    }
                    // else already ...other..., so continue
                }
            }
            doc.EndUndoGroup();
        }

        // endIndexExclusive is the index just after the zone.
        private void UpdateZone(int startIndex, int endIndexExclusive, ZoneType zoneType)
        {
            if (startIndex >= endIndexExclusive)
            {
                return;
            }

            bool lightTheme = m_richEditBox.ActualTheme != ElementTheme.Dark;

            var doc = m_richEditBox.Document;
            var range = doc.GetRange(startIndex, endIndexExclusive);
            Windows.UI.Color foregroundColor = lightTheme ? Microsoft.UI.Colors.Black : Microsoft.UI.Colors.White;
            switch (zoneType)
            {
                case ZoneType.OpenTag:
                case ZoneType.EndTag:
                    foregroundColor = lightTheme ? Microsoft.UI.Colors.Blue : Microsoft.UI.Colors.Gray;
                    break;

                case ZoneType.TagName:
                    foregroundColor = lightTheme ? Microsoft.UI.Colors.Brown : Microsoft.UI.Colors.White;
                    break;

                case ZoneType.PropertyName:
                    foregroundColor = lightTheme ? Microsoft.UI.Colors.Red : Microsoft.UI.Colors.LightSkyBlue;
                    break;

                case ZoneType.PropertyValue:
                    foregroundColor = lightTheme ? Microsoft.UI.Colors.Blue : Microsoft.UI.Colors.DodgerBlue;
                    break;

                case ZoneType.Whitespace:
                case ZoneType.Content:
                    foregroundColor = lightTheme ? Microsoft.UI.Colors.Black : Microsoft.UI.Colors.White;
                    break;

                case ZoneType.Comment:
                    foregroundColor = lightTheme ? Microsoft.UI.Colors.Green : Microsoft.UI.Colors.LimeGreen;
                    break;
            }

            range.CharacterFormat.ForegroundColor = foregroundColor;
        }
    }
}
