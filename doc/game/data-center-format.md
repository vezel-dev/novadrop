# Data Center Format

This page describes the encrypted and compressed data center format used by
TERA.

* C/C++-like primitive types, `enum`s, `struct`s, and `union`s will be used.
* `bool` is equivalent to `uint8_t` but only allows the values `true` (`1`) and
  `false` (`0`).
* Integers (`uint8_t`, `int8_t`, `uint16_t`, `int16_t`, etc) are little endian.
* `float` and `double` are IEEE 754 `binary32` and `binary64`, respectively.
* Characters (i.e. `char16_t`) are UTF-16 and little endian.
* Fields are laid out in the declared order with no implied padding anywhere.

Note that the format uses both zero-based and one-based array indexes in
various, seemingly random places.

## Encryption

Data center files are encrypted with the
[AES](https://www.nist.gov/publications/advanced-encryption-standard-aes)
algorithm in CFB mode and with block, key, and feedback sizes all set to 128
bits. No padding is done for the final block.

The encryption key and initialization vector can both be extracted from a
running TERA client, e.g. with the [novadrop-scan](../tools/scan.md) tool. These
values are usually freshly generated for each client build.

## Physical Structure

The overall structure can be described like this:

```cpp
struct DataCenterFile
{
    DataCenterCompressionHeader compression_header;
    DataCenterHeader header;
    DataCenterSimpleRegion<DataCenterKey, false> keys;
    DataCenterSegmentedRegion<DataCenterAttribute> attributes;
    DataCenterSegmentedRegion<DataCenterNode> nodes;
    DataCenterStringTable<1024> values;
    DataCenterStringTable<512> names;
    DataCenterFooter footer;
};
```

### Compression Header

After decryption, there is a small header of the form:

```cpp
struct DataCenterCompressionHeader
{
    uint32_t uncompressed_size;
    uint16_t zlib_header;
};
```

All data immediately following this header is compressed with the Deflate
algorithm.

`uncompressed_size` is the size of the data center file once inflated.

`zlib_header` is a [zlib](https://tools.ietf.org/html/rfc1950) (§2.2) header. In
official data center files, it is usually of the form `0x9c78`, but it can be
any valid zlib header.

### File Header

After decompression, a data center file starts with this header:

```cpp
struct DataCenterHeader
{
    uint32_t version;
    double timestamp;
    uint32_t revision;
    int32_t unknown_1;
    int32_t unknown_2;
    int32_t unknown_3;
    int32_t unknown_4;
};
```

`version` is currently `6`.

`timestamp` is a Unix timestamp indicating when the file was produced.

`unknown_1`, `unknown_2`, `unknown_3`, and `unknown_4` are all always `0`. They
are actually part of a tree structure describing the
[XSD](https://www.w3.org/TR/xmlschema-1) schema of the data tree, but official
data centers never include this information.

`revision` indicates the version of the data tree contained within the file. It
is sometimes (but not always) equal to the value sent by the client in the
`C_CHECK_VERSION` packet.

### File Footer

A data center file ends with this footer:

```cpp
struct DataCenterFooter
{
    int32_t marker;
}
```

`marker` is always `0` and has no known purpose in the client.

### Regions

Most of the content in data center files is arranged into regions, which may be
segmented. The region structures used throughout the format are described here:

```cpp
struct DataCenterSimpleRegion<typename T, bool off_by_one>
{
    uint32_t count;
    T elements[off_by_one ? count - 1 : count];
};

struct DataCenterSegmentedSimpleRegion<typename T, uint32_t count>
{
    DataCenterSimpleRegion<T, false> segments[count];
};

struct DataCenterRegion<typename T>
{
    uint32_t full_count;
    uint32_t used_count;
    T elements[full_count];
};

struct DataCenterSegmentedRegion<typename T>
{
    uint32_t count;
    DataCenterRegion<T> segments[count];
};
```

In a `DataCenterRegion`, the `used_count` can be less than the `full_count`.
`full_count` is usually `65535`, even when `used_count` is much smaller. All
data in the region that goes beyond `used_count` can be arbitrary and should be
considered undefined.

A `DataCenterSegmentedSimpleRegion` is mostly the same as a
`DataCenterSegmentedRegion`, with the main difference being that it has a static
amount of segments.

Addresses are frequently used to refer to elements within both types of
segmented regions:

```cpp
struct DataCenterAddress
{
    uint16_t segment_index;
    uint16_t element_index;
};
```

Here, `segment_index` is a **zero-based** index into the `segments` array of the
segmented region, while `element_index` is a **zero-based** index into the
`elements` array of the segment.

## String Tables

All strings, whether they are names or values, are arranged into string tables,
which are effectively used as hash tables by the client. A string table has the
form:

```cpp
struct DataCenterStringTable<uint32_t count>
{
    DataCenterSegmentedRegion<char16_t> data;
    DataCenterSegmentedSimpleRegion<DataCenterString, count> table;
    DataCenterSimpleRegion<DataCenterAddress, true> addresses;
};
```

A string entry in the `table` region looks like this:

```cpp
struct DataCenterString
{
    uint32_t hash;
    uint32_t length;
    uint32_t index;
    DataCenterAddress address;
};
```

`hash` is a hash code for the string, given by the expression
`data_center_string_hash(value)` where `value` is the string value. In a typical
data center file, there is only a very tiny amount of hash collisions.

`length` is the length of the string in terms of characters, including the NUL
character.

`index` is a **one-based** index into the string table's `addresses` region. The
address at this index must match the `address` field exactly.

`address` is an address into the string table's `data` region. This address
points to the actual string data. The string read from this address must have
the same length as the `length` field. Notably, if the string data straddles the
end of its segment, the NUL character may be omitted. Readers should therefore
not rely exclusively on the presence of a NUL character, but also check the
segment bounds.

A string entry must be placed in the correct `table` segment based on its `hash`
field. The segment index is given by the expression
`(hash ^ hash >> 16) % count` where `count` is the static size of the `table`
region. Further, entries in a segment must be sorted by their hash code in
ascending order.

For the `names` table, the special names `__root__` and `__value__` must be
added to the table last, so that their index values are greater than all other
entries. Also, they must be present even if they are not used.

Finally, it is worth noting that the `names` table is always referred to by
index, whereas the `values` table is always referred to by address. The reason
for this is that the `names` table is small enough that all entries can be
accessed by a `uint16_t` index value into its `addresses` region, whereas that
is not the case for the `values` table. In spite of this difference, `names`
and `values` must both have valid `addresses` regions.

### String Hash

The `data_center_string_hash` function is a variant of CRC32 and is defined as
follows:

```cpp
const uint32_t string_hash_table[256] =
{
    0x00000000, 0x04c11db7, 0x09823b6e, 0x0d4326d9, 0x130476dc, 0x17c56b6b, 0x1a864db2, 0x1e475005,
    0x2608edb8, 0x22c9f00f, 0x2f8ad6d6, 0x2b4bcb61, 0x350c9b64, 0x31cd86d3, 0x3c8ea00a, 0x384fbdbd,
    0x4c11db70, 0x48d0c6c7, 0x4593e01e, 0x4152fda9, 0x5f15adac, 0x5bd4b01b, 0x569796c2, 0x52568b75,
    0x6a1936c8, 0x6ed82b7f, 0x639b0da6, 0x675a1011, 0x791d4014, 0x7ddc5da3, 0x709f7b7a, 0x745e66cd,
    0x9823b6e0, 0x9ce2ab57, 0x91a18d8e, 0x95609039, 0x8b27c03c, 0x8fe6dd8b, 0x82a5fb52, 0x8664e6e5,
    0xbe2b5b58, 0xbaea46ef, 0xb7a96036, 0xb3687d81, 0xad2f2d84, 0xa9ee3033, 0xa4ad16ea, 0xa06c0b5d,
    0xd4326d90, 0xd0f37027, 0xddb056fe, 0xd9714b49, 0xc7361b4c, 0xc3f706fb, 0xceb42022, 0xca753d95,
    0xf23a8028, 0xf6fb9d9f, 0xfbb8bb46, 0xff79a6f1, 0xe13ef6f4, 0xe5ffeb43, 0xe8bccd9a, 0xec7dd02d,
    0x34867077, 0x30476dc0, 0x3d044b19, 0x39c556ae, 0x278206ab, 0x23431b1c, 0x2e003dc5, 0x2ac12072,
    0x128e9dcf, 0x164f8078, 0x1b0ca6a1, 0x1fcdbb16, 0x018aeb13, 0x054bf6a4, 0x0808d07d, 0x0cc9cdca,
    0x7897ab07, 0x7c56b6b0, 0x71159069, 0x75d48dde, 0x6b93dddb, 0x6f52c06c, 0x6211e6b5, 0x66d0fb02,
    0x5e9f46bf, 0x5a5e5b08, 0x571d7dd1, 0x53dc6066, 0x4d9b3063, 0x495a2dd4, 0x44190b0d, 0x40d816ba,
    0xaca5c697, 0xa864db20, 0xa527fdf9, 0xa1e6e04e, 0xbfa1b04b, 0xbb60adfc, 0xb6238b25, 0xb2e29692,
    0x8aad2b2f, 0x8e6c3698, 0x832f1041, 0x87ee0df6, 0x99a95df3, 0x9d684044, 0x902b669d, 0x94ea7b2a,
    0xe0b41de7, 0xe4750050, 0xe9362689, 0xedf73b3e, 0xf3b06b3b, 0xf771768c, 0xfa325055, 0xfef34de2,
    0xc6bcf05f, 0xc27dede8, 0xcf3ecb31, 0xcbffd686, 0xd5b88683, 0xd1799b34, 0xdc3abded, 0xd8fba05a,
    0x690ce0ee, 0x6dcdfd59, 0x608edb80, 0x644fc637, 0x7a089632, 0x7ec98b85, 0x738aad5c, 0x774bb0eb,
    0x4f040d56, 0x4bc510e1, 0x46863638, 0x42472b8f, 0x5c007b8a, 0x58c1663d, 0x558240e4, 0x51435d53,
    0x251d3b9e, 0x21dc2629, 0x2c9f00f0, 0x285e1d47, 0x36194d42, 0x32d850f5, 0x3f9b762c, 0x3b5a6b9b,
    0x0315d626, 0x07d4cb91, 0x0a97ed48, 0x0e56f0ff, 0x1011a0fa, 0x14d0bd4d, 0x19939b94, 0x1d528623,
    0xf12f560e, 0xf5ee4bb9, 0xf8ad6d60, 0xfc6c70d7, 0xe22b20d2, 0xe6ea3d65, 0xeba91bbc, 0xef68060b,
    0xd727bbb6, 0xd3e6a601, 0xdea580d8, 0xda649d6f, 0xc423cd6a, 0xc0e2d0dd, 0xcda1f604, 0xc960ebb3,
    0xbd3e8d7e, 0xb9ff90c9, 0xb4bcb610, 0xb07daba7, 0xae3afba2, 0xaafbe615, 0xa7b8c0cc, 0xa379dd7b,
    0x9b3660c6, 0x9ff77d71, 0x92b45ba8, 0x9675461f, 0x8832161a, 0x8cf30bad, 0x81b02d74, 0x857130c3,
    0x5d8a9099, 0x594b8d2e, 0x5408abf7, 0x50c9b640, 0x4e8ee645, 0x4a4ffbf2, 0x470cdd2b, 0x43cdc09c,
    0x7b827d21, 0x7f436096, 0x7200464f, 0x76c15bf8, 0x68860bfd, 0x6c47164a, 0x61043093, 0x65c52d24,
    0x119b4be9, 0x155a565e, 0x18197087, 0x1cd86d30, 0x029f3d35, 0x065e2082, 0x0b1d065b, 0x0fdc1bec,
    0x3793a651, 0x3352bbe6, 0x3e119d3f, 0x3ad08088, 0x2497d08d, 0x2056cd3a, 0x2d15ebe3, 0x29d4f654,
    0xc5a92679, 0xc1683bce, 0xcc2b1d17, 0xc8ea00a0, 0xd6ad50a5, 0xd26c4d12, 0xdf2f6bcb, 0xdbee767c,
    0xe3a1cbc1, 0xe760d676, 0xea23f0af, 0xeee2ed18, 0xf0a5bd1d, 0xf464a0aa, 0xf9278673, 0xfde69bc4,
    0x89b8fd09, 0x8d79e0be, 0x803ac667, 0x84fbdbd0, 0x9abc8bd5, 0x9e7d9662, 0x933eb0bb, 0x97ffad0c,
    0xafb010b1, 0xab710d06, 0xa6322bdf, 0xa2f33668, 0xbcb4666d, 0xb8757bda, 0xb5365d03, 0xb1f740b4,
};

uint32_t data_center_string_hash(char16_t *string)
{
    uint32_t hash = 0;

    for (char16_t *current = string; *current; current++)
    {
        char16_t value = htole16(*current);

        for (size_t i = 0; i < 2; i++)
            hash = string_hash_table[(uint8_t)(hash ^ ((uint8_t *)&value)[i])] ^ hash >> 8;
    }

    return hash;
}
```

(Note that `string_hash_table` is the same as [`value_hash_table`](#value-hash).)

## Data Tree

The actual content in a data center file is stored as a sort of tree structure,
which is essentially [XML](https://www.w3.org/TR/xml) serialized to a binary
format.

### Nodes

Each node is of the form:

```cpp
struct DataCenterNode
{
    uint16_t name_index;
    uint8_t key_flags : 4;
    uint16_t key_index : 12;
    uint16_t attribute_count;
    uint16_t child_count;
    DataCenterAddress attribute_address;
    uint32_t padding_1;
    DataCenterAddress child_address;
    uint32_t padding_2;
};
```

`name_index` is a **one-based** index into the `addresses` region of the `names`
table. If this value is `0`, it indicates that this node has no name or
associated data of any kind, and should be disregarded; in this case, all other
fields of the node should be considered undefined. Such nodes are usually
incidental leftovers in official data center files and need not be present.

`key_flags` is `0` in official data center files. It may have a combination of
the following values:

```cpp
enum DataCenterKeyFlags : uint8_t
{
    DATA_CENTER_KEY_FLAGS_NONE = 0b0000,
    DATA_CENTER_KEY_FLAGS_UNCACHED = 0b0001,
};
```

If `DATA_CENTER_KEY_FLAGS_UNCACHED` is set, the results of a query against this
node will not be cached by the client.

`key_index` is a **zero-based** index into the `keys` region.

`attribute_count` and `child_count` indicate how many attributes and child
nodes should be read for this node, respectively. If a count field is `0`, then
the corresponding address field should be considered undefined, though it will
usually be `65535:65535` in official data center files.

`attribute_address` is an address into the `attributes` region.
`attribute_count` attributes should be read at this address. These attributes
must be sorted by their name index in ascending order.

`child_address` is an address into the `node` region. `child_count` nodes should
be read at this address. These children must be sorted first by their name
index, then by name indexes of keys (if any), in ascending order. Note that the
sort should be stable since the order of multiple sibling nodes with the same
name can be significant for the interpretation of the data.

`padding_1` and `padding_2` should be considered undefined. They were added in
the 64-bit data center format.

The root node of the data tree must be located at the address `0:0`. It must
have the name `__root__` and have zero attributes.

### Keys

Keys are used to signal to a data center reading layer (e.g. in the client) that
certain attributes of a node will be used frequently for lookups. The reading
layer can then decide to construct an optimized lookup table for those specific
paths in the tree, transparently making those lookups faster. It is effectively
a trade-off between speed and memory usage.

```cpp
struct DataCenterKey
{
    uint16_t name_index_1;
    uint16_t name_index_2;
    uint16_t name_index_3;
    uint16_t name_index_4;
};
```

`name_index_1` and friends are **one-based** indexes into the `addresses` region
of the `names` table. A value of `0` indicates that the field does not define a
key. A key definition can specify between zero and four keys. These fields may
not refer to the special `__value__` attribute.

There need not be any keys defined in a data center file at all, but the client
will be *very slow* without certain key definitions. At minimum, a data center
file must contain a key definition at index `0` with all fields `0` (i.e. with
no keys) which all nodes can point to by default.

### Attributes

Each node in the data tree has zero or more attributes, which are name/value
pairs. They are of the form:

```cpp
struct DataCenterAttribute
{
    uint16_t name_index;
    DataCenterTypeCode type_code : 2;
    uint16_t extended_code : 14;
    union
    {
        int32_t i;
        bool b;
        float f;
        DataCenterAddress a;
    } value;
    uint32_t padding_1;
};
```

`name_index` is a **one-based** index into the `addresses` region of the `names`
table.

`type_code` specifies the kind of value the attribute holds. Valid values are as
follows:

```cpp
enum DataCenterTypeCode : uint8_t
{
    DATA_CENTER_TYPE_CODE_INT = 1,
    DATA_CENTER_TYPE_CODE_FLOAT = 2,
    DATA_CENTER_TYPE_CODE_STRING = 3,
};
```

`extended_code` specifies extra information based on the value of `type_code`:

* If `type_code` is `DATA_CENTER_TYPE_CODE_INT`, then the lowest bit of
  `extended_code` is either `0` or `1` to indicate whether the attribute's value
  is constrained to `1` (`true`) and `0` (`false`), i.e. whether it is a Boolean
  value. The higher bits are `0`.
* If `type_code` is `DATA_CENTER_TYPE_CODE_FLOAT`, then `extended_code` is `0`.
* If `type_code` is `DATA_CENTER_TYPE_CODE_STRING`, then `extended_code` is
  given by the expression `data_center_value_hash(value)` where `value` is the
  string value.

`value` holds the attribute value and is interpreted according to `type_code`
and `extended_code`. In the case of `DATA_CENTER_TYPE_CODE_STRING`, the `a`
field holds an address into the `data` region of the `values` table. For other
type codes, the value is written directly and is accessed through the `i`, `b`,
or `f` fields.

`padding_1` should be considered undefined. It was added in the 64-bit data
center format.

Some nodes will have a special attribute named `__value__`. In XML terms, this
represents the text of a node. For example, `<Foo>bar</Foo>` would be serialized
to a node called `Foo` containing an attribute named `__value__` with the string
value `bar`. It is worth noting that a node can have both text and child nodes,
such as `Foo` in this example:

```xml
<Foo>
    bar

    <Baz>
        ...
    </Baz>
</Foo>
```

Note that the `__value__` attribute, if present, may only be a string.

#### Value Hash

The `data_center_value_hash` function is a bizarre variant of CRC32 and is
defined as follows:

```cpp
const uint32_t value_hash_table[256] =
{
    0x00000000, 0x04c11db7, 0x09823b6e, 0x0d4326d9, 0x130476dc, 0x17c56b6b, 0x1a864db2, 0x1e475005,
    0x2608edb8, 0x22c9f00f, 0x2f8ad6d6, 0x2b4bcb61, 0x350c9b64, 0x31cd86d3, 0x3c8ea00a, 0x384fbdbd,
    0x4c11db70, 0x48d0c6c7, 0x4593e01e, 0x4152fda9, 0x5f15adac, 0x5bd4b01b, 0x569796c2, 0x52568b75,
    0x6a1936c8, 0x6ed82b7f, 0x639b0da6, 0x675a1011, 0x791d4014, 0x7ddc5da3, 0x709f7b7a, 0x745e66cd,
    0x9823b6e0, 0x9ce2ab57, 0x91a18d8e, 0x95609039, 0x8b27c03c, 0x8fe6dd8b, 0x82a5fb52, 0x8664e6e5,
    0xbe2b5b58, 0xbaea46ef, 0xb7a96036, 0xb3687d81, 0xad2f2d84, 0xa9ee3033, 0xa4ad16ea, 0xa06c0b5d,
    0xd4326d90, 0xd0f37027, 0xddb056fe, 0xd9714b49, 0xc7361b4c, 0xc3f706fb, 0xceb42022, 0xca753d95,
    0xf23a8028, 0xf6fb9d9f, 0xfbb8bb46, 0xff79a6f1, 0xe13ef6f4, 0xe5ffeb43, 0xe8bccd9a, 0xec7dd02d,
    0x34867077, 0x30476dc0, 0x3d044b19, 0x39c556ae, 0x278206ab, 0x23431b1c, 0x2e003dc5, 0x2ac12072,
    0x128e9dcf, 0x164f8078, 0x1b0ca6a1, 0x1fcdbb16, 0x018aeb13, 0x054bf6a4, 0x0808d07d, 0x0cc9cdca,
    0x7897ab07, 0x7c56b6b0, 0x71159069, 0x75d48dde, 0x6b93dddb, 0x6f52c06c, 0x6211e6b5, 0x66d0fb02,
    0x5e9f46bf, 0x5a5e5b08, 0x571d7dd1, 0x53dc6066, 0x4d9b3063, 0x495a2dd4, 0x44190b0d, 0x40d816ba,
    0xaca5c697, 0xa864db20, 0xa527fdf9, 0xa1e6e04e, 0xbfa1b04b, 0xbb60adfc, 0xb6238b25, 0xb2e29692,
    0x8aad2b2f, 0x8e6c3698, 0x832f1041, 0x87ee0df6, 0x99a95df3, 0x9d684044, 0x902b669d, 0x94ea7b2a,
    0xe0b41de7, 0xe4750050, 0xe9362689, 0xedf73b3e, 0xf3b06b3b, 0xf771768c, 0xfa325055, 0xfef34de2,
    0xc6bcf05f, 0xc27dede8, 0xcf3ecb31, 0xcbffd686, 0xd5b88683, 0xd1799b34, 0xdc3abded, 0xd8fba05a,
    0x690ce0ee, 0x6dcdfd59, 0x608edb80, 0x644fc637, 0x7a089632, 0x7ec98b85, 0x738aad5c, 0x774bb0eb,
    0x4f040d56, 0x4bc510e1, 0x46863638, 0x42472b8f, 0x5c007b8a, 0x58c1663d, 0x558240e4, 0x51435d53,
    0x251d3b9e, 0x21dc2629, 0x2c9f00f0, 0x285e1d47, 0x36194d42, 0x32d850f5, 0x3f9b762c, 0x3b5a6b9b,
    0x0315d626, 0x07d4cb91, 0x0a97ed48, 0x0e56f0ff, 0x1011a0fa, 0x14d0bd4d, 0x19939b94, 0x1d528623,
    0xf12f560e, 0xf5ee4bb9, 0xf8ad6d60, 0xfc6c70d7, 0xe22b20d2, 0xe6ea3d65, 0xeba91bbc, 0xef68060b,
    0xd727bbb6, 0xd3e6a601, 0xdea580d8, 0xda649d6f, 0xc423cd6a, 0xc0e2d0dd, 0xcda1f604, 0xc960ebb3,
    0xbd3e8d7e, 0xb9ff90c9, 0xb4bcb610, 0xb07daba7, 0xae3afba2, 0xaafbe615, 0xa7b8c0cc, 0xa379dd7b,
    0x9b3660c6, 0x9ff77d71, 0x92b45ba8, 0x9675461f, 0x8832161a, 0x8cf30bad, 0x81b02d74, 0x857130c3,
    0x5d8a9099, 0x594b8d2e, 0x5408abf7, 0x50c9b640, 0x4e8ee645, 0x4a4ffbf2, 0x470cdd2b, 0x43cdc09c,
    0x7b827d21, 0x7f436096, 0x7200464f, 0x76c15bf8, 0x68860bfd, 0x6c47164a, 0x61043093, 0x65c52d24,
    0x119b4be9, 0x155a565e, 0x18197087, 0x1cd86d30, 0x029f3d35, 0x065e2082, 0x0b1d065b, 0x0fdc1bec,
    0x3793a651, 0x3352bbe6, 0x3e119d3f, 0x3ad08088, 0x2497d08d, 0x2056cd3a, 0x2d15ebe3, 0x29d4f654,
    0xc5a92679, 0xc1683bce, 0xcc2b1d17, 0xc8ea00a0, 0xd6ad50a5, 0xd26c4d12, 0xdf2f6bcb, 0xdbee767c,
    0xe3a1cbc1, 0xe760d676, 0xea23f0af, 0xeee2ed18, 0xf0a5bd1d, 0xf464a0aa, 0xf9278673, 0xfde69bc4,
    0x89b8fd09, 0x8d79e0be, 0x803ac667, 0x84fbdbd0, 0x9abc8bd5, 0x9e7d9662, 0x933eb0bb, 0x97ffad0c,
    0xafb010b1, 0xab710d06, 0xa6322bdf, 0xa2f33668, 0xbcb4666d, 0xb8757bda, 0xb5365d03, 0xb1f740b4,
};

uint16_t data_center_value_hash(char16_t *string)
{
    uint32_t hash = 0;

    for (char16_t *current = string; *current; current++)
    {
        char16_t value = *current;

        if (value == u'\x9c')
            value = u'\x8c';
        else if (value == u'\xff')
            value = u'\x9f';
        else if (value == u'\x151')
            value = u'\x151';
        else if (((value >= u'a' && value <= u'z') || (value >= u'\xe0' && value <= u'\xfe')) &&
            !(value == u'\xf0' || value == u'\xf7'))
            value = c - u' ';

        value = htole16(value);

        for (size_t i = 0; i < 2; i++)
            hash = value_hash_table[(uint8_t)(hash ^ ((uint8_t *)&value)[i])] ^ hash >> 8;
    }

    return hash & 0x3fff;
}
```

(Note that `value_hash_table` is the same as [`string_hash_table`](#string-hash).)
