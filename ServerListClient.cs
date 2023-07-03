using System.Net.Sockets;
using MCStatus.Packets;

namespace MCStatus; 

public class ServerListClient : IDisposable {
    private readonly TcpClient _tcp;

    public string Host { get; }
    public int Port { get; }

    public StatusResponse Response { get; private set; }

    public static async ValueTask<StatusResponse> GetStatusAsync(string host, ushort port = 25565) {
        using var ret = new ServerListClient(host, port);
        var client = ret._tcp;
        await client.ConnectAsync(host, port);
        var stream = client.GetStream();

        await using var packet = new HandshakePacket(host, port);
        await packet.SendToAsync(stream);

        await using var statusPacket = new StatusPacket();
        await statusPacket.SendToAsync(stream);
        var res = await statusPacket.ReceiveAsync(stream);

        ret.Response = res;
        return res;
    }

    private ServerListClient(string host, int port) {
        _tcp = new();
        Host = host;
        Port = port;
    }

    // Currently unused, this only seemed to work once before it would terminate the connection for some reason
    // The utility is also questionable
    private async ValueTask PingAsync() {
        await using var packet = new PingPacket();
        var stream = _tcp.GetStream();
        await packet.SendToAsync(stream);
        await packet.ReceiveFromAsync(stream);
    }

    public void Dispose() {
        _tcp.Dispose();
    }
}
