using System;

namespace QuizyfyAPI.Helpers
{
    public static class Hash
    {
        public static void Create(string stringToHash, out byte[] generatedHash, out byte[] generatedSalt)
        {
            if (stringToHash == null) throw new ArgumentNullException("stringToHash");
            if (string.IsNullOrWhiteSpace(stringToHash)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "stringToHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                generatedSalt = hmac.Key;
                generatedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(stringToHash));
            }
        }

        public static bool Verify(string stringToVerify, byte[] storedHash, byte[] storedSalt)
        {
            if (string.IsNullOrWhiteSpace(stringToVerify)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "stringToVerify");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "storedHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "storedSalt");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(stringToVerify));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }
    }
}
