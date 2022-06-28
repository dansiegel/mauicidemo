using System;
using System.IO;
using System.Linq;
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
        .Produces(ArtifactsDirectory)
        .Executes(() =>
        {
            DotNetPublish(settings =>
                settings.SetConfiguration(Configuration)
                    .SetFramework("net6.0-android")
                    .AddProperty("AndroidSigningKeyPass", Android_Keystore_Password)
                    .AddProperty("AndroidSigningStorePass", Android_Keystore_Password)
                    .AddProperty("AndroidSigningKeyAlias", Android_Keystore_Name)
                    .AddProperty("AndroidSigningKeyStore", KeystorePath)
                    .AddProperty("ApplicationDisplayVersion", $"{Versioning.VersionMajor}.{Versioning.VersionMinor}.{Versioning.VersionRevision}")
                    .AddProperty("ApplicationVersion", DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 1656042000)
                    .SetOutput(ArtifactsDirectory));

            Assert.True(Directory.EnumerateFiles(ArtifactsDirectory, "*-Signed.apk").Any(), "No Signed APK was found in the output directory");
            Assert.True(Directory.EnumerateFiles(ArtifactsDirectory, "*-Signed.aab").Any(), "No Signed AAB was found in the output directory");
        });
}
