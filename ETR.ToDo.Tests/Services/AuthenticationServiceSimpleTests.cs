using ETR.ToDo.Core.Entities.Users;
using ETR.ToDo.Core.Repositories;
using Moq;
using ETR.ToDo.Services.Authentication;
using FluentAssertions;

namespace ETR.ToDo.Tests.Services;

/// <summary>
/// Simple tests for AuthenticationService password verification
/// Proves core authentication logic works correctly
/// </summary>
public class AuthenticationServiceSimpleTests
{
    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var authService = new AuthenticationService(new Mock<IRepository<User, Guid>>().Object); // Don't need repository for this test
        
        var password = "MySecurePassword123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        // Act
        var result = authService.VerifyPassword(password, passwordHash);

        // Assert
        result.Should().BeTrue("because the password matches the hash");
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var authService = new AuthenticationService(new Mock<IRepository<User, Guid>>().Object);
        
        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword456!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(correctPassword);

        // Act
        var result = authService.VerifyPassword(wrongPassword, passwordHash);

        // Assert
        result.Should().BeFalse("because the password does not match the hash");
    }

    [Fact]
    public void HashPassword_GeneratesValidHash()
    {
        // Arrange
        var authService = new AuthenticationService(new Mock<IRepository<User, Guid>>().Object);
        var password = "TestPassword123!";

        // Act
        var hash = authService.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty("because HashPassword should return a valid hash");
        hash.Should().StartWith("$2", "because BCrypt hashes start with $2a or $2b");
        
        // Verify we can use this hash to validate the password
        var isValid = authService.VerifyPassword(password, hash);
        isValid.Should().BeTrue("because the hash should validate the original password");
    }
}
