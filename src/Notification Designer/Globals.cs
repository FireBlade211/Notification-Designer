using Microsoft.Windows.ApplicationModel.WindowsAppRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification_Designer
{
    public static class Globals
    {
        public static string Version => "v1.0";
        public static string WinAppSdkRuntimeDetails => $"Windows App SDK {ReleaseInfo.AsString}" +
            $", Windows App Runtime {RuntimeInfo.AsString}";
    }
}
