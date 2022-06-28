using System.Reflection;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Utilities.Collections;

[GitHubActions("android-build",
    GitHubActionsImage.WindowsLatest,
    FetchDepth = 0,
    AutoGenerate = true,
    OnPushBranches = new [] { MasterBranch },
    InvokedTargets = new [] { nameof(IHazAndroidBuild.CompileAndroid) },
    ImportSecrets = new [] { nameof(IHazAndroidKeystore.Android_Keystore_Name), nameof(IHazAndroidKeystore.Android_Keystore_B64), nameof(IHazAndroidKeystore.Android_Keystore_Password)}
    )]
[GitHubActions("ios-build",
    GitHubActionsImage.MacOsLatest,
    FetchDepth = 0,
    AutoGenerate = true,
    OnPushBranches = new [] { MasterBranch },
    InvokedTargets = new [] { nameof(IHazIOSBuild.CompileIos) },
    ImportSecrets = new []
    {
        nameof(IHazIOSCertificate.IOS_P12_B64),
        nameof(IHazIOSCertificate.IOS_P12_Password),
        nameof(IRestoreAppleProvisioningProfile.Apple_IssuerId),
        nameof(IRestoreAppleProvisioningProfile.Apple_KeyId),
        nameof(IRestoreAppleProvisioningProfile.Apple_AuthKey_P8),
        nameof(IRestoreAppleProvisioningProfile.Apple_ProfileId)
    }
)]
class Build : NukeBuild,
    IHazAndroidBuild,
    IHazIOSBuild
{
    public static int Main () => Execute<Build>(x => ((IHazAndroidBuild)x).CompileAndroid);

    const string MasterBranch = "master";

    [CI]
    readonly GitHubActions GitHubActions;

    protected override void WriteLogo()
    {
        Debug();
        new[]
        {
            "░█████╗░██╗░░░██╗░█████╗░███╗░░██╗████████╗██╗██████╗░░█████╗░██╗███╗░░██╗████████╗",
            "██╔══██╗██║░░░██║██╔══██╗████╗░██║╚══██╔══╝██║██╔══██╗██╔══██╗██║████╗░██║╚══██╔══╝",
            "███████║╚██╗░██╔╝███████║██╔██╗██║░░░██║░░░██║██████╔╝██║░░██║██║██╔██╗██║░░░██║░░░",
            "██╔══██║░╚████╔╝░██╔══██║██║╚████║░░░██║░░░██║██╔═══╝░██║░░██║██║██║╚████║░░░██║░░░",
            "██║░░██║░░╚██╔╝░░██║░░██║██║░╚███║░░░██║░░░██║██║░░░░░╚█████╔╝██║██║░╚███║░░░██║░░░",
            "╚═╝░░╚═╝░░░╚═╝░░░╚═╝░░╚═╝╚═╝░░╚══╝░░░╚═╝░░░╚═╝╚═╝░░░░░░╚════╝░╚═╝╚═╝░░╚══╝░░░╚═╝░░░",
        }.ForEach(x => Debug(x));
        Debug();
    }

    private void Debug(string text = null)
    {
        var hostType = typeof(Nuke.Common.Host);
        var method = hostType.GetMethod("Debug", BindingFlags.Static | BindingFlags.NonPublic);
        method.Invoke(null, new[] {text});
    }
}
