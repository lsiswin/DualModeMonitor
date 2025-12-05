using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MonitorLibrary.Reactive;

namespace OpcUaMonitorServer.Services
{
    /// <summary>
    /// 应用程序托管服务 - 负责启动和停止OPC UA服务器和数据采集服务
    /// </summary>
    public class ApplicationHostedService : BackgroundService
    {
        private readonly ReactiveLogger _logger;
        private readonly IOpcUaServerService _opcUaServerService;
        private readonly IDataCollectionService _dataCollectionService;

        public ApplicationHostedService(
            ReactiveLogger logger,
            IOpcUaServerService opcUaServerService,
            IDataCollectionService dataCollectionService
        )
        {
            _logger = logger;
            _opcUaServerService = opcUaServerService;
            _dataCollectionService = dataCollectionService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("应用程序服务启动中...");

            try
            {
                // 启动OPC UA服务器
                await _opcUaServerService.StartAsync(stoppingToken);

                // 启动数据采集服务
                await _dataCollectionService.StartAsync(stoppingToken);

                _logger.LogInformation("所有服务已启动");

                // 保持运行直到取消
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("应用程序正在关闭...");
            }
            catch (Exception ex)
            {
                _logger.LogError("应用程序服务运行时出错", ex);
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("停止应用程序服务...");

            try
            {
                // 停止数据采集服务
                await _dataCollectionService.StopAsync();

                // 停止OPC UA服务器
                await _opcUaServerService.StopAsync();

                _logger.LogInformation("所有服务已停止");
            }
            catch (Exception ex)
            {
                _logger.LogError("停止服务时出错", ex);
            }

            await base.StopAsync(cancellationToken);
        }
    }
}
