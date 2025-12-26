using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorLibrary.Models.Enums;

namespace MonitorLibrary.Models.Dto
{
    public class DeviceInfoDto : BindableBase
    {
        public string Name { get; set; }

        private DataPointDto _temperature;

        public DataPointDto Temperature
        {
            get { return _temperature; }
            set { _temperature = value; }
        }
        private DataPointDto _humidity;

        public DataPointDto Humidity
        {
            get { return _humidity; }
            set { _humidity = value; }
        }

        public DateTime LastUpdate { get; set; }
    }
}
