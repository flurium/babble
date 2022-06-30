﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace Client
{
    public class HashService
    {
        private const int SaltSize = 16;
        private const int HashSize = 20;

    /// <summary>
    /// Creates a hash from a password. 
    /// </summary>
    /// <param name="password">The password.</param>
    /// <param name="iterations">Number of iterations.</param>
    /// <returns>The hash.</returns>

    public static string Hash(string password, int iterations)
    {
      using (var rng = new RNGCryptoServiceProvider())
      {
        byte[] salt;
        rng.GetBytes(salt = new byte[SaltSize]);
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
        {
          var hash = pbkdf2.GetBytes(HashSize);
          var hashBytes = new byte[SaltSize + HashSize];
          Array.Copy(salt, 0, hashBytes, 0, SaltSize);
          Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);
          // Convert to base64
          var base64Hash = Convert.ToBase64String(hashBytes);

          // Format hash with extra information
          return $"$HASsssH|V111243${iterations}${base64Hash}";
        }
      }

    }

    /// <summary>
    /// Creates a hash from a password with 10000 iterations
    /// </summary>
    /// <param name="password">The password.</param>
    /// <returns>The hash.</returns>

    public static string Hash(string password)
    {
      return Hash(password, 10000);
    }

    /// <summary>
    /// Checks if hash is supported.
    /// </summary>
    /// <param name="hashString">The hash.</param>
    /// <returns>Is supported?</returns>
    public static bool IsHashSupported(string hashString)
        {
            return hashString.Contains("HASsssH|V111243$");
        }

        /// <summary>
        /// Verifies a password against a hash.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="hashedPassword">The hash.</param>
        /// <returns>Could be verified?</returns>
        public static bool Verify(string password, string hashedPassword)
        {
            // Check hash
            if (!IsHashSupported(hashedPassword))
            {
                throw new NotSupportedException("The hashtype is not supported");
            }

            // Extract iteration and Base64 string
            var splittedHashString = hashedPassword.Replace("$HASsssH|V111243$", "").Split('$');
            var iterations = int.Parse(splittedHashString[0]);
            var base64Hash = splittedHashString[1];

            // Get hash bytes
            var hashBytes = Convert.FromBase64String(base64Hash);

            // Get salt
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Create hash with given salt
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);

                // Get result
                for (var i = 0; i < HashSize; i++)
                {
                    if (hashBytes[i + SaltSize] != hash[i])
                    {
                        return false;
                    }
                }

                return true;
            }

        }
    
}
}
