using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PioneerControlToMqtt.Mqtt;

namespace PioneerControlToMqtt
{
    public class PioneerConnection : IPioneerConnection
    {
        private readonly ILogger<PioneerConnection> logger;
        private readonly IOptions<PioneerControlSettings> settings;
        private readonly IEnumerable<IMessageHandler> messageHandlers;
        private readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public PioneerConnection(ILogger<PioneerConnection> logger, IOptions<PioneerControlSettings> settings, IEnumerable<IMessageHandler> messageHandlers)
        {
            this.logger = logger;
            this.settings = settings;
            this.messageHandlers = messageHandlers;
        }

        public async Task ConnectAsync()
        {
            if (socket.Connected) return;

            logger.LogInformation($"Connecting Pioneer receiver at {settings.Value.HostName}");
            try
            {
                await socket.ConnectAsync(new IPEndPoint(IPAddress.Parse(settings.Value.HostName), settings.Value.Port));
            }
            catch (SocketException e)
            {
                logger.LogError(e, "An error occured trying to connect");
                throw;
            }

            logger.LogInformation("Connected");
            await Task.WhenAll(messageHandlers.Select(p => p.SubscribeToCommandTopic()));

#pragma warning disable 4014
            ReceiveLoop();
#pragma warning restore 4014
        }

        public async Task SendCommandAsync(string command)
        {
            logger.LogInformation($"Sending command: {command}");
            var buffer = Encoding.Default.GetBytes($"{command}\r\n");
            var segment = new ArraySegment<byte>(buffer);

            await socket.SendAsync(segment, SocketFlags.None);
        }

        private async Task ReceiveLoop()
        {
            while (true)
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