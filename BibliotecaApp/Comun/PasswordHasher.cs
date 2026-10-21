using System;
using System.Security.Cryptography;

namespace BibliotecaApp
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bits
        private const int HashSize = 32; // 256 bits
        private const int Iterations = 100_000; 
        public static string Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("La contraseña no puede ser nula o vacía.", nameof(password));

            using var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[SaltSize];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool Verify(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
                return false;

            try
            {
                // Parsear formato: iteraciones.salt.hash
                var parts = storedHash.Split('.');
                if (parts.Length != 3)
                    return false; // Formato legacy o inválido

                int iterations = int.Parse(parts[0]);
                byte[] salt = Convert.FromBase64String(parts[1]);
                byte[] hash = Convert.FromBase64String(parts[2]);

                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
                byte[] computedHash = pbkdf2.GetBytes(hash.Length);

                // Comparación en tiempo constante
                return CryptographicOperations.FixedTimeEquals(computedHash, hash);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsLegacyHash(string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return true;
            var parts = storedHash.Split('.');
            return parts.Length != 3 || !int.TryParse(parts[0], out _);
        }
    }
}