using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorLibrary.Models;
using MonitorLibrary.Models.Enums;

namespace DualModeMonitorSystem.Services
{
    public interface IDeviceService
    {
        Task<ApiResponse<List<HumitureDevices>>> GetAllDevicesAsync();
        Task<ApiResponse<HumitureDevices>>GetDeviceByIdAsync(int id);
        Task<ApiResponse<HumitureDevices>> CreateDeviceAsync(HumitureDevices device);
        Task<ApiResponse<HumitureDevices>> UpdateDeviceAsync(HumitureDevices device);
        Task<bool> DeleteDeviceAsync(int id);
        Task<List<DeviceStatus>> GetDeviceStatusAsync();

        Task<ApiResponse<List<DataPoint>>> GetDataPointByDevice(int id);
        Task<ApiResponse<DataPoint>> UpdateDataPointAsync(DataPoint dataPoint);
    }
}
