using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AppStoreConnect;
using JetBrains.Annotations;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Nuke.Common;
using Nuke.Components;

[PublicAPI]
public interface IRestoreAppleProvisioningProfile : INukeBuild
{
    [Parameter("Apple Issuer Id is required"), Secret]
    string Apple_IssuerId => TryGetValue(() => Apple_IssuerId);

    [Parameter("Apple Key Id is required"),Secret]
    string Apple_KeyId => TryGetValue(() => Apple_KeyId);

    [Parameter("Apple AuthKey P8 text value"),Secret]
    string Apple_AuthKey_P8 => TryGetValue(() => Apple_AuthKey_P8);

    [Parameter("Apple Profile Id is required"),Secret]
    string Apple_ProfileId => TryGetValue(() => Apple_ProfileId);

    Target DownloadProvisioningProfile => _ => _
        .OnlyWhenStatic(() => EnvironmentInfo.Platform == PlatformFamily.OSX)
        .TryBefore<IRestore>()
        .TryBefore<IHazMauiWorkload>()
        .Requires(() => Apple_IssuerId)
        .Requires(() => Apple_KeyId)
        .Requires(() => Apple_ProfileId)
        .Requires(() => Apple_AuthKey_P8)
        .Executes(() =>
        {
            var profileResponse = GetProvisioningProfiles();
            Assert.NotEmpty(profileResponse.Data, "No Provisioning Profiles found.");
            var profiles = profileResponse.Data.Where(x =>
                    x.Attributes.ProfileState == ProfileState.ACTIVE && x.Id == Apple_ProfileId)
                .ToArray();
            if(!profiles.Any())
                Assert.Fail($"Profiles:\n{string.Join(", ", profiles.Select(x => $"{x.Attributes.Name} ({x.Attributes.ProfileState})"))}");
            // var errorMessage = string.Join('\n', profiles.Select(x => $"- {x.Attributes.Name}"));
            // Assert.NotEmpty(profiles, $"No Active Profiles found:\n{profiles}");

            foreach (var profile in profiles)
            {
                // "$HOME/Library/MobileDevice/Provisioning Profiles/${UUID}.mobileprovision"
                var filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Library",
                    "MobileDevice",
                    "Provisioning Profiles",
                    $"{profile.Attributes.Uuid}.mobileprovision");
                var data = Convert.FromBase64String(profile.Attributes.ProfileContent);
                File.WriteAllBytes(filePath, data);
                Serilog.Log.Information(messageTemplate: "Downloaded Provisioning Profile: {0}", propertyValue: profile.Attributes.Name);
                Serilog.Log.Information(File.ReadAllText(filePath));
            }
        });

    GetProfileResponse GetProvisioningProfiles()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateToken());
        var responseTask = client.GetAsync("https://api.appstoreconnect.apple.com/v1/profiles");
        responseTask.Wait();
        using var response = responseTask.Result;
        Assert.True(response.IsSuccessStatusCode, $"Unable to retrieve Apple Provisioning Profiles - ({response.StatusCode}) {response.ReasonPhrase}");

        using var contentStream = response.Content.ReadAsStream();
        var profileResponse = JsonSerializer.Deserialize<GetProfileResponse>(contentStream);
        profileResponse.NotNull("Unable to process response from Apple App Store Connect");
        return profileResponse;
    }

    string GenerateToken()
    {
        var p8 = Encoding.Default.GetString(Convert.FromBase64String(Apple_AuthKey_P8));
        var key = ECDsa.Create();
        key.NotNull("Unable to create ECDsa Key");
        key.ImportFromPem(p8.AsSpan());

        var now = DateTime.UtcNow;

        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Apple_IssuerId,
            Audience = "appstoreconnect-v1",
            NotBefore = now,
            Expires = now.AddMinutes(20),
            IssuedAt = now,
            // Claims = new Dictionary<string, object> { { "scope", new [] { "GET /v1/profiles" } } },
            SigningCredentials = new SigningCredentials(new ECDsaSecurityKey(key) { KeyId = Apple_KeyId }, "ES256")
        });
    }
}
