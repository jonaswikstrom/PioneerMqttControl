using System.Threading;
using System.Threading.Tasks;

namespace PioneerControlToMqtt
{
    public interface IMessageHandler
    {
        Task HandleReceiverMessage(string message);
        Task HandleMqttMessage();

        Task SubscribeToCommandTopic();
    }
}