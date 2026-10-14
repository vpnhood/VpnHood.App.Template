using VpnHood.AppLib.App.Utils;

namespace VpnApp;

// What app/appsettings.json can say: the settings every VpnHood app has (AppConfigs) and this app's
// own below. None has a value in code: a key the file leaves out is null, and what needs it is off,
// or fails where it is used.
public class VpnAppConfigs : AppConfigs
{
    // Your portal: sign-in, plans and purchases. Left out, the app has no account at all.
    public Uri? PortalBaseUri { get; set; }

    // Google sign-in on the Play build: the OAuth client id Google issued for your package and your
    // signing key. The Play build needs it once the portal is set.
    public string? GoogleSignInClientId { get; set; }

    // The SHA-256 of the certificate your website's APK is signed with, in hex: that build downloads
    // and installs its update only over a copy signed with it, as Android refuses any other. Left
    // out, a Release build keeps the update's link instead.
    public string? WebReleaseCertificateSha256 { get; set; }

    // The settings as this project embeds them, for every build.
    public static VpnAppConfigs Load() => AppConfigsLoader.Load<VpnAppConfigs>();
}
