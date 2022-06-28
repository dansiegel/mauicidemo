using System.IO;
using System.Text;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Components;

[PublicAPI]
public interface IHazAndroidKeystore : INukeBuild
{
    [Parameter("Android KeyStore must be Base64 Encoded"), Secret]
    string Android_Keystore_B64 => TryGetValue(() => Android_Keystore_B64);

    [Parameter("Android KeyStore name must be provided"), Secret]
    string Android_Keystore_Name => TryGetValue(() => Android_Keystore_Name);

    [Parameter("Android KeyStore must be provided"), Secret]
    string Android_Keystore_Password => TryGetValue(() => Android_Keystore_Password);

    AbsolutePath KeystorePath => (AbsolutePath) Path.Combine(EnvironmentInfo.WorkingDirectory, $"{Android_Keystore_Name}.keystore");

    Target RestoreKeystore => _ => _
        .TryBefore<IRestore>()
        .TryBefore<IHazMauiWorkload>()
        .Requires(() => Android_Keystore_B64)
        .Requires(() => Android_Keystore_Name)
        .Requires(() => Android_Keystore_Password)
        .Executes(() =>
        {
            var contents = Encoding.Default.GetBytes(Android_Keystore_B64!);
            File.WriteAllBytes(KeystorePath, contents);
        });
}
