using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PioneerControlToMqtt
{
    public class PioneerConnection : IPioneerConnection
    {
        private readonly ILogger<PioneerConnection> logger;
        private readonly IConnectionHandler connectionHandler;
        private readonly IEnumerable<IMessageHandler> messageHandlers;
        private readonly PioneerConnectionInfo connectionInfo;
        private readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public PioneerConnection(ILogger<PioneerConnection> logger, IConfiguration configuration, IConnectionHandler connectionHandler,
            IEnumerable<IMessageHandler> messageHandlers)
        {
            this.logger = logger;
            this.connectionHandler = connectionHandler;
            this.messageHandlers = messageHandlers;
            this.connectionInfo = configuration.ConnectionInfo;
        }

        public async Task ConnectAsync()
        {
            if (socket.Connected) return;

            logger.LogInformation($"Connecting {connectionInfo}");
            try
            {
                await socket.ConnectAsync(connectionInfo.IpEndPoint);
            }
            catch (SocketException e)
            {
                logger.LogError(e, "An error occured trying to connect");
                await Reconnect();
            }

            connectionHandler.Reset();
            logger.LogInformation("Connected");

            await Task.WhenAll(messageHandlers.Select(p => p.SubscribeToCommandTopic()));

            DisconnectLoop();
            ReceiveLoop();
        }

        public async Task SendCommandAsync(string command)
        {
            logger.LogInformation($"Sending command: {command}");
            var buffer = Encoding.Default.GetBytes($"{command}\r\n");
            var segment = new ArraySegment<byte>(buffer);

            await socket.SendAsync(segment, SocketFlags.None);
        }

        private async Task DisconnectLoop()
        {
            while (socket.Connected && connectionHandler.IsConnected)
            {
                await Task.Delay(500);
            }

            socket.Disconnect(true);
            logger.LogInformation("Connection closed");
            await Reconnect();
        }

        private async Task Reconnect()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            logger.LogInformation("Reconnecting");
            await ConnectAsync();
        }

        private async Task ReceiveLoop()
        {
            while (socket.Connected && connectionHandler.IsConnected)
            {
                var message = await ReceiveAsync();
                if (string.IsNullOrWhiteSpace(message)) continue;
                logger.LogInformation($"Message received: '{message}'");

                var messageTasks = messageHandlers.Select(p => p.HandleReceiverMessage(message));
                await Task.WhenAll(messageTasks);
            }
        }

        private async Task<string> ReceiveAsync()
        {
            var receiveBytes = new byte[100];
            var segment = new ArraySegment<byte>(receiveBytes);
            var receive = await socket.ReceiveAsync(segment, SocketFlags.None);

            if (receive == 0) return null;
            var encoded = Encoding.ASCII.GetString(receiveBytes, 0, receive);
            return encoded.Replace("\r\n", "");
        }

        public void Dispose()
        {
            socket?.Dispose();
        }
    }
}