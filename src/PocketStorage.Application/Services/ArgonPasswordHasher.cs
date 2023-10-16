﻿using System.Security.Cryptography;
using System.Text;
using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PocketStorage.Application.Externals;

namespace PocketStorage.Application.Services;

public class ArgonPasswordHasher<TUser> : IPasswordHasher<TUser> where TUser : IdentityUser
{
    public ArgonPasswordHasher(IOptions<ArgonPasswordHasherOptions> options) => Options = options.Value;

    public ArgonPasswordHasherOptions Options { get; set; }

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

        return $"{Convert.ToBase64String(derivedKeyBytes)}.{Convert.ToBase64String(randomlyGeneratedSaltBytes)}";
    }

    public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
    {
        Guard.IsNotNull(user);
        Guard.IsNotNullOrWhiteSpace(hashedPassword);
        Guard.IsNotNullOrWhiteSpace(providedPassword);

        string passwordWithPepper = providedPassword + Options.Pepper;

        byte[] derivedKeyBytes = new byte[Options.KeySize];

        string[] hashedPasswordWithSaltBase64 = hashedPassword.Split('.', StringSplitOptions.RemoveEmptyEntries);

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
