using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualModeMonitorSystem.ViewModels
{
    public class MessageDialogViewModel : BindableBase,IDialogAware
    {
        private string _Message = "MessageDialogViewModel";

        public string Message
        {
            get { return _Message; }
            set { _Message = value; RaisePropertyChanged(); }
        }


        public DialogCloseListener RequestClose { get; }

        public DelegateCommand CloseDialogCommand { get; }

        public MessageDialogViewModel()
        {
            CloseDialogCommand = new DelegateCommand(() => RequestClose.Invoke(new DialogResult(ButtonResult.OK)));
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("message"))
            {
                Message = parameters.GetValue<string>("message");
            }
        }
    }
}
