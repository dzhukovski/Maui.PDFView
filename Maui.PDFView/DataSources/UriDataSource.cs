using Microsoft.Extensions.Logging;
using UriTypeConverter = Microsoft.Maui.Controls.UriTypeConverter;

namespace Maui.PDFView.Helpers.DataSource;

[System.ComponentModel.TypeConverter(typeof(DataSourceConverter))]
public sealed partial class UriDataSource : DataSource, IUriDataSource
{
	public static readonly BindableProperty UriProperty = BindableProperty.Create(
		nameof(Uri),
		typeof(Uri),
		typeof(UriDataSource),
		propertyChanged: (bindable, _, _) => ((UriDataSource)bindable).OnUriChanged(),
		validateValue: (_, value) => value == null || ((Uri)value).IsAbsoluteUri
	);

	public static readonly BindableProperty CacheValidityProperty = BindableProperty.Create(
		nameof(CacheValidity),
		typeof(TimeSpan),
		typeof(UriDataSource),
		TimeSpan.FromDays(1)
	);

	public static readonly BindableProperty CachingEnabledProperty = BindableProperty.Create(
		nameof(CachingEnabled),
		typeof(bool),
		typeof(UriDataSource),
		true
	);
	
	public override bool IsEmpty => Uri == null;

	public TimeSpan CacheValidity
	{
		get => (TimeSpan)GetValue(CacheValidityProperty);
		set => SetValue(CacheValidityProperty, value);
	}

	public bool CachingEnabled
	{
		get => (bool)GetValue(CachingEnabledProperty);
		set => SetValue(CachingEnabledProperty, value);
	}

	[System.ComponentModel.TypeConverter(typeof(UriTypeConverter))]
	public Uri? Uri
	{
		get => (Uri?)GetValue(UriProperty);
		set => SetValue(UriProperty, value);
	}
	
	public override string ToString()
	{
		return Uri?.ToString() ?? String.Empty;
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
			var stream = await StreamAsync(Uri!, CancellationTokenSource?.Token ?? CancellationToken.None);
			OnLoadingCompleted(false);
			return stream;
		}
		catch (OperationCanceledException)
		{
			OnLoadingCompleted(true);
			throw;
		}
		catch (Exception ex)
		{
			Logger()?.LogWarning(ex, "Error getting stream for {Uri}", Uri);
			throw;
		}
	}
	async Task<Stream?> StreamAsync(Uri uri, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		Stream? stream = null;
		if (CachingEnabled)
		{
			stream ??= await DownloadStreamAsync(uri, cancellationToken).ConfigureAwait(false);
		}
		else
		{
			stream = await DownloadStreamAsync(uri, cancellationToken).ConfigureAwait(false);
		}

		return stream;
	}

	async Task<Stream?> DownloadStreamAsync(Uri uri, CancellationToken cancellationToken)
	{
		try
		{
			using var client = new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true });
			return await StreamWrapper
				.GetStreamAsync(uri, cancellationToken, client)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			Logger()?.LogWarning(ex, "Error getting stream for {Uri}", Uri);
			return null;
		}
	}
	
	private void OnUriChanged()
	{
		CancellationTokenSource?.Cancel();
		OnSourceChanged();
	}
	
	private ILogger? Logger(IMauiContext? context = null)
	{
		context ??= Application.Current?.FindMauiContext();
		return context?.CreateLogger<UriDataSource>();
	} 
}