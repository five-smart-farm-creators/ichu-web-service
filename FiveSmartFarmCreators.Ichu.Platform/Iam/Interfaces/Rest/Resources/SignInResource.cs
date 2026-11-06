using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>
///     Contains the email and password submitted for account sign-in.
/// </summary>
public sealed record SignInResource([Required, EmailAddress] string Email, [Required] string Password);
