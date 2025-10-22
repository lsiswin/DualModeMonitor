using System.Configuration;
using System.Data;
using System.Windows;

namespace DualModeMonitorSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return MainWindow;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            
        }
    }

}
