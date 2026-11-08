using System.Security.Cryptography;
using System.Text;

namespace KampConnect.User.Business.Logic.Helpers;

public static class InviteCodeHelper
{
    // Geen 0/O, 1/I/L om typfouten te vermijden
    public const string Alphabet = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";
    public const int CodeLength = 6;
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(15);

    public static string Generate()
    {
        return RandomNumberGenerator.GetString(Alphabet, CodeLength);
    }

    // Spaties en kleine letters tolereren bij invoer
    public static string Normalize(string code)
    {
        return code.Replace(" ", "").Replace("-", "").Trim().ToUpperInvariant();
    }

    public static byte[] Hash(string code)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(Normalize(code)));
    }
}
