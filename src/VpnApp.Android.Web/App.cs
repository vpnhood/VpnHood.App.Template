using Android.Runtime;
using Microsoft.Extensions.Logging;
using VpnHood.AppLib.Abstractions.Accounts;
using VpnHood.AppLib.App;
using VpnHood.AppLib.App.Android.Constants;
using VpnHood.AppLib.Portal;
using VpnHood.AppUi.Hosting.Avalonia.Android;
using VpnHood.AppUi.Presentation.Classic.Avalonia;
using VpnHood.Net.Toolkit.Logging;

namespace VpnApp.Android.Web;

[Application(
    Label = AppConstants.AppName,
    Icon = AndroidAppConstants.Icon,
    Banner = AndroidAppConstants.Banner,
    NetworkSecurityConfig = AndroidAppConstants.NetworkSecurityConfig,
    SupportsRtl = AndroidAppConstants.SupportsRtl,
    Debuggable = AppConstants.IsDebugMode,
    AllowBackup = AndroidAppConstants.AllowBackup)]
// The website's APK: the app starts from the params below, then the UI, which MainActivity shows.
public class App(IntPtr javaReference, JniHandleOwnership transfer)
    : AndroidAvaloniaApplication<ClassicAvaloniaApp>(javaReference, transfer)
{
    // Called by the platform only in the app's own process: never in the VPN service's or the tile's.
    protected override AppStartParams CreateStartParams()
    {
        return new AppStartParams {
            AppId = PackageName ?? throw new InvalidOperationException("The app has no package name."),
            StorageFolderName = "app",
            AppOptionsFactory = CreateAppOptions
        };
    }

    // The app's options, and this build's lines on top.
    private static AppOptions CreateAppOptions(AppOptionsContext context)
    {
        var appConfigs = VpnAppConfigs.Load();
        var options = VpnAppOptions.Create(context, appConfigs, AppConstants.AppName, AppConstants.IsDebugMode);
        options.AdjustForSystemBars = false;
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

            // The portal's own sign-in, not Google's: an Android OAuth client is bound to a package name
            // and a signing key, and this build shares neither with the Play build. Checkout is in the
            // browser.
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
