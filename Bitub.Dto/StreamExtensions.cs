using System.IO;

namespace Bitub.Dto
{
    public static class StreamExtensions
    {
        public static byte[] ToByteArray(this Stream stream, int bufferSize = 1024)
        {
            var buffer = new byte[bufferSize];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    ms.Write(buffer, 0, read);

                return ms.ToArray();
            }
        }
    }
}
