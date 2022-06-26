using System;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Nuke.Components;
using static  Nuke.Common.Tools.DotNet.DotNetTasks;

[PublicAPI]
public interface IHazAndroidBuild :
    IHazArtifacts,
    IHazBuildConfiguration,
    IRestore,
    IHazMauiWorkload,
    IHazAndroidKeystore,
    IHazNerdbankGitVersioning
{
    Target CompileAndroid => _ => _
        .DependsOn(RestoreKeystore, InstallWorkload, Restore)
        .Produces(ArtifactsDirectory / "*-Signed.apk", ArtifactsDirectory / "*-Signed.aab")
        .Executes(() =>
        {
            DotNetPublish(settings =>
                settings.SetConfiguration(Configuration)
                    .SetFramework("net6.0-android")
                    .AddProperty("AndroidSigningKeyPass", ANDROID_KEYSTORE_PASSWORD)
                    .AddProperty("AndroidSigningStorePass", ANDROID_KEYSTORE_PASSWORD)
                    .AddProperty("AndroidSigningKeyAlias", ANDROID_KEYSTORE_NAME)
                    .AddProperty("AndroidSigningKeyStore", KeystorePath)
                    .AddProperty("ApplicationDisplayVersion", $"{Versioning.VersionMajor}.{Versioning.VersionMinor}.{Versioning.VersionRevision}")
                    .AddProperty("ApplicationVersion", DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 1656042000)
                    .SetOutput(ArtifactsDirectory));
        });
}
