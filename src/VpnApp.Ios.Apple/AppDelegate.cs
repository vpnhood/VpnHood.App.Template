using Foundation;
using Microsoft.Extensions.Logging;
using VpnHood.AppLib.Abstractions.Accounts;
using VpnHood.AppLib.App;
using VpnHood.AppLib.App.Ios;
using VpnHood.AppLib.App.Services.Updaters;
using VpnHood.AppLib.Portal;
using VpnHood.AppLib.Stores.AppStore;
using VpnHood.AppUi.Hosting.Avalonia.Ios;
using VpnHood.AppUi.Presentation.Classic.Avalonia;
using VpnHood.Core.Client.Abstractions;
using VpnHood.Core.Client.VpnServices.Abstractions.Tracking;
using VpnHood.Net.Toolkit.Logging;

namespace VpnApp.Ios.Apple;

// The App Store build: the app starts from the params below as launching finishes, then the UI.
[Register("AppDelegate")]
public class AppDelegate : IosAvaloniaAppDelegate<ClassicAvaloniaApp>
{
    // The platform builds the device from the App Group and the extension's bundle id.
    protected override IosStartParams CreateStartParams()
    {
        return new IosStartParams {
            // the bundle's own id, which the build took from app/app.props
            AppId = NSBundle.MainBundle.BundleIdentifier ??
                    throw new InvalidOperationException("The app's bundle has no identifier."),
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
        var options = VpnAppOptions.Create(context, appConfigs, AppConstants.AppName, AppConstants.IsDebugMode);

        // Apple holds a VPN app to more than other apps: nothing about its use goes to a third party,
        // so this build sends no analytics whatever the settings say.
        options.Ga4MeasurementId = null;
        options.TrackerFactory = new NullTrackerFactory();
        options.AllowEndPointTracker = false;
        // A purchase here is governed by Apple's standard EULA until you register your own in App
        // Store Connect; then name yours in the settings and delete this line.
        options.TermsOfUseUrl = new Uri("https://www.apple.com/legal/internet-services/itunes/dev/stdeula/");
        // the store took the person's acceptance at install
        options.IsLicenseAgreementRequired = false;
        options.UserReviewProvider = new AppStoreInAppUserReviewProvider();
        // The premium tier stays as VpnAppOptions makes it: the App Store forbids unlocking with a
        // code typed in, and steering a buyer to your own shop.
        options.AccountProvider = CreateAccountProvider(appConfigs, context);
        // the UI pads itself to the safe areas the platform reports
        options.AdjustForSystemBars = false;
        // "Designed for iPad" on a Mac runs the extension without iOS's memory cap, yet reports iOS:
        // only Foundation tells it apart.
        options.Transport = NSProcessInfo.ProcessInfo.IsiOSApplicationOnMac
            ? ClientTransportOptions.NormalMemory
            : ClientTransportOptions.ForCurrentPlatform();
        options.LogServiceOptions = new LogServiceOptions {
            MinLogLevel = LogLevel.Information
        };
        // Updates come from the App Store: the provider finds the released version by bundle id.
        options.UpdaterOptions = new AppUpdaterOptions {
            UpdaterProvider = new AppStoreAppUpdaterProvider()
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
            var authenticationProvider = new PortalAuthenticationProvider(context.StoragePath,
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
