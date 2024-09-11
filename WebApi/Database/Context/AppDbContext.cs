using App.Model.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Database.Context;

public class AppDbContext(
    DbContextOptions options,
    IConfiguration configuration) : IdentityDbContext<User>(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
    }
}