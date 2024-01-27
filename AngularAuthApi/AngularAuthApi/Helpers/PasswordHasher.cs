using System.Security.Cryptography;

namespace AngularAuthApi.Helpers
{
    public class PasswordHasher
    {
        private static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        private static readonly int SaltSize = 16;
        private static readonly int HashSize = 20;
        private static readonly int Iterations = 10000;
        public static string HashPassword(string password)
        {
            if (password == null)
                return "your password is wrong";
            byte[] salt;
            rng.GetBytes(salt = new byte[SaltSize]);
            var key = new Rfc2898DeriveBytes(password,salt,Iterations);
            var hash = key.GetBytes(HashSize);
            var hashByte = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashByte, 0, SaltSize);
            Array.Copy(hash, 0, hashByte, SaltSize, HashSize);
            var Base64Hash = Convert.ToBase64String(hashByte);
            return Base64Hash;
        }
        public static bool VerifyPassword(string password,string Base64Hash)
        {
            var HashByte = Convert.FromBase64String(Base64Hash);
            var salt = new byte[SaltSize];
            Array.Copy(HashByte, 0, salt, 0, SaltSize);
            var key = new Rfc2898DeriveBytes(password, salt, Iterations);
            byte[] hash = key.GetBytes(HashSize);
            for(var i = 0; i < HashSize; i++)
            {
                if (HashByte[i + SaltSize] != hash[i])
                    return false;

            }
            return true;

        }

    }
}
