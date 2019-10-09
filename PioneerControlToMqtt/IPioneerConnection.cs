using System;
using System.Threading.Tasks;

namespace PioneerControlToMqtt
{
    public interface IPioneerConnection : IDisposable
    {
        Task ConnectAsync();
        Task SendCommandAsync(string command);

    }
}