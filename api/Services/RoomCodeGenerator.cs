using System.Security.Cryptography;
using System.Text;

namespace PartyJukebox.Api.Services;

public interface IRoomCodeGenerator
{
    string GenerateCode();
    string GenerateSecret();
}

public class RoomCodeGenerator : IRoomCodeGenerator
{
    private static readonly char[] CodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    public string GenerateCode() => GenerateRandomString(6).ToUpperInvariant();

    public string GenerateSecret() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    private static string GenerateRandomString(int length)
    {
        var sb = new StringBuilder(length);
        for (var i = 0; i < length; i++)
        {
            var index = RandomNumberGenerator.GetInt32(CodeChars.Length);
            sb.Append(CodeChars[index]);
        }

        return sb.ToString();
    }
}
