using Libs;
using Microsoft.AspNetCore.Identity;

namespace Database.Factories;

internal sealed class RoleFactory : Factory<IdentityRole>
{
    public RoleFactory()
    {
        RuleFor(p => p.Name, f => f.Name.JobType());
    }
}