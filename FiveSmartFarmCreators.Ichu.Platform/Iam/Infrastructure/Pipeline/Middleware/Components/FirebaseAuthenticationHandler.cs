using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Pipeline.Middleware.Components;

/// <summary>
///     Authenticates bearer identity tokens using the external identity provider.
/// </summary>
public sealed class FirebaseAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder, IIdentityProvider provider)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Firebase";

    private const string IdentityUnavailableItem = "IamIdentityUnavailable";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return AuthenticateResult.NoResult();
        }

        if (!AuthenticationHeaderValue.TryParse(Request.Headers.Authorization, out var header) ||
            !header.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(header.Parameter))
        {
            return AuthenticateResult.Fail("A bearer identity token is required.");
        }

        var identity = await provider.VerifyIdTokenAsync(header.Parameter, Context.RequestAborted);
        if (identity.IsFailure)
        {
            // The challenge answers 503 only when the provider is unavailable, so the outage is remembered for it.
            if (identity.DomainError.Type == EErrorType.Unavailable)
            {
                Context.Items[IdentityUnavailableItem] = true;
            }

            return AuthenticateResult.Fail(identity.Message);
        }

        var verified = identity.Value!;
        var claims = new[]
        {
            new Claim("firebase_uid", verified.Uid),
            new Claim(ClaimTypes.Email, verified.Email.Value),
            new Claim("email_verified", verified.EmailVerified ? "true" : "false")
        };
        return AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName)), SchemeName));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = Context.Items.ContainsKey(IdentityUnavailableItem) ? 503 : 401;
        Response.Headers.WWWAuthenticate = "Bearer";
        return Task.CompletedTask;
    }
}
