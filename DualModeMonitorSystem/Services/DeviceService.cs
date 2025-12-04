using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorLibrary.HttpService;
using MonitorLibrary.Models;
using MonitorLibrary.Models.Enums;

namespace DualModeMonitorSystem.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IHttpService _httpService;

        public DeviceService(IHttpService httpService)
        {
            this._httpService = httpService;
        }

        public async Task<ApiResponse<HumitureDevices>> CreateDeviceAsync(HumitureDevices device)
        {
            return await _httpService.PostAsync<ApiResponse<HumitureDevices>>(
                "/api/devices",
                device
            );
        }

        public async Task<ApiResponse<bool>> DeleteDeviceAsync(int id)
        {
            return await _httpService.DeleteAsync<ApiResponse<bool>>($"/api/devices/{id}");
        }

        public async Task<ApiResponse<List<HumitureDevices>>> GetAllDevicesAsync()
        {
            return await _httpService.GetAsync<ApiResponse<List<HumitureDevices>>>("/api/devices");
        }

        public async Task<ApiResponse<HumitureDevices>> GetDeviceByIdAsync(int id)
        {
            return await _httpService.GetAsync<ApiResponse<HumitureDevices>>($"/api/devices/{id}");
        }

        public async Task<List<DeviceStatus>> GetDeviceStatusAsync()
        {
            return await _httpService.GetAsync<List<DeviceStatus>>("/api/devices/status");
        }

        public async Task<ApiResponse<HumitureDevices>> UpdateDeviceAsync(HumitureDevices device)
        {
            return await _httpService.PutAsync<ApiResponse<HumitureDevices>>(
                $"/api/devices/{device.Id}",
                device
            );
        }

        public async Task<ApiResponse<List<DataPoint>>> GetDataPointByDevice(int id)
        {
            return await _httpService.GetAsync<ApiResponse<List<DataPoint>>>(
                $"/api/DataPoint/GetDataPointsByDeviceId/{id}"
            );
        }

        public async Task<ApiResponse<DataPoint>> UpdateDataPointAsync(DataPoint dataPoint)
        {
            return await _httpService.PutAsync<ApiResponse<DataPoint>>(
                $"/api/DataPoint/UpdateDataPoint/{dataPoint.Id}",
                dataPoint
            );
        }

        public async Task<ApiResponse<bool>> DeleteDataPointAsync(int id)
        {
            return await _httpService.DeleteAsync<ApiResponse<bool>>(
                $"/api/DataPoint/DeleteDataPoint/{id}"
            );
        }

        public async Task<ApiResponse<DataPoint>> CreateDataPointAsync(DataPoint newDataPoint)
        {
            return await _httpService.PostAsync<ApiResponse<DataPoint>>(
                $"/api/DataPoint/CreateDataPoint",
                newDataPoint
            );
        }
    }
}
