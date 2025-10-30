using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DualModeMonitorSystem.Convert;

namespace DualModeMonitorSystem.Models
{
    public class Alert
    {
        public string Title { get; set; }
        public DateTime Time { get; set; }

        public AlertLevel Level { get; set; }

        public AlertStatus Status { get;set; }

        public string Description { get; set; }
    }
}
