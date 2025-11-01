using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualModeMonitorSystem.Models
{
    /// <summary>
    /// 统计结果模型
    /// </summary>
    public class StatisticsItem : ModelBase
    {
        #region 私有字段

        private string _title;
        private string _value;
        private string _icon;
        private string _unit;
        private double _numericValue;
        private string _trendText;

        #endregion

        #region 公共属性

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        public double NumericValue
        {
            get => _numericValue;
            set
            {
                if (SetProperty(ref _numericValue, value))
                {
                    RaisePropertyChanged(nameof(FormattedValue));
                }
            }
        }

        public string TrendText
        {
            get => _trendText;
            set => SetProperty(ref _trendText, value);
        }

       

        #endregion

        #region 计算属性

        public string FormattedValue => string.IsNullOrEmpty(Unit)
            ? $"{NumericValue:F0}"
            : $"{NumericValue:F0} {Unit}";

        #endregion

        #region 构造函数

        public StatisticsItem()
        {
            Title = "统计项";
            Value = "0";
            Icon = "ChartLine";
            Unit = "";
            NumericValue = 0;
            TrendText = "";
        }

        public StatisticsItem(string title, string value, string icon)
            : this()
        {
            Title = title;
            Value = value;
            Icon = icon;
        }

        #endregion

        #region 公共方法

        public void SetValue(double value, string unit = "")
        {
            NumericValue = value;
            Value = FormattedValue;
            Unit = unit;
        }

        public override string ToString() => $"{Title}: {Value}";

        #endregion
    }
}
