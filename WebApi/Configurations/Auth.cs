using Microsoft.AspNetCore.Identity;

namespace WebApi.Configurations;

public class Auth
{
    public required string SigningKey { get; init; }
    public required IdentityOptions IdentityOptions { get; init; }
}