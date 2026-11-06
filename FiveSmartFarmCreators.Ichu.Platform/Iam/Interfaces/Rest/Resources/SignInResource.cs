using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>Defines the Sign In Resource contract.</summary>
public sealed record SignInResource([Required, EmailAddress] string Email, [Required] string Password);
