using System.Runtime.Serialization;

namespace Bet.Extensions.LetsEncrypt.Order
{
    /// <summary>
    /// The supported algorithms.
    /// </summary>
    public enum KeyAlgorithm
    {
        /// <summary>
        /// RSASSA-PKCS1-v1_5 using SHA-256.
        /// </summary>
        [EnumMember(Value = "RS256")]
        RS256 = 0,

        /// <summary>
        /// ECDSA using P-256 and SHA-256.
        /// </summary>
        [EnumMember(Value = "ES256")]
        ES256 = 1,

        /// <summary>
        /// ECDSA using P-384 and SHA-384.
        /// </summary>
        [EnumMember(Value = "ES384")]
        ES384 = 2,

        /// <summary>
        /// ECDSA using P-521 and SHA-512.
        /// </summary>
        [EnumMember(Value = "ES512")]
        ES512 = 3
    }
}
