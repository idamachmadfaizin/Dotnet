using Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Entities;

namespace Database.Context;

public class AppDbContext(
    DbContextOptions options,
    IOptions<ConnectionStrings> connectionStrings) : IdentityDbContext<User>(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(connectionStrings.Value.DefaultConnection);
    }
}