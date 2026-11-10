using System.ComponentModel.DataAnnotations;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

public sealed record SignInResource([Required, EmailAddress] string Email, [Required] string Password);
