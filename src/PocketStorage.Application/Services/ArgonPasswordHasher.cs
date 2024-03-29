﻿using System.Security.Cryptography;
using System.Text;
using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PocketStorage.Application.External;

namespace PocketStorage.Application.Services;

public class ArgonPasswordHasher<TUser>(IOptions<ArgonPasswordHasherOptions> options) : IPasswordHasher<TUser>
    where TUser : IdentityUser
{
    public ArgonPasswordHasherOptions Options { get; set; } = options.Value;

    public string HashPassword(TUser user, string password)
    {
        Guard.IsNotNull(user);
        Guard.IsNotNullOrWhiteSpace(password);

        string passwordWithPepper = password + Options.Pepper;

        byte[] derivedKeyBytes = new byte[Options.KeySize];
        byte[] randomlyGeneratedSaltBytes = new byte[Options.SaltSize];

        SodiumLibrary.randombytes_buf(randomlyGeneratedSaltBytes, randomlyGeneratedSaltBytes.Length);

        int passwordHashingResult = SodiumLibrary.crypto_pwhash(
            derivedKeyBytes,
            derivedKeyBytes.Length,
            Encoding.UTF8.GetBytes(passwordWithPepper),
            passwordWithPepper.Length,
            randomlyGeneratedSaltBytes,
            Options.OperationsLimit,
            Options.MemoryLimit,
            (int)Options.Algorithm);

        Guard.IsEqualTo(passwordHashingResult, 0);

        return $"{Convert.ToBase64String(derivedKeyBytes)}{ArgonPasswordHasherOptions.Delimiter}{Convert.ToBase64String(randomlyGeneratedSaltBytes)}";
    }

    public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
    {
        Guard.IsNotNull(user);
        Guard.IsNotNullOrWhiteSpace(hashedPassword);
        Guard.IsNotNullOrWhiteSpace(providedPassword);

        string passwordWithPepper = providedPassword + Options.Pepper;

        byte[] derivedKeyBytes = new byte[Options.KeySize];

        string[] hashedPasswordWithSaltBase64 = hashedPassword.Split(ArgonPasswordHasherOptions.Delimiter, StringSplitOptions.RemoveEmptyEntries);

        Guard.HasSizeEqualTo(hashedPasswordWithSaltBase64, 2);

        byte[] originalDerivedKeyBytes = Convert.FromBase64String(hashedPasswordWithSaltBase64[0]);
        byte[] originalSaltBytes = Convert.FromBase64String(hashedPasswordWithSaltBase64[1]);

        int passwordHashingResult = SodiumLibrary.crypto_pwhash(
            derivedKeyBytes,
            derivedKeyBytes.Length,
            Encoding.UTF8.GetBytes(passwordWithPepper),
            passwordWithPepper.Length,
            originalSaltBytes,
            Options.OperationsLimit,
            Options.MemoryLimit,
            (int)Options.Algorithm);

        Guard.IsEqualTo(passwordHashingResult, 0);

        if (CryptographicOperations.FixedTimeEquals(derivedKeyBytes, originalDerivedKeyBytes))
        {
            return PasswordVerificationResult.Success;
        }

        return PasswordVerificationResult.Failed;
    }
}
