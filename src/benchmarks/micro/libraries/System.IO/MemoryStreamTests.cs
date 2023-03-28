#if NET8_0_OR_GREATER
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Extensions;
using MicroBenchmarks;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO.Tests
{
    [BenchmarkCategory(Categories.Libraries)]
    public class MemoryStreamTests
    {
        private byte[] _readWriteBuffer;
        private byte[] _streamBufferArray;
        private Memory<byte> _streamBufferMemory;

        [Params(false, true)]
        public bool UseMemoryCtor;

        [Params(64, 1024, 10_000)]
        public int BufferSize;

        [GlobalSetup(Targets = new[] { nameof(ReadByteArray), nameof(ReadSpan), nameof(ReadAsyncByteArray), nameof(ReadAsyncMemory) })]
        public void ReadSetup()
        {
            _readWriteBuffer = new byte[BufferSize];
            _streamBufferArray = ValuesGenerator.Array<byte>(BufferSize);
            _streamBufferMemory = _streamBufferArray;
        }

        [GlobalSetup(Targets = new[] { nameof(WriteByteArray), nameof(WriteSpan), nameof(WriteAsyncByteArray), nameof(WriteAsyncMemory) })]
        public void WriteSetup()
        {
            _readWriteBuffer = ValuesGenerator.Array<byte>(BufferSize);
            _streamBufferArray = new byte[BufferSize];
            _streamBufferMemory = _streamBufferArray;
        }

        private Stream GetMemoryStream() => UseMemoryCtor ?
                new MemoryStream(_streamBufferMemory) :
                new MemoryStream(_streamBufferArray);

        [Benchmark]
        public void ReadByteArray()
        {
            using var memoryStream = GetMemoryStream();
            while (memoryStream.Read(_readWriteBuffer, 0, _readWriteBuffer.Length) > 0) ;
        }

        [Benchmark]
        public void ReadSpan()
        {
            using var memoryStream = GetMemoryStream();
            while (memoryStream.Read(_readWriteBuffer) > 0) ;
        }

        [Benchmark]
        public async Task ReadAsyncByteArray()
        {
            using var memoryStream = GetMemoryStream();
            while (await memoryStream.ReadAsync(_readWriteBuffer, 0, _readWriteBuffer.Length, CancellationToken.None) > 0) ;
        }

        [Benchmark]
        public async Task ReadAsyncMemory()
        {
            using var memoryStream = GetMemoryStream();
            while (await memoryStream.ReadAsync(_readWriteBuffer, CancellationToken.None) > 0) ;
        }

        [Benchmark]
        public void WriteByteArray()
        {
            using var memoryStream = GetMemoryStream();
            memoryStream.Write(_readWriteBuffer, 0, _readWriteBuffer.Length);
        }

        [Benchmark]
        public void WriteSpan()
        {
            using var memoryStream = GetMemoryStream();
            memoryStream.Write(_readWriteBuffer);
        }

        [Benchmark]
        public async Task WriteAsyncByteArray()
        {
            using var memoryStream = GetMemoryStream();
            await memoryStream.WriteAsync(_readWriteBuffer, 0, _readWriteBuffer.Length, CancellationToken.None);
        }

        [Benchmark]
        public async Task WriteAsyncMemory()
        {
            using var memoryStream = GetMemoryStream();
            await memoryStream.WriteAsync(_readWriteBuffer, CancellationToken.None);
        }
    }
}
#endif