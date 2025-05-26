using System.Reflection;

namespace Maui.PDFView.Helpers.DataSource;

[System.ComponentModel.TypeConverter(typeof(DataSourceConverter))]
public abstract partial class DataSource : Element, IDataSource
{
	private readonly object _synchandle = new object();
	private readonly WeakEventManager _weakEventManager = new WeakEventManager();
	private CancellationTokenSource? _cancellationTokenSource;
	private TaskCompletionSource<bool>? _completionSource;

	protected DataSource()
	{
	}

	internal event EventHandler SourceChanged
	{
		add => _weakEventManager.AddEventHandler(value);
		remove => _weakEventManager.RemoveEventHandler(value);
	}

	public virtual bool IsEmpty => false;

	public abstract Task<Stream?> StreamAsync(CancellationToken cancellationToken = default);

	private protected CancellationTokenSource? CancellationTokenSource
	{
		get => _cancellationTokenSource;
		private set
		{
			if (_cancellationTokenSource == value)
			{
				return;
			}

			if (_cancellationTokenSource != null)
			{
				_cancellationTokenSource.Cancel();
			}

			_cancellationTokenSource = value;
		}
	}

	bool IsLoading => _cancellationTokenSource != null;

	public static bool IsNullOrEmpty(IDataSource? dataSource) => dataSource == null || dataSource.IsEmpty;
	
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

	private protected void OnLoadingCompleted(bool cancelled)
	{
		if (!IsLoading || _completionSource == null)
			return;

		TaskCompletionSource<bool>? tcs = Interlocked.Exchange(ref _completionSource, null);
		if (tcs != null)
		{
			tcs.SetResult(cancelled);
		}

		lock (_synchandle)
		{
			CancellationTokenSource = null;
		}
	}

	private protected void OnLoadingStarted()
	{
		lock (_synchandle)
		{
			CancellationTokenSource = new CancellationTokenSource();
		}
	}

	protected void OnSourceChanged()
	{
		_weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(SourceChanged));
	}
}