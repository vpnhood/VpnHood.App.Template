# VpnHood App Template

> [!WARNING]
> **Under construction.** This template isn't ready for use yet. It builds only against VpnHood
> packages that haven't been published, and any part of it may still change.

Your own VPN app on VpnHood, for Android (Google Play and your website), iOS (App Store), Windows and
Linux. Everything VpnHood comes from NuGet: this repository holds only your app.

## Make it your app

Change what's in `app/`:

1. **`app/app.props`**: your app's id base and name. Every build takes its ids from them (the
   Android packages, the iOS bundles and App Group, the Windows and Linux ids), and the name it
   shows. Choose the base before your first release: the stores and every install know your app by
   these ids, so they can't change later.
2. **`app/appsettings.json`**: your settings. A key you leave out, or set to `null`, is off.

   | Key | What it is |
   | --- | --- |
   | `CompanyName` | Your name as the app's maker, used in the consent summary and the other documents the app shows. |
   | `UiTheme` | `violet` or `blue`. |
   | `DefaultAccessKey` | The access key of your server, the one the app connects to. A Debug build without one uses VpnHood's sample server. |
   | `PrivacyPolicyUrl`, `TermsOfUseUrl` | The links the app shows. |
   | `PortalBaseUri` | Your portal, for sign-in, plans and purchases. Without it the app has no accounts. |
   | `GoogleSignInClientId` | Google sign-in on the Play build; required once `PortalBaseUri` is set. |
   | `RemoteSettingsUrl` | Settings the app fetches while it runs. |
   | `Ga4MeasurementId`, `AllowEndPointTracker` | Analytics, off unless set. The iOS build never sends any. |

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

To take a newer VpnHood, raise `VpnHoodVersion` in `Directory.Build.props`.

## Not in the template yet

- Ads, crash reports, and install attribution.
- Update feeds for the website builds, installers, and publishing to the stores.
- The Avalonia UI's browser build: the page a phone opens when it's paired with a TV.
