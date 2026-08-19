// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using MicroBenchmarks;

namespace System.Text.Json.Document.Tests
{
    [BenchmarkCategory(Categories.Libraries, Categories.JSON)]
    public class Perf_ElementParseValue
    {
        private byte[] _string;
        private byte[] _number;
        private byte[] _object;

        [GlobalSetup]
        public void Setup()
        {
            _string = Encoding.UTF8.GetBytes("\"a short json string value\"");
            _number = Encoding.UTF8.GetBytes("123456789");
            _object = Encoding.UTF8.GetBytes("{\"value\":123456789}");
        }

        // JsonElement.ParseValue drives JsonDocument.TryParseValue with useArrayPools: false,
        // the path that reaches JsonDocument.ParseUnrented and its primitive (String/Number)
        // fast path which avoids renting a MetadataDb and allocating a StackRowStack. This
        // mirrors deserializing a top-level primitive into JsonElement/object/JsonNode.
        [Benchmark]
        public JsonValueKind ParseString() => Parse(_string);

        [Benchmark]
        public JsonValueKind ParseNumber() => Parse(_number);

        [Benchmark]
        public JsonValueKind ParseObject() => Parse(_object);

        private static JsonValueKind Parse(byte[] utf8Json)
        {
            var reader = new Utf8JsonReader(utf8Json);
            return JsonElement.ParseValue(ref reader).ValueKind;
        }
    }
}
