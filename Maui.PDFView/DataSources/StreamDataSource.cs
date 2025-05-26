namespace Maui.PDFView.Helpers.DataSource;

[System.ComponentModel.TypeConverter(typeof(DataSourceConverter))]
public partial class StreamDataSource : DataSource
{
    public static readonly BindableProperty StreamProperty = BindableProperty.Create(
        nameof(Stream),
        typeof(Func<CancellationToken, Task<Stream>>),
        typeof(StreamDataSource)
    );

    public override bool IsEmpty => Stream == null;

    public virtual Func<CancellationToken, Task<Stream>>? Stream
    {
        get => (Func<CancellationToken, Task<Stream>>)GetValue(StreamProperty);
        set => SetValue(StreamProperty, value);
    }

    public override async Task<Stream?> StreamAsync(CancellationToken cancellationToken = default)
    {
        if (IsEmpty)
        {
            return null;
        }

        OnLoadingStarted();
        if (CancellationTokenSource != null)
        {
            cancellationToken.Register(CancellationTokenSource.Cancel);
        }
       
        try
        {
            Stream? stream = null;
            if (Stream != null)
            {
                stream = await Stream(CancellationTokenSource?.Token ?? CancellationToken.None);
            }
            
            OnLoadingCompleted(false);
            return stream;
        }
        catch (OperationCanceledException)
        {
            OnLoadingCompleted(true);
            throw;
        }
    }
    
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        if (propertyName == StreamProperty.PropertyName)
        {
            OnSourceChanged();
        }
        base.OnPropertyChanged(propertyName);
    }
}