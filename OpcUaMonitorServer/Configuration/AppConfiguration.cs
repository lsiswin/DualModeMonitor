using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcUaMonitorServer.Configuration
{
    public class OpcServerConfiguration
    {
        public string ApplicationName { get; set; } = "Monitor OPC UA Server";
        public string ApplicationUri { get; set; } = "urn:localhost:MonitorOPCServer";
        public string ProductUri { get; set; } = "http://monitor.com/opcserver";
        public int Port { get; set; } = 4840;
        public int MaxSessionTimeout { get; set; } = 3600000;
        public int MinSessionTimeout { get; set; } = 10000;
    }

    public class MonitorApiConfiguration
    {
        public string BaseUrl { get; set; } = "https://localhost:7137";
    }

    public class DataCollectionConfiguration
    {
        public int ScanIntervalMs { get; set; } = 5000;
        public int RetryAttempts { get; set; } = 3;
        public int RetryDelayMs { get; set; } = 1000;
    }
}
