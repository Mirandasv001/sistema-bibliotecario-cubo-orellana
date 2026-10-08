using System;
using System.Security.Cryptography;

namespace BibliotecaApp
{
    /// <summary>
    /// Helper para hashing seguro de contraseñas usando PBKDF2 (Rfc2898DeriveBytes).
    /// Compatible con .NET 10, sin dependencias externas.
    /// Formato de almacenamiento: {iteraciones}.{saltBase64}.{hashBase64}
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bits
        private const int HashSize = 32; // 256 bits
        private const int Iterations = 100_000; // Ajustable según hardware

        /// <summary>
        /// Genera un hash seguro de la contraseña.
        /// </summary>
        public static string Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("La contraseña no puede ser nula o vacía.", nameof(password));

            using var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[SaltSize];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            // Formato: iteraciones.salt.hash (todo en Base64)
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Verifica una contraseña contra su hash almacenado.
        /// </summary>
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
                // Formato inválido, corrupción, o hash legacy (texto plano)
                return false;
            }
        }

        /// <summary>
        /// Detecta si un hash almacenado usa el formato nuevo (PBKDF2) o es texto plano legacy.
        /// </summary>
        public static bool IsLegacyHash(string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return true;
            var parts = storedHash.Split('.');
            return parts.Length != 3 || !int.TryParse(parts[0], out _);
        }
    }
}