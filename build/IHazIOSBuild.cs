using System;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Nuke.Components;
using Serilog;

[PublicAPI]
public interface IHazIOSBuild :
    IHazArtifacts,
    IHazBuildConfiguration,
    IHazIOSCertificate,
    IRestoreAppleProvisioningProfile,
    IRestore,
    IHazMauiWorkload,
    IHazNerdbankGitVersioning
{
    Target CompileIos => _ => _
        .OnlyWhenStatic(() => EnvironmentInfo.Platform == PlatformFamily.OSX)
        .DependsOn(RestoreIOSCertificate, DownloadProvisioningProfile, InstallWorkload, Restore)
        .Produces(ArtifactsDirectory / "*.ipa")
        .Executes(() =>
        {
            var buildVersion = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 1656042000;
            var displayVersion = Versioning.NuGetPackageVersion;

            Log.Information($"Display Version: {displayVersion}");
            Log.Information($"Build Version: {buildVersion}");
            DotNetTasks.DotNetPublish(settings =>
                settings.SetConfiguration(Configuration)
                    .SetFramework("net6.0-ios")
                    .AddProperty("IsPublishing", true)
                    .AddProperty("ArchiveOnBuild", true)
                    .AddProperty("ApplicationDisplayVersion", Versioning.NuGetPackageVersion)
                    .AddProperty("ApplicationVersion", buildVersion)
                    .SetOutput(ArtifactsDirectory));
        });
}
