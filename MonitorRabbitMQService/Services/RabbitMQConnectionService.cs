using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MonitorRabbitMQService.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MonitorRabbitMQService.Services
{
    /// <summary>
    /// RabbitMQ连接管理服务接口
    /// </summary>
    public interface IRabbitMQConnectionService : IDisposable
    {
        /// <summary>
        /// 获取RabbitMQ连接
        /// </summary>
        Task<IConnection> GetConnection();

        /// <summary>
        /// 获取Channel
        /// </summary>
        Task<IChannel> CreateChannel();

        /// <summary>
        /// 连接是否打开
        /// </summary>
        bool IsConnected { get; }
    }

    /// <summary>
    /// RabbitMQ连接管理服务实现
    /// </summary>
    public class RabbitMQConnectionService : IRabbitMQConnectionService
    {
        private readonly RabbitMQConfiguration _config;
        private readonly ILogger<RabbitMQConnectionService> _logger;
        private IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private bool _disposed;
        public bool IsConnected => _connection != null && _connection.IsOpen;

        public RabbitMQConnectionService(
            IOptions<RabbitMQConfiguration> config,
            ILogger<RabbitMQConnectionService> logger
        )
        {
            _config = config.Value ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            InitializeConnectionFactory();
        }

        /// <summary>
        /// 初始化RabbitMQ连接工厂
        /// </summary>
        private void InitializeConnectionFactory()
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = _config.HostName,
                Port = _config.Port,
                UserName = _config.UserName,
                Password = _config.Password,
                VirtualHost = _config.VirtualHost,
                RequestedConnectionTimeout = TimeSpan.FromMilliseconds(
                    _config.RequestedConnectionTimeout
                ),
                RequestedHeartbeat = TimeSpan.FromSeconds(_config.RequestedHeartbeat),
                AutomaticRecoveryEnabled = _config.AutomaticRecoveryEnabled,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(_config.NetworkRecoveryInterval),
                ConsumerDispatchConcurrency = 1,
            };
            _logger.LogInformation(
                "RabbitMQ连接工厂已初始化: {HostName}:{Port}",
                _config.HostName,
                _config.Port
            );
        }

        /// <summary>
        /// 获取RabbitMQ连接
        /// </summary>
        /// <returns></returns>
        public async Task<IConnection> GetConnection()
        {
            if (IsConnected)
            {
                return _connection;
            }

            try
            {
                _connection = await _connectionFactory.CreateConnectionAsync();

                _connection.ConnectionShutdownAsync += OnConnectionShutdown;
                _connection.CallbackExceptionAsync += OnCallbackException;
                _connection.ConnectionBlockedAsync += OnConnectionBlocked;
                _connection.ConnectionUnblockedAsync += OnConnectionUnblocked;
                _logger.LogInformation("RabbitMQ连接已建立: {Endpoint}", _connection.Endpoint);

                return _connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建RabbitMQ连接失败");
                throw;
            }
        }

        /// <summary>
        /// 获取Channel
        /// </summary>
        /// <returns></returns>
        public async Task<IChannel> CreateChannel()
        {
            if (!IsConnected)
            {
                await GetConnection();
            }
            try
            {
                var channel = await _connection.CreateChannelAsync();
                _logger.LogDebug("创建了新的RabbitMQ Channel");
                return channel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建RabbitMQ Channel失败");
                throw;
            }
        }

        /// <summary>
        /// 释放RabbitMQ连接
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                if (_connection != null)
                {
                    _connection.ConnectionShutdownAsync -= OnConnectionShutdown;
                    _connection.CallbackExceptionAsync -= OnCallbackException;
                    _connection.ConnectionBlockedAsync -= OnConnectionBlocked;
                    _connection.ConnectionUnblockedAsync -= OnConnectionUnblocked;

                    _connection.CloseAsync();
                    _connection.Dispose();
                }

                _logger.LogInformation("RabbitMQ连接已释放");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "释放RabbitMQ连接时出错");
            }
            finally
            {
                _disposed = true;
            }
        }

        /// <summary>
        /// 连接关闭事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private Task OnConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogWarning("RabbitMQ连接已关闭: {ReplyText}", e.ReplyText);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 连接回调异常事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private Task OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            _logger.LogError(e.Exception, "RabbitMQ回调异常");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 连接阻塞事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private Task OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            _logger.LogWarning("RabbitMQ连接被阻塞: {Reason}", e.Reason);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 连接解除阻塞事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="event"></param>
        /// <returns></returns>
        private Task OnConnectionUnblocked(object sender, AsyncEventArgs @event)
        {
            _logger.LogWarning("RabbitMQ连接已解除阻塞");
            return Task.CompletedTask;
        }
    }
}
