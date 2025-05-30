namespace Maui.PDFView.Helpers.DataSource;

internal class StreamWrapper(Stream wrapped, IDisposable? additionalDisposable = null) : Stream
{
	private readonly Stream _wrapped = wrapped ?? throw new ArgumentNullException(nameof(wrapped));
	private IDisposable? _additionalDisposable = additionalDisposable;

	public event EventHandler? Disposed;

	public override bool CanRead => _wrapped.CanRead;

	public override bool CanSeek => _wrapped.CanSeek;

	public override bool CanWrite => _wrapped.CanWrite;

	public override long Length => _wrapped.Length;

	public override long Position
	{
		get => _wrapped.Position;
		set => _wrapped.Position = value;
	}

	public override void Flush()
	{
		_wrapped.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return _wrapped.Read(buffer, offset, count);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return _wrapped.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		_wrapped.SetLength(value);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		_wrapped.Write(buffer, offset, count);
	}

	protected override void Dispose(bool disposing)
	{
		_wrapped.Dispose();
		Disposed?.Invoke(this, EventArgs.Empty);
		_additionalDisposable?.Dispose();
		_additionalDisposable = null;

		base.Dispose(disposing);
	}

	public static async Task<Stream?> GetStreamAsync(Uri uri, CancellationToken cancellationToken, HttpClient client)
	{
		var response = await client.GetAsync(uri, cancellationToken).ConfigureAwait(false);
		response.EnsureSuccessStatusCode();
		
		// the HttpResponseMessage needs to be disposed of after the calling code is done with the stream
		// otherwise the stream may get disposed before the caller can use it
		return new StreamWrapper(
			await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false),
			response
		);
	}
}