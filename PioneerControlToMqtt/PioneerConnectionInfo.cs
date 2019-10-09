using System.Net;

namespace PioneerControlToMqtt
{
    public struct PioneerConnectionInfo
    {
        public string Host { get; }
        public int Port { get; }

        public PioneerConnectionInfo(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public IPEndPoint IpEndPoint => new IPEndPoint(IPAddress.Parse(Host), Port);

        public override string ToString()
        {
            return $"{Host}:{Port}";
        }
    }
}