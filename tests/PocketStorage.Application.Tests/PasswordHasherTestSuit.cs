using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PocketStorage.Application.External;
using PocketStorage.Application.Services;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.Application.Tests;

public class PasswordHasherTestSuit
{
    [Fact]
    public void HashPassword_WhenValidInputs_ThenReturnPasswordHash()
    {
        // Arrange.
        const string password = "Password123$";

        ArgonPasswordHasherOptions passwordHasherOptions = new()
        {
            Pepper = "pepper",
            Algorithm = Argon2Type.Argon2id,
            KeySize = 32,
            MemoryLimit = 512_000,
            OperationsLimit = 4,
            SaltSize = 16
        };

        User user = new();
        ArgonPasswordHasher<User> passwordHasher = new(Options.Create(passwordHasherOptions));

        // Act.
        string passwordHash = passwordHasher.HashPassword(user, password);

        // Assert.
        passwordHash.Should().NotBeNullOrWhiteSpace();
        passwordHash.Should().Contain(ArgonPasswordHasherOptions.Delimiter);
    }

    [Fact]
    public void HashPassword_WhenInvalidInputs_ThenThrowException()
    {
        // Arrange.
        ArgonPasswordHasherOptions passwordHasherOptions = new()
        {
            Pepper = "pepper",
            Algorithm = Argon2Type.Argon2id,
            KeySize = 32,
            MemoryLimit = 512_000,
            OperationsLimit = 4,
            SaltSize = 16
        };

        User user = new();
        ArgonPasswordHasher<User> passwordHasher = new(Options.Create(passwordHasherOptions));

        // Act.
        Exception? exception = Record.Exception(() => passwordHasher.HashPassword(user, string.Empty));

        // Assert.
        exception.Should().NotBeNull();
    }

    [Fact]
    public void VerifyHashedPassword_WhenValidInputs_ThenReturnSuccessStatus()
    {
        // Arrange.
        const string password = "Password123$";

        ArgonPasswordHasherOptions passwordHasherOptions = new()
        {
            Pepper = "pepper",
            Algorithm = Argon2Type.Argon2id,
            KeySize = 32,
            MemoryLimit = 512_000,
            OperationsLimit = 4,
            SaltSize = 16
        };

        User user = new();
        ArgonPasswordHasher<User> passwordHasher = new(Options.Create(passwordHasherOptions));

        // Act.
        string passwordHash = passwordHasher.HashPassword(user, password);
        PasswordVerificationResult verificationResult = passwordHasher.VerifyHashedPassword(user, passwordHash, password);

        // Assert.
        verificationResult.Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public void VerifyHashedPassword_WhenInvalidInputs_ThenReturnFailStatus()
    {
        // Arrange.
        const string password = "Password123$";

        ArgonPasswordHasherOptions passwordHasherOptions = new()
        {
            Pepper = "pepper",
            Algorithm = Argon2Type.Argon2id,
            KeySize = 32,
            MemoryLimit = 512_000,
            OperationsLimit = 4,
            SaltSize = 16
        };

        User user = new();
        ArgonPasswordHasher<User> passwordHasher = new(Options.Create(passwordHasherOptions));

        // Act.
        string passwordHash = passwordHasher.HashPassword(user, password);
        PasswordVerificationResult verificationResult = passwordHasher.VerifyHashedPassword(user, passwordHash, $"{password}{password}");

        // Assert.
        verificationResult.Should().Be(PasswordVerificationResult.Failed);
    }
}
