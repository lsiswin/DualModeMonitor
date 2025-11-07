using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorLibrary.Models;

namespace MonitorApi.Services
{
    class ModbusConfigService : GenericService<ModbusConfig>, IModbusConfigService
    {
        public ModbusConfigService(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
