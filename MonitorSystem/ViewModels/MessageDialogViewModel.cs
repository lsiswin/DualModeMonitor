using System;
using Prism.Commands;
using Prism.Mvvm;

namespace DualModeMonitorSystem.ViewModels
{
    public class MessageDialogViewModel : BindableBase, IDialogAware
    {
        private string _Title = "通知"; // 新增 Title 属性
        public string Title
        {
            get => _Title;
            set => SetProperty(ref _Title, value);
        }

        private string _Message = "消息内容";
        public string Message
        {
            get => _Message;
            set => SetProperty(ref _Message, value);
        }

        public DelegateCommand CloseDialogCommand { get; }

        public DialogCloseListener RequestClose { get; }

        public MessageDialogViewModel()
        {
            CloseDialogCommand = new DelegateCommand(
                () => RequestClose.Invoke(new DialogResult(ButtonResult.OK))
            );
        }

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { /* 清理逻辑 */
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("title"))
            {
                Title = parameters.GetValue<string>("title");
            }
            if (parameters.ContainsKey("message"))
            {
                Message = parameters.GetValue<string>("message");
            }
        }
    }
}
