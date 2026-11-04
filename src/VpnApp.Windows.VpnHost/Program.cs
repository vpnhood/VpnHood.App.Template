using VpnHood.Core.Client.Devices.WindowsTun;

namespace VpnApp.Windows.VpnHost;

// The VPN service's own process, which the app's service starts at each connect.
internal static class Program
{
    private static Task<int> Main(string[] args)
    {
        return WindowsVpnHost.Run(args);
    }
}
