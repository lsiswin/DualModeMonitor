using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorLibrary.Models;

namespace MonitorApi.Services
{
    public interface IDataPointService: IGenericService<DataPoint>
    {
        //根据DeviceId获取数据点列表
        Task<List<DataPoint>> GetDataPointsByDeviceIdAsync(int deviceId);
    }

}
