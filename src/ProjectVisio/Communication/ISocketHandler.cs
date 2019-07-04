using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace ProjectVisio.Communication
{
    public interface ISocketHandler
    {
        event Action<string> OnMessageReceived;

        void BroadcastMessage(string message);

        void Subscribe(WebSocket socket);

        Task GetIncomingMessages(WebSocket socket);
    }
}