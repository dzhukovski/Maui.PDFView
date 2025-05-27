using System.Reflection;

namespace Maui.PDFView.Helpers.DataSource;

[System.ComponentModel.TypeConverter(typeof(DataSourceConverter))]
public abstract partial class DataSource : Element, IDataSource
{
	public static readonly BindableProperty LoadingErrorProperty = BindableProperty.Create(
		nameof(LoadingError),
		typeof(Exception),
		typeof(DataSource)
	);
	
	private readonly object _syncHandle = new object();
	private readonly WeakEventManager _weakEventManager = new WeakEventManager();
	private CancellationTokenSource? _cancellationTokenSource;
	private TaskCompletionSource<bool>? _completionSource;

	internal event EventHandler SourceChanged
	{
		add => _weakEventManager.AddEventHandler(value);
		remove => _weakEventManager.RemoveEventHandler(value);
	}

	public Exception? LoadingError
	{
		get => (Exception?)GetValue(LoadingErrorProperty);
		set => SetValue(LoadingErrorProperty, value);
	}
	
	public virtual bool IsEmpty => false;

	private protected CancellationTokenSource? CancellationTokenSource
	{
		get => _cancellationTokenSource;
		private set
		{
			if (_cancellationTokenSource != value)
			{
				_cancellationTokenSource?.Cancel();
				_cancellationTokenSource = value;
			}
		}
	}

	bool IsLoading => _cancellationTokenSource != null;

	public async Task<Stream?> StreamAsync(CancellationToken cancellationToken = default)
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
			var stream = await StreamImplAsync(CancellationTokenSource?.Token ?? CancellationToken.None);
			OnLoadingCompleted(false);
			return stream;
		}
		catch (OperationCanceledException)
		{
			OnLoadingCompleted(true);
			throw;
		}
	}
	
	public virtual Task<bool> Cancel()
	{
		if (!IsLoading)
		{
			return Task.FromResult(false);
		}

		var original = Interlocked.CompareExchange(
			ref _completionSource,
			new TaskCompletionSource<bool>(),
			null
		);
		if (original is null)
		{
			_cancellationTokenSource?.Cancel();
			return Task.FromResult(false);
		}

		return original.Task;
	}

	public static FileDataSource FromFile(string file)
	{
		return new FileDataSource { File = file };
	}

	public static StreamDataSource FromResource(string resource, Type resolvingType)
	{
		return FromResource(resource, resolvingType.Assembly);
	}

	public static StreamDataSource FromResource(string resource, Assembly? sourceAssembly = null)
	{
		sourceAssembly ??= Assembly.GetCallingAssembly();
		return FromStream(
			() => sourceAssembly.GetManifestResourceStream(resource) ?? throw new InvalidOperationException($"Resource '{resource}' not found in {sourceAssembly.FullName}")
		);
	}

	public static StreamDataSource FromStream(Func<Stream> stream)
	{
		return new StreamDataSource { Stream = token => Task.Run(stream, token) };
	}

	public static StreamDataSource FromStream(Func<CancellationToken, Task<Stream>> stream)
	{
		return new StreamDataSource { Stream = stream };
	}

	public static UriDataSource FromUri(Uri uri)
	{
		if (!uri.IsAbsoluteUri)
		{
			throw new ArgumentException("uri is relative");
		}
		return new UriDataSource { Uri = uri };
	}

	public static implicit operator DataSource(string source)
	{
		return Uri.TryCreate(source, UriKind.Absolute, out var uri) && uri.Scheme != "file"
			? FromUri(uri)
			: FromFile(source);
	}
	
	public static implicit operator DataSource?(Uri? uri)
	{
		if (uri == null)
		{
			return null;
		}

		if (!uri.IsAbsoluteUri)
		{
			throw new ArgumentException("uri is relative");
		}
		return FromUri(uri);
	}
	
	protected abstract Task<Stream?> StreamImplAsync(CancellationToken cancellationToken = default);

	private protected void OnLoadingCompleted(bool cancelled)
	{
		if (!IsLoading || _completionSource == null)
			return;

		TaskCompletionSource<bool>? tcs = Interlocked.Exchange(ref _completionSource, null);
		if (tcs != null)
		{
			tcs.SetResult(cancelled);
		}

		lock (_syncHandle)
		{
			CancellationTokenSource = null;
		}
	}

	private protected void OnLoadingStarted()
	{
		lock (_syncHandle)
		{
			CancellationTokenSource = new CancellationTokenSource();
		}
	}

	protected void OnSourceChanged()
	{
		_weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(SourceChanged));
	}
}