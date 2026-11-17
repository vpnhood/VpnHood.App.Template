# AGENTS.md

This repository is one VPN app built on VpnHood, for Android, iOS and the Apple TV, Windows and Linux. The app's own
files are in `app/`; `src/` holds a head for each platform and store; everything else comes from VpnHood's NuGet
packages. [README.md](README.md) is the full guide: read it before you change anything.

## Where a change goes

Keep the user's changes in `app/`, so the template's updates merge into this repository cleanly. Edit a head in `src/`
only for what `app/` cannot say, such as an Android file the README names.

| To ... | Change |
| --- | --- |
| rename the app, or change its maker, ids or version | `app/app.props` |
| connect to the user's own server | `DefaultAccessKey` in `app/appsettings.json` |
| change the theme, the links, sign-in or analytics | the other keys of `app/appsettings.json`, which the README lists |
| change the logo | `app/branding/images/logo.png` |
| change the icons | the four SVGs and `app.ico` in `app/icons/` |
| change the privacy summary of the first run, or add a language to it | `app/branding/content/<language>/privacy-consent.md` |
| take a newer VpnHood | `<VpnHoodVersion>` in `app/app.props` |

## Rules that break a release when ignored

- `VhAppIdBase` names the app in the stores and on every device: never change it after the first release.
- The third number of `Version` counts releases and only goes up, one for each release, even when the first two change.
- `DefaultAccessKey` opens the user's servers: never commit a real one to a public repository.
- Leave the id, name, version and App Group lines of `src/VpnApp.Ios.Extension` as they are: iOS accepts the extension
  only under the app's ids.

## Build and check

| Build | Command | Needs |
| --- | --- | --- |
| Windows | `dotnet build src/VpnApp.Windows.Web` | the .NET 11 SDK |
| Linux | `dotnet build src/VpnApp.Linux.Web` | the .NET 11 SDK |
| Android | `dotnet build src/VpnApp.Android.Google` | the `android` workload |
| iOS and the Apple TV | `dotnet build src/VpnApp.Ios.Apple -f net11.0-ios` (or `net11.0-tvos`) | the `ios` and `tvos` workloads, and a Mac with Xcode |

The Windows or Linux build is the quickest check of a change. A Debug build without a `DefaultAccessKey` connects to
VpnHood's test server.

## VpnHood's documentation

- [developer.vpnhood.com](https://developer.vpnhood.com): the guides, a page for every package and the API reference.
  [Build your VPN app](https://developer.vpnhood.com/articles/build-your-app.html) is this template's guide.
- [llms.txt](https://developer.vpnhood.com/llms.txt) indexes the site for AI tools, and
  [llms-full.txt](https://developer.vpnhood.com/llms-full.txt) holds its guides as plain text.
- The packages carry their doc comments: read a type's for its contract rather than guessing from its name.
- To run VPN servers and give out access keys from code, the user's own or a billing system's: the
  [VpnHood! MANAGER API](https://developer.vpnhood.com/manager-api/).
