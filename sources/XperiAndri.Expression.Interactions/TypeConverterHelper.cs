using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace Microsoft.Expression.Interactivity
{
    internal static class TypeConverterHelper
{
    // Methods
    internal static object DoConversionFrom(TypeConverter converter, object value)
    {
        object obj2 = value;
        try
        {
            if (((converter != null) && (value != null)) && converter.CanConvertFrom(value.GetType()))
            {
                obj2 = converter.ConvertFrom(value);
            }
        }
        catch (Exception exception)
        {
            if (!ShouldEatException(exception))
            {
                throw;
            }
        }
        return obj2;
    }

    internal static TypeConverter GetTypeConverter(Type type)
    {
        TypeConverterAttribute attribute = (TypeConverterAttribute) Attribute.GetCustomAttribute(type, typeof(TypeConverterAttribute), false);
        if (attribute != null)
        {
            try
            {
                Type type2 = Type.GetType(attribute.ConverterTypeName, false);
                if (type2 != null)
                {
                    return (Activator.CreateInstance(type2) as TypeConverter);
                }
            }
            catch
            {
            }
        }
        return new ExtendedStringConverter(type);
    }

    private static bool ShouldEatException(Exception e)
    {
        bool flag = false;
        if (e.InnerException != null)
        {
            flag |= ShouldEatException(e.InnerException);
        }
        return (flag | (e is FormatException));
    }

    // Nested Types
    private class ExtendedStringConverter : TypeConverter
    {
        // Fields
        private Type type;

        // Methods
        public ExtendedStringConverter(Type type)
        {
            this.type = type;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if ((sourceType != typeof(string)) && (sourceType != typeof(uint)))
            {
                return base.CanConvertFrom(context, sourceType);
            }
            return true;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((typeof(IConvertible).GetTypeInfo().IsAssignableFrom(this.type) && typeof(IConvertible).IsAssignableFrom(destinationType)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string a = value as string;
            if (a != null)
            {
                Type nullableType = this.type;
                Type underlyingType = Nullable.GetUnderlyingType(nullableType);
                if (underlyingType != null)
                {
                    if (string.Equals(a, "null", StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                    nullableType = underlyingType;
                }
                else if (nullableType.GetTypeInfo().IsGenericType)
                {
                    return base.ConvertFrom(context, culture, value);
                }
                object obj2 = new object();
                object content = obj2;
                if (nullableType == typeof(bool))
                {
                    content = bool.Parse(a);
                }
                else if (nullableType.GetTypeInfo().IsEnum)
                {
                    content = Enum.Parse(this.type, a, false);
                }
                else
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendFormat("<ContentControl xmlns='http://schemas.microsoft.com/client/2007' xmlns:c='{0}'>\n", string.Concat("clr-namespace:", nullableType.Namespace, ";assembly=", nullableType.GetTypeInfo().Assembly.FullName.Split(new char[] { ',' })[0]));
                    builder.AppendFormat("<c:{0}>\n", nullableType.Name);
                    builder.Append(a);
                    builder.AppendFormat("</c:{0}>\n", nullableType.Name);
                    builder.Append("</ContentControl>");
                    ContentControl control = XamlReader.Load(builder.ToString()) as ContentControl;
                    if (control != null)
                    {
                        content = control.Content;
                    }
                }
                if (content != obj2)
                {
                    return content;
                }
            }
            else if (value is uint)
            {
                if (this.type == typeof(bool))
                {
                    return (((uint) value) != 0);
                }
                if (this.type.GetTypeInfo().IsEnum)
                {
                    return Enum.Parse(this.type, Enum.GetName(this.type, value), false);
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            IConvertible convertible = value as IConvertible;
            if ((convertible != null) && typeof(IConvertible).IsAssignableFrom(destinationType))
            {
                return convertible.ToType(destinationType, CultureInfo.InvariantCulture);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

}
