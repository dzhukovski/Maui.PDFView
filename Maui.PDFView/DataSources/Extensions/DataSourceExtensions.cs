namespace Maui.PDFView.Helpers.DataSource;

public static class DataSourceExtensions
{
    public static void LoadToFile(
        this IPdfView pdfView,
        CancellationToken cancellationToken = default,
        Action<string>? finished = null,
        Action<Exception>? error = null)
    {
        pdfView.Source.LoadToFile(cancellationToken, finished, error);
    }
    
    public static async void LoadToFile(
        this IDataSource source,
        CancellationToken cancellationToken = default,
        Action<string>? finished = null,
        Action<Exception>? error = null)
    {
        try
        {
            await using var stream = await source.StreamAsync(cancellationToken);
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
            error?.Invoke(e);
        }
    }
}