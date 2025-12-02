using System;
using System.Globalization;
using System.Windows.Data;

namespace DualModeMonitorSystem.Convert
{
    /// <summary>
    /// 将一个布尔值转换为两个可选字符串中的一个。
    /// 配合 MultiBinding 使用，根据第一个参数（bool）的值，返回第二个参数（TrueText）或第三个参数（FalseText）。
    /// </summary>
    public class BooleanToTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // 至少需要 3 个值: [0] IsEditing (bool), [1] TrueText (string), [2] FalseText (string)
            if (values == null || values.Length < 3)
            {
                return "配置标题错误：参数不足";
            }

            // 1. 获取布尔值 (IsEditing)
            if (values[0] is bool isTrue)
            {
                // 2. 获取 TrueText (编辑数据点配置)
                string trueText = values[1] as string;

                // 3. 获取 FalseText (新增数据点配置)
                string falseText = values[2] as string;

                // 4. 根据布尔值返回对应的文本
                if (isTrue)
                {
                    // isTrue == true，返回 TrueText (编辑)
                    return trueText ?? string.Empty;
                }
                else
                {
                    // isTrue == false，返回 FalseText (新增)
                    return falseText ?? string.Empty;
                }
            }

            // 如果第一个值不是布尔类型
            return "配置标题错误：布尔值无效";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            // 单向转换，无需实现
            throw new NotSupportedException("BooleanToTextConverter 仅支持单向转换。");
        }
    }
}