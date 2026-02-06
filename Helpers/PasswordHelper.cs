using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace MetaPlApi.Helpers
{
    public static class PasswordHelper
    {
        public static byte[] GetSecureSalt()
        {
            return RandomNumberGenerator.GetBytes(32);
        }
        
        public static string HashPassword(string password, byte[] salt)
        {
            byte[] derivedKey = KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 32);
            
            return Convert.ToBase64String(derivedKey);
        }
        
        public static bool VerifyPassword(string password, string hashedPassword, string saltBase64)
        {
            var salt = Convert.FromBase64String(saltBase64);
            var hashToVerify = HashPassword(password, salt);
            return hashedPassword == hashToVerify;
        }
    }
}