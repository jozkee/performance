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
        private byte[] _streamBuffer;

        [Params(1, 100, 100_000, 100_000_000)]//0x7FFFFFC7 /*Array max length*/)]
        public int BufferSize;

        [GlobalSetup(Targets = new[] { nameof(ReadByte), nameof(ReadByteArray), nameof(ReadAsyncByteArray),
#if NETCOREAPP
            nameof(ReadSpan), nameof(ReadAsyncMemory),
#endif
            nameof(CopyTo), nameof(CopyToAsync) })]
        public void ReadSetup()
        {
            _readWriteBuffer = new byte[BufferSize];
            _streamBuffer = ValuesGenerator.Array<byte>(BufferSize);
        }

        [GlobalSetup(Targets = new[] { nameof(WriteByte), nameof(WriteByteArray), nameof(WriteAsyncByteArray),
#if NETCOREAPP
            nameof(WriteSpan), nameof(WriteAsyncMemory)
#endif
        })]
        public void WriteSetup()
        {
            _readWriteBuffer = ValuesGenerator.Array<byte>(BufferSize);
            _streamBuffer = new byte[BufferSize];
        }

        private Stream GetMemoryStream() => new MemoryStream(_streamBuffer);

        [Benchmark]
        public void ReadByte()
        {
            using Stream memoryStream = GetMemoryStream();
            while (memoryStream.ReadByte() != -1) ;
        }

        [Benchmark]
        public void ReadByteArray()
        {
            using Stream memoryStream = GetMemoryStream();
            while (memoryStream.Read(_readWriteBuffer, 0, _readWriteBuffer.Length) > 0) ;
        }

        [Benchmark]
        [BenchmarkCategory(Categories.NoWASM)]
        public async Task ReadAsyncByteArray()
        {
            using Stream memoryStream = GetMemoryStream();
            while (await memoryStream.ReadAsync(_readWriteBuffer, 0, _readWriteBuffer.Length, CancellationToken.None) > 0) ;
        }

#if NETCOREAPP
        [Benchmark]
        public void ReadSpan()
        {
            using Stream memoryStream = GetMemoryStream();
            while (memoryStream.Read(_readWriteBuffer) > 0) ;
        }

        [Benchmark]
        [BenchmarkCategory(Categories.NoWASM)]
        public async Task ReadAsyncMemory()
        {
            using Stream memoryStream = GetMemoryStream();
            while (await memoryStream.ReadAsync(_readWriteBuffer, CancellationToken.None) > 0) ;
        }
#endif

        [Benchmark]
        public void WriteByte()
        {
            using Stream memoryStream = GetMemoryStream();

            int i = 0;
            while (i < _readWriteBuffer.Length)
            {
                memoryStream.WriteByte(_readWriteBuffer[i++]);
            }
        }

        [Benchmark]
        public void WriteByteArray()
        {
            using Stream memoryStream = GetMemoryStream();
            memoryStream.Write(_readWriteBuffer, 0, _readWriteBuffer.Length);
        }

        [Benchmark]
        [BenchmarkCategory(Categories.NoWASM)]
        public async Task WriteAsyncByteArray()
        {
            using Stream memoryStream = GetMemoryStream();
            await memoryStream.WriteAsync(_readWriteBuffer, 0, _readWriteBuffer.Length, CancellationToken.None);
        }

#if NETCOREAPP
        [Benchmark]
        public void WriteSpan()
        {
            using Stream memoryStream = GetMemoryStream();
            memoryStream.Write(_readWriteBuffer);
        }

        [Benchmark]
        [BenchmarkCategory(Categories.NoWASM)]
        public async Task WriteAsyncMemory()
        {
            using Stream memoryStream = GetMemoryStream();
            await memoryStream.WriteAsync(_readWriteBuffer, CancellationToken.None);
        }
#endif

        [Benchmark]
        public void CopyTo()
        {
            using Stream src = GetMemoryStream();
            using Stream dest = new MemoryStream();
            src.CopyTo(dest);
        }

        [Benchmark]
        [BenchmarkCategory(Categories.NoWASM)]
        public async Task CopyToAsync()
        {
            using Stream src = GetMemoryStream();
            using Stream dest = new MemoryStream();
            await src.CopyToAsync(dest);
        }
    }
}
