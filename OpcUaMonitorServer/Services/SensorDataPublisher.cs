using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MonitorLibrary.Reactive;
using MonitorRabbitMQService.Models;
using MonitorRabbitMQService.Services;
using Opc.Ua;

namespace OpcUaMonitorServer.Services
{
    /// <summary>
    /// 传感器数据发布服务
    /// </summary>
    public interface ISensorDataPublisher
    {
        Task PublishAsync(OpcDataMessage data);
        Task PublishBatchAsync(IEnumerable<OpcDataMessage> dataList);
    }

    public class SensorDataPublisher : ISensorDataPublisher
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly ReactiveLogger _logger;

        public SensorDataPublisher(IMessagePublisher messagePublisher, ReactiveLogger logger)
        {
            _messagePublisher = messagePublisher;
            _logger = logger;
        }

        public async Task PublishAsync(OpcDataMessage data)
        {
            try
            {
                await _messagePublisher.PublishOpcDataAsync<OpcDataMessage>(data);
                _logger.LogDebug($"发布传感器数据: {data.Name}: {data.DataType} = {data.Value}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"发布传感器数据失败: {data.Name}", ex);
            }
        }

        public async Task PublishBatchAsync(IEnumerable<OpcDataMessage> dataList)
        {
            try
            {
                foreach (var data in dataList)
                {
                    await PublishAsync(data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"批量发布传感器数据失败", ex);
            }
        }
    }
}
