namespace MCStatus.Packets; 

internal class StatusPacket : Packet {
    protected override int PacketId => 0x00;

    protected override ValueTask InitAsync(Stream stream) {
        // no fields
        return ValueTask.CompletedTask;
    }

    public async ValueTask<StatusResponse> ReceiveAsync(Stream stream) {
        await ReceiveHeaderAsync(stream);
        var json = await stream.ReadLengthPrefixedUtf8StringAsync();
        return StatusResponse.Deserialize(json);
    }
}
