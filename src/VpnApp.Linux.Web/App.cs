using Microsoft.Extensions.Logging;
using VpnHood.AppLib.Abstractions.Accounts;
using VpnHood.AppLib.App;
using VpnHood.AppLib.Portal;
using VpnHood.AppUi.Hosting.Avalonia.Desktop;
using VpnHood.AppUi.Hosting.Cli;
using VpnHood.AppUi.Hosting.Cli.Linux;
using VpnHood.AppUi.Presentation.Classic.Avalonia;
using VpnHood.Net.Toolkit.Logging;

namespace VpnApp.Linux.Web;

// The Linux build: the app to run when this process is the service, and the UI to show when it is
// the window. Which of the two a run is, the commands and the service itself are the host's.
internal static class App
{
    private static int Main(string[] args)
    {
        return LinuxCliHost.Run(args, new CliHeadParams {
            AppId = AppConstants.AppId,
            AppOptionsFactory = CreateAppOptions,
            IsAddAccessKeySupported = VpnAppOptions.IsAddAccessKeySupported,
            Ui = new AvaloniaDesktopHost<ClassicAvaloniaApp>()
        });
    }

    // The app's options, and this build's lines on top.
    private static AppOptions CreateAppOptions(AppOptionsContext context)
    {
        var appConfigs = VpnAppConfigs.Load();
        var options = VpnAppOptions.Create(context, appConfigs, AppConstants.AppName, AppConstants.IsDebugMode);
        options.LogServiceOptions.SingleLineConsole = false;
        // No store rules this build: a buyer may type a premium code in, or buy in your own shop.
        options.Premium = VpnAppOptions.CreatePremium(allowImportAccessCode: true, isPurchaseUrlSupported: true);
        options.AccountProvider = CreateAccountProvider(appConfigs, context);
        return options;
    }

    private static IAccountProvider? CreateAccountProvider(VpnAppConfigs appConfigs, AppOptionsContext context)
    {
        try {
            // no portal: the app has no account at all rather than half of one
            if (appConfigs.PortalBaseUri == null)
                return null;

            // the portal's own sign-in, and checkout in the browser of the person at the window
            var authenticationProvider = new PortalAuthenticationProvider(context.StoragePath,
                appConfigs.PortalBaseUri, context.AppId, []);
            var billingProvider = new PortalWebBillingProvider(appConfigs.PortalBaseUri, context.AppId);
            return new PortalAccountProvider(authenticationProvider, billingProvider: billingProvider,
                portalBaseUrl: appConfigs.PortalBaseUri, packageName: context.AppId);
        }
        catch (Exception ex) {
            VhLogger.Instance.LogError(ex, "Could not create the account provider.");
            return null;
        }
    }
}
