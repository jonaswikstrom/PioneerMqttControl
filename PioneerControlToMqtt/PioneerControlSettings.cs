using System;
using System.Collections.Generic;
using System.Text;

namespace PioneerControlToMqtt
{
    public class PioneerControlSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public TimeSpan HeartbeatTimeout { get; set; } = TimeSpan.Parse("00:01:30");
    }
}
