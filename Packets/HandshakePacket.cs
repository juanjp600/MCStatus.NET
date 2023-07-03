namespace MCStatus.Packets; 

internal class HandshakePacket : Packet {
    private readonly string _host;
    private readonly ushort _port;

    protected override int PacketId => 0x00;

    protected override async ValueTask InitAsync(Stream stream) {
        await stream.WriteVarIntAsync(-1); // protocol version
        await stream.WriteLengthPrefixedUtf8StringAsync(_host);
        await stream.WriteUnsignedShortAsync(_port);
        await stream.WriteVarIntAsync(1); // next state: 1 => status, 2 => login
    }

    public HandshakePacket(string host, ushort port) {
        _host = host;
        _port = port;
    }
}
