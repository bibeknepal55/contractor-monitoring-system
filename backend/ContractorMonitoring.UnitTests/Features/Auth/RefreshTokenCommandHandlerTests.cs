using FluentAssertions;
using Moq;
using Xunit;
using ContractorMonitoring.Application.Features.Auth.Commands.RefreshToken;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Application.Interfaces.Repositories;
using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.UnitTests.Features.Auth;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IJwtService> _jwt = new();
    private readonly Mock<IPermissionResolver> _resolver = new();
    private readonly Mock<IGenericRepository<User>> _users = new();

    private RefreshTokenCommandHandler CreateHandler()
    {
        _uow.Setup(u => u.Users).Returns(_users.Object);
        return new RefreshTokenCommandHandler(_uow.Object, _jwt.Object, _resolver.Object);
    }

    private static User ValidUser(string refreshToken = "valid_rt") => new()
    {
        Id = Guid.NewGuid(),
        Email = "user@example.com",
        RefreshToken = refreshToken,
        RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7),
        RefreshTokenFamily = "family-1",
        TenantId = Guid.NewGuid()
    };

    [Fact]
    public async Task Handle_ValidTokens_RotatesRefreshToken()
    {
        var user = ValidUser();
        _jwt.Setup(j => j.ValidateToken("access", false)).ReturnsAsync((true, user.Id));
        _users.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _jwt.Setup(j => j.GenerateAccessToken(user, It.IsAny<List<string>?>(), It.IsAny<List<string>?>())).ReturnsAsync("new_access");
        _jwt.Setup(j => j.GenerateRefreshToken()).ReturnsAsync("new_rt");
        _jwt.Setup(j => j.GetTokenExpiryTime("new_access")).ReturnsAsync("2099-01-01T00:00:00Z");
        _resolver.Setup(r => r.GetUserRolesAsync(user.Id)).ReturnsAsync([]);
        _resolver.Setup(r => r.GetUserPermissionsAsync(user.Id)).ReturnsAsync([]);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await CreateHandler().Handle(
            new RefreshTokenCommand { AccessToken = "access", RefreshToken = "valid_rt" },
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data!.RefreshToken.Should().Be("new_rt");
        user.RefreshToken.Should().Be("new_rt");
    }

    [Fact]
    public async Task Handle_TokenReuse_RevokesFamily()
    {
        var user = ValidUser("stored_rt");
        _jwt.Setup(j => j.ValidateToken("access", false)).ReturnsAsync((true, user.Id));
        _users.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await CreateHandler().Handle(
            new RefreshTokenCommand { AccessToken = "access", RefreshToken = "different_rt" },
            CancellationToken.None);

        result.Success.Should().BeFalse();
        // Family should be cleared (all sessions revoked)
        user.RefreshTokenFamily.Should().BeNull();
        user.RefreshToken.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ExpiredRefreshToken_ReturnsFail()
    {
        var user = ValidUser();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1);
        _jwt.Setup(j => j.ValidateToken("access", false)).ReturnsAsync((true, user.Id));
        _users.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await CreateHandler().Handle(
            new RefreshTokenCommand { AccessToken = "access", RefreshToken = "valid_rt" },
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("expired");
    }

    [Fact]
    public async Task Handle_InvalidAccessToken_ReturnsFail()
    {
        _jwt.Setup(j => j.ValidateToken("bad", false)).ReturnsAsync((false, Guid.Empty));

        var result = await CreateHandler().Handle(
            new RefreshTokenCommand { AccessToken = "bad", RefreshToken = "rt" },
            CancellationToken.None);

        result.Success.Should().BeFalse();
    }
}
