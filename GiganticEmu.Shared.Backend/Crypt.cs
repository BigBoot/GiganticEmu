using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;

namespace GiganticEmu.Shared.Backend;

public static class Crypt
{
    private static readonly uint[] _lookup32 = CreateLookup32();

    private static uint[] CreateLookup32()
    {
        var result = new uint[256];
        for (int i = 0; i < 256; i++)
        {
            string s = i.ToString("x2");
            result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
        }
        return result;
    }

    private static string ByteArrayToHex(byte[] bytes)
    {
        var lookup32 = _lookup32;
        var result = new char[bytes.Length * 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            var val = lookup32[bytes[i]];
            result[2 * i] = (char)val;
            result[2 * i + 1] = (char)(val >> 16);
        }
        return new string(result);
    }

    public static string CreateSecureRandomString(int count = 32)
        => ByteArrayToHex(RandomNumberGenerator.GetBytes(count));

    public static string HashToken(string token)
        => new PasswordHasher<string>().HashPassword("", token);

    public static bool VerifyToken(string token, string hash)
        => new PasswordHasher<string>().VerifyHashedPassword("", hash, token) != PasswordVerificationResult.Failed;
}