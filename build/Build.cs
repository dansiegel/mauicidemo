using System;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;

[GitHubActions("android-build",
    GitHubActionsImage.MacOsLatest,
    FetchDepth = 0,
    AutoGenerate = true,
    OnPushBranches = new [] { MasterBranch },
    InvokedTargets = new [] { nameof(IHazAndroidBuild.CompileAndroid) },
    ImportSecrets = new [] { nameof(IHazAndroidKeystore.ANDROID_KEYSTORE_NAME), nameof(IHazAndroidKeystore.ANDROID_KEYSTORE_B64), nameof(IHazAndroidKeystore.ANDROID_KEYSTORE_PASSWORD)}
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
}
