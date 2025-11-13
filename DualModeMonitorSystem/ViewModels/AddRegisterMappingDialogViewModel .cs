using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorLibrary.Models.Enums;

namespace DualModeMonitorSystem.ViewModels
{
    public class AddRegisterMappingDialogViewModel : BindableBase, IDialogAware
    {
        private string _title = "添加寄存器映射";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }


        // --- 输入属性 ---
        private string _dataType;
        public string DataType
        {
            get { return _dataType; }
            set { SetProperty(ref _dataType, value); }
        }

        private ushort _address;
        public ushort Address
        {
            get { return _address; }
            set { SetProperty(ref _address, value); }
        }

        private ModbusDataFormat _format;
        public ModbusDataFormat Format
        {
            get { return _format; }
            set { SetProperty(ref _format, value); }
        }

        private string _unit;
        public string Unit
        {
            get { return _unit; }
            set { SetProperty(ref _unit, value); }
        }

        private decimal _factor = 1.0m; // 默认值
        public decimal Factor
        {
            get { return _factor; }
            set { SetProperty(ref _factor, value); }
        }

        private decimal _offset = 0.0m; // 默认值
        public decimal Offset
        {
            get { return _offset; }
            set { SetProperty(ref _offset, value); }
        }


        // --- Commands ---
        private DelegateCommand _confirmCommand;
        public DelegateCommand ConfirmCommand =>
            _confirmCommand ?? (_confirmCommand = new DelegateCommand(OnConfirm));

        private DelegateCommand _cancelCommand;
        public DelegateCommand CancelCommand =>
            _cancelCommand ?? (_cancelCommand = new DelegateCommand(OnCancel));

        public DialogCloseListener RequestClose { get; }

        // --- IDialogAware 实现 ---
        public bool CanCloseDialog()
        {
            return true; // 可以根据业务逻辑决定是否允许关闭
        }

        public void OnDialogClosed()
        {
            // 对话框关闭后的清理工作
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            // 可以在这里接收从打开对话框时传递过来的参数
            // 例如: var deviceId = parameters.GetValue<int>("deviceId");
        }


        // --- 方法 ---
        private void OnConfirm()
        {
            if (string.IsNullOrWhiteSpace(DataType))
            {
                // 这里可以加入更详细的验证逻辑和提示
                // 例如使用 Prism 的 IMessageBoxService 或者自定义提示
                // MessageBox.Show("数据类型不能为空");
                // return;
            }

            var resultParams = new DialogParameters();
            resultParams.Add("DataType", DataType);
            resultParams.Add("Address", Address);
            resultParams.Add("Format", Format);
            resultParams.Add("Unit", Unit);
            resultParams.Add("Factor", Factor);
            resultParams.Add("Offset", Offset);
            RequestClose.Invoke(new DialogResult(ButtonResult.OK)
            {
                Parameters = resultParams
            });
        }

        private void OnCancel()
        {
            RequestClose.Invoke(new DialogResult(ButtonResult.Cancel));
        }
    }
}
