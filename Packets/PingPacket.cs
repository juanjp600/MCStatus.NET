namespace MCStatus.Packets; 

internal class PingPacket : Packet {
    private readonly long _sent = DateTimeOffset.UtcNow.UtcTicks;

    protected override int PacketId => 0x01;

    protected override ValueTask InitAsync(Stream stream) {
        return stream.WriteLongAsync(_sent);
    }

    public async ValueTask ReceiveFromAsync(Stream stream) {
        await ReceiveHeaderAsync(stream);
        var resp = await stream.ReadLongAsync();
        if (resp != _sent) {
            throw new($"Returned value did not match sent value: {resp} != {_sent}");
        }
    }
}
