using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opc.Ua;

namespace OpcUaMonitorServer.Model
{
    /// <summary>
    /// 每个设备对应的节点引用集合
    /// </summary>
    public class DeviceNodeGroup
    {
        public VariableTypeNode Temperature { get; set; }
        public VariableTypeNode Humidity { get; set; }
    }
}
