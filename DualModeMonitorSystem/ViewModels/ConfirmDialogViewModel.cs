using System.Windows.Media;
using Prism.Commands;
using Prism.Mvvm;

// 假设您使用 Prism 框架

public class ConfirmDialogViewModel : BindableBase, IDialogAware
{
    // --- 属性 ---
    private string _title = "操作确认";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _message;
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private string _confirmText = "确定";
    public string ConfirmText
    {
        get => _confirmText;
        set => SetProperty(ref _confirmText, value);
    }

    private string _cancelText = "取消";
    public string CancelText
    {
        get => _cancelText;
        set => SetProperty(ref _cancelText, value);
    }

    // 默认使用红色警告（例如：删除操作）
    private Brush _confirmButtonColor = new SolidColorBrush(Color.FromRgb(0xFF, 0x4D, 0x4F));
    public Brush ConfirmButtonColor
    {
        get => _confirmButtonColor;
        set => SetProperty(ref _confirmButtonColor, value);
    }

    private bool _showWarningIcon = true;
    public bool ShowWarningIcon
    {
        get => _showWarningIcon;
        set => SetProperty(ref _showWarningIcon, value);
    }

    // --- 命令 ---
    public DelegateCommand ConfirmCommand { get; }
    public DelegateCommand CancelCommand { get; }

    public DialogCloseListener RequestClose { get; }

    public ConfirmDialogViewModel()
    {
        ConfirmCommand = new DelegateCommand(
            () => RequestClose.Invoke(new DialogResult(ButtonResult.OK))
        );
        CancelCommand = new DelegateCommand(
            () => RequestClose.Invoke(new DialogResult(ButtonResult.Cancel))
        );
    }

    public bool CanCloseDialog()
    {
        return true;
    }

    public void OnDialogClosed() { }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        Message = parameters.GetValue<string>("Message");
        // 可选：设置自定义文本和颜色
        if (parameters.ContainsKey("ConfirmText"))
            ConfirmText = parameters.GetValue<string>("ConfirmText");
        if (parameters.ContainsKey("ConfirmColor"))
            ConfirmButtonColor = parameters.GetValue<Brush>("ConfirmColor");
    }
}
