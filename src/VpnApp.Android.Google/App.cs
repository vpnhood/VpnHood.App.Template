using Android.Runtime;
using Microsoft.Extensions.Logging;
using VpnHood.AppLib.Abstractions.Accounts;
using VpnHood.AppLib.Abstractions.Billing;
using VpnHood.AppLib.App;
using VpnHood.AppLib.App.Android.Constants;
using VpnHood.AppLib.App.Services.Updaters;
using VpnHood.AppLib.Portal;
using VpnHood.AppLib.Stores.GooglePlay;
using VpnHood.AppUi.Hosting.Avalonia.Android;
using VpnHood.AppUi.Presentation.Classic.Avalonia;
using VpnHood.Net.Toolkit.Logging;

namespace VpnApp.Android.Google;

[Application(
    Label = AppConstants.AppName,
    Icon = AndroidAppConstants.Icon,
    Banner = AndroidAppConstants.Banner,
    NetworkSecurityConfig = AndroidAppConstants.NetworkSecurityConfig,
    SupportsRtl = AndroidAppConstants.SupportsRtl,
    Debuggable = AppConstants.IsDebugMode,
    AllowBackup = AndroidAppConstants.AllowBackup)]
// The Google Play build: the app starts from the params below, then the UI, which MainActivity shows.
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

    // The app's options, and Google Play's lines on top.
    private static AppOptions CreateAppOptions(AppOptionsContext context)
    {
        var appConfigs = VpnAppConfigs.Load();
        var options = VpnAppOptions.Create(context, appConfigs, AppConstants.AppName, AppConstants.IsDebugMode);
        // the store took the person's acceptance at install
        options.IsLicenseAgreementRequired = false;
        options.AdjustForSystemBars = false;
        options.UserReviewProvider = new GooglePlayInAppUserReviewProvider();
        options.AccountProvider = CreateAccountProvider(appConfigs, context);
        // A premium code may be typed in here; buying in your own shop may not: Play forbids steering
        // a buyer out of the store.
        options.Premium = VpnAppOptions.CreatePremium(allowImportAccessCode: true, isPurchaseUrlSupported: false);
        options.UpdaterOptions = new AppUpdaterOptions {
            UpdaterProvider = new GooglePlayAppUpdaterProvider()
        };
        return options;
    }

    private static IAccountProvider? CreateAccountProvider(VpnAppConfigs appConfigs, AppOptionsContext context)
    {
        try {
            // no portal: the app has no account at all rather than half of one
            if (appConfigs.PortalBaseUri == null)
                return null;

            var googleSignInClientId = appConfigs.GoogleSignInClientId ??
                throw new InvalidOperationException("app/appsettings.json names a portal but no GoogleSignInClientId.");

            // Google sign-in, Play's billing, and your portal behind both: the portal maps each store
            // product to the plan it redeems.
            var authenticationProvider = new PortalAuthenticationProvider(context.StoragePath,
                appConfigs.PortalBaseUri, context.AppId, [new GooglePlayAuthenticationProvider(googleSignInClientId)],
                restoreCredentialProvider: new GoogleRestoreCredentialProvider());
            return new PortalAccountProvider(authenticationProvider, TryCreateBillingProvider(),
                portalBaseUrl: appConfigs.PortalBaseUri, packageName: context.AppId);
        }
        catch (Exception ex) {
            VhLogger.Instance.LogError(ex, "Could not create the account provider.");
            return null;
        }
    }

    private static IBillingProvider? TryCreateBillingProvider()
    {
        try {
            return new GooglePlayBillingProvider();
        }
        catch (Exception ex) {
            VhLogger.Instance.LogError(ex, "Could not create the Google Play billing provider.");
            return null;
        }
    }
}
