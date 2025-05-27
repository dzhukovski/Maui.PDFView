using Microsoft.Maui.Handlers;

namespace Maui.PDFView.Helpers.DataSource;

[System.ComponentModel.TypeConverter(typeof(FileDataSourceConverter))]
public sealed partial class FileDataSource : DataSource, IFileDataSource
{
    public static readonly BindableProperty FileProperty = BindableProperty.Create(
        nameof(File),
        typeof(string),
        typeof(FileDataSource)
    );

    public override bool IsEmpty => string.IsNullOrEmpty(File);

    public string File
    {
        get => (string)GetValue(FileProperty);
        set => SetValue(FileProperty, value);
    }
    
    public override async Task<Stream?> StreamAsync(CancellationToken cancellationToken = default)
    {
        if (IsEmpty)
        {
            return null;
        }
        
        if (Path.IsPathFullyQualified(File) || File.Contains("file://"))
        {
            await ExternalStorageReadPermissionGrantedAsync();
            var file = File.Replace("file://", string.Empty);
            return System.IO.File.OpenRead(file);
        }

        return await LoadAsMauiAssetAsync();
    }

    public override Task<bool> Cancel()
    {
        return Task.FromResult(false);
    }

    public override string ToString()
    {
        return $"File: {File}";
    }

    public static implicit operator FileDataSource(string file)
    {
        return FromFile(file);
    }

    public static implicit operator string(FileDataSource? file)
    {
        return file?.File ?? string.Empty;
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        if (propertyName == FileProperty.PropertyName)
        {
            OnSourceChanged();
        }
        base.OnPropertyChanged(propertyName);
    }
    
    private async Task<bool> ExternalStorageReadPermissionGrantedAsync()
    {
        return await MainThread
            .InvokeOnMainThreadAsync(Permissions.RequestAsync<Permissions.StorageRead>)
            .ConfigureAwait(false) == PermissionStatus.Granted;
    }
}