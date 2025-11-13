using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace DualModeMonitorSystem
{
    public class EnumBindingSourceExtension : MarkupExtension
    {
        private Type _enumType;

        public Type EnumType
        {
            get => _enumType;
            set
            {
                if (value != _enumType)
                {
                    if (value != null)
                    {
                        var underlyingType = Nullable.GetUnderlyingType(value) ?? value;
                        if (!underlyingType.IsEnum)
                            throw new ArgumentException("Type must be an Enum");
                    }

                    _enumType = value;
                }
            }
        }

        public EnumBindingSourceExtension() { }

        public EnumBindingSourceExtension(Type enumType)
        {
            EnumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_enumType == null)
                throw new InvalidOperationException("EnumType must be specified");

            var actualEnumType = Nullable.GetUnderlyingType(_enumType) ?? _enumType;
            return Enum.GetValues(actualEnumType);
        }
    }
}
