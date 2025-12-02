using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MonitorApi.Services;
using MonitorLibrary.Models;

namespace MonitorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DataPointController
    {
        private readonly IDataPointService dataPointService;

        public DataPointController(IDataPointService dataPointService)
        {
            this.dataPointService = dataPointService;
        }

        /// <summary>
        /// 根据ID获取数据点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ApiResponse<DataPoint>> GetDataPointById(int id)
        {
            var dataPoint = await dataPointService.GetByIdAsync(id);
            if (dataPoint == null)
            {
                return ApiResponse<DataPoint>.ErrorResult("找不到对应数据点");
            }
            return ApiResponse<DataPoint>.SuccessResult(dataPoint);
        }

        /// <summary>
        /// 创建数据点
        /// </summary>
        /// <param name="dataPoint"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<ApiResponse<DataPoint>> CreateDataPoint(DataPoint dataPoint)
        {
            await dataPointService.AddAsync(dataPoint);
            return ApiResponse<DataPoint>.SuccessResult(dataPoint);
        }

        /// <summary>
        /// 更新数据点
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dataPoint"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<ApiResponse<DataPoint>> UpdateDataPoint(
            int id,
            [FromBody] DataPoint dataPoint
        )
        {
            await dataPointService.UpdateAsync(id, dataPoint);
            return ApiResponse<DataPoint>.SuccessResult(dataPoint);
        }

        /// <summary>
        /// 删除数据点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public ApiResponse<bool> DeleteDataPoint(int id)
        {
            var existingDataPoint = dataPointService.GetByIdAsync(id);
            if (existingDataPoint == null)
            {
                return ApiResponse<bool>.ErrorResult("找不到对应数据点");
            }
            dataPointService.DeleteAsync(id);
            return ApiResponse<bool>.SuccessResult(true, "删除成功");
        }

        /// <summary>
        /// 根据设备Id查询所有数据点
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [HttpGet("{deviceId}")]
        public async Task<ApiResponse<IEnumerable<DataPoint>>> GetDataPointsByDeviceId(int deviceId)
        {
            var dataPoints = await dataPointService.GetDataPointsByDeviceIdAsync(deviceId);
            return ApiResponse<IEnumerable<DataPoint>>.SuccessResult(dataPoints);
        }
    }
}
