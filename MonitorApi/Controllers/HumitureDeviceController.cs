using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MonitorApi.Services;
using MonitorLibrary.Models;

namespace MonitorApi.Controllers
{
    [ApiController]
    [Route("api/devices")]
    public class HumitureDeviceController
    {
        private readonly IHumitureDeviceService deviceService;

        public HumitureDeviceController(IHumitureDeviceService deviceService)
        {
            this.deviceService = deviceService;
        }
        /// <summary>
        /// 获取所有设备信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResponse<List<HumitureDevices>>> GetDevices()
        {
            var devices = await deviceService.GetAllAsync("SerialPortConfig");
            return ApiResponse<List<HumitureDevices>>.SuccessResult(devices);
        }
        /// <summary>
        /// 根据ID获取设备
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ApiResponse<HumitureDevices>> GetDeviceById(int id)
        {
            var device = await deviceService.GetByIdAsync(id);
            if (device == null)
            {
                return ApiResponse<HumitureDevices>.ErrorResult("找不到对应设备") ;
            }
            return ApiResponse<HumitureDevices>.SuccessResult(device);
        }
        /// <summary>
        /// 新增设备
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResponse<HumitureDevices>> CreateDevice(HumitureDevices device)
        {
            await deviceService.AddAsync(device);
            return ApiResponse<HumitureDevices>.SuccessResult(device);
        }
        /// <summary>
        /// 更新设备
        /// </summary>
        /// <param name="id"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<ApiResponse<HumitureDevices>> UpdateDevice(int id, HumitureDevices device)
        {
            var existingDevice = await deviceService.GetByIdAsync(id);
            if (existingDevice == null)
            {
                return ApiResponse<HumitureDevices>.ErrorResult("找不到对应设备");
            }
            var updatedDevice = deviceService.UpdateAsync(device);
            return ApiResponse<HumitureDevices>.SuccessResult(device);
        }
        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public ApiResponse<bool> DeleteDevice(int id)
        {
            var existingDevice = deviceService.GetByIdAsync(id);
            if (existingDevice == null)
            {
                return ApiResponse<bool>.ErrorResult("找不到对应设备");
            }
            deviceService.DeleteAsync(id);
            return ApiResponse<bool>.SuccessResult(true,"删除成功");
        }


    }
}
