
using CsToml.Extensions.Utility;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;

namespace CsToml.Extensions;

public partial class CsTomlStreamingSerializer
{
    public static ValueTask<TPackage> DeserializeAsync<TPackage>(Stream stream, CsTomlSerializerOptions? options = null, bool configureAwait = false, CancellationToken cancellationToken = default)
        where TPackage : CsTomlPackage, ICsTomlPackageCreator<TPackage>
    {
        return DeserializeAsync<TPackage>(PipeReader.Create(stream), options, configureAwait, cancellationToken);
    }

    public static ValueTask<TPackage> DeserializeAsync<TPackage>(PipeReader reader, CsTomlSerializerOptions? options = null, bool configureAwait = false, CancellationToken cancellationToken = default)
        where TPackage : CsTomlPackage, ICsTomlPackageCreator<TPackage>
    {
        cancellationToken.ThrowIfCancellationRequested();

        return ReadAsync(reader, cancellationToken);

        [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
        async ValueTask<TPackage> ReadAsync(PipeReader reader, CancellationToken cancellationToken)
        {
            var result = await reader.ReadAsync(cancellationToken).ConfigureAwait(configureAwait);
            if (Utf8Helper.TryReadSequenceWithoutBOM(result.Buffer, out var buffer) == Utf8Helper.ReadSequenceWithoutBOMResult.Existed)
            {
                return CsTomlSerializer.Deserialize<TPackage>(buffer, options);
            }

            return CsTomlSerializer.Deserialize<TPackage>(result.Buffer, options);
        }

    }

}

