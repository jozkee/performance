// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using MicroBenchmarks;
using System.Buffers;

namespace System.Text.Json.Tests
{
    [BenchmarkCategory(Categories.Libraries, Categories.JSON)]
    public class Perf_CommentLineSeparators
    {
        private const int SegmentSize = 100;

        [Params(JsonCommentHandling.Skip, JsonCommentHandling.Allow)]
        public JsonCommentHandling CommentHandling;

        [Params(false, true)]
        public bool MultiSegment;

        private byte[] _jsonPayload;
        private ReadOnlySequence<byte> _jsonPayloadSequence;

        [GlobalSetup]
        public void Setup()
        {
            // Single-line ("//") comments are the only path that scans for the U+2028/U+2029
            // line and paragraph separators. U+2027 ('\u2027', UTF-8 E2 80 A7) is a near-miss:
            // it shares the E2 80 lead bytes but is neither A8 nor A9, so every occurrence
            // exercises the dangerous-line-separator check (single- and multi-segment variants)
            // without producing invalid JSON. Many small segments make the E2/80/A7 bytes
            // straddle segment boundaries, driving the multi-segment carry-over path.
            _jsonPayload = Encoding.UTF8.GetBytes("{}//" + new string('\u2027', 2000) + "\n");
            _jsonPayloadSequence = Utf8JsonReaderCommentsTests.GetSequence(_jsonPayload, SegmentSize);
        }

        [Benchmark]
        public void ReadCommentWithSeparators()
        {
            var state = new JsonReaderState(new JsonReaderOptions { CommentHandling = CommentHandling });
            Utf8JsonReader reader = MultiSegment
                ? new Utf8JsonReader(_jsonPayloadSequence, isFinalBlock: true, state)
                : new Utf8JsonReader(_jsonPayload, isFinalBlock: true, state);

            while (reader.Read())
            {
            }
        }
    }
}
