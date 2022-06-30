using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using Nuke.Components;
using Serilog;

[PublicAPI]
public interface IHazIOSCertificate : INukeBuild
{
    [Parameter("iOS P12 Certificate must be Base64 Encoded"),Secret]
    string IOS_P12_B64 => TryGetValue(() => IOS_P12_B64);

    [Parameter("iOS P12 Certificate must be provided"),Secret]
    string IOS_P12_Password => TryGetValue(() => IOS_P12_Password);

    [PathExecutable("security")]
    Tool Security => TryGetValue(() => Security);

    AbsolutePath P12CertifiatePath => (AbsolutePath) Path.Combine(EnvironmentInfo.WorkingDirectory, "apple.p12");

    Target RestoreIOSCertificate => _ => _
        .OnlyWhenStatic(() => EnvironmentInfo.Platform == PlatformFamily.OSX)
        .TryBefore<IRestore>()
        .TryBefore<IHazMauiWorkload>()
        .Requires(() => IOS_P12_B64)
        .Requires(() => IOS_P12_Password)
        .Executes(() =>
        {
            var data = Convert.FromBase64String(IOS_P12_B64);
            File.WriteAllBytes(P12CertifiatePath, data);

            // security import certificate.pfx -k ~/Library/Keychains/login.keychain -P \$tup1dP@ssw0rd
            var escaped = IOS_P12_Password.StartsWith("$") ? '\\' + IOS_P12_Password : IOS_P12_Password;

            try
            {
                var keychainPath = TemporaryDirectory / "app-signing.keychain-db";
                Security.Invoke($"create-keychain -p \"p@55word\" {keychainPath}");
                Security.Invoke($"set-keychain-settings -lut 21600 {keychainPath}");
                Security.Invoke($"unlock-keychain -p \"p@55word\" {keychainPath}");

                Security.Invoke($"import {P12CertifiatePath} -k {keychainPath} -P {escaped}");
                Security.Invoke($"list-keychain -d user -s {keychainPath}");
            }
            catch
            {
                Log.Error("Error Encountered by Security Tool");
                Assert.Fail("Unable to import p12 into the keychain");
            }
        });
}
