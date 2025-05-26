namespace Maui.PDFView.Helpers.DataSource;

public partial class FileDataSource
{
    private Task<Stream> LoadAsMauiAssetAsync()
    {
        return FileSystem.OpenAppPackageFileAsync(File);
    }
}
