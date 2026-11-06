using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Pipeline.Middleware.Components;

/// <summary>Authenticates bearer identity tokens using the external identity provider.</summary>
public sealed class FirebaseAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder, IIdentityProvider provider)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Firebase";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization")) return AuthenticateResult.NoResult();
        if (!AuthenticationHeaderValue.TryParse(Request.Headers.Authorization, out var header) ||
            !header.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(header.Parameter))
            return AuthenticateResult.Fail("A bearer identity token is required.");
        try
        {
            var identity = await provider.VerifyIdTokenAsync(header.Parameter, Context.RequestAborted);
            var claims = new[] { new Claim("firebase_uid", identity.Uid), new Claim(ClaimTypes.Email, identity.Email.Value),
                new Claim("email_verified", identity.EmailVerified ? "true" : "false") };
            return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName)), SchemeName));
        }
        catch (IamException error) when (error.Code == "invalid_credentials")
        {
            return AuthenticateResult.Fail("The identity token could not be verified.");
        }
        catch (IamException error) when (error.Code == "unavailable")
        {
            Context.Items["IamIdentityUnavailable"] = true;
            return AuthenticateResult.Fail("The identity provider is unavailable.");
        }
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = Context.Items.ContainsKey("IamIdentityUnavailable") ? 503 : 401;
        Response.Headers.WWWAuthenticate = "Bearer";
        return Task.CompletedTask;
    }
}
