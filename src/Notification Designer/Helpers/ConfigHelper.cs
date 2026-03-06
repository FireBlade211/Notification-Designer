using Microsoft.UI.Xaml;
using Microsoft.Windows.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification_Designer.Helpers
{
    public static class ConfigHelper
    {
        public static object? ReadValue(string key)
        {
            using (ApplicationData data = ApplicationData.GetDefault())
            {
                if (data.LocalSettings.Values.TryGetValue(key, out var val))
                {
                    return val;
                }
            }

            return null;
        }

        public static void WriteValue(string key, object value)
        {
            using (ApplicationData data = ApplicationData.GetDefault())
            {
                data.LocalSettings.Values[key] = value;
            }
        }
    }
}
