using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DualModeMonitorSystem.Models
{
    /// <summary>
    /// 模型基类 - 继承自 Prism.Mvvm.BindableBase，提供属性变更通知和验证功能
    /// </summary>
    public abstract class ModelBase : BindableBase, IDataErrorInfo
    {
        #region 属性设置增强

        /// <summary>
        /// 设置属性值并执行回调
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, Action onChanged, [CallerMemberName] string propertyName = null)
        {
            if (SetProperty(ref field, value, propertyName))
            {
                onChanged?.Invoke();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 设置属性值，验证后标记为脏
        /// </summary>
        protected bool SetPropertyWithValidation<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (SetProperty(ref field, value, propertyName))
            {
                ValidateProperty(propertyName);
                return true;
            }
            return false;
        }

        #endregion

        #region IDataErrorInfo 实现 - 数据验证

        /// <summary>
        /// 验证错误字典
        /// </summary>
        private readonly Dictionary<string, string> _errors = new Dictionary<string, string>();

        /// <summary>
        /// 获取整个对象的错误信息
        /// </summary>
        public string Error
        {
            get
            {
                return _errors.Count > 0 ? string.Join(Environment.NewLine, _errors.Values) : null;
            }
        }

        /// <summary>
        /// 获取指定属性的错误信息
        /// </summary>
        public string this[string propertyName]
        {
            get
            {
                _errors.TryGetValue(propertyName, out string error);
                return error;
            }
        }

        /// <summary>
        /// 添加验证错误
        /// </summary>
        protected void AddError(string propertyName, string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                RemoveError(propertyName);
                return;
            }

            if (_errors.ContainsKey(propertyName))
            {
                if (_errors[propertyName] != errorMessage)
                {
                    _errors[propertyName] = errorMessage;
                    RaisePropertyChanged(propertyName);
                }
            }
            else
            {
                _errors[propertyName] = errorMessage;
                RaisePropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// 移除验证错误
        /// </summary>
        protected void RemoveError(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
                RaisePropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// 清除所有错误
        /// </summary>
        protected void ClearErrors()
        {
            _errors.Clear();
            RaisePropertyChanged(nameof(HasErrors));
        }

        /// <summary>
        /// 验证指定属性（子类可重写）
        /// </summary>
        protected virtual string ValidateProperty(string propertyName)
        {
            return null;
        }

        /// <summary>
        /// 验证所有属性
        /// </summary>
        public virtual bool Validate()
        {
            ClearErrors();

            var properties = GetType().GetProperties();
            foreach (var property in properties)
            {
                var error = ValidateProperty(property.Name);
                if (!string.IsNullOrEmpty(error))
                {
                    AddError(property.Name, error);
                }
            }

            return _errors.Count == 0;
        }

        /// <summary>
        /// 是否有验证错误
        /// </summary>
        public bool HasErrors => _errors.Count > 0;

        /// <summary>
        /// 获取所有错误信息
        /// </summary>
        public IReadOnlyDictionary<string, string> Errors => _errors;

        #endregion

       
    }
}
