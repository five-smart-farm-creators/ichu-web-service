using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

/// <summary>Defines the input for SignIn.</summary>
public sealed record SignInCommand(string Email, string Password);
