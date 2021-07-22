using System;
using System.Security.Cryptography;

namespace Kebapi.Services.Hashing
{
    /// <summary>
    /// Hashing Service. 
    /// <para>
    /// Provides methods for creating and checking string values (typically passwords) 
    /// against "hash bundles". We are defining a hash bundle as a delimited string 
    /// in this format:
    /// </para>
    /// <para>
    ///      Algorithm.Iterations.Salt.Hash
    /// </para>
    /// e.g.
    /// <para>
    ///      SHA512.10000.kMsQ/KtK0KmLAC/U3BNDZGQ72EomNdTe.0FiLdBGPh9oydfSPnSpX...
    /// </para>
    /// </summary>
    public class HashingService : IHashingService
    {
        private const string HashingDelimiter = ".";
        private const int HashingIterations = 10000;

        /// <summary>
        /// Creates a hash bundle from the given string value. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns>A hash bundle, which is a <see cref="string"/> in the format:
        /// Algorithm.Iterations.Salt.Hash.</returns>
        public string GenerateHashBundleFromValue(string value)
        {
            var salt = new byte[24];
            new RNGCryptoServiceProvider().GetBytes(salt);

            Rfc2898DeriveBytes db = new(value,
                                        salt,
                                        HashingIterations,
                                        HashAlgorithmName.SHA512);
            byte[] hash = db.GetBytes(32);
            return
                $"{db.HashAlgorithm.Name}{HashingDelimiter}" +
                $"{db.IterationCount}{HashingDelimiter}" +
                $"{Convert.ToBase64String(db.Salt)}{HashingDelimiter}" +
                $"{Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Checks if a given value is in (or rather was used to create) the given
        /// hash bundle.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="hashBundle">A <see cref="string"/> in the format:
        /// Algorithm.Iterations.Salt.Hash.</param>
        /// <returns>
        /// True if hashBundle was generated from value, false otherwise.
        /// </returns>
        public bool IsValueInHashBundle(string value, string hashBundle)
        {
            // Hashes can't be un-hashed (that's the point) so to test, we hash
            // the value passed in, and compare that to the existing hash in the
            // hash bundle. If the hashes match, then we can deduce that the
            // value is identical to the one used for the hash bundle.
            var hashBundleComponents = hashBundle.Split(HashingDelimiter);
            // algorithmName is unused. If needed we can grab it:
            // var algorithmName = hashBundleComponents[0].Trim(); 
            var iterations = Int32.Parse(hashBundleComponents[1].Trim());
            var salt = Convert.FromBase64String(hashBundleComponents[2].Trim());
            var hash = hashBundleComponents[3].Trim();

            var db = new Rfc2898DeriveBytes(value,
                                            salt,
                                            iterations,
                                            HashAlgorithmName.SHA512);
            byte[] testHash = db.GetBytes(32);

            if (Convert.ToBase64String(testHash) != hash)
                return false;

            return true;
        }


    }
}
