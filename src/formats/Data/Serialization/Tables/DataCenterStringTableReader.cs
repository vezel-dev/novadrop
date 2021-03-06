using Vezel.Novadrop.Data.Serialization.Items;
using Vezel.Novadrop.Data.Serialization.Regions;

namespace Vezel.Novadrop.Data.Serialization.Tables;

sealed class DataCenterStringTableReader
{
    readonly DataCenterSegmentedRegion<DataCenterRawChar> _data = new();

    readonly DataCenterSegmentedSimpleRegion<DataCenterRawString> _strings;

    readonly DataCenterSimpleRegion<DataCenterRawAddress> _addresses = new(true);

    readonly Dictionary<DataCenterAddress, string> _addressCache = new(ushort.MaxValue);

    readonly List<string> _indexCache = new(ushort.MaxValue);

    public DataCenterStringTableReader(int count)
    {
        _strings = new(count);
    }

    public async ValueTask ReadAsync(bool strict, StreamBinaryReader reader, CancellationToken cancellationToken)
    {
        await _data.ReadAsync(strict, reader, cancellationToken).ConfigureAwait(false);
        await _strings.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
        await _addresses.ReadAsync(reader, cancellationToken).ConfigureAwait(false);

        if (strict && _data.Segments.Count > DataCenterAddress.MaxValue.SegmentIndex)
            throw new InvalidDataException($"String table is too large ({_data.Segments.Count} segments).");

        var cache = new List<(int Index, string Value)>(ushort.MaxValue);

        foreach (var (i, seg) in _strings.Segments.Select((seg, i) => (i, seg)))
        {
            var last = -1L;

            foreach (var str in seg.Elements)
            {
                var index = str.Index - 1;

                if (index < 0 || index >= _addresses.Elements.Count)
                    throw new InvalidDataException(
                        $"String index {index + 1} is out of bounds (1..{_addresses.Elements.Count}).");

                var length = str.Length - 1; // Includes the terminator.

                if (length < 0)
                    throw new InvalidDataException($"String has invalid length {length + 1}.");

                var addr = str.Address;
                var segIdx = addr.SegmentIndex;
                var segs = _data.Segments;

                if (segIdx >= segs.Count)
                    throw new InvalidDataException(
                        $"String segment index {segIdx} is out of bounds (0..{segs.Count}).");

                var elemIdx = addr.ElementIndex;
                var elems = segs[segIdx].Elements;

                // Note that if the string straddles the end of the segment, the terminator may be omitted.
                if (elemIdx + length >= elems.Count)
                    throw new InvalidDataException(
                        $"String range {elemIdx}..{elemIdx + length + 1} is out of bounds (0..{elems.Count - 1}).");

                var value = new string(elems.GetRange(elemIdx, length).Select(c => c.Value).ToArray());

                if (strict)
                {
                    var realAddr = _addresses.Elements[index];

                    if ((DataCenterAddress)addr != realAddr)
                        throw new InvalidDataException(
                            $"String address {addr} does not match expected address {realAddr}.");

                    var hash = str.Hash;
                    var realHash = DataCenterHash.ComputeStringHash(value);

                    if (hash != realHash)
                        throw new InvalidDataException(
                            $"String hash 0x{hash:x8} does not match expected hash 0x{realHash:x8}.");

                    if (hash < last)
                        throw new InvalidDataException(
                            $"String hash 0x{hash:x8} is less than previous hash (0x{last:x8}).");

                    last = hash;

                    var bucket = (hash ^ hash >> 16) % (uint)_strings.Segments.Length;

                    if (i != bucket)
                        throw new InvalidDataException($"String bucket {i} does not match expected bucket {bucket}.");
                }

                if (!_addressCache.TryAdd(addr, value))
                    throw new InvalidDataException($"String address {addr} already recorded earlier.");

                cache.Add((index, value));
            }
        }

        foreach (var (_, val) in cache.OrderBy(tup => tup.Index))
            _indexCache.Add(val);
    }

    public string GetString(int index)
    {
        return index < _indexCache.Count
            ? _indexCache[index]
            : throw new InvalidDataException($"String table index {index} is invalid.");
    }

    public string GetString(DataCenterAddress address)
    {
        return _addressCache.TryGetValue(address, out var s)
            ? s
            : throw new InvalidDataException($"String table address {address} is invalid.");
    }
}
