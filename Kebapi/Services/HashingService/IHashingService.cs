namespace Kebapi.Services.Hashing
{
    /// <summary>
    /// Interface for our hashing service.
    /// <para>
    /// Defines methods for creating and checking string values 
    /// (typically passwords) against "hash bundles". We are defining a hash bundle
    /// as a delimited string in this format:
    /// </para>
    /// <para>
    ///      Algorithm.Iterations.Salt.Hash
    /// </para>
    /// e.g.
    /// <para>
    ///      SHA512.10000.kMsQ/KtK0KmLAC/U3BNDZGQ72EomNdTe.0FiLdBGPh9oydfSPnSpX...
    /// </para>
    /// </summary>
    public interface IHashingService
    {
        string GenerateHashBundleFromValue(string value);
        bool IsValueInHashBundle(string value, string hashBundle);
    }
}