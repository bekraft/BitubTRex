using System;
using System.Text;
using System.Security.Cryptography;

using Google.Protobuf;

namespace Bitub.Transfer
{
    public static class MessageExtensions
    {
        private static MD5 Md5Hashing = MD5.Create();

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
            var hash = Md5Hashing.ComputeHash(message.ToByteArray());
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
            var hash = Md5Hashing.ComputeHash(message.ToByteArray());
            return Convert.ToBase64String(hash);
        }

        public static bool IsDifferentToBase64(this IMessage message, string base64)
        {
            return StringComparer.Ordinal.Compare(message, message.ToMd5Base64()) == 0;
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
