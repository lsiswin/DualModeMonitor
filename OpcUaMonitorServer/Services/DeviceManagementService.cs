using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MonitorLibrary.HttpService;
using MonitorLibrary.Models;
using MonitorLibrary.Models.Enums;
using MonitorLibrary.Reactive;
using Newtonsoft.Json;
using OpcUaMonitorServer.Configuration;
using OpcUaMonitorServer.Model;

namespace OpcUaMonitorServer.Services
{
    /// <summary>
    /// 设备管理服务 - 通过API查询设备和数据点
    /// </summary>
    public interface IDeviceManagementService
    {
        Task<List<DeviceInfo>> GetDevicesAsync();
        Task<List<DataPointInfo>> GetDataPointsAsync(int deviceId);
        Task RefreshDevicesAsync();
    }

    public class DeviceManagementService : IDeviceManagementService
    {
        private readonly IHttpService _httpService;
        private readonly ReactiveLogger _logger;
        private List<DeviceInfo> _cachedDevices = new();
        private ConcurrentDictionary<int, List<DataPointInfo>> _cachedDataPoints = new();

        public DeviceManagementService(
            IHttpService httpService,
            ReactiveLogger logger,
            IOptions<MonitorApiConfiguration> options
        )
        {
            _httpService = httpService;
            _httpService.SetBaseAddress(options.Value.BaseUrl);
            _logger = logger;
        }

        /// <summary>
        /// 获取指定设备的数据点列表
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<List<DataPointInfo>> GetDataPointsAsync(int deviceId)
        {
            try
            {
                if (_cachedDataPoints.TryGetValue(deviceId, out var cachedDataPoints))
                {
                    _logger.LogInformation(
                        $"从缓存中获取到 {cachedDataPoints.Count} 个数据点，设备ID: {deviceId}"
                    );
                    return cachedDataPoints;
                }
                var response = await _httpService.GetAsync<ApiResponse<IEnumerable<DataPoint>>>(
                    $"/api/datapoint/GetDataPointsByDeviceId/{deviceId}"
                );
                if (response.Success && response.Data != null)
                {
                    var dataPoints = response
                        .Data.Select(dp => new DataPointInfo
                        {
                            Id = dp.Id,
                            DeviceId = dp.DeviceId,
                            Code = dp.Code,
                            Name = dp.Name,
                            Unit = dp.Unit,
                            DataType = DataTypeHelper.Parse(dp.ModbusConfig.DataFormat.ToString()),
                            Address = dp.ModbusConfig?.RegisterStart ?? 0,
                            Scale = (double)(dp.ModbusConfig?.DataMultiplier ?? 1.0m),
                            Offset = (double)(dp.ModbusConfig?.Offset ?? 0.0m),
                            IsEnable = dp.EnableAlarm,
                        })
                        .ToList();
                    _cachedDataPoints.TryAdd(deviceId, dataPoints);
                    _logger.LogInformation(
                        $"从API获取到 {dataPoints.Count} 个数据点，设备ID: {deviceId}"
                    );
                    return dataPoints;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"获取设备数据点失败，设备ID: {deviceId}", ex);
            }
            return new List<DataPointInfo>();
        }

        /// <summary>
        /// 获取设备列表
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<List<DeviceInfo>> GetDevicesAsync()
        {
            if (_cachedDevices.Any())
            {
                _logger.LogInformation($"从缓存中获取到 {_cachedDevices.Count} 个设备");
                return _cachedDevices;
            }
            await RefreshDevicesAsync();
            return _cachedDevices;
        }

        /// <summary>
        /// 刷新设备列表
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task RefreshDevicesAsync()
        {
            try
            {
                var response = await _httpService.GetAsync<ApiResponse<List<HumitureDevices>>>(
                    "api/devices"
                );
                if (response.Success && response.Data != null)
                {
                    _cachedDevices = response
                        .Data.Select(d => new DeviceInfo
                        {
                            Id = d.Id,
                            DeviceCode = d.DeviceCode,
                            Name = d.Name,
                            Location = d.Location,
                            PortConfig = d.SerialPortConfig,
                            IsEnabled =
                                d.Status != MonitorLibrary.Models.Enums.DeviceStatus.Offline, // 使用Status判断是否启用
                        })
                        .ToList();
                    _logger.LogInformation($"从API刷新了 {_cachedDevices.Count} 个设备");
                }
                _cachedDataPoints.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError($"刷新设备列表失败", ex);
            }
        }
    }
}
