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
    
    public override Task<Stream?> StreamAsync(CancellationToken cancellationToken = default)
    {
        if (IsEmpty)
        {
            return Task.FromResult<Stream?>(null);
        }
        
        if (Path.IsPathFullyQualified(File))
        {
            return Task.FromResult<Stream?>(System.IO.File.Open(File, FileMode.Open));
        }

        return LoadAsMauiAssetAsync();
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
}