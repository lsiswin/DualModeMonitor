using System.Configuration;
using System.Data;
using System.Windows;
using DualModeMonitorSystem.Views;

namespace DualModeMonitorSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {

            var window = Container.Resolve<MainView>();
            return window;
        }

        protected override void RegisterTypes(IContainerRegistry container)
        {
            container.RegisterForNavigation<DashboardView>("DashboardView");
            container.RegisterForNavigation<SettingsView>("SettingsView");
            container.RegisterForNavigation<SerialConfigView>("SerialConfigView");
            container.RegisterForNavigation<HistoryDataView>("HistoryDataView");
            container.RegisterForNavigation<RealTimeMonitorView>("RealTimeMonitorView");
        }
    }

}
