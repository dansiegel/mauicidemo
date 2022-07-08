using System;
using System.Collections.Generic;
using AvantiPoint.Nuke.Maui;
using AvantiPoint.Nuke.Maui.Apple;
using AvantiPoint.Nuke.Maui.CI;
using AvantiPoint.Nuke.Maui.CI.GitHubActions;
using AvantiPoint.Nuke.Maui.Windows;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tools.NerdbankGitVersioning;

[GitHubWorkflow(typeof(DemoBuild))]
class Build : MauiBuild
{
    public static int Main() => Execute<Build>();

    public GitHubActions GitHubActions => GitHubActions.Instance;

    [NerdbankGitVersioning]
    readonly NerdbankGitVersioning NerdbankVersioning;

    public override string ApplicationDisplayVersion => NerdbankVersioning.SimpleVersion;
    public override long ApplicationVersion => IsLocalBuild ?
        (DateTimeOffset.Now.ToUnixTimeSeconds() - new DateTimeOffset(2022, 7, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds()) / 60 :
        GitHubActions.RunNumber;
}

public class DemoBuild : CIBuild
{
    public override PushTrigger OnPush => "master";
    public override IEnumerable<ICIStage> Stages => new[]
    {
        new CIStage
        {
            Jobs = new ICIJob[]
            {
                new AndroidJob(),
                new DemoIOSJob(),
                WindowsJob.AzureSigned,
            }
        }
    };
}

public class DemoIOSJob : iOSJob
{
    public override SecretImportCollection ImportSecrets => new()
    {
        { nameof(IHazAppleCertificate.P12B64), "IOS_P12_B64" },
        { nameof(IHazAppleCertificate.P12Password), "IOS_P12_PASSWORD" },
        nameof(IRestoreAppleProvisioningProfile.AppleIssuerId),
        nameof(IRestoreAppleProvisioningProfile.AppleKeyId),
        nameof(IRestoreAppleProvisioningProfile.AppleAuthKeyP8),
        nameof(IRestoreAppleProvisioningProfile.AppleProfileId)
    };
}
