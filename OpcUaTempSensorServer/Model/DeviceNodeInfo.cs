using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opc.Ua;

namespace OpcUaTempSensorServer.Model
{
    /// <summary>
    /// 设备节点信息
    /// </summary>
    public class DeviceNodeInfo
    {
        public int DeviceId { get; set; }
        public FolderState DeviceFolder { get; set; } = null!;
        public Dictionary<int, BaseDataVariableState> DataPointNodes { get; set; } = null!;
    }
}
