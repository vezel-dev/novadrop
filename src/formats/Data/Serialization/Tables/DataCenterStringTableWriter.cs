// SPDX-License-Identifier: 0BSD

using Vezel.Novadrop.Data.Serialization.Items;
using Vezel.Novadrop.Data.Serialization.Regions;

namespace Vezel.Novadrop.Data.Serialization.Tables;

internal sealed class DataCenterStringTableWriter
{
    private readonly DataCenterSegmentedRegion<DataCenterRawChar> _data = new();

    private readonly DataCenterSegmentedSimpleRegion<DataCenterRawString> _strings;

    private readonly DataCenterSimpleRegion<DataCenterRawAddress> _addresses = new(offByOne: true);

    private readonly Dictionary<string, DataCenterRawString> _entries = new(ushort.MaxValue);

    private readonly bool _limit;

    public DataCenterStringTableWriter(int count, bool limit)
    {
        _strings = new(count);
        _limit = limit;
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask WriteAsync(
        DataCenterArchitecture architecture, StreamBinaryWriter writer, CancellationToken cancellationToken)
    {
        await _data.WriteAsync(architecture, writer, cancellationToken).ConfigureAwait(false);

        foreach (var seg in _strings.Segments)
            seg.Elements.Sort(static (a, b) => a.Hash.CompareTo(b.Hash));

        await _strings.WriteAsync(architecture, writer, cancellationToken).ConfigureAwait(false);
        await _addresses.WriteAsync(architecture, writer, cancellationToken).ConfigureAwait(false);
    }

    public DataCenterRawString AddString(string value)
    {
        ref var raw = ref CollectionsMarshal.GetValueRefOrAddDefault(_entries, value, out var exists);

        if (!exists)
        {
            // The name table is accessed with one-based 16-bit indexes rather than full addresses.
            Check.Operation(
                !_limit || _addresses.Elements.Count != ushort.MaxValue,
                $"String address table is full ({_addresses.Elements.Count} elements).");

            var max = DataCenterAddress.MaxValue;
            var segIdx = 0;
            var elemIdx = 0;
            var segment = default(DataCenterRegion<DataCenterRawChar>);

            // Try to find a region that the string can fit in.
            for (; segIdx < _data.Segments.Count; segIdx++)
            {
                var seg = _data.Segments[segIdx];

                if (seg.Elements.Count + value.Length + 1 <= max.ElementIndex)
                {
                    elemIdx = seg.Elements.Count;
                    segment = seg;

                    break;
                }
            }

            if (segment == null)
            {
                segIdx = _data.Segments.Count;

                Check.Operation(segIdx <= max.SegmentIndex, $"String table is full ({segIdx} segments).");

                segment = new();

                _data.Segments.Add(segment);
            }

            foreach (var ch in value)
                segment.Elements.Add(new()
                {
                    Value = ch,
                });

            segment.Elements.Add(default);

            var addr = new DataCenterRawAddress
            {
                SegmentIndex = (ushort)segIdx,
                ElementIndex = (ushort)elemIdx,
            };

            _addresses.Elements.Add(addr);

            var hash = DataCenterHash.ComputeStringHash(value);

            raw = new()
            {
                Hash = hash,
                Length = value.Length + 1,
                Index = _addresses.Elements.Count,
                Address = addr,
            };

            _strings.Segments[(int)((hash ^ hash >> 16) % (uint)_strings.Segments.Count)].Elements.Add(raw);
        }

        return raw;
    }

    public DataCenterRawString GetString(string value)
    {
        return _entries[value];
    }
}
