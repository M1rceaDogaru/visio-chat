using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectVisio.Communication
{
    public class SocketHandler : ISocketHandler
    {
        private static readonly SocketHandler _socketHandler = new SocketHandler();

        public static SocketHandler Instance => _socketHandler;

        public event Action<string> OnMessageReceived;

        private readonly List<WebSocket> _sockets;
        private List<string> _messageChunks;

        public SocketHandler()
        {
            _sockets = new List<WebSocket>();
            _messageChunks = new List<string>();
        }

        public void BroadcastMessage(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);

            var closedSockets = new List<WebSocket>();
            _sockets.ForEach(async (socket) =>
            {
                if (socket != null && socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else if (socket.State != WebSocketState.Connecting)
                {
                    closedSockets.Add(socket);
                }
            });

            closedSockets.ForEach(socket => _sockets.Remove(socket));
        }

        public void Subscribe(WebSocket socket)
        {
            _sockets.Add(socket);
        }

        public async Task GetIncomingMessages(WebSocket socket)
        {
            while (socket.State == WebSocketState.Open)
            {
                var token = CancellationToken.None;
                var buffer = new ArraySegment<byte>(new byte[4096]);
                var received = await socket.ReceiveAsync(buffer, token);

                var receivedMessage = Encoding.UTF8.GetString(buffer.Array, 0, received.Count);
                _messageChunks.Add(receivedMessage);

                if (received.EndOfMessage)
                {
                    OnMessageReceived?.Invoke(string.Concat(_messageChunks));
                    _messageChunks.Clear();
                }
            }
        }
    }
}
