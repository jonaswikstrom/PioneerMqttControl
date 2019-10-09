using System;

namespace PioneerControlToMqtt
{
    public class ConnectionHandler : IConnectionHandler
    {
        private readonly IConfiguration configuration;
        private DateTime? resetDate;

        public ConnectionHandler(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void Reset()
        {
            resetDate = DateTime.Now;
        }

        public void Disconnect()
        {
            resetDate = DateTime.MinValue;
        }

        public bool IsConnected => resetDate == null || (DateTime.Now-resetDate.Value) <= configuration.ConnectionTimeOut;
    }
}