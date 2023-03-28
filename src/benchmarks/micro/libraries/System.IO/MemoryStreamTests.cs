using BenchmarkDotNet.Attributes;
using MicroBenchmarks;
using System.Reflection;
using System.Threading.Tasks;

namespace System.IO.Tests
{
    [BenchmarkCategory(Categories.Libraries)]
    public class MemoryStreamTests
    {
        private Stream _stream;
        private byte[] _buffer;
        private static Random s_random = new Random(42);

        [Params(false, true)]
        public bool UseMemoryCtor;

        [Params(64, 1024, 10_000)]
        public int BufferSize;

        [GlobalSetup(Targets = new[] {nameof(ReadAll), nameof(ReadAllAsync)})]
        public void ReadSetup()
        {
            _buffer = new byte[BufferSize];
            Setup();
        }

        [GlobalSetup(Targets = new[] {nameof(WriteAll), nameof(WriteAllAsync)})]
        public void WriteSetup()
        {
            _buffer = new byte[BufferSize];
            s_random.NextBytes(_buffer);
            Setup();
        }

        public void Setup()
        {
            if (UseMemoryCtor)
            {
                Type type = typeof(MemoryStream);
                ConstructorInfo ctor = type.GetConstructor(new[] { typeof(Memory<byte>) });
                _stream = (Stream)ctor.Invoke(new object[] { new byte[BufferSize].AsMemory() });
            }
            else
            {
                _stream = new MemoryStream(new byte[BufferSize]);
            }
        }

        [GlobalCleanup] 
        public void GlobalCleanup()
        {
            _stream.Dispose();
        }

        [Benchmark]
        public void ReadAll()
            => _stream.ReadExactly(_buffer);

        [Benchmark]
        public ValueTask ReadAllAsync()
            => _stream.ReadExactlyAsync(_buffer);

        [Benchmark]
        public void WriteAll()
            => _stream.Write(_buffer);

        [Benchmark]
        public ValueTask WriteAllAsync()
            => _stream.WriteAsync(_buffer);
    }
}