using System;
using System.Runtime.CompilerServices;
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
            var hash = Convert.FromBase64String(hashBundleComponents[3].Trim()); 

            var db = new Rfc2898DeriveBytes(value,
                                            salt,
                                            iterations,
                                            HashAlgorithmName.SHA512);
            byte[] testHash = db.GetBytes(32);

            return UnoptimisedEqual(testHash, hash); 
        }

        /// <summary>
        /// Compares the equality of two byte arrays. Specifically written so that
        /// the loop is not optimized. 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>True if equal, false otherwise.</returns>
        // Brute force attacks can take advantage of equality timing. E.g. a vanilla
        // string comparison (string a == string b) will fail as soon as the first
        // byte doesn't match. That can give an indication of how unsuccessful a
        // particular attack was because it may have failed quicker or slower than a
        // previous attack, hence giving the attacker an opportunity to fine tune.
        // The purpose of an unoptimised comparison, is that every single comparison
        // always takes the same amount of time. Here, all iterable bytes are alwayss
        // compared before returning a result, and any null/length comparison failures
        // should also complete consistently.
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static bool UnoptimisedEqual(byte[] a, byte[] b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }
            var areEqual = true;
            for (var i = 0; i < a.Length; i++)
            {
                areEqual &= (a[i] == b[i]);
            }
            return areEqual;
        }

    }
}
