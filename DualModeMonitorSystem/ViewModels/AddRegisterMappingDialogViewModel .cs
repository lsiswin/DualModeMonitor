using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorLibrary.Models;
using MonitorLibrary.Models.Enums;
using Prism.Commands;
using Prism.Mvvm;

namespace DualModeMonitorSystem.ViewModels
{
    public class AddRegisterMappingDialogViewModel : BindableBase, IDialogAware
    {
        // --- 数据点基本信息 ---
        private DataPoint _currentDataPoint;

        public DataPoint CurrentDataPoint
        {
            get { return _currentDataPoint; }
            set
            {
                _currentDataPoint = value;
                RaisePropertyChanged();
            }
        }

        private bool isEditing;

        public bool IsEditing
        {
            get { return isEditing; }
            set
            {
                isEditing = value;
                RaisePropertyChanged();
            }
        }

        // --- 验证错误信息 ---
        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { SetProperty(ref _errorMessage, value); }
        }

        // --- Commands ---
        private DelegateCommand _confirmCommand;
        public DelegateCommand ConfirmCommand =>
            _confirmCommand ??= new DelegateCommand(OnConfirm, CanConfirm);

        private DelegateCommand _cancelCommand;
        public DelegateCommand CancelCommand =>
            _cancelCommand ?? (_cancelCommand = new DelegateCommand(OnCancel));

        public DialogCloseListener RequestClose { get; }

        public AddRegisterMappingDialogViewModel() { }

        // --- IDialogAware 实现 ---
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            // 对话框关闭后的清理工作
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            // 编辑模式
            if (parameters.ContainsKey("IsEdit") && parameters.GetValue<bool>("IsEdit"))
            {
                IsEditing = parameters.GetValue<bool>("IsEdit");
                if (parameters.ContainsKey("DataPoint"))
                    CurrentDataPoint = parameters.GetValue<DataPoint>("DataPoint");
                CurrentDataPoint.PropertyChanged += OnDataPointPropertyChanged;
            }
            else
            {
                CurrentDataPoint = new DataPoint();
                if (CurrentDataPoint.ModbusConfig == null)
                {
                    CurrentDataPoint.ModbusConfig = new ModbusConfig();
                }
                CurrentDataPoint.PropertyChanged += OnDataPointPropertyChanged;
            }
        }

        private void OnDataPointPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            ConfirmCommand.RaiseCanExecuteChanged();
            ValidateInput();
        }

        // --- 验证方法 ---
        private bool CanConfirm()
        {
            if (CurrentDataPoint == null)
                return false;
            if (CurrentDataPoint.ModbusConfig == null)
                return false;
            bool isValid = ValidateInput();
            return isValid;
        }

        private bool ValidateInput()
        {
            ErrorMessage = string.Empty;

            if (
                CurrentDataPoint.ModbusConfig.DeviceAddress < 1
                || CurrentDataPoint.ModbusConfig.DeviceAddress > 247
            )
            {
                ErrorMessage = "从站地址必须在 1-247 之间";
                return false;
            }

            if (CurrentDataPoint.ModbusConfig.RegisterStart > 65535)
            {
                ErrorMessage = "寄存器地址超出范围 (0-65535)";
                return false;
            }

            if (CurrentDataPoint.ModbusConfig.DataMultiplier == 0)
            {
                ErrorMessage = "系数不能为零";
                return false;
            }

            if (
                !string.IsNullOrWhiteSpace(CurrentDataPoint.Unit)
                && CurrentDataPoint.Unit.Length > 20
            )
            {
                ErrorMessage = "单位字符不能超过20个字符";
                return false;
            }
            if (
                !string.IsNullOrWhiteSpace(CurrentDataPoint.Name)
                && CurrentDataPoint.Name.Length > 20
            )
            {
                ErrorMessage = "名称不能超过20个字符";
                return false;
            }

            return true;
        }

        // --- 方法 ---
        private void OnConfirm()
        {
            if (!ValidateInput())
            {
                return;
            }
            var resultParams = new DialogParameters();
            resultParams.Add("DataPoint", CurrentDataPoint);
            RequestClose.Invoke(new DialogResult(ButtonResult.OK) { Parameters = resultParams });
        }

        private void OnCancel()
        {
            RequestClose.Invoke(new DialogResult(ButtonResult.Cancel));
        }
    }
}
