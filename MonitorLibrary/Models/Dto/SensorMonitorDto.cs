using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorLibrary.Models.Dto
{
    public class SensorMonitorDto : BindableBase
    {
        // 核心标识：来自 OpcDataMessage.Name
        public string DeviceName { get; set; }

        // --- 温度维度 ---
        private double _temperature;
        public double Temperature
        {
            get => _temperature;
            set => SetProperty(ref _temperature, value);
        }

        private string _tempQuality;
        public string TempQuality
        {
            get => _tempQuality;
            set => SetProperty(ref _tempQuality, value);
        }

        // --- 湿度维度 ---
        private double _humidity;
        public double Humidity
        {
            get => _humidity;
            set => SetProperty(ref _humidity, value);
        }

        private string _humQuality;
        public string HumQuality
        {
            get => _humQuality;
            set => SetProperty(ref _humQuality, value);
        }

        // --- 公共状态 ---
        private DateTime _lastUpdate;
        public DateTime LastUpdate
        {
            get => _lastUpdate;
            set => SetProperty(ref _lastUpdate, value);
        }

        // 辅助属性：界面单位
        public string TempUnit => "℃";
        public string HumUnit => "%RH";
    }
}
