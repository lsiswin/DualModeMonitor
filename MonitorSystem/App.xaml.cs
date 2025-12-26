using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using DualModeMonitorSystem.Services;
using DualModeMonitorSystem.ViewModels;
using DualModeMonitorSystem.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MonitorLibrary.HttpService;
using MonitorLibrary.Reactive;
using MonitorRabbitMQService.Configuration;
using MonitorRabbitMQService.Services;
using MonitorSystem.Configuration;
using MonitorSystem.Services;
using Prism.Ioc;

namespace DualModeMonitorSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        protected override Window CreateShell()
        {
            var window = Container.Resolve<MainView>();
            return window;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            // 从配置中获取基础地址和令牌
            var baseAddress = "https://localhost:7137/";

            var httpService = Container.Resolve<IHttpService>();
            httpService.SetBaseAddress(baseAddress);
        }

        protected override void RegisterTypes(IContainerRegistry container)
        {
            // 1. 获取 IConfiguration 实例（通常通过读取 appsettings.json）
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            container.RegisterSingleton<ReactiveLogger>();
            // 2. 将配置段转换为对象并注册为 IOptions<T>
            // 以 QueueConfiguration 为例
            var queueSection = config.GetSection("Queue").Get<QueueConfiguration>();
            container.RegisterInstance<IOptions<QueueConfiguration>>(Options.Create(queueSection));
            var rabbitMQSection = config.GetSection("RabbitMQ").Get<RabbitMQConfiguration>();
            container.RegisterInstance<IOptions<RabbitMQConfiguration>>(
                Options.Create(rabbitMQSection)
            );
            var exchange = config.GetSection("Exchange").Get<ExchangeConfiguration>();
            container.RegisterInstance<IOptions<ExchangeConfiguration>>(Options.Create(exchange));
            var routingKey = config.GetSection("RoutingKey").Get<RoutingKeyConfiguration>();
            container.RegisterInstance<IOptions<RoutingKeyConfiguration>>(
                Options.Create(routingKey)
            );
            container.RegisterForNavigation<DashboardView>("DashboardView");
            container.RegisterForNavigation<SettingsView>("SettingsView");
            container.RegisterForNavigation<DeviceConfigView>("DeviceConfigView");
            container.RegisterForNavigation<HistoryDataView>("HistoryDataView");
            container.RegisterForNavigation<RealTimeMonitorView>("RealTimeMonitorView");

            // 注册自定义对话框窗口
            container.Register<IDialogWindow, CustomDialogWindow>();
            container.RegisterDialog<AddRegisterMappingDialog, AddRegisterMappingDialogViewModel>(
                "AddRegisterMappingDialog"
            );
            container.RegisterDialog<MessageDialog, MessageDialogViewModel>("MessageDialog");
            container.RegisterDialog<AddDeviceDialog, AddDeviceDialogViewModel>("AddDeviceDialog");
            container.RegisterDialog<ConfirmDialog, ConfirmDialogViewModel>("ConfirmDialog");
            container.RegisterSingleton<IDeviceDataService, OpcDataService>();
            container.RegisterScoped<IRabbitMQConnectionService, RabbitMQConnectionService>();
            // 注册消息消费者为单例
            container.RegisterSingleton<IMessageConsumer, MessageConsumer>();
            // 注册HttpService，通常注册为单例，因为HttpClient最好复用
            container.RegisterSingleton<IHttpService, HttpService>();
            container.Register<IDeviceService, DeviceService>();
        }
    }
}
