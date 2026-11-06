using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Tokens.Firebase.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Tokens.Firebase.Services;

/// <summary>
///     Adapts Firebase authentication responses to the IAM identity-provider contract.
/// </summary>
public sealed class FirebaseAuthenticationAcl(HttpClient client, IOptions<FirebaseOptions> options, TimeProvider clock) : IIdentityProvider
{
    /// <inheritdoc />
    public async Task<string> SignInAsync(Email email, string password, CancellationToken cancellationToken)
    {
        using var result = await SendAsync("accounts:signInWithPassword", new { email = email.Value, password, returnSecureToken = true }, cancellationToken);
        if (!result.RootElement.TryGetProperty("idToken", out var token) || token.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(token.GetString()))
            throw new IamException("unavailable", "The identity provider returned an invalid response.");
        return token.GetString()!;
    }

    /// <inheritdoc />
    public async Task<ExternalIdentity> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken)
    {
        var settings = Settings();
        if (string.IsNullOrWhiteSpace(idToken) || idToken.Length > 16384)
            throw new IamException("invalid_credentials", "The identity token is invalid.");
        string subject;
        try
        {
            var parts = idToken.Split('.');
            if (parts.Length != 3) throw new FormatException();
            var encoded = parts[1].Replace('-', '+').Replace('_', '/');
            encoded = encoded.PadRight((encoded.Length + 3) / 4 * 4, '=');
            using var payload = JsonDocument.Parse(Convert.FromBase64String(encoded));
            var claims = payload.RootElement;
            subject = claims.GetProperty("sub").GetString() ?? string.Empty;
            if (subject.Length is < 1 or > 128 || claims.GetProperty("aud").GetString() != settings.ProjectId ||
                claims.GetProperty("iss").GetString() != $"https://securetoken.google.com/{settings.ProjectId}" ||
                claims.GetProperty("exp").GetInt64() <= clock.GetUtcNow().ToUnixTimeSeconds())
                throw new FormatException();
        }
        catch (Exception error) when (error is FormatException or JsonException or KeyNotFoundException or InvalidOperationException)
        {
            throw new IamException("invalid_credentials", "The identity token is invalid.");
        }

        // Google verifies the token signature; the local checks reject mismatched projects and expired tokens early.
        using var result = await SendAsync("accounts:lookup", new { idToken }, cancellationToken);
        if (!result.RootElement.TryGetProperty("users", out var users) || users.ValueKind != JsonValueKind.Array)
            throw new IamException("unavailable", "The identity provider returned an invalid response.");
        if (users.GetArrayLength() != 1) throw new IamException("invalid_credentials", "The identity token is invalid.");
        var user = users[0];
        if (user.GetProperty("localId").GetString() != subject ||
            user.TryGetProperty("disabled", out var disabled) && disabled.GetBoolean())
            throw new IamException("invalid_credentials", "The identity token is invalid.");
        return new ExternalIdentity(subject, new Email(user.GetProperty("email").GetString() ?? string.Empty),
            user.TryGetProperty("emailVerified", out var verified) && verified.GetBoolean());
    }

    private FirebaseOptions Settings()
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ProjectId) || string.IsNullOrWhiteSpace(settings.ApiKey))
            throw new IamException("unavailable", "Firebase has not been configured.");
        return settings;
    }

    private async Task<JsonDocument> SendAsync(string operation, object request, CancellationToken cancellationToken)
    {
        var settings = Settings();
        try
        {
            using var response = await client.PostAsJsonAsync(
                $"https://identitytoolkit.googleapis.com/v1/{operation}?key={Uri.EscapeDataString(settings.ApiKey)}", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                if ((int)response.StatusCode >= 500 || response.StatusCode == HttpStatusCode.TooManyRequests)
                    throw new IamException("unavailable", "The identity provider is temporarily unavailable.");
                throw new IamException("invalid_credentials", "Access could not be granted.");
            }
            return JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        }
        catch (HttpRequestException) { throw new IamException("unavailable", "The identity provider is temporarily unavailable."); }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new IamException("unavailable", "The identity provider timed out.");
        }
        catch (JsonException) { throw new IamException("unavailable", "The identity provider returned an invalid response."); }
    }
}
