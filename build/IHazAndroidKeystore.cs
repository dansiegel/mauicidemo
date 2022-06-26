using System.IO;
using System.Text;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.IO;

[PublicAPI]
public interface IHazAndroidKeystore : INukeBuild
{
    [Parameter, Secret]
    string Android_Keystore_B64 => TryGetValue(() => Android_Keystore_B64);

    [Parameter, Secret]
    string Android_Keystore_Name => TryGetValue(() => Android_Keystore_Name);

    [Parameter, Secret]
    string Android_Keystore_Password => TryGetValue(() => Android_Keystore_Password);

    AbsolutePath KeystorePath => (AbsolutePath) Path.Combine(EnvironmentInfo.WorkingDirectory, $"{Android_Keystore_Name}.keystore");

    Target RestoreKeystore => _ => _
        .Executes(() =>
        {
            Android_Keystore_B64.NotNullOrEmpty("The Android KeyStore Must be available as a Base64 encoded string.");
            Android_Keystore_Name.NotNullOrEmpty("The Android KeyStore Name is required.");

            var contents = Encoding.Default.GetBytes(Android_Keystore_B64!);
            File.WriteAllBytes(KeystorePath, contents);
        });
}
