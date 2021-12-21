using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Reading;

public interface IReader
{
    public ValueTask<object?> ReadObjectAsync(CancellationToken cancellationToken);
}

public interface IReader<T> : IReader
{
    public ValueTask<T> ReadAsync(CancellationToken cancellationToken);
}

public abstract class BaseReader
{
    protected XnbStreamReader Rx { get; }

    public BaseReader(XnbStreamReader rx)
    {
        Rx = rx;
    }
}

public abstract class SyncReader<T> : BaseReader, IReader<T>
{
    public SyncReader(XnbStreamReader rx) : base(rx) { }

    public abstract T Read();

    ValueTask<T> IReader<T>.ReadAsync(CancellationToken cancellationToken) => ValueTask.FromResult(Read());

    ValueTask<object?> IReader.ReadObjectAsync(CancellationToken cancellationToken) => ValueTask.FromResult<object?>(Read());
}

public abstract class AsyncReader<T> : BaseReader, IReader<T>
{
    public AsyncReader(XnbStreamReader rx) : base(rx) { }

    public abstract ValueTask<T> ReadAsync(CancellationToken cancellationToken);

    async ValueTask<object?> IReader.ReadObjectAsync(CancellationToken cancellationToken) => await ReadAsync(cancellationToken);
}
