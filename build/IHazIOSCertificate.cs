using System;
using System.IO;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Components;

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
            Security.Invoke($"import {P12CertifiatePath} -k -k ~/Library/Keychains/login.keychain -P {escaped}");
        });
}
