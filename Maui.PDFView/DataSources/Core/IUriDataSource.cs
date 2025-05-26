namespace Maui.PDFView.Helpers.DataSource;

public interface IUriDataSource : IDataSource
{
    Uri Uri { get; }

    TimeSpan CacheValidity { get; }

    bool CachingEnabled { get; }
}