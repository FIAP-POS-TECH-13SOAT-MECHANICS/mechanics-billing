using Mechanics.Domain.Auth;
using Mechanics.Infra.Security.Models;
using Microsoft.AspNetCore.Identity;

namespace Mechanics.Tests.Unit.Mocks;

public static class UserMocks
{
    public static User CreateUser(Guid userId, string name, string cpf, string roleName)
    {
        var role = CreateRole(roleName);

        return new User
        {
            Id = userId,
            FullName = name.ToUpper(),
            CpfNumber = cpf,
            RoleId = role.Id,
            Role = role,
            Email = $"{name.Replace(' ', '.').ToLower()}@mechanics.com",
            PasswordHash = "",
            SecurityStamp = userId.ToString(),
        };
    }

    public static User CreateUser(string userName, string userPassword)
    {
        var role = CreateRole(RoleNames.Administrator);
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = userName.ToUpper(),
            CpfNumber = userName,
            RoleId = role.Id,
            Role = role,
            Email = $"{userName}@mechanics.com",
            PasswordHash = "",
            SecurityStamp = userName,
        };

        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, userPassword);

        return user;
    }

    private static Role CreateRole(string roleName) => new()
    {
        Id = Guid.NewGuid(),
        Name = roleName,
        CreationDate = DateTime.UtcNow,
    };
}
