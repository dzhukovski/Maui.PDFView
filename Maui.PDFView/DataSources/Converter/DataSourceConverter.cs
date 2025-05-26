using System.ComponentModel;
using System.Globalization;

namespace Maui.PDFView.Helpers.DataSource;

public sealed class DataSourceConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string);

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        => destinationType == typeof(string);

    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        var strValue = value?.ToString();
        if (strValue != null)
            return Uri.TryCreate(strValue, UriKind.Absolute, out Uri? uri) && uri.Scheme != "file"
                ? DataSource.FromUri(uri)
                : DataSource.FromFile(strValue);

        throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(DataSource)}");
    }

    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (value is FileDataSource fis)
        {
            return fis.File;
        }
        if (value is UriDataSource uis)
            return uis.Uri.ToString();
        throw new NotSupportedException();
    }
}