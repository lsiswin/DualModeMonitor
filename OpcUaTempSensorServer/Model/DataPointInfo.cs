using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcUaTempSensorServer.Model
{
    public class DataPointInfo
    {
        public int Id { get; set; }

        /// <summary>
        /// 外键：关联的传感器ID
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// 数据点编码（唯一标识，如"Temp""Hum"）
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 数据点名称（如"温度""湿度"）
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 单位（如"℃""%RH"）
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// 数据格式
        /// </summary>
        public string DataType { get; set; } = string.Empty;

        /// <summary>
        /// 寄存器起始地址
        /// </summary>
        public int Address { get; set; }

        /// <summary>
        /// 倍率
        /// </summary>
        public double Scale { get; set; } = 1.0;

        /// <summary>
        /// 偏移
        /// </summary>
        public double Offset { get; set; } = 0.0;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnable { get; set; }
    }
}
