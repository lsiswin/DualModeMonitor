using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorLibrary.Models;

namespace MonitorApi.Services
{
    public class DataPointService : GenericService<DataPoint>, IDataPointService
    {
        public DataPointService(ApplicationDbContext dbContext) : base(dbContext)
        {

        }
        /// <summary>
        /// 根据设备ID获取数据点列表
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public async Task<List<DataPoint>> GetDataPointsByDeviceIdAsync(int deviceId)
        {
            // 使用LINQ查询数据库，获取指定设备ID的数据点列表
            return await GetByConditionAsync(dp => dp.DeviceId == deviceId);           

        }
    }
}
