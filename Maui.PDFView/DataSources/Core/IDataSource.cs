namespace Maui.PDFView.Helpers.DataSource;

[System.ComponentModel.TypeConverter(typeof(DataSourceConverter))]
public interface IDataSource
{
    bool IsEmpty { get; }
    
    Task<Stream?> StreamAsync(CancellationToken cancellationToken = default);
}