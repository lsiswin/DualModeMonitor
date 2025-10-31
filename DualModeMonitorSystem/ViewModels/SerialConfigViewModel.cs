using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualModeMonitorSystem.ViewModels
{
    /// <summary>
    /// 串口配置视图模型
    /// </summary>
    public class SerialConfigViewModel : ViewModelBase, INavigationAware
    {
        public ObservableCollection<RegisterMapping> RegisterMappings { get; set; } = new ObservableCollection<RegisterMapping>
{
    new RegisterMapping { DataType = "温度", Address = 0x0000, Format = "FLOAT32", Unit = "°C", Factor = 0.1, Offset = 0 },
    new RegisterMapping { DataType = "湿度", Address = 0x0002, Format = "FLOAT32", Unit = "%", Factor = 0.1, Offset = 0 },
    new RegisterMapping { DataType = "电压", Address = 0x0004, Format = "INT16", Unit = "V", Factor = 0.01, Offset = 0 }
};

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {

        }
    }
}
public class RegisterMapping
{
    public string DataType { get; set; }
    public int Address { get; set; }
    public string Format { get; set; }
    public string Unit { get; set; }
    public double Factor { get; set; }
    public double Offset { get; set; }
}