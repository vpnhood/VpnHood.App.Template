using Foundation;
using Microsoft.Extensions.Logging;
using VpnHood.AppLogic.Abstractions.Accounts;
using VpnHood.AppLogic.App;
using VpnHood.AppLogic.App.Ios;
using VpnHood.AppLogic.App.Services.Updaters;
using VpnHood.AppLogic.Portal;
using VpnHood.AppLogic.Stores.AppStore;
using VpnHood.AppHosting.Avalonia.Ios;
using VpnHood.AppUi.Avalonia.Classic;
using VpnHood.Core.Client.Abstractions;
using VpnHood.Net.Toolkit.Logging;

namespace VpnApp.Ios.Apple;

// The App Store build, on iOS and the Apple TV: the app starts from the params below as launching
// finishes, then the UI, which takes its TV layout on a TV.
[Register("AppDelegate")]
public class AppDelegate : IosAvaloniaAppDelegate<ClassicAvaloniaApp>
{
    // The platform builds the device from the App Group and the extension's bundle id.
    protected override IosInitParams CreateInitParams()
    {
        return new IosInitParams {
            // the bundle's own id, which the build took from app/app.props
            AppId = NSBundle.MainBundle.BundleIdentifier ??
                    throw new InvalidOperationException("The app's bundle has no identifier."),
            AppName = AppConstants.AppName,
            AppVersion = Version.Parse(AppConstants.AppVersion),
            StorageFolderName = "app",
            AppGroupId = AppConstants.AppGroupId,
            ProviderBundleId = AppConstants.ProviderBundleId,
            AppOptionsFactory = CreateAppOptions
        };
    }

    // The app's options, and the App Store's lines on top.
    private static AppOptions CreateAppOptions(AppOptionsContext context)
    {
        var appConfigs = VpnAppConfigs.Load();
        var options = VpnAppOptions.Create(context, appConfigs);

        // Apple holds a VPN app to more than other apps: nothing about its use goes to a third party,
        // so this build sends no analytics whatever the settings say.
        options.TrackerFactories = [];
        options.AllowEndPointTracker = false;
        // A purchase here is under the licence App Store Connect names: Apple's standard EULA, unless you
        // register your terms (Links.TermsOfUse) there; terms you don't register must name Apple's EULA.
        // Without terms, the app links that EULA itself.
        options.Links = options.Links with {
            TermsOfUse = options.Links.TermsOfUse ??
                         new Uri("https://www.apple.com/legal/internet-services/itunes/dev/stdeula/")
        };
        // The first-run summary stays on, unlike in the Google Play build: App Review 5.4 wants a VPN app
        // to say what it collects on a screen before first use, and the store's own pages are not one.
        // The premium tier stays as VpnAppOptions makes it: the App Store forbids unlocking with a
        // code typed in, and steering a buyer to your own shop.
        options.AccountProvider = CreateAccountProvider(appConfigs, context);
#if TVOS
        // StoreKit has no in-app rating prompt on tvOS, and a TV is never a Mac.
        options.Transport = ClientTransportOptions.ForCurrentPlatform();
#else
        options.UserReviewProvider = new AppStoreInAppUserReviewProvider();
        // "Designed for iPad" on a Mac runs the extension without iOS's memory cap, yet reports iOS:
        // only Foundation tells it apart.
        options.Transport = NSProcessInfo.ProcessInfo.IsiOSApplicationOnMac
            ? ClientTransportOptions.NormalMemory
            : ClientTransportOptions.ForCurrentPlatform();
#endif
        options.LogServiceOptions = new LogServiceOptions {
            MinLogLevel = LogLevel.Information
        };
        // Updates come from the App Store: the provider finds the released version by bundle id.
        options.UpdaterOptions = new AppUpdaterOptions {
            UpdaterProvider = new AppStoreAppUpdaterProvider(options.AppVersion)
        };
        return options;
    }

    private static IAccountProvider? CreateAccountProvider(VpnAppConfigs appConfigs, AppOptionsContext context)
    {
        try {
            // no portal: the app has no account at all rather than half of one
            if (appConfigs.PortalBaseUri == null)
                return null;

            // Sign in with Apple, StoreKit billing, and your portal behind both: the portal maps each
            // store product to the plan it redeems.
            var authenticationProvider = new PortalAuthenticationProvider(context.NoBackupStorage,
                appConfigs.PortalBaseUri, context.AppId, [new AppleAuthenticationProvider()]);
            return new PortalAccountProvider(authenticationProvider, new AppStoreBillingProvider(),
                portalBaseUrl: appConfigs.PortalBaseUri, packageName: context.AppId);
        }
        catch (Exception ex) {
            VhLogger.Instance.LogError(ex, "Could not create the account provider.");
            return null;
        }
    }
}
