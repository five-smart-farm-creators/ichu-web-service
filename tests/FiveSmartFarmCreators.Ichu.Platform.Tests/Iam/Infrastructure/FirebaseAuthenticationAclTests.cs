using System.Net;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Tokens.Firebase.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Tokens.Firebase.Services;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Iam.Infrastructure;

public class FirebaseAuthenticationAclTests
{
    [Fact]
    public async Task VerifyIdToken_WhenTheTokenIsNotAJwt_ReturnsInvalidCredentials()
    {
        var acl = CreateAcl(_ => throw new InvalidOperationException("A malformed token must not reach the provider."));

        var result = await acl.VerifyIdTokenAsync("not-an-identity-token", CancellationToken.None);

        Assert.Equal(IamErrors.InvalidCredentials, result.DomainError);
    }

    [Fact]
    public async Task SignIn_WhenThePasswordIsRejected_ReturnsInvalidCredentials()
    {
        var acl = CreateAcl(_ => new HttpResponseMessage(HttpStatusCode.BadRequest));

        var result = await acl.SignInAsync(new Email("manager@example.com"), "wrong-password", CancellationToken.None);

        Assert.Equal(IamErrors.InvalidCredentials, result.DomainError);
    }

    [Fact]
    public async Task SignIn_WhenTheProviderAnswersServerError_ReturnsIdentityProviderUnavailable()
    {
        var acl = CreateAcl(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var result = await acl.SignInAsync(new Email("manager@example.com"), "password", CancellationToken.None);

        Assert.Equal(IamErrors.IdentityProviderUnavailable, result.DomainError);
    }

    [Fact]
    public async Task SignIn_WhenTheRequestTimesOut_ReturnsIdentityProviderUnavailable()
    {
        var acl = CreateAcl(_ => throw new TaskCanceledException("The identity provider did not answer in time."));

        var result = await acl.SignInAsync(new Email("manager@example.com"), "password", CancellationToken.None);

        Assert.Equal(IamErrors.IdentityProviderUnavailable, result.DomainError);
    }

    [Fact]
    public async Task SignIn_WithoutFirebaseSettings_ReturnsIdentityProviderNotConfigured()
    {
        var acl = CreateAcl(_ => new HttpResponseMessage(HttpStatusCode.OK), new FirebaseOptions());

        var result = await acl.SignInAsync(new Email("manager@example.com"), "password", CancellationToken.None);

        Assert.Equal(IamErrors.IdentityProviderNotConfigured, result.DomainError);
    }

    private static FirebaseAuthenticationAcl CreateAcl(Func<HttpRequestMessage, HttpResponseMessage> respond,
        FirebaseOptions? settings = null)
    {
        var client = new HttpClient(new StubHandler(respond));
        var configured = settings ?? new FirebaseOptions { ProjectId = "ichu-test", ApiKey = "test-key" };
        return new FirebaseAuthenticationAcl(client, Options.Create(configured), TimeProvider.System);
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
    }
}
