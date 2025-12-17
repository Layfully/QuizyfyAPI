using System.Security.Cryptography;
using System.Text;

namespace QuizyfyAPI.Helpers;

internal static class Hash
{
    private const int Iterations = 350000; 
    private const int HashSize = 64;
    private const int SaltSize = 64;
    
    private static readonly HashAlgorithmName _algorithm = HashAlgorithmName.SHA512;
    
    public static void Create(string stringToHash, out byte[] generatedHash, out byte[] generatedSalt)
    {
        if (string.IsNullOrWhiteSpace(stringToHash))
        {
            throw new ArgumentException("Value cannot be empty.", nameof(stringToHash));
        }
        
        generatedSalt = RandomNumberGenerator.GetBytes(SaltSize);

        generatedHash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(stringToHash),
            generatedSalt,
            Iterations,
            _algorithm,
            HashSize
        );
    }

    public static bool Verify(string stringToVerify, byte[] storedHash, byte[] storedSalt)
    {
        if (string.IsNullOrWhiteSpace(stringToVerify))
        {
            throw new ArgumentException("Value cannot be empty.", nameof(stringToVerify));

        }

        if (storedHash.Length != HashSize || storedSalt.Length != SaltSize)
        {
            return false;
        }

        byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(stringToVerify),
            storedSalt,
            Iterations,
            _algorithm,
            HashSize
        );
        
        return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
    }
}
