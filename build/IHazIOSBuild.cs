using System;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Nuke.Components;

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
        .DependsOn(RestoreIOSCertificate, DownloadProvisioningProfile, InstallWorkload, Restore)
        .Produces(ArtifactsDirectory)
        .Executes(() =>
        {
            DotNetTasks.DotNetPublish(settings =>
                settings.SetConfiguration(Configuration)
                    .SetFramework("net6.0-ios")
                    .AddProperty("ArchiveOnBuild", true)
                    .AddProperty("ApplicationDisplayVersion", Versioning.NuGetPackageVersion)
                    .AddProperty("ApplicationVersion", DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 1656042000)
                    .SetOutput(ArtifactsDirectory));
        });
}
