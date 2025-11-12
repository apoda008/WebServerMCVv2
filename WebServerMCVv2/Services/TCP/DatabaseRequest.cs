using System.Buffers.Binary;
using System.Text;

namespace WebServerMVCv2.Services.TCP
{
    public class DatabaseRequest
    {
        public static byte[] BuildMessage(string auth, bool isStream, long videoPosition, long requestLength, string request)
        {
            // fixed header = 77 bytes + variable request
            byte[] requestBytes = Encoding.UTF8.GetBytes(request ?? string.Empty);
            int total = 64 + 1 + 8 + 8 + 4 + requestBytes.Length;

            byte[] buf = new byte[total];
            var span = buf.AsSpan();

            // auth (64 bytes, zero-padded)
            var authBytes = Encoding.ASCII.GetBytes(auth ?? string.Empty);

            //checks length to ensure that 64 bytes is actually filled
            int copy = Math.Min(authBytes.Length, 64);
            authBytes.AsSpan(0, copy).CopyTo(span[..64]);

            // kind (1 byte)
            span[64] = isStream ? (byte)1 : (byte)0;

            // video_position (8 bytes, big-endian)
            BinaryPrimitives.WriteUInt64BigEndian(span.Slice(65, 8), unchecked((ulong)videoPosition));

            // request length (8 bytes, big-endian)
            BinaryPrimitives.WriteUInt64BigEndian(span.Slice(73, 8), unchecked((ulong)requestLength));

            // request length (4 bytes, big-endian)
            BinaryPrimitives.WriteUInt32BigEndian(span.Slice(81, 4), (uint)requestBytes.Length);

            // request payload
            requestBytes.AsSpan().CopyTo(span.Slice(85));

            return buf;
        }
    }
}
