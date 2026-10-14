using VpnHood.AppLib.Api.App;
using VpnHood.AppLib.Api.Premium;
using VpnHood.AppLib.Api.WebHost;
using VpnHood.AppLib.App;
using VpnHood.Core.Client.Abstractions;
using VpnHood.Core.Client.VpnServices.Abstractions.Tracking;
using VpnHood.Net.Toolkit.Assets;

namespace VpnApp;

// What every build of the app says the same way: its settings (app/appsettings.json), the premium
// tier it sells, and the files the VpnHood packages place beside every build. A build adds its
// store's lines on top - its billing, sign-in, review and updates - and the platform fills its own
// defaults after it.
public static class VpnAppOptions
{
    // One built-in key - your server's - and no way to add another. A constant, since the desktop
    // commands need it before any app exists.
    public const bool IsAddAccessKeySupported = false;

    // What the premium tier unlocks.
    private static readonly AppFeature[] PremiumFeatures = [
        AppFeature.CustomDns,
        AppFeature.AlwaysOn,
        AppFeature.QuickLaunch,
        AppFeature.SplitIpViaApp,
        AppFeature.SplitIpViaDevice,
        AppFeature.SplitDomain
    ];

    public static AppOptions Create(AppOptionsContext context, VpnAppConfigs appConfigs)
    {
        var packagedAssetProvider = context.PackagedAssetProvider;
        // your server's key; a Debug build without one starts with VpnHood's sample server
        var defaultAccessKey = appConfigs.DefaultAccessKey ??
                               (AppConstants.IsDebugMode ? ClientOptions.SampleAccessKey : null);
        return new AppOptions(context, AppConstants.IsDebugMode) {
            PackageTitle = AppConstants.PackageTitle,
            CompanyName = AppConstants.CompanyName,
            // The app's own, from app/branding: the consent summary names the app and its maker from
            // the app's name and CompanyName.
            LogoAssetPath = "images/logo.png",
            PrivacyConsentAssetName = "privacy-consent",
            Links = appConfigs.Links,
            ShowPoweredBy = appConfigs.ShowPoweredBy,
            UiTheme = appConfigs.UiTheme,
            // an empty key would not parse
            AccessKeys = string.IsNullOrEmpty(defaultAccessKey) ? [] : [defaultAccessKey],
            IsAddAccessKeySupported = IsAddAccessKeySupported,
            AllowRecommendUserReviewByServer = true,
            Premium = CreatePremium(allowImportAccessCode: false, isPurchaseUrlSupported: false),
            TrackerFactories = [new Ga4TrackerFactory { MeasurementId = appConfigs.Ga4MeasurementId }],
            RemoteSettingsUrl = appConfigs.RemoteSettingsUrl,
            AllowEndPointTracker = appConfigs.AllowEndPointTracker,
            AllowEndPointStrategy = appConfigs.AllowEndPointStrategy,
            WebUiPort = appConfigs.WebUiPort,
            IpLocationZipAsset = new Asset(packagedAssetProvider, "iplocations/IpLocations.zip"),
            // the app's own files (app/branding) first: each replaces the store's of the same path
            UiZipAssets = [
                new Asset(packagedAssetProvider, "assets/app.zip"),
                new Asset(packagedAssetProvider, "assets/ui.zip")
            ],
            // the page a paired phone opens: this same UI, as its browser build
            WebRootZipAsset = new Asset(packagedAssetProvider, "assets/web-root.zip"),
            WebHostFactory = new VpnHoodAppWebHostFactory()
        };
    }

    // The premium tier, with the two things only a store's rules decide: typing a premium code in,
    // and buying in your own shop outside the store. Create leaves both off, as the App Store
    // requires; a build whose store permits one replaces the tier with this.
    public static AppPremiumOptions CreatePremium(bool allowImportAccessCode, bool isPurchaseUrlSupported)
    {
        return new AppPremiumOptions {
            Features = PremiumFeatures,
            AllowImportAccessCode = allowImportAccessCode,
            IsPurchaseUrlSupported = isPurchaseUrlSupported
        };
    }
}
