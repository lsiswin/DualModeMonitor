using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MonitorLibrary.Reactive;
using Opc.Ua;
using Opc.Ua.Configuration;
using Opc.Ua.Server;
using OpcUaMonitorServer.Configuration;
using OpcUaMonitorServer.Model;

namespace OpcUaMonitorServer.Services
{
    /// <summary>
    /// OPC UA Server服务接口
    /// </summary>
    public interface IOpcUaServerService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync();
        void CreateDeviceNodes(DeviceInfo device, List<DataPointInfo> dataPoints);
        void UpdateDataPointValue(int deviceId, int dataPointId, double value, DateTime timestamp);
        void RemoveDeviceNode(int deviceId);
        bool IsRunning { get; }
    }

    /// <summary>
    /// OPC UA Server服务实现
    /// </summary>
    public class OpcUaServerService : IOpcUaServerService
    {
        private readonly ReactiveLogger _logger;
        private readonly OpcServerConfiguration _config;
        private MonitorServer? _server;
        private DeviceNodeManager? _nodeManager;
        private ApplicationInstance? _application;
        private bool _isRunning;

        public bool IsRunning => _isRunning;

        public OpcUaServerService(IOptions<OpcServerConfiguration> config, ReactiveLogger logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_isRunning)
                return;

            try
            {
                _logger.LogInformation("正在启动OPC UA Server...");

                // 创建应用程序实例
                _application = new ApplicationInstance
                {
                    ApplicationName = _config.ApplicationName,
                    ApplicationType = ApplicationType.Server,
                    ConfigSectionName = "MonitorOpcUaServer",
                };

                // 加载应用程序配置
                var appConfig = await LoadApplicationConfiguration();
                _application.ApplicationConfiguration = appConfig;
                // 检查应用程序实例证书
                bool certOk = await _application.CheckApplicationInstanceCertificatesAsync(
                    silent: false,
                    1024
                );

                if (!certOk)
                {
                    _logger.LogError("应用程序证书检查失败");
                    throw new InvalidOperationException("无法获取有效的应用程序证书");
                }
                // 创建服务器实例
                _server = new MonitorServer(_logger);
                _nodeManager = _server.GetNodeManager();
                // 启动服务器
                await _application.StartAsync(_server);

                _isRunning = true;
                _logger.LogInformation($"OPC UA Server已启动，端口: {_config.Port}");
            }
            catch (Exception ex)
            {
                _logger.LogError("启动OPC UA Server失败", ex);
                throw;
            }
        }

        public async Task StopAsync()
        {
            if (!_isRunning)
                return;

            try
            {
                _logger.LogInformation("正在停止OPC UA Server...");

                _server?.Stop();

                _isRunning = false;
                _logger.LogInformation("OPC UA Server已停止");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError("停止OPC UA Server失败", ex);
            }
        }

        public void CreateDeviceNodes(DeviceInfo device, List<DataPointInfo> dataPoints)
        {
            if (_nodeManager == null)
            {
                _logger.LogWarning("NodeManager未初始化，无法创建设备节点");
                return;
            }

            try
            {
                _nodeManager.CreateDeviceNode(device, dataPoints);
                _logger.LogInformation(
                    $"已创建设备节点: {device.Name}, 数据点数: {dataPoints.Count}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"创建设备节点失败: {device.Name}", ex);
            }
        }

        public void UpdateDataPointValue(
            int deviceId,
            int dataPointId,
            double value,
            DateTime timestamp
        )
        {
            if (_nodeManager == null)
                return;

            try
            {
                _nodeManager.UpdateDataPointValue(deviceId, dataPointId, value, timestamp);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"更新数据点值失败: DeviceId={deviceId}, DataPointId={dataPointId}",
                    ex
                );
            }
        }

        public void RemoveDeviceNode(int deviceId)
        {
            if (_nodeManager == null)
                return;

            try
            {
                _nodeManager.RemoveDeviceNode(deviceId);
                _logger.LogInformation($"已移除设备节点: DeviceId={deviceId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"移除设备节点失败: DeviceId={deviceId}", ex);
            }
        }

        private async Task<ApplicationConfiguration> LoadApplicationConfiguration()
        {
            var config = new ApplicationConfiguration
            {
                ApplicationName = _config.ApplicationName,
                ApplicationUri = _config.ApplicationUri,
                ProductUri = _config.ProductUri,
                ApplicationType = ApplicationType.Server,

                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier
                    {
                        StoreType = "Directory",
                        StorePath = "OPC Foundation/CertificateStores/MachineDefault",
                        SubjectName = _config.ApplicationName,
                    },
                    TrustedIssuerCertificates = new CertificateTrustList
                    {
                        StoreType = "Directory",
                        StorePath = "OPC Foundation/CertificateStores/UA Certificate Authorities",
                    },
                    TrustedPeerCertificates = new CertificateTrustList
                    {
                        StoreType = "Directory",
                        StorePath = "OPC Foundation/CertificateStores/UA Applications",
                    },
                    RejectedCertificateStore = new CertificateTrustList
                    {
                        StoreType = "Directory",
                        StorePath = "OPC Foundation/CertificateStores/RejectedCertificates",
                    },
                    AutoAcceptUntrustedCertificates = true,
                    AddAppCertToTrustedStore = true,
                },

                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },

                ServerConfiguration = new ServerConfiguration
                {
                    BaseAddresses = new StringCollection
                    {
                        $"opc.tcp://localhost:{_config.Port}/MonitorOpcServer",
                    },
                    MinRequestThreadCount = 5,
                    MaxRequestThreadCount = 100,
                    MaxQueuedRequestCount = 200,
                    MaxSessionCount = 100,
                    MinSessionTimeout = _config.MinSessionTimeout,
                    MaxSessionTimeout = _config.MaxSessionTimeout,
                    MaxBrowseContinuationPoints = 10,
                    MaxQueryContinuationPoints = 10,
                    MaxHistoryContinuationPoints = 100,
                    MaxRequestAge = 600000,
                    MinPublishingInterval = 100,
                    MaxPublishingInterval = 3600000,
                    PublishingResolution = 50,
                    MaxSubscriptionLifetime = 3600000,
                    MaxMessageQueueSize = 100,
                    MaxNotificationQueueSize = 100,
                    MaxNotificationsPerPublish = 1000,
                    MaxPublishRequestCount = 20,
                    MaxSubscriptionCount = 100,
                    MaxEventQueueSize = 10000,
                },

                TraceConfiguration = new TraceConfiguration
                {
                    OutputFilePath = "Logs/MonitorOpcServer.log",
                    DeleteOnLoad = true,
                    TraceMasks = 0, // 0 = no tracing, use for production
                },
            };

            await config.ValidateAsync(ApplicationType.Server);

            // 确保证书在可信存储中
            config.SecurityConfiguration.AddAppCertToTrustedStore = true;
            return config;
        }
    }

    /// <summary>
    /// 自定义OPC UA Server
    /// </summary>
    public class MonitorServer : StandardServer
    {
        private readonly ReactiveLogger _logger;
        private DeviceNodeManager? _nodeManager;

        public MonitorServer(ReactiveLogger logger)
        {
            _logger = logger;
        }

        protected override MasterNodeManager CreateMasterNodeManager(
            IServerInternal server,
            ApplicationConfiguration configuration
        )
        {
            // 在这里创建节点管理器，此时 server 已经可用
            _nodeManager = new DeviceNodeManager(server, configuration, _logger);

            var masterNodeManager = new MasterNodeManager(
                server,
                configuration,
                null,
                _nodeManager
            );
            return masterNodeManager;
        }

        public DeviceNodeManager? GetNodeManager()
        {
            return _nodeManager;
        }
    }
}
