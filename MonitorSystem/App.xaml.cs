using System;
using System.Configuration;
using System.Data;
using System.Windows;
using DualModeMonitorSystem.Services;
using DualModeMonitorSystem.ViewModels;
using DualModeMonitorSystem.Views;
using Microsoft.Extensions.Configuration;
using MonitorLibrary.HttpService;
using MonitorRabbitMQService.Services;
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

            container.RegisterScoped<IRabbitMQConnectionService, RabbitMQConnectionService>();
            // 注册HttpService，通常注册为单例，因为HttpClient最好复用
            container.RegisterSingleton<IHttpService, HttpService>();
            container.Register<IDeviceService, DeviceService>();
        }
    }
}
