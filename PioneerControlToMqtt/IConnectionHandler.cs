namespace PioneerControlToMqtt
{
    public interface IConnectionHandler
    {
        void Reset();

        void Disconnect();
        bool IsConnected { get; }
    }
}