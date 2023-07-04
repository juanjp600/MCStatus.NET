using System.Text;

namespace MCStatus; 

internal static class StreamExtensions {
    private const int SEGMENT_BITS = 0x7F;
    private const int CONTINUE_BIT = 0x80;

    public static ValueTask WriteByteAsync(this Stream stream, byte b) {
        var bytes = new[] {b};
        return stream.WriteAsync(bytes);
    }

    public static ValueTask WriteUnsignedShortAsync(this Stream stream, ushort s) {
        return stream.WriteAsync(BitConverter.GetBytes(s));
    }

    public static ValueTask WriteLongAsync(this Stream stream, long l) {
        return stream.WriteAsync(BitConverter.GetBytes(l));
    }

    // Source: https://wiki.vg/Protocol#VarInt_and_VarLong
    public static async ValueTask WriteVarIntAsync(this Stream stream, int i) {
        while (true) {
            if ((i & ~SEGMENT_BITS) == 0) {
                await stream.WriteByteAsync((byte)i);
                return;
            }

            await stream.WriteByteAsync((byte)((i & SEGMENT_BITS) | CONTINUE_BIT));

            // Note: >>> means that the sign bit is shifted with the rest of the number rather than being left alone
            // Reimplemented in terms of casts and long right shift because .NET 6 doesn't have unsigned right shift
            i = (int)(((long)(uint)i) >> 7);
        }
    }

    public static async ValueTask WriteLengthPrefixedUtf8StringAsync(this Stream stream, string str) {
        var bytes = Encoding.UTF8.GetBytes(str);
        await stream.WriteVarIntAsync(bytes.Length);
        await stream.WriteAsync(bytes);
    }

    public static async ValueTask ReadExactlyAsync(this Stream stream, byte[] bytes)
    {
        int readCount = await stream.ReadAsync(bytes);
        if (readCount < bytes.Length) { throw new EndOfStreamException(); }
    }

    public static async ValueTask<long> ReadLongAsync(this Stream stream) {
        var bytes = new byte[sizeof(long)];
        await stream.ReadExactlyAsync(bytes);
        return BitConverter.ToInt64(bytes);
    }

    // Source: https://wiki.vg/Protocol#VarInt_and_VarLong
    public static async ValueTask<int> ReadVarIntAsync(this Stream stream) {
        int value = 0;
        int position = 0;
        var bytes = new byte[1];

        while (true) {
            await stream.ReadExactlyAsync(bytes);

            value |= (bytes[0] & SEGMENT_BITS) << position;

            if ((bytes[0] & CONTINUE_BIT) == 0) break;

            position += 7;

            if (position >= 32) throw new("VarInt is too big");
        }

        return value;
    }

    public static async ValueTask<string> ReadLengthPrefixedUtf8StringAsync(this Stream stream) {
        var length = await stream.ReadVarIntAsync();
        var bytes = new byte[length];
        await stream.ReadExactlyAsync(bytes);
        return Encoding.UTF8.GetString(bytes);
    }
}
