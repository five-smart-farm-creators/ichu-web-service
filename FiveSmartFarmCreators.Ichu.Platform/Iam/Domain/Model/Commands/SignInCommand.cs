using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

/// <summary>
///     Carries the credentials submitted for account sign-in.
/// </summary>
/// <param name="Email">
///     The email address associated with the account.
/// </param>
/// <param name="Password">
///     The password submitted to the identity provider.
/// </param>
public sealed record SignInCommand(string Email, string Password);
