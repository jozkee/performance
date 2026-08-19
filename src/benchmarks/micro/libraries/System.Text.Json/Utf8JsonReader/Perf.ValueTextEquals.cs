// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using MicroBenchmarks;
using System.Buffers;
using System.Collections.Generic;
using System.Memory;

namespace System.Text.Json.Tests
{
    [BenchmarkCategory(Categories.Libraries, Categories.JSON)]
    public class Perf_ValueTextEquals
    {
        private const int PropertyCount = 100;

        // When true, every property name in the payload is written using \uXXXX escapes,
        // forcing Utf8JsonReader.ValueTextEquals down its unescaping comparison path.
        [Params(false, true)]
        public bool Escaped;

        [Params(false, true)]
        public bool MultiSegment;

        private byte[] _dataUtf8;
        private ReadOnlySequence<byte> _sequence;
        private byte[] _lookupUtf8;

        [GlobalSetup]
        public void Setup()
        {
            // The property we compare against on every PropertyName token. It never matches,
            // which forces ValueTextEquals to compare the full span/sequence each time rather
            // than bailing out early on a length mismatch.
            _lookupUtf8 = Encoding.UTF8.GetBytes("property_" + PropertyCount);

            var builder = new StringBuilder("{");
            for (int i = 0; i < PropertyCount; i++)
            {
                if (i != 0)
                {
                    builder.Append(',');
                }

                builder.Append('"');
                AppendPropertyName(builder, i, Escaped);
                builder.Append("\":");
                builder.Append(i);
            }
            builder.Append('}');

            _dataUtf8 = Encoding.UTF8.GetBytes(builder.ToString());

            ReadOnlyMemory<byte> memory = _dataUtf8;
            var first = new BufferSegment<byte>(memory.Slice(0, _dataUtf8.Length / 2));
            ReadOnlyMemory<byte> secondMemory = memory.Slice(_dataUtf8.Length / 2);
            BufferSegment<byte> second = first.Append(secondMemory);
            _sequence = new ReadOnlySequence<byte>(first, 0, second, secondMemory.Length);
        }

        private static void AppendPropertyName(StringBuilder builder, int index, bool escaped)
        {
            const string Prefix = "property_";
            if (!escaped)
            {
                builder.Append(Prefix).Append(index);
                return;
            }

            foreach (char c in Prefix)
            {
                builder.Append("\\u").Append(((int)c).ToString("x4"));
            }

            builder.Append(index);
        }

        [Benchmark]
        public int MatchPropertyNames()
        {
            Utf8JsonReader reader = MultiSegment
                ? new Utf8JsonReader(_sequence)
                : new Utf8JsonReader(_dataUtf8);

            int matches = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals(_lookupUtf8))
                {
                    matches++;
                }
            }

            return matches;
        }
    }
}
