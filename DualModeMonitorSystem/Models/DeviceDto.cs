using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace DualModeMonitorSystem.Models
{
    public class DeviceDto
    {
        public Guid Id { get; set; }
        public string PortName { get; set; }
        public string Position { get; set; }

        public string Temperature { get; set; }
        public string Humidity { get; set; }

        public string Tag { get; set; }
        public string StatusText { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdated { get; set; }

    }
}
