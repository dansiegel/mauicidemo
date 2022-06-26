using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using AppStoreConnect;
using JetBrains.Annotations;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Nuke.Common;

[PublicAPI]
public interface IRestoreAppleProvisioningProfile : INukeBuild
{
    [Parameter, Secret]
    string Apple_IssuerId => TryGetValue(() => Apple_IssuerId);

    [Parameter,Secret]
    string Apple_KeyId => TryGetValue(() => Apple_KeyId);

    [Parameter,Secret]
    string Apple_AuthKey_P8 => TryGetValue(() => Apple_AuthKey_P8);

    [Parameter,Secret]
    string Apple_ProfileId => TryGetValue(() => Apple_ProfileId);

    Target DownloadProvisioningProfile => _ => _
        .Executes(() =>
        {
            var profileResponse = GetProvisioningProfiles();
            var profiles = profileResponse.Data.Where(x =>
                    x.Attributes.ProfileState == ProfileState.ACTIVE && x.Id == Apple_ProfileId)
                .ToArray();
            Assert.NotEmpty(profiles, "No Active Profiles found");

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
            }
        });

    GetProfileResponse GetProvisioningProfiles()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateToken());
        var responseTask = client.GetAsync("https://api.appstoreconnect.apple.com/v1/profiles");
        responseTask.Wait();
        using var response = responseTask.Result;
        Assert.True(response.IsSuccessStatusCode, "Unable to retrieve Apple Provisioning Profiles");

        using var contentStream = response.Content.ReadAsStream();
        var profileResponse = JsonSerializer.Deserialize<GetProfileResponse>(contentStream);
        profileResponse.NotNull("Unable to process response from Apple App Store Connect");
        return profileResponse;
    }

    string GenerateToken()
    {
        Apple_KeyId.NotNullOrEmpty("The Apple KeyId is required");
        Apple_IssuerId.NotNullOrEmpty("The Apple IssuerId is required");
        Apple_AuthKey_P8.NotNullOrEmpty("The Apple AuthKey P8 Contents are required");

        var key = ECDsa.Create();
        key.NotNull("Unable to create ECDsa Key");
        key.ImportFromPem(Apple_AuthKey_P8.AsSpan());

        var now = DateTime.UtcNow;

        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Apple_IssuerId,
            Audience = "appstoreconnect-v1",
            NotBefore = now,
            Expires = now.AddMinutes(1),
            IssuedAt = now,
            Claims = new Dictionary<string, object> { { "scope", new [] { "GET /v1/profiles" } } },
            SigningCredentials = new SigningCredentials(new ECDsaSecurityKey(key) { KeyId = Apple_KeyId }, "ES256")
        });
    }
}
