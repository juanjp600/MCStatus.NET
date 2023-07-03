namespace MCStatus.Packets; 

internal abstract class Packet : IDisposable, IAsyncDisposable {
    private bool _initialized = false;

    private readonly MemoryStream _memoryStream;

    protected Packet() {
        _memoryStream = new();
    }

    public void Dispose() {
        _memoryStream.Dispose();
    }

    public ValueTask DisposeAsync() {
        return _memoryStream.DisposeAsync();
    }

    protected abstract int PacketId { get; }

    protected abstract ValueTask InitAsync(Stream stream);

    public async ValueTask SendToAsync(Stream stream) {
        if (!_initialized) {
            _initialized = true;
            await _memoryStream.WriteVarIntAsync(PacketId);
            await InitAsync(_memoryStream);
        }

        await stream.WriteVarIntAsync((int)_memoryStream.Length);
        await stream.WriteAsync(new(_memoryStream.GetBuffer(), 0, (int)_memoryStream.Length));
    }

    protected async ValueTask<int> ReceiveHeaderAsync(Stream stream) {
        var length = await stream.ReadVarIntAsync();
        var id = await stream.ReadVarIntAsync();
        if (id != PacketId) {
            throw new($"Unexpected packet id: 0x{id:X2} != 0x{PacketId:X2}");
        }

        return length;
    }
}
