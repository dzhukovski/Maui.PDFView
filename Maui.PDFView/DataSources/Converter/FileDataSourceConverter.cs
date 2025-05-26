using System.ComponentModel;
using System.Globalization;

namespace Maui.PDFView.Helpers.DataSource;

public sealed class FileDataSourceConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string);

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        => true;

    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        var strValue = value?.ToString();
        if (strValue != null)
        {
            return DataSource.FromFile(strValue);
        }

        throw new InvalidOperationException(
            $"Cannot convert \"{strValue}\" into {typeof(FileImageSource)}"
        );
    }

    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (value is not FileDataSource fis)
            throw new NotSupportedException();
        return fis.File;
    }
}
