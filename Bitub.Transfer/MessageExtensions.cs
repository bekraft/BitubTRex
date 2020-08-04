using System;
using System.Text;
using System.Security.Cryptography;

using Google.Protobuf;

namespace Bitub.Transfer
{
    public static class MessageExtensions
    {
        private static MD5 HashingMD5 = MD5.Create();

        /// <summary>
        /// Converts an array of bytes into a protobuf byte string.
        /// </summary>
        /// <param name="bytes">The array</param>
        /// <returns>The protobuf byte string</returns>
        public static ByteString ToByteString(this byte[] bytes)
        {
            return ByteString.CopyFrom(bytes);
        }

        /// <summary>
        /// Compute the Md5 hash a return the hex description of it.
        /// </summary>
        /// <param name="message">Any proto message</param>
        /// <returns>A string using hex alphabet</returns>
        public static string ToMd5Hex(this IMessage message)
        {
            var hash = HashingMD5.ComputeHash(message.ToByteArray());
            var sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("x2"));            

            return sb.ToString();
        }

        /// <summary>
        /// Compute the Md5 hash and returns the bas64 description of it.
        /// </summary>
        /// <param name="message">Any proto message</param>
        /// <see cref="System.Convert.ToBase64String(byte[])"/>
        /// <returns>A string using base64 alphabet</returns>
        public static string ToMd5Base64(this IMessage message)
        {
            var hash = HashingMD5.ComputeHash(message.ToByteArray());
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Compares the MD5 Base64 representations.
        /// </summary>
        /// <param name="message">A protobuf message</param>
        /// <param name="base64">A Base64 MD5 stamp</param>
        /// <returns>True, if equal</returns>
        public static bool IsEqualMd5Base64(this IMessage message, string base64)
        {
            return StringComparer.Ordinal.Compare(message.ToMd5Base64(), base64) == 0;
        }

        public static int AsMask<T>(params T[] someCases) where T : Enum
        {
            if (Enum.GetUnderlyingType(typeof(T)) != typeof(Int32))
                throw new ArgumentException($"Expecting only integer types");

            int mask = 0;
            foreach (var caseOfEnum in someCases)
                mask |= Convert.ToInt32(caseOfEnum);

            return mask;
        }

        public static bool IsFlaggedExactly(this Enum anEnum, int mask)
        {
            if (Enum.GetUnderlyingType(anEnum.GetType()) != typeof(Int32))
                throw new ArgumentException($"Expecting only integer types");

            return (Convert.ToInt32(anEnum) & mask) == mask;
        }

        public static bool IsFlaggedExactly<T>(this T anEnum, params T[] mask) where T : Enum
        {
            if (Enum.GetUnderlyingType(typeof(T)) != typeof(Int32))
                throw new ArgumentException($"Expecting only integer types");

            int nMask = AsMask(mask);

            return (Convert.ToInt32(anEnum) & nMask) == nMask;
        }

        public static bool IsFlagged(this Enum anEnum, int mask)
        {
            if (Enum.GetUnderlyingType(anEnum.GetType()) != typeof(Int32))
                throw new ArgumentException($"Expecting only integer types");

            return (Convert.ToInt32(anEnum) & mask) != 0;
        }

        public static bool IsFlagged<T>(this T anEnum, params T[] mask) where T : Enum
        {
            if (Enum.GetUnderlyingType(typeof(T)) != typeof(Int32))
                throw new ArgumentException($"Expecting only integer types");

            int nMask = AsMask(mask);

            return (Convert.ToInt32(anEnum) & nMask) != 0;
        }

    }
}
