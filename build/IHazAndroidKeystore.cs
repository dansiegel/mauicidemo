using System.IO;
using System.Text;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.IO;

[PublicAPI]
public interface IHazAndroidKeystore : INukeBuild
{
    [Parameter, Secret]
    string ANDROID_KEYSTORE_B64 => TryGetValue(() => ANDROID_KEYSTORE_B64);

    [Parameter, Secret]
    string ANDROID_KEYSTORE_NAME => TryGetValue(() => ANDROID_KEYSTORE_NAME);

    [Parameter, Secret]
    string ANDROID_KEYSTORE_PASSWORD => TryGetValue(() => ANDROID_KEYSTORE_PASSWORD);

    AbsolutePath KeystorePath => (AbsolutePath) Path.Combine(EnvironmentInfo.WorkingDirectory, $"{ANDROID_KEYSTORE_NAME}");

    Target RestoreKeystore => _ => _
        .Executes(() =>
        {
            ANDROID_KEYSTORE_B64.NotNullOrEmpty("The Android KeyStore Must be available as a Base64 encoded string.");
            ANDROID_KEYSTORE_NAME.NotNullOrEmpty("The Android KeyStore Name is required.");

            var contents = Encoding.Default.GetBytes(ANDROID_KEYSTORE_B64!);
            File.WriteAllBytes(KeystorePath, contents);
        });
}
