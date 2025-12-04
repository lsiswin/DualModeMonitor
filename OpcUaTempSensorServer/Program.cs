using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MonitorLibrary.HttpService;
using MonitorRabbitMQService.Configuration;
using MonitorRabbitMQService.Services;
using OpcUaTempSensorServer.Configuration;
using OpcUaTempSensorServer.Services;

var builder = Host.CreateApplicationBuilder(args);

// 配置绑定
builder.Services.Configure<OpcServerConfiguration>(builder.Configuration.GetSection("OpcServer"));
builder.Services.Configure<MonitorApiConfiguration>(builder.Configuration.GetSection("MonitorApi"));
builder.Services.Configure<RabbitMQConfiguration>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<ExchangeConfiguration>(builder.Configuration.GetSection("Exchange"));
builder.Services.Configure<QueueConfiguration>(builder.Configuration.GetSection("Queue"));
builder.Services.Configure<RoutingKeyConfiguration>(builder.Configuration.GetSection("RoutingKey"));
builder.Services.Configure<DataCollectionConfiguration>(
    builder.Configuration.GetSection("DataCollection")
);

// 注册HTTP服务
builder.Services.AddSingleton<IHttpService, HttpService>();

// 注册RabbitMQ服务
builder.Services.AddSingleton<IRabbitMQConnectionService, RabbitMQConnectionService>();
builder.Services.AddSingleton<IMessagePublisher, MessagePublisher>();
builder.Services.AddSingleton<MessageConsumer>();

// 注册应用服务
builder.Services.AddSingleton<IDeviceManagementService, DeviceManagementService>();
builder.Services.AddSingleton<ISensorDataPublisher, SensorDataPublisher>();
builder.Services.AddSingleton<IOpcUaServerService, OpcUaServerService>();
builder.Services.AddSingleton<IDataCollectionService, DataCollectionService>();

var host = builder.Build();
host.Run();
