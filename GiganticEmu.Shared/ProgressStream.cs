using System;
using System.IO;

namespace GiganticEmu.Shared;

public class ProgressStream : Stream
{
    private Stream _inner;
    private long _totalRead = 0;
    private Action<long> _callback;

    public ProgressStream(Stream inner, Action<long> callback)
    {
        _inner = inner;
        _callback = callback;
    }

    protected override void Dispose(bool disposing)
    {
        _inner.Dispose();
        base.Dispose(disposing);
    }

    public override bool CanRead => _inner.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => _inner.Length;

    public override long Position { get => _inner.Position; set => _inner.Position = value; }

    public override void Flush() => _inner.Flush();

    public override int Read(byte[] buffer, int offset, int count)
    {
        var read = _inner.Read(buffer, offset, count);
        _totalRead += read;
        _callback.Invoke(_totalRead);
        return read;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new System.NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new System.NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new System.NotSupportedException();
    }
}