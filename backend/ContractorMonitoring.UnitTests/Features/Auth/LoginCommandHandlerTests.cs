using FluentAssertions;
using Moq;
using Xunit;
using ContractorMonitoring.Application.DTOs.Auth;
using ContractorMonitoring.Application.Features.Auth.Commands.Login;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Application.Interfaces.Repositories;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.UnitTests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IJwtService> _jwt = new();
    private readonly Mock<IPasswordService> _pwd = new();
    private readonly Mock<IPermissionResolver> _resolver = new();
    private readonly Mock<IGenericRepository<User>> _users = new();

    private LoginCommandHandler CreateHandler()
    {
        _uow.Setup(u => u.Users).Returns(_users.Object);
        return new LoginCommandHandler(_uow.Object, _jwt.Object, _pwd.Object, _resolver.Object);
    }

    private static User ActiveUser() => new()
    {
        Id = Guid.NewGuid(),
        Email = "test@example.com",
        PasswordHash = "hash",
        IsActive = true,
        LoginAttempts = 0,
        TenantId = Guid.NewGuid()
    };

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsSuccess()
    {
        var user = ActiveUser();
        _users.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
              .ReturnsAsync(user);
        _pwd.Setup(p => p.VerifyPassword("pass", "hash")).Returns(true);
        _jwt.Setup(j => j.GenerateAccessToken(user)).ReturnsAsync("access_token");
        _jwt.Setup(j => j.GenerateRefreshToken()).ReturnsAsync("refresh_token");
        _jwt.Setup(j => j.GetTokenExpiryTime("access_token")).ReturnsAsync("2099-01-01T00:00:00Z");
        _resolver.Setup(r => r.GetUserRolesAsync(user.Id)).ReturnsAsync(["Admin"]);
        _resolver.Setup(r => r.GetUserPermissionsAsync(user.Id)).ReturnsAsync(["projects.read"]);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await CreateHandler().Handle(
            new LoginCommand { Request = new LoginRequest { Email = user.Email, Password = "pass" } },
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data!.AccessToken.Should().Be("access_token");
        result.Data.User.Roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsFail()
    {
        var user = ActiveUser();
        _users.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
              .ReturnsAsync(user);
        _pwd.Setup(p => p.VerifyPassword("wrong", "hash")).Returns(false);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await CreateHandler().Handle(
            new LoginCommand { Request = new LoginRequest { Email = user.Email, Password = "wrong" } },
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid");
    }

    [Fact]
    public async Task Handle_LockedAccount_ReturnsFail()
    {
        var user = ActiveUser();
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);
        _users.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
              .ReturnsAsync(user);

        var result = await CreateHandler().Handle(
            new LoginCommand { Request = new LoginRequest { Email = user.Email, Password = "any" } },
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("locked");
    }

    [Fact]
    public async Task Handle_InactiveUser_ReturnsFail()
    {
        var user = ActiveUser();
        user.IsActive = false;
        _users.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
              .ReturnsAsync(user);
        _pwd.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var result = await CreateHandler().Handle(
            new LoginCommand { Request = new LoginRequest { Email = user.Email, Password = "pass" } },
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("deactivated");
    }

    [Fact]
    public async Task Handle_FiveFailedAttempts_LocksAccount()
    {
        var user = ActiveUser();
        user.LoginAttempts = 4;
        _users.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
              .ReturnsAsync(user);
        _pwd.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await CreateHandler().Handle(
            new LoginCommand { Request = new LoginRequest { Email = user.Email, Password = "wrong" } },
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("locked");
        user.LockoutEnd.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFail()
    {
        _users.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
              .ReturnsAsync((User?)null);

        var result = await CreateHandler().Handle(
            new LoginCommand { Request = new LoginRequest { Email = "nobody@example.com", Password = "pass" } },
            CancellationToken.None);

        result.Success.Should().BeFalse();
    }
}
