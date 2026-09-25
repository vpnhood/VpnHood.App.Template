using VpnHood.AppLib.App.Utils;

namespace VpnApp;

// What app/appsettings.json can say: the settings every VpnHood app has (AppConfigs) and this app's
// own below. None has a value in code: a key the file leaves out is null, and what needs it is off,
// or fails where it is used.
public class VpnAppConfigs : AppConfigs
{
    // Your name as the app's maker, for the words that name you ({companyName} in the consent
    // summary and the other documents the UI shows).
    public string? CompanyName { get; set; }

    // The look: "violet" or "blue", the two the UI knows. Left out, the UI's own default.
    public string? UiTheme { get; set; }

    // Your portal: sign-in, plans and purchases. Left out, the app has no account at all.
    public Uri? PortalBaseUri { get; set; }

    // Google sign-in on the Play build: the OAuth client id Google issued for your package and your
    // signing key. The Play build needs it once the portal is set.
    public string? GoogleSignInClientId { get; set; }

    // The settings as this project embeds them, for every build.
    public static VpnAppConfigs Load() => AppConfigsLoader.Load<VpnAppConfigs>();
}
