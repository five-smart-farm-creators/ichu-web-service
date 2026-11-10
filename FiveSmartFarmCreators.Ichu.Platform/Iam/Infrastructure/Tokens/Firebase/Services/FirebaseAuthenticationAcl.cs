using System.Net;
using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Tokens.Firebase.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Tokens.Firebase.Services;

/// <summary>
///     Adapts Firebase authentication to the IAM identity-provider port. Transport and protocol exceptions end here, so
///     the rest of IAM only sees error codes.
/// </summary>
public sealed class FirebaseAuthenticationAcl(HttpClient client, IOptions<FirebaseOptions> options, TimeProvider clock) : IIdentityProvider
{
    private const int MaximumTokenLength = 16384;
    private const int MaximumSubjectLength = 128;

    /// <inheritdoc />
    public async Task<Result<string>> SignInAsync(Email email, string password, CancellationToken cancellationToken)
    {
        var response = await SendAsync("accounts:signInWithPassword",
            new { email = email.Value, password, returnSecureToken = true }, cancellationToken);
        if (response.IsFailure)
        {
            return Result<string>.Failure(response.DomainError);
        }

        using var document = response.Value!;
        if (!document.RootElement.TryGetProperty("idToken", out var token) || token.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(token.GetString()))
        {
            return Result<string>.Failure(IamErrors.IdentityProviderUnavailable);
        }

        return Result<string>.Success(token.GetString()!);
    }

    /// <inheritdoc />
    public async Task<Result<ExternalIdentity>> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken)
    {
        var settings = Settings();
        if (settings.IsFailure)
        {
            return Result<ExternalIdentity>.Failure(settings.DomainError);
        }

        // Google verifies the signature; the local checks reject another project or an expired token before the lookup.
        var subject = ReadSubject(idToken, settings.Value!);
        if (subject.IsFailure)
        {
            return Result<ExternalIdentity>.Failure(subject.DomainError);
        }

        var uid = subject.Value!;
        var lookup = await SendAsync("accounts:lookup", new { idToken }, cancellationToken);
        if (lookup.IsFailure)
        {
            return Result<ExternalIdentity>.Failure(lookup.DomainError);
        }

        using var document = lookup.Value!;
        if (!document.RootElement.TryGetProperty("users", out var users) || users.ValueKind != JsonValueKind.Array)
        {
            return Result<ExternalIdentity>.Failure(IamErrors.IdentityProviderUnavailable);
        }

        if (users.GetArrayLength() != 1)
        {
            return Result<ExternalIdentity>.Failure(IamErrors.InvalidCredentials);
        }

        var user = users[0];
        var localId = user.TryGetProperty("localId", out var localIdValue) ? localIdValue.GetString() : null;
        var disabled = user.TryGetProperty("disabled", out var disabledValue) && disabledValue.GetBoolean();
        if (localId != uid || disabled)
        {
            return Result<ExternalIdentity>.Failure(IamErrors.InvalidCredentials);
        }

        var providerEmail = user.TryGetProperty("email", out var emailValue) ? emailValue.GetString() : null;
        var address = Email.Create(providerEmail ?? string.Empty);
        if (address.IsFailure)
        {
            return Result<ExternalIdentity>.Failure(IamErrors.InvalidCredentials);
        }

        var verified = user.TryGetProperty("emailVerified", out var emailVerified) && emailVerified.GetBoolean();
        return Result<ExternalIdentity>.Success(new ExternalIdentity(uid, address.Value!, verified));
    }

    private Result<FirebaseOptions> Settings()
    {
        var settings = options.Value;
        return string.IsNullOrWhiteSpace(settings.ProjectId) || string.IsNullOrWhiteSpace(settings.ApiKey)
            ? Result<FirebaseOptions>.Failure(IamErrors.IdentityProviderNotConfigured)
            : Result<FirebaseOptions>.Success(settings);
    }

    private Result<string> ReadSubject(string idToken, FirebaseOptions settings)
    {
        var invalid = Result<string>.Failure(IamErrors.InvalidCredentials);
        if (string.IsNullOrWhiteSpace(idToken) || idToken.Length > MaximumTokenLength)
        {
            return invalid;
        }

        try
        {
            var parts = idToken.Split('.');
            if (parts.Length != 3)
            {
                return invalid;
            }

            var encoded = parts[1].Replace('-', '+').Replace('_', '/');
            encoded = encoded.PadRight((encoded.Length + 3) / 4 * 4, '=');
            using var payload = JsonDocument.Parse(Convert.FromBase64String(encoded));
            var claims = payload.RootElement;
            var subject = claims.GetProperty("sub").GetString() ?? string.Empty;
            if (subject.Length is < 1 or > MaximumSubjectLength ||
                claims.GetProperty("aud").GetString() != settings.ProjectId ||
                claims.GetProperty("iss").GetString() != $"https://securetoken.google.com/{settings.ProjectId}" ||
                claims.GetProperty("exp").GetInt64() <= clock.GetUtcNow().ToUnixTimeSeconds())
            {
                return invalid;
            }

            return Result<string>.Success(subject);
        }
        catch (Exception error) when (error is FormatException or JsonException or KeyNotFoundException or InvalidOperationException)
        {
            // A token that does not decode is a caller error, so the parser exceptions end here.
            return invalid;
        }
    }

    private async Task<Result<JsonDocument>> SendAsync(string operation, object request, CancellationToken cancellationToken)
    {
        var settings = Settings();
        if (settings.IsFailure)
        {
            return Result<JsonDocument>.Failure(settings.DomainError);
        }

        try
        {
            using var response = await client.PostAsJsonAsync(
                $"https://identitytoolkit.googleapis.com/v1/{operation}?key={Uri.EscapeDataString(settings.Value!.ApiKey)}",
                request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return (int)response.StatusCode >= 500 || response.StatusCode == HttpStatusCode.TooManyRequests
                    ? Result<JsonDocument>.Failure(IamErrors.IdentityProviderUnavailable)
                    : Result<JsonDocument>.Failure(IamErrors.InvalidCredentials);
            }

            return Result<JsonDocument>.Success(JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken)));
        }
        catch (HttpRequestException)
        {
            return Result<JsonDocument>.Failure(IamErrors.IdentityProviderUnavailable);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // The client timeout cancels the request although the caller did not ask for it.
            return Result<JsonDocument>.Failure(IamErrors.IdentityProviderUnavailable);
        }
        catch (JsonException)
        {
            return Result<JsonDocument>.Failure(IamErrors.IdentityProviderUnavailable);
        }
    }
}
