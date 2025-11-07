using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorLibrary.Models;

namespace MonitorApi.Services
{
    class HumitureDeviceService : GenericService<HumitureDevices>, IHumitureDeviceService
    {
        public HumitureDeviceService(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

    }
}
