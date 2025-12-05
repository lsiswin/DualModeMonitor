using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MonitorLibrary.Models;
using MonitorLibrary.Reactive;

namespace OpcUaMonitorServer.Services
{
    public interface IModbusServiceFactory
    {
        // 创建并连接 Modbus 服务
        Task<(IModbusService? service, bool isConnected)> CreateAndConnectAsync(
            SerialPortConfig config
        );
        // 可选：提供一个方法来释放/清理工厂创建的资源，但这通常由 DI 容器和 ModbusService 自己的 Dispose 处理
        // void Release(IModbusService service);
    }

    // 工厂的具体实现
    public class ModbusServiceFactory : IModbusServiceFactory
    {
        private readonly IServiceProvider _serviceProvider; // 用于从 DI 容器解析服务

        public ModbusServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<(IModbusService? service, bool isConnected)> CreateAndConnectAsync(
            SerialPortConfig config
        )
        {
            using var scope = _serviceProvider.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ReactiveLogger>();
            // 手动创建实例并注入 logger
            // 注意：这里直接 new 具体类，因为我们希望每个连接都有独立的实例
            var serialPortInstance = new SerialPortService(logger); // 传入 logger
            var modbusInstance = new ModbusService(serialPortInstance, logger); // 传入 serialPort 和 logger

            var connected = await modbusInstance.ConnectAsync(config);

            if (connected)
            {
                // 返回具体实例，并标记为已连接
                // 注意：这个实例的生命周期现在由调用者（DataCollectionService）管理
                return (modbusInstance, true);
            }
            else
            {
                // 连接失败，清理资源
                modbusInstance.Dispose(); // 确保释放资源
                return (null, false);
            }
        }
    }
}
