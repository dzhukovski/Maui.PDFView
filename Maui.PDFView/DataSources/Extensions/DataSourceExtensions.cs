namespace Maui.PDFView.Helpers.DataSource;

public static class DataSourceExtensions
{
    public static bool IsNullOrEmpty(this IDataSource? dataSource) => dataSource == null || dataSource.IsEmpty;
    
    public static async void LoadToFile(
        this IPdfView pdfView,
        CancellationToken cancellationToken = default,
        Action<string>? finished = null,
        Action<Exception>? error = null)
    {
        try
        {
            pdfView.Source.LoadingError = null;
            pdfView.IsLoading = true;
            await using var stream = await pdfView.Source.StreamAsync(cancellationToken);
            if (stream != null)
            {
                var fileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                await using (var fileStream = File.Create(fileName))
                {
                    await stream.CopyToAsync(fileStream, cancellationToken);
                }
                
                finished?.Invoke(fileName);
            }
        }
        catch (Exception e)
        {
            pdfView.Source.LoadingError = e;
            error?.Invoke(e);
        }
        finally
        {
            pdfView.IsLoading = false;
        }
    }
}