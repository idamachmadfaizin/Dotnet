using Microsoft.AspNetCore.Identity;

namespace Configurations;

public class Auth
{
    public required string SigningKey { get; init; }
    public required IdentityOptions IdentityOptions { get; init; }
}