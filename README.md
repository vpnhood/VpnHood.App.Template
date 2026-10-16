# VpnHood App Template

> [!WARNING]
> **Under construction.** This template isn't ready for use yet, and any part of it may still
> change.

Your own VPN app on VpnHood, for Android (Google Play and your website), iOS (App Store), Windows and
Linux. Everything VpnHood comes from NuGet: this repository holds only your app.

## Make it your app

Change what's in `app/`:

1. **`app/app.props`**: your app's id base and name, your company, and its version. Every build
   takes its ids from them (the Android packages, the iOS bundles and App Group, the Windows and
   Linux ids), and the name it shows. Choose the base before your first release: the stores and
   every install know your app by these ids, so they can't change later. `Company` is you as the
   app's maker, named in the consent summary, the other documents the app shows and the
   executables' details.

   `Version` is three numbers, and its third counts your releases: 1 for the first, as it stands,
   and one more for each after it, committed with the release. Never set it back, even when you raise the first two
   (1.0.7, then 1.1.8): it's Android's version code and iOS's build number, which the stores take
   only higher, and a phone installs no update from your website with a lower one.

   The iOS network extension builds its bundle id, its name, its versions and the App Group from
   these values in its own project (`src/VpnApp.Ios.Extension`). Leave those lines as they are: iOS
   accepts the extension only under an id that starts with the app's, App Store Connect wants its
   versions to be the app's, and the app and the extension share their data only through that one
   App Group.
2. **`app/appsettings.json`**: your settings. A key you leave out, or set to `null`, is off.

   | Key | What it is |
   | --- | --- |
   | `UiTheme` | `violet` or `blue`. |
   | `DefaultAccessKey` | The access key of your server, the one the app connects to. A Debug build without one uses VpnHood's sample server. |
   | `Links` | The pages the app links to: `PrivacyPolicy` and `TermsOfUse` (the paywall, Settings > Privacy and the first run), and the menu's `Website`, `WhatsNew`, `Feedback`, `LinkedIn`, `Instagram` and `X`. A link left out isn't shown. Without `TermsOfUse`, the App Store build links Apple's standard EULA; with it, register the same terms as the app's EULA in App Store Connect. |
   | `ShowPoweredBy` | The menu's "Powered by VpnHood! ENGINE"; `false` hides it. |
   | `WebUiPort` | The port of the app's local web host, which a paired phone's address names; two apps on one device need two. Left out, 9090, and 9091 in a Debug build. |
   | `PortalBaseUri` | Your portal, for sign-in, plans and purchases. Without it the app has no accounts. |
   | `GoogleSignInClientId` | Google sign-in on the Play build; required once `PortalBaseUri` is set. |
   | `RemoteSettingsUrl` | Settings the app fetches while it runs. |
   | `Ga4MeasurementId`, `AllowEndPointTracker` | Analytics, off unless set. The iOS build never sends any. |
   | `UpdateInfoBaseUrl` | Where the website APK's update feeds are ([Publish the website build](#publish-the-website-build)). Left out, that build hears of no update. |
   | `WebReleaseCertificateSha256` | The SHA-256 of the certificate your website APK is signed with, in hex (`apksigner verify --print-certs` shows it). The app downloads and installs its update only over a copy signed with it; left out, a Release build shows the update's link instead. |

   An `appsettings.Debug.json` or `appsettings.Release.json` beside it overrides the keys it names,
   in that configuration only.
3. **`app/branding/`**: what the app shows as its own. Each file here replaces the VpnHood UI's file
   of the same path.

   | File | What it is |
   | --- | --- |
   | `images/logo.png` | Your logo: a square PNG, 150×150 or larger. The app shows it at 50×50. |
   | `content/en/privacy-consent.md` | The summary the app shows on first run, with `{appName}` and `{companyName}` filled in. It's legal text, so make it say what you and your servers actually do. For another language, add `content/<language>/privacy-consent.md`; a language without one shows English. |

The access key gives access to your servers, so keep it out of a public repository.

Then replace the icons, which are neutral placeholders, with yours:

- **Android** (both projects): `Resources/mipmap-*` and `Resources/drawable`, including the TV banner
  (`ic_banner`) and the white-on-transparent notification and Quick Settings icons; the colours are
  in `Resources/values`.
- **iOS** (both projects): `Assets.xcassets/AppIcon.appiconset`. Keep the images opaque; iOS rounds
  the corners itself.
- **Windows**: `Resources/app.ico`.

## Build

| Build | Project | Needs |
| --- | --- | --- |
| Google Play | `src/VpnApp.Android.Google` | .NET SDK with the `android` workload |
| Website APK | `src/VpnApp.Android.Web` | .NET SDK with the `android` workload |
| App Store | `src/VpnApp.Ios.Apple` (bundles `src/VpnApp.Ios.Extension`) | .NET 11 SDK with the `ios` workload, and a Mac with Xcode to sign and run it |
| Windows | `src/VpnApp.Windows.Web` | .NET SDK |
| Linux | `src/VpnApp.Linux.Web` | .NET SDK |

For example, `dotnet build src/VpnApp.Windows.Web`.

The VpnHood packages come at the version this template names. To take another, a newer VpnHood for
example, add `<VpnHoodVersion>9.0.900</VpnHoodVersion>` to `app/app.props`, with the version you want.

## Publish the website build

A publish signs the website APK and, once `UpdateInfoBaseUrl` is set, writes its update feed beside
it, in the project's `publish` folder. Each release takes two:

```shell
dotnet publish src/VpnApp.Android.Web -c Release <signing>
dotnet publish src/VpnApp.Android.Web -c Release -p:VpnAppArm64Apk=true <signing>
```

The first makes `<title>-android-web.apk`, for every phone, and its feed `<title>-android-web.json`;
the second the arm64 APK, a third the size, and `<title>-android-web-arm64.json`. Put all four at
`UpdateInfoBaseUrl` with every release: a phone with an arm64 processor, which most are, reads only
the arm64 feed, and any other only the universal one. `<signing>` names your keystore
(`-p:AndroidKeyStore=true -p:AndroidSigningKeyStore=… -p:AndroidSigningKeyAlias=…
-p:AndroidSigningStorePass=… -p:AndroidSigningKeyPass=…`). Sign every release with the same key:
Android installs an update only over a copy signed with it.

## Not in the template yet

- Ads, crash reports, and install attribution.
- The Windows and Linux builds' installers and update feeds, and publishing to the stores.
- The Avalonia UI's browser build: the page a phone opens when it's paired with a TV.
